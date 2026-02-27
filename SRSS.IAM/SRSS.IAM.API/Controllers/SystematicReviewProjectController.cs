using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.SystematicReviewProject;
using SRSS.IAM.Services.DTOs.Common;
using SRSS.IAM.Services.SystematicReviewProjectService;
using Microsoft.AspNetCore.Authorization;
using SRSS.IAM.Services.ProjectMemberInvitationService;
using SRSS.IAM.Services.DTOs.ProjectMemberInvitation;
using SRSS.IAM.Services.UserService;

namespace SRSS.IAM.API.Controllers
{
    /// <summary>
    /// API endpoints for managing Systematic Review Projects
    /// </summary>
    [ApiController]
    [Route("api/projects")]
    public class SystematicReviewProjectController : BaseController
    {
        private readonly ISystematicReviewProjectService _projectService;
        private readonly IProjectInvitationService _invitationService;
        private readonly ICurrentUserService _currentUserService;

        public SystematicReviewProjectController(
            ISystematicReviewProjectService projectService,
            IProjectInvitationService invitationService,
            ICurrentUserService currentUserService)
        {
            _projectService = projectService;
            _invitationService = invitationService;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Create a new systematic review project
        /// </summary>
        /// <param name="request">Project creation request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created project details</returns>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<SystematicReviewProjectResponse>>> CreateProject(
            [FromBody] CreateSystematicReviewProjectRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _projectService.CreateProjectAsync(request, cancellationToken);
            return Created(result, "Project created successfully.");
        }

        /// <summary>
        /// Get project by ID with full details including review processes
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Project details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<SystematicReviewProjectDetailResponse>>> GetProjectById(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _projectService.GetProjectByIdAsync(id, cancellationToken);


            return Ok(result, "Project retrieved successfully.");
        }

        /// <summary>
        /// Get paginated list of projects with optional status filter
        /// </summary>
        /// <param name="status">Optional status filter (Draft, Active, Completed, Archived)</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10, max: 100)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated list of projects</returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<SystematicReviewProjectResponse>>>> GetProjects(
            [FromQuery] ProjectStatus? status,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var result = await _projectService.GetProjectsAsync(status, pageNumber, pageSize, cancellationToken);
            return Ok(result, "Projects retrieved successfully.");
        }

        /// <summary>
        /// Update project details (title, domain, description)
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <param name="request">Update request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated project details</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<SystematicReviewProjectResponse>>> UpdateProject(
            [FromRoute] Guid id,
            [FromBody] UpdateSystematicReviewProjectRequest request,
            CancellationToken cancellationToken)
        {
            if (id != request.Id)
            {
                return BadRequest<SystematicReviewProjectResponse>("ID in route does not match ID in request body.");
            }

            var result = await _projectService.UpdateProjectAsync(request, cancellationToken);
            return Ok(result, "Project updated successfully.");
        }

        /// <summary>
        /// Activate a project (Draft → Active)
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated project details</returns>
        [HttpPost("{id}/activate")]
        public async Task<ActionResult<ApiResponse<SystematicReviewProjectResponse>>> ActivateProject(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _projectService.ActivateProjectAsync(id, cancellationToken);
            return Ok(result, "Project activated successfully.");
        }

        /// <summary>
        /// Complete a project (Active → Completed). All processes must be completed first.
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated project details</returns>
        [HttpPost("{id}/complete")]
        public async Task<ActionResult<ApiResponse<SystematicReviewProjectResponse>>> CompleteProject(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _projectService.CompleteProjectAsync(id, cancellationToken);
            return Ok(result, "Project completed successfully.");
        }

        /// <summary>
        /// Archive a project (Active/Completed → Archived)
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated project details</returns>
        [HttpPost("{id}/archive")]
        public async Task<ActionResult<ApiResponse<SystematicReviewProjectResponse>>> ArchiveProject(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _projectService.ArchiveProjectAsync(id, cancellationToken);
            return Ok(result, "Project archived successfully.");
        }

        /// <summary>
        /// Delete a project
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> DeleteProject(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _projectService.DeleteProjectAsync(id, cancellationToken);


            return Ok("Project deleted successfully.");
        }

        /// <summary>
        /// Get all members of a project by project ID
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of project members</returns>
        [HttpGet("{projectId}/members")]
        public async Task<ActionResult<ApiResponse<List<ProjectMemberDto>>>> GetProjectMembers(
            [FromRoute] Guid projectId,
            CancellationToken cancellationToken)
        {
            var result = await _projectService.GetProjectMembersAsync(projectId, cancellationToken);
            return Ok(result, "Project members retrieved successfully.");
        }

        /// <summary>
        /// Get all projects that the currently authenticated user is a member of
        /// </summary>
        /// <returns>List of projects with user's role</returns>
        [HttpGet("my")]
        public async Task<ActionResult<ApiResponse<List<MyProjectResponse>>>> GetMyProjects()
        {
            var result = await _projectService.GetMyProjectsAsync();
            return Ok(result, "Your projects retrieved successfully.");
        }

        /// <summary>
        /// Create invitations for a project
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="request">Invitation request</param>
        /// <returns>Success response</returns>
        [HttpPost("{projectId}/invitations")]
        public async Task<ActionResult<ApiResponse>> CreateInvitations(
            [FromRoute] Guid projectId,
            [FromBody] CreateProjectInvitationRequest request)
        {
            var (userId, _) = _currentUserService.GetCurrentUser();
            await _invitationService.CreateInvitationsAsync(projectId, Guid.Parse(userId), request);
            return Ok("Invitations created successfully.");
        }
    }
}
