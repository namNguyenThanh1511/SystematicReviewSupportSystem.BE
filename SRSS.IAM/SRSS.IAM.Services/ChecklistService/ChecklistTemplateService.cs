using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.Checklist;

namespace SRSS.IAM.Services.ChecklistService
{
    public class ChecklistTemplateService : IChecklistTemplateService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ChecklistTemplateService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<ChecklistTemplateSummaryDto>> GetAllTemplatesAsync(bool? isSystem = null, CancellationToken cancellationToken = default)
        {
            var templates = await _unitOfWork.ChecklistTemplates.GetAllWithItemsAsync(isSystem, cancellationToken);

            return templates.Select(MapTemplateSummary).ToList();
        }

        public async Task<List<ChecklistTemplateSummaryDto>> GetSystemTemplatesAsync(CancellationToken cancellationToken = default)
        {
            var templates = await _unitOfWork.ChecklistTemplates.GetSystemTemplatesAsync(cancellationToken);
            return templates.Select(MapTemplateSummary).ToList();
        }

        public async Task<ChecklistTemplateDetailDto?> GetTemplateByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var template = await _unitOfWork.ChecklistTemplates.GetByIdWithItemsAsync(id, cancellationToken);
            return template == null ? null : MapTemplateDetail(template);
        }

        /// <summary>
        /// Creates a user-defined checklist template including hierarchical item templates.
        /// </summary>
        public async Task<ChecklistTemplateDetailDto> CreateCustomTemplateAsync(CreateChecklistTemplateDto dto, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new InvalidOperationException("Template name is required.");
            }

            if (dto.Items.Count == 0)
            {
                throw new InvalidOperationException("At least one checklist item is required.");
            }

            var now = DateTimeOffset.UtcNow;
            var template = new ChecklistTemplate
            {
                Id = Guid.NewGuid(),
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim(),
                IsSystem = false,
                Version = now.ToString("yyyyMMddHHmmss"),
                CreatedAt = now,
                UpdatedAt = now
            };

            var itemByNumber = new Dictionary<string, ChecklistItemTemplate>(StringComparer.OrdinalIgnoreCase);
            foreach (var itemDto in dto.Items.OrderBy(x => x.Order).ThenBy(x => x.ItemNumber))
            {
                if (string.IsNullOrWhiteSpace(itemDto.ItemNumber) || string.IsNullOrWhiteSpace(itemDto.Section) || string.IsNullOrWhiteSpace(itemDto.Topic) || string.IsNullOrWhiteSpace(itemDto.Description))
                {
                    throw new InvalidOperationException("Each item requires item number, section, topic and description.");
                }

                if (itemByNumber.ContainsKey(itemDto.ItemNumber))
                {
                    throw new InvalidOperationException($"Duplicate item number detected: {itemDto.ItemNumber}");
                }

                var entity = new ChecklistItemTemplate
                {
                    Id = Guid.NewGuid(),
                    TemplateId = template.Id,
                    ItemNumber = itemDto.ItemNumber.Trim(),
                    Section = itemDto.Section.Trim().ToUpperInvariant(),
                    Topic = itemDto.Topic.Trim(),
                    Description = itemDto.Description.Trim(),
                    Order = itemDto.Order,
                    IsRequired = itemDto.IsRequired,
                    HasLocationField = itemDto.HasLocationField,
                    DefaultSampleAnswer = itemDto.DefaultSampleAnswer,
                    CreatedAt = now,
                    ModifiedAt = now
                };

                template.ItemTemplates.Add(entity);
                itemByNumber[entity.ItemNumber] = entity;
            }

            foreach (var itemDto in dto.Items)
            {
                if (string.IsNullOrWhiteSpace(itemDto.ParentItemNumber))
                {
                    continue;
                }

                if (!itemByNumber.TryGetValue(itemDto.ItemNumber, out var current) || !itemByNumber.TryGetValue(itemDto.ParentItemNumber, out var parent))
                {
                    throw new InvalidOperationException($"Invalid parent reference for item {itemDto.ItemNumber}.");
                }

                current.ParentId = parent.Id;
            }

            await _unitOfWork.ChecklistTemplates.AddAsync(template, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapTemplateDetail(template);
        }

        /// <summary>
        /// Creates a checklist instance for a review by cloning all template items into editable responses.
        /// </summary>
        public async Task<ReviewChecklistDto> CloneTemplateToReviewAsync(Guid templateId, Guid reviewId, CancellationToken cancellationToken = default)
        {
            var template = await _unitOfWork.ChecklistTemplates.GetByIdWithItemsAsync(templateId, cancellationToken)
                ?? throw new InvalidOperationException($"Checklist template {templateId} was not found.");

            var reviewExists = await _unitOfWork.SystematicReviewProjects.AnyAsync(x => x.Id == reviewId, isTracking: false, cancellationToken: cancellationToken);
            if (!reviewExists)
            {
                throw new InvalidOperationException($"Review {reviewId} was not found.");
            }

            var existing = await _unitOfWork.ReviewChecklists.FindFirstOrDefaultAsync(
                x => x.ProjectId == reviewId && x.TemplateId == templateId,
                isTracking: false,
                cancellationToken: cancellationToken);
            if (existing != null)
            {
                throw new InvalidOperationException("This review already has a checklist for the selected template.");
            }

            var now = DateTimeOffset.UtcNow;
            var reviewChecklist = new ReviewChecklist
            {
                Id = Guid.NewGuid(),
                ProjectId = reviewId,
                TemplateId = template.Id,
                IsCompleted = false,
                CompletionPercentage = 0,
                LastUpdatedAt = now,
                CreatedAt = now,
                ModifiedAt = now
            };

            foreach (var item in template.ItemTemplates)
            {
                reviewChecklist.ItemResponses.Add(new ChecklistItemResponse
                {
                    Id = Guid.NewGuid(),
                    ReviewChecklistId = reviewChecklist.Id,
                    ItemTemplateId = item.Id,
                    Content = item.DefaultSampleAnswer,
                    Location = null,
                    IsNotApplicable = false,
                    IsReported = false,
                    LastUpdatedAt = now,
                    CreatedAt = now,
                    ModifiedAt = now
                });
            }

            await _unitOfWork.ReviewChecklists.AddAsync(reviewChecklist, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var created = await _unitOfWork.ReviewChecklists.GetByIdWithDetailsAsync(reviewChecklist.Id, cancellationToken)
                ?? throw new InvalidOperationException("Checklist clone was created but could not be loaded.");

            return ChecklistViewMapper.MapReviewChecklist(created);
        }

        private static ChecklistTemplateSummaryDto MapTemplateSummary(ChecklistTemplate template)
        {
            return new ChecklistTemplateSummaryDto
            {
                Id = template.Id,
                Name = template.Name,
                Description = template.Description,
                IsSystem = template.IsSystem,
                Version = template.Version,
                ItemCount = template.ItemTemplates.Count,
                CreatedAt = template.CreatedAt,
                UpdatedAt = template.UpdatedAt
            };
        }

        private static ChecklistTemplateDetailDto MapTemplateDetail(ChecklistTemplate template)
        {
            return new ChecklistTemplateDetailDto
            {
                Id = template.Id,
                Name = template.Name,
                Description = template.Description,
                IsSystem = template.IsSystem,
                Version = template.Version,
                CreatedAt = template.CreatedAt,
                UpdatedAt = template.UpdatedAt,
                Items = template.ItemTemplates
                    .OrderBy(x => x.Order)
                    .ThenBy(x => x.ItemNumber)
                    .Select(x => new ChecklistItemTemplateDto
                    {
                        Id = x.Id,
                        TemplateId = x.TemplateId,
                        ParentId = x.ParentId,
                        ItemNumber = x.ItemNumber,
                        Section = x.Section,
                        Topic = x.Topic,
                        Description = x.Description,
                        Order = x.Order,
                        IsRequired = x.IsRequired,
                        HasLocationField = x.HasLocationField,
                        DefaultSampleAnswer = x.DefaultSampleAnswer
                    })
                    .ToList()
            };
        }
    }
}
