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

            response.Content = dto.Content?.Trim();
            response.Location = dto.Location?.Trim();
            response.IsNotApplicable = dto.IsNotApplicable;
            response.IsReported = dto.IsReported;
            response.LastUpdatedAt = now;
            response.ModifiedAt = now;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await CalculateCompletionPercentageAsync(reviewChecklistId, cancellationToken);

            return ChecklistViewMapper.MapItem(itemTemplate, response);
        }

        public async Task<ChecklistCompletionDto> CalculateCompletionPercentageAsync(Guid reviewChecklistId, CancellationToken cancellationToken = default)
        {
            var reviewChecklist = await _unitOfWork.ReviewChecklists.GetByIdWithDetailsAsync(reviewChecklistId, cancellationToken)
                ?? throw new InvalidOperationException("Review checklist not found.");

            var totalItems = reviewChecklist.Template.ItemTemplates.Count;
            var responseMap = reviewChecklist.ItemResponses.ToDictionary(x => x.ItemTemplateId);

            var completedItems = 0;
            foreach (var item in reviewChecklist.Template.ItemTemplates)
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
            var items = view.Items
                .Where(x => !request.IncludeOnlyCompletedItems || x.IsNotApplicable || x.IsReported || !string.IsNullOrWhiteSpace(x.Content))
                .ToList();

            using var stream = new MemoryStream();
            using (var wordDoc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, true))
            {
                var mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new Document(new Body());
                var body = mainPart.Document.Body!;

                body.Append(CreateHeading1($"Checklist Report - {view.TemplateName}"));
                body.Append(CreateNormal($"Review: {view.ReviewTitle}"));
                body.Append(CreateNormal($"Generated at (UTC): {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss}"));
                body.Append(CreateNormal($"Completion: {view.CompletionPercentage:F2}%"));
                body.Append(new Paragraph(new Run(new Text(" "))));

                var groupedBySection = items
                    .GroupBy(x => x.Section)
                    .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                foreach (var section in groupedBySection)
                {
                    body.Append(CreateHeading2(section.Key));

                    foreach (var item in section.OrderBy(x => x.Order).ThenBy(x => x.ItemNumber))
                    {
                        var leftIndent = item.ParentId.HasValue ? "720" : "0";
                        body.Append(CreateItemLine($"{item.ItemNumber}. {item.Topic}", leftIndent, true));
                        body.Append(CreateItemLine(item.Description, leftIndent));
                        body.Append(CreateItemLine($"Content: {item.Content ?? "(empty)"}", leftIndent));
                        body.Append(CreateItemLine($"Location: {item.Location ?? "(empty)"}", leftIndent));
                        body.Append(CreateItemLine($"Reported: {item.IsReported} | N/A: {item.IsNotApplicable}", leftIndent));
                        body.Append(new Paragraph(new Run(new Text(" "))));
                    }
                }

                mainPart.Document.Save();
            }

            return stream.ToArray();
        }

        private static bool IsItemCompleted(ChecklistItemTemplate item, ChecklistItemResponse response)
        {
            if (response.IsNotApplicable)
            {
                return true;
            }

            if (response.IsReported)
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(response.Content))
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
            var items = checklist.Template.ItemTemplates
                .OrderBy(x => x.Order)
                .ThenBy(x => x.ItemNumber)
                .Select(item =>
                {
                    responseLookup.TryGetValue(item.Id, out var response);
                    return MapItem(item, response);
                })
                .ToList();

            var sections = items
                .GroupBy(x => x.Section)
                .OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
                .Select(g => new ChecklistSectionDto
                {
                    Section = g.Key,
                    Items = g.OrderBy(x => x.Order).ThenBy(x => x.ItemNumber).ToList()
                })
                .ToList();

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
                IsCompleted = checklist.IsCompleted,
                CompletionPercentage = checklist.CompletionPercentage,
                ItemCount = checklist.Template.ItemTemplates.Count,
                LastUpdatedAt = checklist.LastUpdatedAt
            };
        }

        public static ChecklistItemResponseDto MapItem(ChecklistItemTemplate item, ChecklistItemResponse? response)
        {
            return new ChecklistItemResponseDto
            {
                ItemTemplateId = item.Id,
                ResponseId = response?.Id,
                ParentId = item.ParentId,
                ItemNumber = item.ItemNumber,
                Section = item.Section,
                Topic = item.Topic,
                Description = item.Description,
                Order = item.Order,
                IsRequired = item.IsRequired,
                HasLocationField = item.HasLocationField,
                Content = response?.Content,
                Location = response?.Location,
                IsNotApplicable = response?.IsNotApplicable ?? false,
                IsReported = response?.IsReported ?? false,
                LastUpdatedAt = response?.LastUpdatedAt
            };
        }
    }
}
