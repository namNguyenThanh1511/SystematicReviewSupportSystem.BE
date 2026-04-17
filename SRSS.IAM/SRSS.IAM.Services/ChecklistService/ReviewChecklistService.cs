using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.Checklist;

namespace SRSS.IAM.Services.ChecklistService
{
    public class ReviewChecklistService : IReviewChecklistService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReviewChecklistService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<ReviewChecklistSummaryDto>> GetReviewChecklistsAsync(Guid reviewId, CancellationToken cancellationToken = default)
        {
            var checklists = await _unitOfWork.ReviewChecklists.GetByReviewIdWithDetailsAsync(reviewId, cancellationToken);
            return checklists.Select(ChecklistViewMapper.MapReviewChecklistSummary).ToList();
        }

        public async Task<ReviewChecklistDto?> GetChecklistByIdAsync(Guid checkListId, CancellationToken cancellationToken = default)
        {
            var checklist = await _unitOfWork.ReviewChecklists.GetByIdWithDetailsAsync(checkListId, cancellationToken);
            return checklist == null ? null : ChecklistViewMapper.MapReviewChecklist(checklist);
        }

        /// <summary>
        /// Updates a single checklist item response and recalculates completion metrics.
        /// </summary>
        public async Task<ChecklistItemResponseDto> UpdateItemResponseAsync(Guid reviewChecklistId, Guid itemId, UpdateChecklistItemDto dto, CancellationToken cancellationToken = default)
        {
            var reviewChecklist = await _unitOfWork.ReviewChecklists.GetByIdWithDetailsAsync(reviewChecklistId, cancellationToken)
                ?? throw new InvalidOperationException("Review checklist not found.");

            var itemTemplate = reviewChecklist.Template.ItemTemplates.FirstOrDefault(x => x.Id == itemId)
                ?? throw new InvalidOperationException("Checklist item does not belong to this checklist.");

            var parentItemIds = reviewChecklist.Template.ItemTemplates
                .Where(x => x.ParentId.HasValue)
                .Select(x => x.ParentId!.Value)
                .ToHashSet();
            var canRespond = !itemTemplate.IsSectionHeaderOnly && !parentItemIds.Contains(itemTemplate.Id);
            if (!canRespond)
            {
                throw new InvalidOperationException("Responses are allowed only for leaf checklist items.");
            }

            var now = DateTimeOffset.UtcNow;
            var response = await _unitOfWork.ChecklistItemResponses.GetByReviewChecklistAndItemAsync(reviewChecklistId, itemId, cancellationToken);
            if (response == null)
            {
                response = new ChecklistItemResponse
                {
                    Id = Guid.NewGuid(),
                    ReviewChecklistId = reviewChecklistId,
                    ItemTemplateId = itemId,
                    CreatedAt = now
                };

                await _unitOfWork.ChecklistItemResponses.AddAsync(response, cancellationToken);
            }

            // Handle Location field for Full checklists
            response.Location = dto.Location?.Trim();
            if (string.IsNullOrWhiteSpace(response.Location))
            {
                response.Location = null;
            }

            // Handle IsReported: prefer explicit DTO value, fallback to Location-based logic
            if (dto.IsReported.HasValue)
            {
                // Explicit yes/no response from client
                response.IsReported = dto.IsReported.Value;
                response.IsCompleted = true; // Mark as completed if explicitly reported, regardless of Location
            }
            else if (itemTemplate.HasLocationField)
            {
                // Full checklist: use location-based logic for backward compatibility
                response.IsReported = !string.IsNullOrWhiteSpace(response.Location);
                response.IsCompleted = !string.IsNullOrWhiteSpace(response.Location);
            }
            else
            {
                // Abstract checklist: requires explicit IsReported
                response.IsReported = false;
                response.IsCompleted = false;
            }

            response.LastUpdatedAt = now;
            response.ModifiedAt = now;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await CalculateCompletionPercentageAsync(reviewChecklistId, cancellationToken);

            var hasChildren = reviewChecklist.Template.ItemTemplates.Any(x => x.ParentId == itemTemplate.Id);
            return ChecklistViewMapper.MapItem(itemTemplate, response, hasChildren);
        }

