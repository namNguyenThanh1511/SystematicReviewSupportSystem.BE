using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.ProjectMemberInvitation;
using SRSS.IAM.Services.ProjectMemberInvitationService;
using SRSS.IAM.Services.UserService;

namespace SRSS.IAM.API.Controllers
{
    /// <summary>
    /// API endpoints for managing Project Member Invitations
    /// </summary>
    [ApiController]
    [Route("api/invitations")]
    [Authorize]
    public class InvitationController : BaseController
    {
        private readonly IProjectInvitationService _invitationService;
        private readonly ICurrentUserService _currentUserService;

        public InvitationController(IProjectInvitationService invitationService, ICurrentUserService currentUserService)
        {
            _invitationService = invitationService;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Get all invitations where the current user is invited
        /// </summary>
        /// <returns>List of invitations</returns>
        [HttpGet("my")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProjectInvitationResponse>>>> GetMyInvitations()
        {
            var (userId, _) = _currentUserService.GetCurrentUser();
            var result = await _invitationService.GetMyInvitationsAsync(Guid.Parse(userId));
            return Ok(result, "Your invitations retrieved successfully.");
        }

        /// <summary>
        /// Get pending invitations for a specific project. Only for Leaders or Admins.
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="status">Optional status filter</param>
        /// <returns>List of project invitations</returns>
        [HttpGet("{projectId}/from")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProjectInvitationResponse>>>> GetProjectInvitations(
            [FromRoute] Guid projectId,
            [FromQuery] ProjectMemberInvitationStatus? status = ProjectMemberInvitationStatus.Pending)
        {
            var (userId, _) = _currentUserService.GetCurrentUser();
            var result = await _invitationService.GetProjectInvitationsAsync(projectId, Guid.Parse(userId), status);
            return Ok(result, "Project invitations retrieved successfully.");
        }

        /// <summary>
        /// Accept an invitation
        /// </summary>
        /// <param name="invitationId">Invitation ID</param>
        /// <returns>Success response</returns>
        [HttpPost("{invitationId}/accept")]
        public async Task<ActionResult<ApiResponse>> AcceptInvitation([FromRoute] Guid invitationId)
        {
            var (userId, _) = _currentUserService.GetCurrentUser();
            await _invitationService.AcceptInvitationAsync(invitationId, Guid.Parse(userId));
            return Ok("Invitation accepted successfully. You are now a member of the project.");
        }

        /// <summary>
        /// Reject an invitation
        /// </summary>
        /// <param name="invitationId">Invitation ID</param>
        /// <param name="request">Rejection details</param>
        /// <returns>Success response</returns>
        [HttpPost("{invitationId}/reject")]
        public async Task<ActionResult<ApiResponse>> RejectInvitation(
            [FromRoute] Guid invitationId,
            [FromBody] RejectInvitationRequest request)
        {
            var (userId, _) = _currentUserService.GetCurrentUser();
            await _invitationService.RejectInvitationAsync(invitationId, Guid.Parse(userId), request);
            return Ok("Invitation rejected successfully.");
        }

        /// <summary>
        /// Cancel a pending invitation. Only for Leaders or Admins.
        /// </summary>
        /// <param name="invitationId">Invitation ID</param>
        /// <returns>Success response</returns>
        [HttpDelete("{invitationId}")]
        public async Task<ActionResult<ApiResponse>> CancelInvitation([FromRoute] Guid invitationId)
        {
            var (userId, _) = _currentUserService.GetCurrentUser();
            await _invitationService.CancelInvitationAsync(invitationId, Guid.Parse(userId));
            return Ok("Invitation cancelled successfully.");
        }
    }
}
