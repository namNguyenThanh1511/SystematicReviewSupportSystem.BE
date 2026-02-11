using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.SystematicReviewProject;
using SRSS.IAM.Services.DTOs.Common;
using SRSS.IAM.Services.SystematicReviewProjectService;

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

        public SystematicReviewProjectController(ISystematicReviewProjectService projectService)
        {
            _projectService = projectService;
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
            try
            {
                var result = await _projectService.CreateProjectAsync(request, cancellationToken);
                return Created(result, "Project created successfully.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest<SystematicReviewProjectResponse>(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBuilder.InternalServerError<SystematicReviewProjectResponse>(ex.Message));
            }
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
            try
            {
                var result = await _projectService.GetProjectByIdAsync(id, cancellationToken);

                if (result == null)
                {
                    return NotFound(ResponseBuilder.NotFound<SystematicReviewProjectDetailResponse>($"Project with ID {id} not found."));
                }

                return Ok(result, "Project retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBuilder.InternalServerError<SystematicReviewProjectDetailResponse>(ex.Message));
            }
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
            try
            {
                var result = await _projectService.GetProjectsAsync(status, pageNumber, pageSize, cancellationToken);
                return Ok(result, "Projects retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBuilder.InternalServerError<PaginatedResponse<SystematicReviewProjectResponse>>(ex.Message));
            }
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

            try
            {
                var result = await _projectService.UpdateProjectAsync(request, cancellationToken);
                return Ok(result, "Project updated successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ResponseBuilder.NotFound<SystematicReviewProjectResponse>(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBuilder.InternalServerError<SystematicReviewProjectResponse>(ex.Message));
            }
        }

        /// <summary>
        /// Activate a project (Draft ? Active)
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated project details</returns>
        [HttpPost("{id}/activate")]
        public async Task<ActionResult<ApiResponse<SystematicReviewProjectResponse>>> ActivateProject(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _projectService.ActivateProjectAsync(id, cancellationToken);
                return Ok(result, "Project activated successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest<SystematicReviewProjectResponse>(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBuilder.InternalServerError<SystematicReviewProjectResponse>(ex.Message));
            }
        }

        /// <summary>
        /// Complete a project (Active ? Completed). All processes must be completed first.
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated project details</returns>
        [HttpPost("{id}/complete")]
        public async Task<ActionResult<ApiResponse<SystematicReviewProjectResponse>>> CompleteProject(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _projectService.CompleteProjectAsync(id, cancellationToken);
                return Ok(result, "Project completed successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest<SystematicReviewProjectResponse>(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBuilder.InternalServerError<SystematicReviewProjectResponse>(ex.Message));
            }
        }

        /// <summary>
        /// Archive a project (Active/Completed ? Archived)
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated project details</returns>
        [HttpPost("{id}/archive")]
        public async Task<ActionResult<ApiResponse<SystematicReviewProjectResponse>>> ArchiveProject(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _projectService.ArchiveProjectAsync(id, cancellationToken);
                return Ok(result, "Project archived successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest<SystematicReviewProjectResponse>(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBuilder.InternalServerError<SystematicReviewProjectResponse>(ex.Message));
            }
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
            try
            {
                var result = await _projectService.DeleteProjectAsync(id, cancellationToken);

                if (!result)
                {
                    return NotFound(ResponseBuilder.NotFound($"Project with ID {id} not found."));
                }

                return Ok("Project deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBuilder.InternalServerError(ex.Message));
            }
        }
    }
}
