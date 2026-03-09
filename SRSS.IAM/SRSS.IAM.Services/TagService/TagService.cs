using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.Tag;

namespace SRSS.IAM.Services.TagService
{
    public class TagService : ITagService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TagService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ============================================
        // PAPER TAGS
        // ============================================

        public async Task<PaperTagResponse> AddTagToPaperAsync(Guid paperId, Guid userId, AddPaperTagRequest request, CancellationToken cancellationToken = default)
        {
            var label = request.Label.Trim();
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentException("Tag label cannot be empty.");

            // Validate paper exists
            var paper = await _unitOfWork.Papers.FindSingleAsync(p => p.Id == paperId, cancellationToken: cancellationToken);
            if (paper == null)
                throw new KeyNotFoundException($"Paper with ID {paperId} not found.");

            // Check for duplicate tag on this paper by this user
            var existing = await _unitOfWork.PaperTags.GetExistingTagAsync(paperId, userId, request.Phase, label, cancellationToken);
            if (existing != null)
                throw new InvalidOperationException($"Tag '{label}' already exists on this paper for phase '{request.Phase}'.");

            // Create the paper tag
            var paperTag = new PaperTag
            {
                Id = Guid.NewGuid(),
                PaperId = paperId,
                UserId = userId,
                Phase = request.Phase,
                Label = label
            };
            await _unitOfWork.PaperTags.AddAsync(paperTag, cancellationToken);

            // Upsert into user's tag inventory
            await UpsertUserTagInventoryAsync(userId, request.Phase, label, increment: 1, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToPaperTagResponse(paperTag);
        }

        public async Task RemoveTagFromPaperAsync(Guid tagId, Guid userId, CancellationToken cancellationToken = default)
        {
            var tag = await _unitOfWork.PaperTags.FindSingleAsync(t => t.Id == tagId, cancellationToken: cancellationToken);
            if (tag == null)
                throw new KeyNotFoundException($"Paper tag with ID {tagId} not found.");

            if (tag.UserId != userId)
                throw new UnauthorizedAccessException("You can only remove tags you created.");

            await _unitOfWork.PaperTags.RemoveAsync(tag, cancellationToken);

            // Decrement inventory usage count
            await UpsertUserTagInventoryAsync(userId, tag.Phase, tag.Label, increment: -1, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<PaperTagResponse>> GetTagsByPaperAsync(Guid paperId, CancellationToken cancellationToken = default)
        {
            var tags = await _unitOfWork.PaperTags.GetTagsByPaperAsync(paperId, cancellationToken);
            return tags.Select(MapToPaperTagResponse).ToList();
        }

        public async Task<List<PaperTagResponse>> GetTagsByPaperAndPhaseAsync(Guid paperId, ProcessPhase phase, CancellationToken cancellationToken = default)
        {
            var tags = await _unitOfWork.PaperTags.GetTagsByPaperAndPhaseAsync(paperId, phase, cancellationToken);
            return tags.Select(MapToPaperTagResponse).ToList();
        }

        // ============================================
        // USER TAG INVENTORY
        // ============================================

        public async Task<List<UserTagInventoryResponse>> GetUserTagInventoryAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var items = await _unitOfWork.UserTagInventories.GetByUserAsync(userId, cancellationToken);
            return items.Select(MapToInventoryResponse).ToList();
        }

        public async Task<List<UserTagInventoryResponse>> GetUserTagInventoryByPhaseAsync(Guid userId, ProcessPhase phase, CancellationToken cancellationToken = default)
        {
            var items = await _unitOfWork.UserTagInventories.GetByUserAndPhaseAsync(userId, phase, cancellationToken);
            return items.Select(MapToInventoryResponse).ToList();
        }

        public async Task<UserTagInventoryResponse> AddUserTagAsync(Guid userId, AddUserTagRequest request, CancellationToken cancellationToken = default)
        {
            var label = request.Label.Trim();
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentException("Tag label cannot be empty.");

            var existing = await _unitOfWork.UserTagInventories.GetExistingEntryAsync(userId, request.Phase, label, cancellationToken);
            if (existing != null)
                return MapToInventoryResponse(existing);

            var entry = new UserTagInventory
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Phase = request.Phase,
                Label = label,
                UsageCount = 0
            };
            await _unitOfWork.UserTagInventories.AddAsync(entry, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToInventoryResponse(entry);
        }

        public async Task RemoveUserTagAsync(Guid inventoryId, Guid userId, CancellationToken cancellationToken = default)
        {
            var entry = await _unitOfWork.UserTagInventories.FindSingleAsync(t => t.Id == inventoryId, cancellationToken: cancellationToken);
            if (entry == null)
                throw new KeyNotFoundException($"Tag inventory entry with ID {inventoryId} not found.");

            if (entry.UserId != userId)
                throw new UnauthorizedAccessException("You can only remove tags from your own inventory.");

            await _unitOfWork.UserTagInventories.RemoveAsync(entry, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // ============================================
        // PRIVATE HELPERS
        // ============================================

        private async Task UpsertUserTagInventoryAsync(Guid userId, ProcessPhase phase, string label, int increment, CancellationToken cancellationToken)
        {
            var entry = await _unitOfWork.UserTagInventories.GetExistingEntryAsync(userId, phase, label, cancellationToken);
            if (entry != null)
            {
                entry.UsageCount = Math.Max(0, entry.UsageCount + increment);
                await _unitOfWork.UserTagInventories.UpdateAsync(entry, cancellationToken);
            }
            // When tag is not in inventory and we try to add, create a new entry, if remove then do nothing (no tag count to decrease)
            else if (increment > 0)
            {
                var newEntry = new UserTagInventory
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Phase = phase,
                    Label = label,
                    UsageCount = increment
                };
                await _unitOfWork.UserTagInventories.AddAsync(newEntry, cancellationToken);
            }
        }

        private static PaperTagResponse MapToPaperTagResponse(PaperTag tag)
        {
            return new PaperTagResponse
            {
                Id = tag.Id,
                PaperId = tag.PaperId,
                UserId = tag.UserId,
                Phase = tag.Phase,
                PhaseText = tag.Phase.ToString(),
                Label = tag.Label,
                CreatedAt = tag.CreatedAt
            };
        }

        private static UserTagInventoryResponse MapToInventoryResponse(UserTagInventory entry)
        {
            return new UserTagInventoryResponse
            {
                Id = entry.Id,
                UserId = entry.UserId,
                Phase = entry.Phase,
                PhaseText = entry.Phase.ToString(),
                Label = entry.Label,
                UsageCount = entry.UsageCount,
                CreatedAt = entry.CreatedAt
            };
        }
    }
}
