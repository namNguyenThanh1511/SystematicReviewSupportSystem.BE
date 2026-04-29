using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.Checklist;
using SRSS.IAM.Services.GrobidClient;
using SRSS.IAM.Services.GeminiService;
using SRSS.IAM.Services.NotificationService;
using Microsoft.AspNetCore.Http;
using SRSS.IAM.Services.UserService;

namespace SRSS.IAM.Services.ChecklistService
{
    public class ReviewChecklistService : IReviewChecklistService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGrobidService _grobidService;
        private readonly IGeminiService _geminiService;
        private readonly INotificationService _notificationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IChecklistAutoFillQueue _autoFillQueue;
        
        public ReviewChecklistService(
            IUnitOfWork unitOfWork,
            IGrobidService grobidService,
            IGeminiService geminiService,
            INotificationService notificationService,
            ICurrentUserService currentUserService,
            IChecklistAutoFillQueue autoFillQueue)
        {
            _unitOfWork = unitOfWork;
            _grobidService = grobidService;
            _geminiService = geminiService;
            _notificationService = notificationService;
            _currentUserService = currentUserService;
            _autoFillQueue = autoFillQueue;
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
            var type = checklist.Template.Type;
            var view = ChecklistViewMapper.MapReviewChecklist(checklist);
            var templatePath = ResolvePrismaTemplatePath(type);
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

                    var sectionItems = FlattenItemsForReport(section.Items)
                        .Where(item => !request.IncludeOnlyCompletedItems || IsItemCompleted(item));

                    foreach (var item in sectionItems)
                    {
                        var itemRow = (TableRow)itemPrototypeRow.CloneNode(true);
                        PopulateItemRow(itemRow, item, type);
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

        public async Task<ChecklistAutoFillStatusDto> QueueAutoFillChecklist(Guid checkListId, IFormFile file, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("PDF file is required.");
            }

            var userIdStr = _currentUserService.GetUserId();
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                throw new InvalidOperationException("Unable to determine the current user.");
            }

            // Buffer PDF bytes so the work item outlives the HTTP request
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var pdfBytes = memoryStream.ToArray();

            var workItem = new ChecklistAutoFillWorkItem
            {
                ReviewChecklistId = checkListId,
                UserId = userId,
                FileName = file.FileName,
                PdfBytes = pdfBytes
            };

            if (!_autoFillQueue.TryWrite(workItem))
            {
                throw new InvalidOperationException("Auto-fill queue is full. Please try again later.");
            }

            return new ChecklistAutoFillStatusDto
            {
                ReviewChecklistId = checkListId,
                Status = AutoFillStatus.Queued,
                Message = "Auto-fill job has been queued. Listen for 'OnChecklistAutoFillStatus' SignalR events for progress.",
                Timestamp = DateTimeOffset.UtcNow
            };
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

        private static void PopulateItemRow(TableRow row, ChecklistItemResponseDto item, ChecklistType templateType)
        {
            var cells = row.Elements<TableCell>().ToList();
            if (cells.Count < 4)
            {
                return;
            }

            SetCellText(cells[0], item.Topic ?? string.Empty);
            SetCellText(cells[1], item.ItemNumber ?? string.Empty);
            SetCellText(cells[2], item.Description ?? string.Empty);

            var value = templateType == ChecklistType.Abstract
                ? (item.IsReported ? "Yes" : "No")
                : item.Location ?? string.Empty;

            SetCellText(cells[3], value);
        }

        private static IEnumerable<ChecklistItemResponseDto> FlattenItemsForReport(IEnumerable<ChecklistItemResponseDto> items)
        {
            foreach (var item in items)
            {
                if (!item.IsSectionHeaderOnly)
                {
                    yield return item;
                }

                if (item.Children.Count == 0)
                {
                    continue;
                }

                foreach (var child in FlattenItemsForReport(item.Children))
                {
                    yield return child;
                }
            }
        }

        private static string ResolvePrismaTemplatePath(ChecklistType checklistType)
        {
            var templateFileName = checklistType == ChecklistType.Abstract
                ? "PRISMA_2020_abstract_checklist.docx"
                : "PRISMA_2020_checklist.docx";
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

            throw new InvalidOperationException($"Cannot locate template file '{templateFileName}'. Expected under SRSS.IAM.API/docs/specs.");
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

        public async Task AutoFillChecklistFromPdfAsync(Guid checkListId, Stream pdfStream, string fileName, Guid userId, CancellationToken cancellationToken = default)
        {
            // 1. Get Checklist Details
            await SendAutoFillStatus(userId, checkListId, AutoFillStatus.ExtractingText, "Extracting text from PDF via GROBID...");

            var checklist = await _unitOfWork.ReviewChecklists.GetByIdWithDetailsAsync(checkListId, cancellationToken)
                ?? throw new InvalidOperationException("Review checklist not found.");

            // 2. Extract Fulltext via GROBID
            using var ms = new MemoryStream();
            await pdfStream.CopyToAsync(ms, cancellationToken);
            ms.Position = 0;

            var teiXml = await _grobidService.ProcessFulltextDocumentAsync(ms, cancellationToken);
            if (string.IsNullOrWhiteSpace(teiXml))
            {
                throw new InvalidOperationException("Failed to extract text from PDF via GROBID.");
            }

            var fullText = GrobidTeiParser.ParseBodyText(teiXml);
            if (string.IsNullOrWhiteSpace(fullText))
            {
                throw new InvalidOperationException("Extracted text is empty.");
            }

            await SendAutoFillStatus(userId, checkListId, AutoFillStatus.TextExtracted, "Text extracted successfully. Preparing AI analysis...");

            // 3. Prepare Items for Gemini
            var view = ChecklistViewMapper.MapReviewChecklist(checklist);
            var respondableItems = view.Items.Where(x => x.CanRespond).ToList();
            var itemsToMap = respondableItems
                .Select(x => new { x.ItemNumber, x.Topic, x.Description })
                .ToList();

            // 4. Construct Prompt for Gemini
            var itemsJson = System.Text.Json.JsonSerializer.Serialize(itemsToMap);
            var prompt = $@"
You are a Systematic Literature Review expert assistant. 
Your task is to analyze the provided full text of a research paper and map it to the PRISMA 2020 checklist items.

For each checklist item provided below, identify:
1. 'location': Where in the paper this item is discussed (e.g., 'Methods section, page 4', 'Introduction, paragraph 2').
2. 'isReported': Boolean, true if the item is explicitly addressed in the text.
3. 'reasoning': A brief explanation of why you think this item is addressed or not.

PAPER TEXT:
---
{fullText}
---

CHECKLIST ITEMS:
{itemsJson}

Return the results as a JSON object with a 'mappings' array containing objects with 'itemNumber', 'location', 'isReported', and 'reasoning'.
";

            // 5. Call Gemini
            await SendAutoFillStatus(userId, checkListId, AutoFillStatus.AnalyzingWithAI,
                $"Analyzing {respondableItems.Count} checklist items with AI...",
                totalItems: respondableItems.Count);

            var geminiResponse = await _geminiService.GenerateStructuredContentAsync<GeminiChecklistMappingResponse>(prompt);

            // 6. Update Database
            await SendAutoFillStatus(userId, checkListId, AutoFillStatus.SavingResults,
                $"AI analysis complete. Saving {geminiResponse.Mappings.Count} mapped results...",
                totalItems: respondableItems.Count,
                mappedItems: geminiResponse.Mappings.Count);

            var now = DateTimeOffset.UtcNow;
            foreach (var mapping in geminiResponse.Mappings)
            {
                var item = checklist.Template.ItemTemplates.FirstOrDefault(x => x.ItemNumber == mapping.ItemNumber);
                if (item == null) continue;

                var response = await _unitOfWork.ChecklistItemResponses.GetByReviewChecklistAndItemAsync(checkListId, item.Id, cancellationToken);
                if (response == null)
                {
                    response = new ChecklistItemResponse
                    {
                        Id = Guid.NewGuid(),
                        ReviewChecklistId = checkListId,
                        ItemTemplateId = item.Id,
                        CreatedAt = now
                    };
                    await _unitOfWork.ChecklistItemResponses.AddAsync(response, cancellationToken);
                }

                response.Location = mapping.Location;
                response.IsReported = mapping.IsReported;
                response.IsCompleted = true;
                response.LastUpdatedAt = now;
                response.ModifiedAt = now;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            var completion = await CalculateCompletionPercentageAsync(checkListId, cancellationToken);

            // 7. Send final completion status with full details
            await _notificationService.SendChecklistAutoFillStatusAsync(userId, new ChecklistAutoFillStatusDto
            {
                ReviewChecklistId = checkListId,
                Status = AutoFillStatus.Completed,
                Message = "Checklist auto-fill completed successfully.",
                CompletionPercentage = completion.CompletionPercentage,
                TotalItems = respondableItems.Count,
                MappedItems = geminiResponse.Mappings.Count,
                Timestamp = DateTimeOffset.UtcNow
            });
        }

        private async Task SendAutoFillStatus(Guid userId, Guid checklistId, string status, string message,
            double? completionPercentage = null, int? totalItems = null, int? mappedItems = null)
        {
            await _notificationService.SendChecklistAutoFillStatusAsync(userId, new ChecklistAutoFillStatusDto
            {
                ReviewChecklistId = checklistId,
                Status = status,
                Message = message,
                CompletionPercentage = completionPercentage,
                TotalItems = totalItems,
                MappedItems = mappedItems,
                Timestamp = DateTimeOffset.UtcNow
            });
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
