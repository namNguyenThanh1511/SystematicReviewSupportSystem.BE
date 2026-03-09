using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.Tag;
using SRSS.IAM.Services.TagService;
using SRSS.IAM.Services.UserService;

namespace SRSS.IAM.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class TagController : BaseController
    {
        private readonly ITagService _tagService;
        private readonly ICurrentUserService _currentUserService;

        public TagController(ITagService tagService, ICurrentUserService currentUserService)
        {
            _tagService = tagService;
            _currentUserService = currentUserService;
        }

        // ============================================
        // PAPER TAGS
        // ============================================

        /// <summary>
        /// Add a tag to a paper. Also adds the tag to the current user's tag inventory.
        /// </summary>
        [HttpPost("papers/{paperId}/tags")]
        public async Task<ActionResult<ApiResponse<PaperTagResponse>>> AddTagToPaper(
            [FromRoute] Guid paperId,
            [FromBody] AddPaperTagRequest request,
            CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(_currentUserService.GetUserId());
            var result = await _tagService.AddTagToPaperAsync(paperId, userId, request, cancellationToken);
            return Created(result, "Tag added to paper successfully.");
        }

        /// <summary>
        /// Remove a tag from a paper.
        /// </summary>
        [HttpDelete("papers/{paperId}/tags/{tagId}")]
        public async Task<ActionResult<ApiResponse>> RemoveTagFromPaper(
            [FromRoute] Guid paperId,
            [FromRoute] Guid tagId,
            CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(_currentUserService.GetUserId());
            await _tagService.RemoveTagFromPaperAsync(tagId, userId, cancellationToken);
            return Ok("Tag removed from paper successfully.");
        }

        /// <summary>
        /// Get all tags for a paper.
        /// </summary>
        [HttpGet("papers/{paperId}/tags")]
        public async Task<ActionResult<ApiResponse<List<PaperTagResponse>>>> GetTagsByPaper(
            [FromRoute] Guid paperId,
            [FromQuery] ProcessPhase? phase,
            CancellationToken cancellationToken)
        {
            List<PaperTagResponse> result;
            if (phase.HasValue)
                result = await _tagService.GetTagsByPaperAndPhaseAsync(paperId, phase.Value, cancellationToken);
            else
                result = await _tagService.GetTagsByPaperAsync(paperId, cancellationToken);

            var message = result.Count == 0
                ? "No tags found for this paper."
                : $"Retrieved {result.Count} tags.";

            return Ok(result, message);
        }

        // ============================================
        // USER TAG INVENTORY
        // ============================================

        /// <summary>
        /// Get the current user's tag inventory, optionally filtered by phase.
        /// </summary>
        [HttpGet("users/me/tag-inventory")]
        public async Task<ActionResult<ApiResponse<List<UserTagInventoryResponse>>>> GetMyTagInventory(
            [FromQuery] ProcessPhase? phase,
            CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(_currentUserService.GetUserId());

            List<UserTagInventoryResponse> result;
            if (phase.HasValue)
                result = await _tagService.GetUserTagInventoryByPhaseAsync(userId, phase.Value, cancellationToken);
            else
                result = await _tagService.GetUserTagInventoryAsync(userId, cancellationToken);

            var message = result.Count == 0
                ? "No tags in inventory."
                : $"Retrieved {result.Count} tags from inventory.";

            return Ok(result, message);
        }

        /// <summary>
        /// Manually add a tag to the current user's inventory.
        /// </summary>
        [HttpPost("users/me/tag-inventory")]
        public async Task<ActionResult<ApiResponse<UserTagInventoryResponse>>> AddToMyInventory(
            [FromBody] AddUserTagRequest request,
            CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(_currentUserService.GetUserId());
            var result = await _tagService.AddUserTagAsync(userId, request, cancellationToken);
            return Created(result, "Tag added to inventory.");
        }

        /// <summary>
        /// Remove a tag from the current user's inventory.
        /// </summary>
        [HttpDelete("users/me/tag-inventory/{inventoryId}")]
        public async Task<ActionResult<ApiResponse>> RemoveFromMyInventory(
            [FromRoute] Guid inventoryId,
            CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(_currentUserService.GetUserId());
            await _tagService.RemoveUserTagAsync(inventoryId, userId, cancellationToken);
            return Ok("Tag removed from inventory.");
        }
    }
}