        public async Task<ChecklistCompletionDto> CalculateCompletionPercentageAsync(Guid reviewChecklistId, CancellationToken cancellationToken = default)
        {
            var reviewChecklist = await _unitOfWork.ReviewChecklists.GetByIdWithDetailsAsync(reviewChecklistId, cancellationToken)
                ?? throw new InvalidOperationException("Review checklist not found.");

            var parentItemIds = reviewChecklist.Template.ItemTemplates
                .Where(x => x.ParentId.HasValue)
                .Select(x => x.ParentId!.Value)
                .ToHashSet();
            var eligibleItems = reviewChecklist.Template.ItemTemplates
                .Where(item => !item.IsSectionHeaderOnly && !parentItemIds.Contains(item.Id))
                .ToList();
            var totalItems = eligibleItems.Count;
            var responseMap = reviewChecklist.ItemResponses.ToDictionary(x => x.ItemTemplateId);

            var completedItems = 0;
            foreach (var item in eligibleItems)
            {
                if (!responseMap.TryGetValue(item.Id, out var response))
                {
                    continue;
                }

                if (IsItemCompleted(item, response))
                {
                    completedItems++;
                }
            }

            var percentage = totalItems == 0 ? 0 : Math.Round(completedItems * 100.0 / totalItems, 2);
            reviewChecklist.CompletionPercentage = percentage;
            reviewChecklist.IsCompleted = percentage >= 100;
            reviewChecklist.LastUpdatedAt = DateTimeOffset.UtcNow;
            reviewChecklist.ModifiedAt = reviewChecklist.LastUpdatedAt;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ChecklistCompletionDto
            {
                ReviewChecklistId = reviewChecklist.Id,
                CompletionPercentage = reviewChecklist.CompletionPercentage,
                IsCompleted = reviewChecklist.IsCompleted
            };
        }

        public async Task<ReviewChecklistDto?> GetChecklistForReportAsync(Guid checkListId, CancellationToken cancellationToken = default)
        {
            return await GetChecklistByIdAsync(checkListId, cancellationToken);
        }

        /// <summary>
        /// Generates a Word report for checklist reporting.
        /// </summary>
        public async Task<byte[]> GenerateReportAsync(Guid checkListId, GenerateReportRequest request, CancellationToken cancellationToken = default)
        {
            var checklist = await _unitOfWork.ReviewChecklists.GetByIdWithDetailsAsync(checkListId, cancellationToken)
                ?? throw new InvalidOperationException("Review checklist not found.");

            var view = ChecklistViewMapper.MapReviewChecklist(checklist);
            var templatePath = ResolvePrismaTemplatePath();
            var templateBytes = File.ReadAllBytes(templatePath);
            var sections = view.Sections
                .Select(section => new ChecklistSectionDto
                {
                    Section = section.Section,
                    Items = section.Items
                        .Where(item => !request.IncludeOnlyCompletedItems || IsItemCompleted(item))
                        .OrderBy(item => item.Order)
                        .ThenBy(item => item.ItemNumber)
                        .ToList()
                })
                .Where(section => section.Items.Count > 0)
                .ToList();

            using var stream = new MemoryStream();
            stream.Write(templateBytes, 0, templateBytes.Length);
            stream.Position = 0;

            using (var wordDoc = WordprocessingDocument.Open(stream, true))
            {
                var mainPart = wordDoc.MainDocumentPart
                    ?? throw new InvalidOperationException("The PRISMA template is missing the main document part.");

                var body = mainPart.Document.Body
                    ?? throw new InvalidOperationException("The PRISMA template is missing the document body.");

                var tables = body.Elements<Table>().ToList();
                if (tables.Count == 0)
                {
                    throw new InvalidOperationException("The PRISMA template does not contain any checklist tables.");
                }

                var checklistTable = tables[0];
                var templateRows = checklistTable.Elements<TableRow>().ToList();
                if (templateRows.Count < 3)
                {
                    throw new InvalidOperationException("The PRISMA template checklist table must contain at least one header row, one section row, and one item row.");
                }

                var sectionPrototypeRow = templateRows[1];
                var itemPrototypeRow = templateRows[2];

                foreach (var row in templateRows.Skip(1).ToList())
                {
                    row.Remove();
                }

                foreach (var section in sections)
                {
                    var sectionRow = (TableRow)sectionPrototypeRow.CloneNode(true);
                    PopulateSectionRow(sectionRow, section.Section);
                    checklistTable.AppendChild(sectionRow);

                    foreach (var item in section.Items)
                    {
                        var itemRow = (TableRow)itemPrototypeRow.CloneNode(true);
                        PopulateItemRow(itemRow, item);
                        checklistTable.AppendChild(itemRow);
                    }
                }

                foreach (var extraTable in tables.Skip(1).ToList())
                {
                    extraTable.Remove();
                }

                mainPart.Document.Save();
            }

            return stream.ToArray();
        }

