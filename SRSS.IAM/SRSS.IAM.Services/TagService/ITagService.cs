using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.Tag;

namespace SRSS.IAM.Services.TagService
{
    public interface ITagService
    {
        // ============================================
        // PAPER TAGS
        // ============================================

        /// <summary>
        /// Add a tag to a paper. Also upserts the tag into the user's tag inventory.
        /// </summary>
        Task<PaperTagResponse> AddTagToPaperAsync(Guid paperId, Guid userId, AddPaperTagRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Remove a tag from a paper. Decrements the user's inventory usage count.
        /// </summary>
        Task RemoveTagFromPaperAsync(Guid tagId, Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get all tags for a paper.
        /// </summary>
        Task<List<PaperTagResponse>> GetTagsByPaperAsync(Guid paperId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get tags for a paper filtered by phase.
        /// </summary>
        Task<List<PaperTagResponse>> GetTagsByPaperAndPhaseAsync(Guid paperId, ProcessPhase phase, CancellationToken cancellationToken = default);

        // ============================================
        // USER TAG INVENTORY
        // ============================================

        /// <summary>
        /// Get the user's full tag inventory.
        /// </summary>
        Task<List<UserTagInventoryResponse>> GetUserTagInventoryAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the user's tag inventory filtered by phase.
        /// </summary>
        Task<List<UserTagInventoryResponse>> GetUserTagInventoryByPhaseAsync(Guid userId, ProcessPhase phase, CancellationToken cancellationToken = default);

        /// <summary>
        /// Manually add a tag to the user's inventory (without applying to a paper).
        /// </summary>
        Task<UserTagInventoryResponse> AddUserTagAsync(Guid userId, AddUserTagRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Remove a tag from the user's inventory.
        /// </summary>
        Task RemoveUserTagAsync(Guid inventoryId, Guid userId, CancellationToken cancellationToken = default);
    }
}
