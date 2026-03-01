using Microsoft.EntityFrameworkCore;
using Shared.Exceptions;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.Identification;
using SRSS.IAM.Services.DTOs.ReviewProcess;
using SRSS.IAM.Services.DTOs.StudySelection;

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
                var reviewProcess = project.AddReviewProcess(request.Name,request.Notes);

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

            if (reviewProcess == null)
            {
                throw new NotFoundException($"ReviewProcess with ID {id} not found.");
            }

            var response = MapToResponse(reviewProcess);

            if (reviewProcess.IdentificationProcess != null)
            {
                response.IdentificationProcess!.PrismaStatistics =
                    await GetPrismaStatisticsForIdentificationAsync(reviewProcess.IdentificationProcess.Id, cancellationToken);
            }

            return response;
        }

        public async Task<List<ReviewProcessResponse>> GetReviewProcessesByProjectIdAsync(
            Guid projectId,
            CancellationToken cancellationToken = default)
        {
            var reviewProcesses = await _unitOfWork.ReviewProcesses
                .GetByProjectIdAsync(projectId, cancellationToken);

            var responses = new List<ReviewProcessResponse>();

            foreach (var reviewProcess in reviewProcesses)
            {
                var response = MapToResponse(reviewProcess);

                if (reviewProcess.IdentificationProcess != null)
                {
                    response.IdentificationProcess!.PrismaStatistics =
                        await GetPrismaStatisticsForIdentificationAsync(reviewProcess.IdentificationProcess.Id, cancellationToken);
                }

                responses.Add(response);
            }

            return responses;
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

            // Load StudySelectionProcess for validation
            var studySelectionProcess = await _unitOfWork.StudySelectionProcesses.FindSingleAsync(
                ssp => ssp.ReviewProcessId == id,
                isTracking: false,
                cancellationToken);

            reviewProcess.StudySelectionProcess = studySelectionProcess;

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                //reviewProcess.Complete();
                reviewProcess.CompletedAt = DateTimeOffset.UtcNow;
                reviewProcess.Status = Repositories.Entities.ProcessStatus.Completed;
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
                Name = reviewProcess.Name,
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
                IdentificationProcess = reviewProcess.IdentificationProcess != null
                    ? new IdentificationProcessResponse
                    {
                        Id = reviewProcess.IdentificationProcess.Id,
                        ReviewProcessId = reviewProcess.IdentificationProcess.ReviewProcessId,
                        Status = reviewProcess.IdentificationProcess.Status,
                        StatusText = reviewProcess.IdentificationProcess.Status.ToString(),
                        Notes = reviewProcess.IdentificationProcess.Notes,
                        CreatedAt = reviewProcess.IdentificationProcess.CreatedAt,
                        ModifiedAt = reviewProcess.IdentificationProcess.ModifiedAt
                    }
                    : null,
                StudySelectionProcess = reviewProcess.StudySelectionProcess != null
                    ? new StudySelectionProcessResponse
                    {
                        Id = reviewProcess.StudySelectionProcess.Id,
                        ReviewProcessId = reviewProcess.StudySelectionProcess.ReviewProcessId,
                        Status = reviewProcess.StudySelectionProcess.Status,
                        StatusText = reviewProcess.StudySelectionProcess.Status.ToString(),
                        Notes = reviewProcess.StudySelectionProcess.Notes,
                        CreatedAt = reviewProcess.StudySelectionProcess.CreatedAt,
                        ModifiedAt = reviewProcess.StudySelectionProcess.ModifiedAt
                    }
                    : null
            };
        }

        private async Task<PrismaStatisticsResponse> GetPrismaStatisticsForIdentificationAsync(
            Guid identificationProcessId,
            CancellationToken cancellationToken)
        {
            var searchExecutions = await _unitOfWork.SearchExecutions.FindAllAsync(
                se => se.IdentificationProcessId == identificationProcessId,
                cancellationToken: cancellationToken);

            var searchExecutionIds = searchExecutions.Select(se => se.Id).ToHashSet();

            var allImportBatches = await _unitOfWork.ImportBatches.FindAllAsync(
                ib => ib.SearchExecutionId != null && searchExecutionIds.Contains(ib.SearchExecutionId.Value),
                cancellationToken: cancellationToken);

            var importBatchList = allImportBatches.ToList();
            var totalRecordsImported = importBatchList.Sum(ib => ib.TotalRecords);
            var duplicateRecords = await _unitOfWork.DeduplicationResults.CountDuplicatesByProcessAsync(identificationProcessId, cancellationToken);
            var uniqueRecords = totalRecordsImported - duplicateRecords;

            return new PrismaStatisticsResponse
            {
                TotalRecordsImported = totalRecordsImported,
                DuplicateRecords = duplicateRecords,
                UniqueRecords = uniqueRecords,
                ImportBatchCount = importBatchList.Count
            };
        }
    }
}
