using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.SystematicReviewProject;
using SRSS.IAM.Services.DTOs.Common;

namespace SRSS.IAM.Services.SystematicReviewProjectService
{
    public interface ISystematicReviewProjectService
    {
        Task<SystematicReviewProjectResponse> CreateProjectAsync(
            CreateSystematicReviewProjectRequest request,
            CancellationToken cancellationToken = default);

        Task<SystematicReviewProjectDetailResponse> GetProjectByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<PaginatedResponse<SystematicReviewProjectResponse>> GetProjectsAsync(
            ProjectStatus? status,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<SystematicReviewProjectResponse> UpdateProjectAsync(
            UpdateSystematicReviewProjectRequest request,
            CancellationToken cancellationToken = default);

        Task<SystematicReviewProjectResponse> ActivateProjectAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<SystematicReviewProjectResponse> CompleteProjectAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<SystematicReviewProjectResponse> ArchiveProjectAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<bool> DeleteProjectAsync(
            Guid id,
            CancellationToken cancellationToken = default);
    }
}