        private static bool IsItemCompleted(ChecklistItemResponseDto item)
        {
            if (item.IsReported)
            {
                return true;
            }

            return item.HasLocationField && !string.IsNullOrWhiteSpace(item.Location);
        }

        private static void PopulateSectionRow(TableRow row, string section)
        {
            var cells = row.Elements<TableCell>().ToList();
            if (cells.Count == 0)
            {
                return;
            }

            SetCellText(cells[0], section);

            for (var i = 1; i < cells.Count; i++)
            {
                SetCellText(cells[i], string.Empty);
            }
        }

        private static string GetCellText(TableCell cell)
        {
            return string.Concat(cell.Descendants<Text>().Select(t => t.Text));
        }

        private static void SetCellText(TableCell cell, string value)
        {
            var texts = cell.Descendants<Text>().ToList();
            if (texts.Count > 0)
            {
                texts[0].Text = value;
                texts[0].Space = SpaceProcessingModeValues.Preserve;

                for (var i = 1; i < texts.Count; i++)
                {
                    texts[i].Text = string.Empty;
                }

                return;
            }

            var paragraph = cell.GetFirstChild<Paragraph>();
            if (paragraph == null)
            {
                paragraph = cell.AppendChild(new Paragraph());
            }

            var run = paragraph.GetFirstChild<Run>();
            if (run == null)
            {
                run = paragraph.AppendChild(new Run());
            }

            run.AppendChild(new Text(value) { Space = SpaceProcessingModeValues.Preserve });
        }

        private static void PopulateItemRow(TableRow row, ChecklistItemResponseDto item)
        {
            var cells = row.Elements<TableCell>().ToList();
            if (cells.Count < 4)
            {
                return;
            }

            SetCellText(cells[0], item.Topic ?? string.Empty);
            SetCellText(cells[1], item.ItemNumber ?? string.Empty);
            SetCellText(cells[2], item.Description ?? string.Empty);

            var value = item.Location ?? string.Empty;

            SetCellText(cells[3], value);
        }

        private static string ResolvePrismaTemplatePath()
        {
            const string templateFileName = "PRISMA_2020_checklist.docx";
            var roots = new[]
            {
                Directory.GetCurrentDirectory(),
                AppContext.BaseDirectory
            }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase);

            foreach (var root in roots)
            {
                var current = new DirectoryInfo(root);
                for (var depth = 0; current != null && depth < 12; depth++, current = current.Parent)
                {
                    var directCandidate = Path.Combine(current.FullName, "docs", "specs", templateFileName);
                    if (File.Exists(directCandidate))
                    {
                        return directCandidate;
                    }

                    var apiCandidate = Path.Combine(current.FullName, "SRSS.IAM.API", "docs", "specs", templateFileName);
                    if (File.Exists(apiCandidate))
                    {
                        return apiCandidate;
                    }
                }
            }

