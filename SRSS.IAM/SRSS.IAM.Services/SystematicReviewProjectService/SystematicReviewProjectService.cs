using Microsoft.EntityFrameworkCore;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.SystematicReviewProject;
using SRSS.IAM.Services.DTOs.Common;
using Shared.Exceptions;

namespace SRSS.IAM.Services.SystematicReviewProjectService
{
    public class SystematicReviewProjectService : ISystematicReviewProjectService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SystematicReviewProjectService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<SystematicReviewProjectResponse> CreateProjectAsync(
            CreateSystematicReviewProjectRequest request,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                throw new ArgumentException("Title is required.", nameof(request.Title));
            }

            var project = new Repositories.Entities.SystematicReviewProject
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Domain = request.Domain,
                Description = request.Description,
                Status = ProjectStatus.Draft,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.SystematicReviewProjects.AddAsync(project, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToResponse(project);
        }

        public async Task<SystematicReviewProjectDetailResponse> GetProjectByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var project = await _unitOfWork.SystematicReviewProjects
                .GetByIdWithProcessesAsync(id, cancellationToken);

            if (project == null)
            {
                throw new NotFoundException("Project not found.");
            }

            return MapToDetailResponse(project);
        }

        public async Task<PaginatedResponse<SystematicReviewProjectResponse>> GetProjectsAsync(
            ProjectStatus? status,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = _unitOfWork.SystematicReviewProjects.GetQueryable();

            if (status.HasValue)
            {
                query = query.Where(p => p.Status == status.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var projects = await query
                .Include(p => p.ReviewProcesses)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedResponse<SystematicReviewProjectResponse>
            {
                Items = projects.Select(MapToResponse).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<SystematicReviewProjectResponse> UpdateProjectAsync(
            UpdateSystematicReviewProjectRequest request,
            CancellationToken cancellationToken = default)
        {
            var project = await _unitOfWork.SystematicReviewProjects
                .GetByIdWithProcessesAsync(request.Id, cancellationToken);

            if (project == null)
            {
                throw new InvalidOperationException($"Project with ID {request.Id} not found.");
            }

            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                project.Title = request.Title;
            }

            if (request.Domain != null)
            {
                project.Domain = request.Domain;
            }

            if (request.Description != null)
            {
                project.Description = request.Description;
            }

            project.ModifiedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SystematicReviewProjects.UpdateAsync(project, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToResponse(project);
        }

        public async Task<SystematicReviewProjectResponse> ActivateProjectAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var project = await _unitOfWork.SystematicReviewProjects
                .GetByIdWithProcessesAsync(id, cancellationToken);

            if (project == null)
            {
                throw new InvalidOperationException($"Project with ID {id} not found.");
            }

            project.Activate();

            await _unitOfWork.SystematicReviewProjects.UpdateAsync(project, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToResponse(project);
        }

        public async Task<SystematicReviewProjectResponse> CompleteProjectAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var project = await _unitOfWork.SystematicReviewProjects
                .GetByIdWithProcessesAsync(id, cancellationToken);

            if (project == null)
            {
                throw new InvalidOperationException($"Project with ID {id} not found.");
            }

            project.Complete();

            await _unitOfWork.SystematicReviewProjects.UpdateAsync(project, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToResponse(project);
        }

        public async Task<SystematicReviewProjectResponse> ArchiveProjectAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var project = await _unitOfWork.SystematicReviewProjects
                .GetByIdWithProcessesAsync(id, cancellationToken);

            if (project == null)
            {
                throw new InvalidOperationException($"Project with ID {id} not found.");
            }

            project.Archive();

            await _unitOfWork.SystematicReviewProjects.UpdateAsync(project, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToResponse(project);
        }

        public async Task<bool> DeleteProjectAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var project = await _unitOfWork.SystematicReviewProjects
                .FindSingleAsync(p => p.Id == id, cancellationToken: cancellationToken);

            if (project == null)
            {
                throw new NotFoundException("Project not found.");
            }

            await _unitOfWork.SystematicReviewProjects.RemoveAsync(project, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }

        private static SystematicReviewProjectResponse MapToResponse(Repositories.Entities.SystematicReviewProject project)
        {
            return new SystematicReviewProjectResponse
            {
                Id = project.Id,
                Title = project.Title,
                Domain = project.Domain,
                Description = project.Description,
                Status = project.Status,
                StatusText = project.Status.ToString(),
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                CreatedAt = project.CreatedAt,
                ModifiedAt = project.ModifiedAt,
                TotalProcesses = project.ReviewProcesses.Count,
                CompletedProcesses = project.ReviewProcesses.Count(p => p.Status == ProcessStatus.Completed),
                Processes = project.ReviewProcesses.Select(p => new ReviewProcessSummaryDto
                {
                    Id = p.Id,
                    Status = p.Status,
                    StatusText = p.Status.ToString(),
                    StartedAt = p.StartedAt,
                    CompletedAt = p.CompletedAt
                }).ToList()
            };
        }

        private static SystematicReviewProjectDetailResponse MapToDetailResponse(Repositories.Entities.SystematicReviewProject project)
        {
            return new SystematicReviewProjectDetailResponse
            {
                Id = project.Id,
                Title = project.Title,
                Domain = project.Domain,
                Description = project.Description,
                Status = project.Status,
                StatusText = project.Status.ToString(),
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                CreatedAt = project.CreatedAt,
                ModifiedAt = project.ModifiedAt,
                ReviewProcesses = project.ReviewProcesses.Select(p => new DTOs.ReviewProcess.ReviewProcessResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    ProjectId = p.ProjectId,
                    Status = p.Status,
                    StatusText = p.Status.ToString(),
                    CurrentPhase = p.CurrentPhase,
                    CurrentPhaseText = p.CurrentPhase.ToString(),
                    StartedAt = p.StartedAt,
                    CompletedAt = p.CompletedAt,
                    Notes = p.Notes,
                    CreatedAt = p.CreatedAt,
                    ModifiedAt = p.ModifiedAt
                }).ToList()
            };
        }
    }
}
