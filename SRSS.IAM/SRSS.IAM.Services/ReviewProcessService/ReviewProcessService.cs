using Microsoft.EntityFrameworkCore;
using Shared.Exceptions;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.Identification;
using SRSS.IAM.Services.DTOs.ReviewProcess;

namespace SRSS.IAM.Services.ReviewProcessService
{
    public class ReviewProcessService : IReviewProcessService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReviewProcessService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ReviewProcessResponse> CreateReviewProcessAsync(
            Guid projectId,
            CreateReviewProcessRequest request,
            CancellationToken cancellationToken = default)
        {
            var project = await _unitOfWork.SystematicReviewProjects
                .GetByIdWithProcessesAsync(projectId, cancellationToken);

            if (project == null)
            {
                throw new InvalidOperationException($"Project with ID {projectId} not found.");
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // Create ReviewProcess
                var reviewProcess = project.AddReviewProcess(request.Notes);

                await _unitOfWork.ReviewProcesses.AddAsync(reviewProcess, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Auto-create IdentificationProcess for the new ReviewProcess
                var identificationProcess = new Repositories.Entities.IdentificationProcess
                {
                    Id = Guid.NewGuid(),
                    ReviewProcessId = reviewProcess.Id,
                    Notes = "Auto-created identification process",
                    Status = Repositories.Entities.IdentificationStatus.NotStarted,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                };

                await _unitOfWork.IdentificationProcesses.AddAsync(identificationProcess, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return MapToResponse(reviewProcess);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task<ReviewProcessResponse> GetReviewProcessByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var reviewProcess = await _unitOfWork.ReviewProcesses.GetByIdWithProcessesAsync(id, cancellationToken);

            return reviewProcess == null ? throw new NotFoundException($"ReviewProcess with ID {id} not found.") : MapToResponse(reviewProcess);
        }

        public async Task<List<ReviewProcessResponse>> GetReviewProcessesByProjectIdAsync(
            Guid projectId,
            CancellationToken cancellationToken = default)
        {
            var reviewProcesses = await _unitOfWork.ReviewProcesses
                .GetByProjectIdAsync(projectId, cancellationToken);

            return reviewProcesses.Select(MapToResponse).ToList();
        }

        public async Task<ReviewProcessResponse> UpdateReviewProcessAsync(
            UpdateReviewProcessRequest request,
            CancellationToken cancellationToken = default)
        {
            var reviewProcess = await _unitOfWork.ReviewProcesses
                .FindSingleAsync(rp => rp.Id == request.Id, isTracking: true, cancellationToken);

            if (reviewProcess == null)
            {
                throw new InvalidOperationException($"ReviewProcess with ID {request.Id} not found.");
            }

            if (request.Notes != null)
            {
                reviewProcess.Notes = request.Notes;
            }

            reviewProcess.ModifiedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.ReviewProcesses.UpdateAsync(reviewProcess, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToResponse(reviewProcess);
        }

        public async Task<ReviewProcessResponse> StartReviewProcessAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var reviewProcess = await _unitOfWork.ReviewProcesses
                .GetByIdWithProjectAsync(id, cancellationToken);

            if (reviewProcess == null)
            {
                throw new InvalidOperationException($"ReviewProcess with ID {id} not found.");
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                reviewProcess.Start();

                await _unitOfWork.ReviewProcesses.UpdateAsync(reviewProcess, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return MapToResponse(reviewProcess);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task<ReviewProcessResponse> CompleteReviewProcessAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var reviewProcess = await _unitOfWork.ReviewProcesses
                .GetByIdWithProjectAsync(id, cancellationToken);

            if (reviewProcess == null)
            {
                throw new InvalidOperationException($"ReviewProcess with ID {id} not found.");
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                reviewProcess.Complete();

                await _unitOfWork.ReviewProcesses.UpdateAsync(reviewProcess, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return MapToResponse(reviewProcess);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task<ReviewProcessResponse> CancelReviewProcessAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var reviewProcess = await _unitOfWork.ReviewProcesses
                .GetByIdWithProjectAsync(id, cancellationToken);

            if (reviewProcess == null)
            {
                throw new InvalidOperationException($"ReviewProcess with ID {id} not found.");
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                reviewProcess.Cancel();

                await _unitOfWork.ReviewProcesses.UpdateAsync(reviewProcess, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return MapToResponse(reviewProcess);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task<bool> DeleteReviewProcessAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var reviewProcess = await _unitOfWork.ReviewProcesses
                .FindSingleAsync(rp => rp.Id == id, cancellationToken: cancellationToken);

            if (reviewProcess == null)
            {
                throw new NotFoundException("ReviewProcess not found.");
            }

            await _unitOfWork.ReviewProcesses.RemoveAsync(reviewProcess, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }

        private static ReviewProcessResponse MapToResponse(Repositories.Entities.ReviewProcess reviewProcess)
        {
            return new ReviewProcessResponse
            {
                Id = reviewProcess.Id,
                ProjectId = reviewProcess.ProjectId,
                Status = reviewProcess.Status,
                StatusText = reviewProcess.Status.ToString(),
                CurrentPhase = reviewProcess.CurrentPhase,
                CurrentPhaseText = reviewProcess.CurrentPhase.ToString(),
                StartedAt = reviewProcess.StartedAt,
                CompletedAt = reviewProcess.CompletedAt,
                Notes = reviewProcess.Notes,
                CreatedAt = reviewProcess.CreatedAt,
                ModifiedAt = reviewProcess.ModifiedAt,
                IdentificationProcesses = reviewProcess.IdentificationProcesses?
                    .Select(ip => new IdentificationProcessResponse
                    {
                        Id = ip.Id,
                        ReviewProcessId = ip.ReviewProcessId,
                        Status = ip.Status,
                        StatusText = ip.Status.ToString(),
                        Notes = ip.Notes,
                        CreatedAt = ip.CreatedAt,
                        ModifiedAt = ip.ModifiedAt
                    })
                    .ToList() ?? new List<IdentificationProcessResponse>()
            };
        }
    }
}