            throw new InvalidOperationException("Cannot locate template file 'PRISMA Checklist.docx'. Expected under SRSS.IAM.API/docs/specs.");
        }

        private static bool IsItemCompleted(ChecklistItemTemplate item, ChecklistItemResponse response)
        {
            if (response.IsCompleted)
            {
                return true;
            }

            return item.HasLocationField && !string.IsNullOrWhiteSpace(response.Location);
        }

        private static Paragraph CreateHeading1(string text)
        {
            return new Paragraph(
                new ParagraphProperties(new ParagraphStyleId { Val = "Heading1" }),
                new Run(new Text(text)));
        }

        private static Paragraph CreateHeading2(string text)
        {
            return new Paragraph(
                new ParagraphProperties(new ParagraphStyleId { Val = "Heading2" }),
                new Run(new Text(text)));
        }

        private static Paragraph CreateNormal(string text)
        {
            return new Paragraph(new Run(new Text(text)));
        }

        private static Paragraph CreateItemLine(string text, string leftIndent, bool bold = false)
        {
            var runProps = new RunProperties();
            if (bold)
            {
                runProps.Append(new Bold());
            }

            return new Paragraph(
                new ParagraphProperties(new Indentation { Left = leftIndent }),
                new Run(runProps, new Text(text)));
        }
    }

    internal static class ChecklistViewMapper
    {
        public static ReviewChecklistDto MapReviewChecklist(ReviewChecklist checklist)
        {
            var responseLookup = checklist.ItemResponses.ToDictionary(x => x.ItemTemplateId);
            var childrenByParent = checklist.Template.ItemTemplates
                .Where(x => x.ParentId.HasValue)
                .GroupBy(x => x.ParentId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            ChecklistItemResponseDto MapItemNode(ChecklistItemTemplate item)
            {
                responseLookup.TryGetValue(item.Id, out var response);
                childrenByParent.TryGetValue(item.Id, out var rawChildren);

                var children = (rawChildren ?? new List<ChecklistItemTemplate>())
                    .OrderBy(x => x.Order)
                    .ThenBy(x => x.ItemNumber, StringComparer.OrdinalIgnoreCase)
                    .Select(MapItemNode)
                    .ToList();

                return MapItem(item, response, children);
            }

            List<ChecklistItemResponseDto> FlattenItems(IEnumerable<ChecklistItemResponseDto> nodes)
            {
                var flattened = new List<ChecklistItemResponseDto>();
                foreach (var node in nodes)
                {
                    flattened.Add(node);
                    if (node.Children.Count > 0)
                    {
                        flattened.AddRange(FlattenItems(node.Children));
                    }
                }

                return flattened;
            }

            var rootItems = checklist.Template.ItemTemplates
                .Where(x => x.ParentId == null)
                .OrderBy(x => x.Order)
                .ThenBy(x => x.ItemNumber, StringComparer.OrdinalIgnoreCase)
                .Select(MapItemNode)
                .ToList();

            var items = FlattenItems(rootItems);

            var sections = checklist.Template.Sections
                .OrderBy(x => x.Order)
                .ThenBy(x => x.SectionNumber, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .Select(section => new ChecklistSectionDto
                {
                    SectionId = section.Id,
                    SectionNumber = section.SectionNumber,
                    Section = section.Name,
                    Description = section.Description,
                    Order = section.Order,
                    Items = items
                        .Where(x => x.SectionId == section.Id && x.ParentId == null)
                        .OrderBy(x => x.Order)
                        .ThenBy(x => x.ItemNumber, StringComparer.OrdinalIgnoreCase)
                        .ToList()
                })
                .Where(x => x.Items.Count > 0)
                .ToList();

            if (sections.Count == 0)
            {
                sections = rootItems
                    .GroupBy(x => x.Section)
                    .Select((g, index) => new ChecklistSectionDto
                    {
                        SectionId = null,
                        SectionNumber = (index + 1).ToString(),
                        Section = g.Key,
                        Description = null,
                        Order = index + 1,
                        Items = g.OrderBy(x => x.Order).ThenBy(x => x.ItemNumber, StringComparer.OrdinalIgnoreCase).ToList()
                    })
                    .ToList();
            }

            return new ReviewChecklistDto
            {
                ReviewChecklistId = checklist.Id,
                ReviewId = checklist.ProjectId,
                ReviewTitle = checklist.Project?.Title ?? string.Empty,
                TemplateId = checklist.TemplateId,
                TemplateName = checklist.Template.Name,
                IsCompleted = checklist.IsCompleted,
                CompletionPercentage = checklist.CompletionPercentage,
                LastUpdatedAt = checklist.LastUpdatedAt,
                Sections = sections,
                Items = items
            };
        }

        public static ReviewChecklistSummaryDto MapReviewChecklistSummary(ReviewChecklist checklist)
        {
            return new ReviewChecklistSummaryDto
            {
                ReviewChecklistId = checklist.Id,
                ReviewId = checklist.ProjectId,
                ReviewTitle = checklist.Project?.Title ?? string.Empty,
                TemplateId = checklist.TemplateId,
                TemplateName = checklist.Template.Name,
                Type = checklist.Template.Type,
                TypeName = checklist.Template.Type.ToString(),
                IsCompleted = checklist.IsCompleted,
                CompletionPercentage = checklist.CompletionPercentage,
                ItemCount = checklist.Template.ItemTemplates.Count,
                LastUpdatedAt = checklist.LastUpdatedAt
            };
        }

        public static ChecklistItemResponseDto MapItem(ChecklistItemTemplate item, ChecklistItemResponse? response, bool hasChildren = false)
        {
            return new ChecklistItemResponseDto
            {
                ItemTemplateId = item.Id,
                ResponseId = response?.Id,
                SectionId = item.SectionId,
                ParentId = item.ParentId,
                ItemNumber = item.ItemNumber,
                Section = item.Section,
                Topic = item.Topic,
                Description = item.Description,
                Order = item.Order,
                IsRequired = item.IsRequired,
                HasLocationField = item.HasLocationField,
                IsSectionHeaderOnly = item.IsSectionHeaderOnly,
                HasChildren = hasChildren,
                CanRespond = !item.IsSectionHeaderOnly && !hasChildren,
                Children = new List<ChecklistItemResponseDto>(),
                Location = response?.Location,
                IsReported = response?.IsReported ?? false,
                LastUpdatedAt = response?.LastUpdatedAt
            };
        }

        public static ChecklistItemResponseDto MapItem(ChecklistItemTemplate item, ChecklistItemResponse? response, List<ChecklistItemResponseDto> children)
        {
            return new ChecklistItemResponseDto
            {
                ItemTemplateId = item.Id,
                ResponseId = response?.Id,
                SectionId = item.SectionId,
                ParentId = item.ParentId,
                ItemNumber = item.ItemNumber,
                Section = item.Section,
                Topic = item.Topic,
                Description = item.Description,
                Order = item.Order,
                IsRequired = item.IsRequired,
                HasLocationField = item.HasLocationField,
                IsSectionHeaderOnly = item.IsSectionHeaderOnly,
                HasChildren = children.Count > 0,
                CanRespond = !item.IsSectionHeaderOnly && children.Count == 0,
                Children = children,
                Location = response?.Location,
                IsReported = response?.IsReported ?? false,
                IsCompleted = response?.IsCompleted ?? false,
                LastUpdatedAt = response?.LastUpdatedAt
            };
        }
    }
}
