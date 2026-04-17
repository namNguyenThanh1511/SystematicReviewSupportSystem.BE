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

            var hasSectionFlow = dto.Sections.Count > 0;
            if (!hasSectionFlow && dto.Items.Count == 0)
            {
                throw new InvalidOperationException("At least one checklist item is required.");
            }

            var now = DateTimeOffset.UtcNow;
            var checklistType = (ChecklistType)dto.Type;
            var template = new ChecklistTemplate
            {
                Id = Guid.NewGuid(),
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim(),
                IsSystem = false,
                Version = now.ToString("yyyyMMddHHmmss"),
                Type = checklistType,
                CreatedAt = now,
                UpdatedAt = now
            };

            var usedItemNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (hasSectionFlow)
            {
                BuildTemplateFromSections(template, dto.Sections, usedItemNumbers, now, checklistType);
            }
            else
            {
                BuildTemplateFromLegacyItems(template, dto.Items, usedItemNumbers, now, checklistType);
            }

            if (template.ItemTemplates.Count == 0)
            {
                throw new InvalidOperationException("At least one checklist item is required.");
            }

            await _unitOfWork.ChecklistTemplates.AddAsync(template, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapTemplateDetail(template);
        }

        private static void BuildTemplateFromSections(
            ChecklistTemplate template,
            IEnumerable<CreateChecklistSectionTemplateDto> sections,
            HashSet<string> usedItemNumbers,
            DateTimeOffset now,
            ChecklistType checklistType)
        {
            var orderedSections = sections
                .OrderBy(x => x.Order)
                .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            for (var index = 0; index < orderedSections.Count; index++)
            {
                var sectionDto = orderedSections[index];
                if (string.IsNullOrWhiteSpace(sectionDto.Name))
                {
                    throw new InvalidOperationException("Each section requires a name.");
                }

                var section = new ChecklistSectionTemplate
                {
                    Id = Guid.NewGuid(),
                    TemplateId = template.Id,
                    Name = sectionDto.Name.Trim(),
                    Description = sectionDto.Description?.Trim(),
                    Order = sectionDto.Order > 0 ? sectionDto.Order : index + 1,
                    SectionNumber = string.IsNullOrWhiteSpace(sectionDto.SectionNumber)
                        ? (index + 1).ToString()
                        : sectionDto.SectionNumber.Trim(),
                    CreatedAt = now,
                    ModifiedAt = now
                };

                template.Sections.Add(section);

                var orderedRootItems = sectionDto.Items
                    .OrderBy(x => x.Order)
                    .ThenBy(x => x.Topic, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                for (var itemIndex = 0; itemIndex < orderedRootItems.Count; itemIndex++)
                {
                    CreateItemRecursive(template, section, null, orderedRootItems[itemIndex], itemIndex + 1, usedItemNumbers, now, checklistType);
                }
            }
        }

        private static void BuildTemplateFromLegacyItems(
            ChecklistTemplate template,
            IEnumerable<CreateChecklistItemTemplateDto> items,
            HashSet<string> usedItemNumbers,
            DateTimeOffset now,
            ChecklistType checklistType)
        {
            var groupedBySection = items
                .GroupBy(x => string.IsNullOrWhiteSpace(x.Section) ? "General" : x.Section.Trim())
                .OrderBy(g => g.Min(x => x.Order))
                .ThenBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
                .ToList();

            for (var sectionIndex = 0; sectionIndex < groupedBySection.Count; sectionIndex++)
            {
                var sectionName = groupedBySection[sectionIndex].Key;
                var section = new ChecklistSectionTemplate
                {
                    Id = Guid.NewGuid(),
                    TemplateId = template.Id,
                    Name = sectionName,
                    Description = null,
                    Order = sectionIndex + 1,
                    SectionNumber = (sectionIndex + 1).ToString(),
                    CreatedAt = now,
                    ModifiedAt = now
                };

                template.Sections.Add(section);

                var nodes = BuildLegacyNodes(groupedBySection[sectionIndex].ToList());
                var rootNodes = nodes
                    .Where(x => x.Parent == null)
                    .OrderBy(x => x.Dto.Order)
                    .ThenBy(x => x.Dto.ItemNumber, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(x => x.Dto.Topic, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                for (var itemIndex = 0; itemIndex < rootNodes.Count; itemIndex++)
                {
                    CreateItemRecursive(template, section, null, rootNodes[itemIndex], itemIndex + 1, usedItemNumbers, now, checklistType);
                }
            }
        }

        private static List<LegacyItemNode> BuildLegacyNodes(List<CreateChecklistItemTemplateDto> sectionItems)
        {
            var nodeByItemNumber = new Dictionary<string, LegacyItemNode>(StringComparer.OrdinalIgnoreCase);
            var nodes = sectionItems.Select(x => new LegacyItemNode { Dto = x }).ToList();

            foreach (var node in nodes)
            {
                if (string.IsNullOrWhiteSpace(node.Dto.ItemNumber))
                {
                    continue;
                }

                var key = node.Dto.ItemNumber.Trim();
                if (!nodeByItemNumber.TryAdd(key, node))
                {
                    throw new InvalidOperationException($"Duplicate item number detected: {key}");
                }
            }

            foreach (var node in nodes)
            {
                if (string.IsNullOrWhiteSpace(node.Dto.ParentItemNumber))
                {
                    continue;
                }

                var parentKey = node.Dto.ParentItemNumber.Trim();
                if (!nodeByItemNumber.TryGetValue(parentKey, out var parent))
                {
                    throw new InvalidOperationException($"Invalid parent reference for item {node.Dto.ItemNumber ?? node.Dto.Topic}.");
                }

                node.Parent = parent;
                parent.Children.Add(node);
            }

            return nodes;
        }

        private static ChecklistItemTemplate CreateItemRecursive(
            ChecklistTemplate template,
            ChecklistSectionTemplate section,
            ChecklistItemTemplate? parent,
            LegacyItemNode node,
            int siblingIndex,
            HashSet<string> usedItemNumbers,
            DateTimeOffset now,
            ChecklistType checklistType)
        {
            var item = CreateItemRecursive(template, section, parent, node.Dto, siblingIndex, usedItemNumbers, now, checklistType);

            var orderedChildren = node.Children
                .OrderBy(x => x.Dto.Order)
                .ThenBy(x => x.Dto.ItemNumber, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.Dto.Topic, StringComparer.OrdinalIgnoreCase)
                .ToList();

            for (var index = 0; index < orderedChildren.Count; index++)
            {
                CreateItemRecursive(template, section, item, orderedChildren[index], index + 1, usedItemNumbers, now, checklistType);
            }

            if (orderedChildren.Count > 0)
            {
                item.IsSectionHeaderOnly = true;
                item.HasLocationField = false;
            }

            return item;
        }

        private static ChecklistItemTemplate CreateItemRecursive(
            ChecklistTemplate template,
            ChecklistSectionTemplate section,
            ChecklistItemTemplate? parent,
            CreateChecklistItemTemplateDto dto,
            int siblingIndex,
            HashSet<string> usedItemNumbers,
            DateTimeOffset now,
            ChecklistType checklistType)
        {
            if (string.IsNullOrWhiteSpace(dto.Topic) || string.IsNullOrWhiteSpace(dto.Description))
            {
                throw new InvalidOperationException("Each item requires topic and description.");
            }

            var itemNumber = ResolveItemNumber(dto.ItemNumber, parent?.ItemNumber, siblingIndex, usedItemNumbers);

            var hasChildren = dto.SubItems.Count > 0;
            var isHeaderOnly = dto.IsSectionHeaderOnly || hasChildren;
            
            // Determine HasLocationField based on checklist type and item configuration
            bool hasLocationField;
            if (isHeaderOnly)
            {
                hasLocationField = false;
            }
            else if (checklistType == ChecklistType.Abstract)
            {
                // Abstract checklists never have location fields
                hasLocationField = false;
            }
            else
            {
                // Full checklists use the DTO value
                hasLocationField = dto.HasLocationField;
            }

            var item = new ChecklistItemTemplate
            {
                Id = Guid.NewGuid(),
                TemplateId = template.Id,
                SectionId = section.Id,
                ParentId = parent?.Id,
                ItemNumber = itemNumber,
                Section = section.Name,
                Topic = dto.Topic.Trim(),
                Description = dto.Description.Trim(),
                Order = dto.Order > 0 ? dto.Order : siblingIndex,
                IsRequired = dto.IsRequired,
                HasLocationField = hasLocationField,
                IsSectionHeaderOnly = isHeaderOnly,
                DefaultSampleAnswer = isHeaderOnly ? null : dto.DefaultSampleAnswer,
                CreatedAt = now,
                ModifiedAt = now
            };

            template.ItemTemplates.Add(item);

            var orderedSubItems = dto.SubItems
                .OrderBy(x => x.Order)
                .ThenBy(x => x.ItemNumber, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.Topic, StringComparer.OrdinalIgnoreCase)
                .ToList();

            for (var index = 0; index < orderedSubItems.Count; index++)
            {
                CreateItemRecursive(template, section, item, orderedSubItems[index], index + 1, usedItemNumbers, now, checklistType);
            }

            return item;
        }

        private static string ResolveItemNumber(string? provided, string? parentNumber, int siblingIndex, HashSet<string> usedItemNumbers)
        {
            var candidate = string.IsNullOrWhiteSpace(provided)
                ? GenerateItemNumber(parentNumber, siblingIndex)
                : provided.Trim();

            if (!usedItemNumbers.Add(candidate))
            {
                throw new InvalidOperationException($"Duplicate item number detected: {candidate}");
            }

            return candidate;
        }

        private static string GenerateItemNumber(string? parentNumber, int siblingIndex)
        {
            if (string.IsNullOrWhiteSpace(parentNumber))
            {
                return siblingIndex.ToString();
            }

            // Generate deterministic child numbers while allowing explicit overrides (e.g., 13a/13b).
            return $"{parentNumber}.{siblingIndex}";
        }

        private static bool CanItemReceiveResponse(ChecklistItemTemplate item, ISet<Guid> parentItemIds)
        {
            if (item.IsSectionHeaderOnly)
            {
                return false;
            }

            return !parentItemIds.Contains(item.Id);
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

            // Preserve template hierarchy and order by cloning responses in deterministic order.
            // Parent/child structure is preserved through ItemTemplateId -> ChecklistItemTemplate.ParentId.
            var orderedTemplateItems = template.ItemTemplates
                .OrderBy(x => x.Order)
                .ThenBy(x => x.ItemNumber, StringComparer.OrdinalIgnoreCase)
                .ToList();
            var parentItemIds = orderedTemplateItems
                .Where(x => x.ParentId.HasValue)
                .Select(x => x.ParentId!.Value)
                .ToHashSet();

            foreach (var item in orderedTemplateItems)
            {
                if (!CanItemReceiveResponse(item, parentItemIds))
                {
                    continue;
                }

                reviewChecklist.ItemResponses.Add(new ChecklistItemResponse
                {
                    Id = Guid.NewGuid(),
                    ReviewChecklistId = reviewChecklist.Id,
                    ItemTemplateId = item.Id,
                    Location = null,
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
                Type = template.Type,
                TypeName = template.Type.ToString(),
                ItemCount = template.ItemTemplates.Count,
                CreatedAt = template.CreatedAt,
                UpdatedAt = template.UpdatedAt
            };
        }

        private static ChecklistTemplateDetailDto MapTemplateDetail(ChecklistTemplate template)
        {
            var items = template.ItemTemplates.ToList();
            var childrenByParent = items
                .Where(x => x.ParentId.HasValue)
                .GroupBy(x => x.ParentId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());
            var parentItemIds = childrenByParent.Keys.ToHashSet();

            ChecklistItemTemplateDto MapItemNode(ChecklistItemTemplate item)
            {
                childrenByParent.TryGetValue(item.Id, out var rawChildren);
                var children = (rawChildren ?? new List<ChecklistItemTemplate>())
                    .OrderBy(x => x.Order)
                    .ThenBy(x => x.ItemNumber, StringComparer.OrdinalIgnoreCase)
                    .Select(MapItemNode)
                    .ToList();

                var hasChildren = children.Count > 0;
                return new ChecklistItemTemplateDto
                {
                    Id = item.Id,
                    TemplateId = item.TemplateId,
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
                    DefaultSampleAnswer = item.DefaultSampleAnswer,
                    Children = children
                };
            }

            var sectionDtos = template.Sections
                .OrderBy(x => x.Order)
                .ThenBy(x => x.SectionNumber, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .Select(section => new ChecklistTemplateSectionDto
                {
                    Id = section.Id,
                    TemplateId = section.TemplateId,
                    Name = section.Name,
                    Description = section.Description,
                    Order = section.Order,
                    SectionNumber = section.SectionNumber,
                    Items = items
                        .Where(x => x.SectionId == section.Id && x.ParentId == null)
                        .OrderBy(x => x.Order)
                        .ThenBy(x => x.ItemNumber, StringComparer.OrdinalIgnoreCase)
                        .Select(MapItemNode)
                        .ToList()
                })
                .ToList();

            if (sectionDtos.Count == 0)
            {
                sectionDtos = items
                    .GroupBy(x => x.Section)
                    .OrderBy(g => g.Min(x => x.Order))
                    .ThenBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
                    .Select((g, index) => new ChecklistTemplateSectionDto
                    {
                        Id = Guid.Empty,
                        TemplateId = template.Id,
                        Name = g.Key,
                        Description = null,
                        Order = index + 1,
                        SectionNumber = (index + 1).ToString(),
                        Items = g
                            .Where(x => x.ParentId == null)
                            .OrderBy(x => x.Order)
                            .ThenBy(x => x.ItemNumber, StringComparer.OrdinalIgnoreCase)
                            .Select(MapItemNode)
                            .ToList()
                    })
                    .ToList();
            }

            return new ChecklistTemplateDetailDto
            {
                Id = template.Id,
                Name = template.Name,
                Description = template.Description,
                Type = template.Type,
                TypeName = template.Type.ToString(),
                IsSystem = template.IsSystem,
                Version = template.Version,
                CreatedAt = template.CreatedAt,
                UpdatedAt = template.UpdatedAt,
                Sections = sectionDtos,
                Items = items
                    .OrderBy(x => x.Order)
                    .ThenBy(x => x.ItemNumber, StringComparer.OrdinalIgnoreCase)
                    .Select(x => new ChecklistItemTemplateDto
                    {
                        Id = x.Id,
                        TemplateId = x.TemplateId,
                        SectionId = x.SectionId,
                        ParentId = x.ParentId,
                        ItemNumber = x.ItemNumber,
                        Section = x.Section,
                        Topic = x.Topic,
                        Description = x.Description,
                        Order = x.Order,
                        IsRequired = x.IsRequired,
                        HasLocationField = x.HasLocationField,
                        IsSectionHeaderOnly = x.IsSectionHeaderOnly,
                        HasChildren = parentItemIds.Contains(x.Id),
                        CanRespond = !x.IsSectionHeaderOnly && !parentItemIds.Contains(x.Id),
                        DefaultSampleAnswer = x.DefaultSampleAnswer
                    })
                    .ToList()
            };
        }

        private sealed class LegacyItemNode
        {
            public CreateChecklistItemTemplateDto Dto { get; set; } = new();
            public LegacyItemNode? Parent { get; set; }
            public List<LegacyItemNode> Children { get; set; } = new();
        }
    }
}
