using Microsoft.EntityFrameworkCore;
using Shared.Exceptions;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.Identification;
using SRSS.IAM.Services.DTOs.ReviewProcess;
using SRSS.IAM.Services.DTOs.StudySelection;
using SRSS.IAM.Services.Mappers;
using SRSS.IAM.Services.StudySelectionService;
using SRSS.IAM.Services.QualityAssessmentService;
using SRSS.IAM.Services.UserService;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.DataExtraction;
using SRSS.IAM.Services.DTOs.QualityAssessment;
using SRSS.IAM.Services.IdentificationService;

namespace SRSS.IAM.Services.ReviewProcessService
{
    public class ReviewProcessService : IReviewProcessService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStudySelectionService _studySelectionService;
        private readonly IQualityAssessmentService _qualityAssessmentService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IIdentificationService _identificationService;

        public ReviewProcessService(IUnitOfWork unitOfWork, IStudySelectionService selectionService, IQualityAssessmentService qualityAssessmentService, ICurrentUserService currentUserService, IIdentificationService identificationService)
        {
            _unitOfWork = unitOfWork;
            _studySelectionService = selectionService;
            _qualityAssessmentService = qualityAssessmentService;
            _currentUserService = currentUserService;
            _identificationService = identificationService;
        }

        public async Task<ReviewProcessResponse> CreateReviewProcessAsync(
            Guid projectId,
            CreateReviewProcessRequest request,
            CancellationToken cancellationToken = default)
        {
            await EnsureLeaderPermissionAsync(projectId);

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("Process name is required.", nameof(request.Name));
            }

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
                var reviewProcess = project.AddReviewProcess(request.Name, request.Notes);

                await _unitOfWork.ReviewProcesses.AddAsync(reviewProcess, cancellationToken);

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

                //Auto-create Study Selection Process for the new ReviewProcess
                var studySelectionProcess = new Repositories.Entities.StudySelectionProcess
                {
                    Id = Guid.NewGuid(),
                    ReviewProcessId = reviewProcess.Id,
                    Notes = "Auto-created study selection process",
                    Status = Repositories.Entities.SelectionProcessStatus.NotStarted,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                };

                //Auto-create Quality Assessment Process for the new ReviewProcess
                var qualityAssessmentProcess = new Repositories.Entities.QualityAssessmentProcess
                {
                    Id = Guid.NewGuid(),
                    ReviewProcessId = reviewProcess.Id,
                    Notes = "Auto-created quality assessment process",
                    Status = Repositories.Entities.Enums.QualityAssessmentProcessStatus.NotStarted,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                };

                //Auto-create Data Extraction Process for the new ReviewProcess
                var dataExtractionProcess = new Repositories.Entities.DataExtractionProcess
                {
                    Id = Guid.NewGuid(),
                    ReviewProcessId = reviewProcess.Id,
                    Notes = "Auto-created data extraction process",
                    Status = Repositories.Entities.ExtractionProcessStatus.NotStarted,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                };

                //Auto-create Synthesis Process for the new ReviewProcess
                var synthesisProcess = new Repositories.Entities.SynthesisProcess
                {
                    Id = Guid.NewGuid(),
                    ReviewProcessId = reviewProcess.Id,
                    Status = Repositories.Entities.Enums.SynthesisProcessStatus.NotStarted,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                };

                await _unitOfWork.IdentificationProcesses.AddAsync(identificationProcess, cancellationToken);
                await _unitOfWork.StudySelectionProcesses.AddAsync(studySelectionProcess, cancellationToken);
                await _unitOfWork.QualityAssessmentProcesses.AddAsync(qualityAssessmentProcess, cancellationToken);
                await _unitOfWork.DataExtractionProcesses.AddAsync(dataExtractionProcess, cancellationToken);
                await _unitOfWork.SynthesisProcesses.AddAsync(synthesisProcess, cancellationToken);
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

            if (reviewProcess.StudySelectionProcess != null)
            {
                response.StudySelectionProcess!.SelectionStatistics = await _studySelectionService.GetSelectionStatisticsAsync(reviewProcess.StudySelectionProcess.Id, cancellationToken);
            }

            if (reviewProcess.QualityAssessmentProcess != null)
            {
                response.QualityAssessmentProcess!.QualityStatistics = await _qualityAssessmentService.GetQualityStatisticsAsync(reviewProcess.QualityAssessmentProcess.Id);
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

                if (reviewProcess.QualityAssessmentProcess != null)
                {
                    response.QualityAssessmentProcess!.QualityStatistics = await _qualityAssessmentService.GetQualityStatisticsAsync(reviewProcess.QualityAssessmentProcess.Id);
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

            await EnsureLeaderPermissionAsync(reviewProcess.ProjectId);

            if (request.Notes != null)
            {
                reviewProcess.Notes = request.Notes;
            }

            if (request.Name != null)
            {
                reviewProcess.Name = request.Name;
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

            await EnsureLeaderPermissionAsync(reviewProcess.ProjectId);

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                reviewProcess.Start();
                //Also start the identification process if it exists
                reviewProcess.IdentificationProcess?.Start();
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

            await EnsureLeaderPermissionAsync(reviewProcess.ProjectId);

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

            await EnsureLeaderPermissionAsync(reviewProcess.ProjectId);

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

            await EnsureLeaderPermissionAsync(reviewProcess.ProjectId);

            await _unitOfWork.ReviewProcesses.RemoveAsync(reviewProcess, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<ReviewProcessResponse> ReopenPhaseAsync(
            Guid reviewProcessId,
            ProcessPhase phase,
            CancellationToken cancellationToken = default)
        {
            var reviewProcess = await _unitOfWork.ReviewProcesses.GetByIdWithProjectAsync(reviewProcessId, cancellationToken);
            if (reviewProcess == null)
            {
                throw new NotFoundException($"Review process with ID {reviewProcessId} not found.");
            }

            await EnsureLeaderPermissionAsync(reviewProcess.ProjectId);

            switch (phase)
            {
                case ProcessPhase.Identification:
                    if (reviewProcess.IdentificationProcess == null) throw new NotFoundException("Identification process not found.");
                    reviewProcess.IdentificationProcess.Reopen();
                    break;
                case ProcessPhase.StudySelection:
                    if (reviewProcess.StudySelectionProcess == null) throw new NotFoundException("Study selection process not found.");
                    reviewProcess.StudySelectionProcess.Reopen();
                    break;
                case ProcessPhase.QualityAssessment:
                    if (reviewProcess.QualityAssessmentProcess == null) throw new NotFoundException("Quality assessment process not found.");
                    reviewProcess.QualityAssessmentProcess.Reopen();
                    break;
                case ProcessPhase.DataExtraction:
                    if (reviewProcess.DataExtractionProcess == null) throw new NotFoundException("Data extraction process not found.");
                    reviewProcess.DataExtractionProcess.Reopen();
                    break;
                default:
                    throw new ArgumentException($"Phase {phase} does not support reopening or is not recognized.", nameof(phase));
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return await GetReviewProcessByIdAsync(reviewProcessId, cancellationToken);
        }

        private async Task EnsureLeaderPermissionAsync(Guid projectId)
        {
            var userIdString = _currentUserService.GetUserId();
            if (string.IsNullOrEmpty(userIdString))
            {
                throw new UnauthorizedException("User is not authenticated.");
            }
            var userId = Guid.Parse(userIdString);
            var isLeader = await _unitOfWork.SystematicReviewProjects.IsProjectLeaderAsync(projectId, userId);
            if (!isLeader)
            {
                throw new ForbiddenException("Only the project leader can perform this operation.");
            }
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
                    : null,
                QualityAssessmentProcess = reviewProcess.QualityAssessmentProcess != null
                    ? new QualityAssessmentProcessResponse
                    {
                        Id = reviewProcess.QualityAssessmentProcess.Id,
                        ReviewProcessId = reviewProcess.QualityAssessmentProcess.ReviewProcessId,
                        Status = reviewProcess.QualityAssessmentProcess.Status,
                        StatusText = reviewProcess.QualityAssessmentProcess.Status.ToString(),
                        Notes = reviewProcess.QualityAssessmentProcess.Notes,
                        CreatedAt = reviewProcess.QualityAssessmentProcess.CreatedAt,
                        ModifiedAt = reviewProcess.QualityAssessmentProcess.ModifiedAt
                    }
                    : null,
                DataExtractionProcess = reviewProcess.DataExtractionProcess != null
                    ? new DataExtractionProcessResponse
                    {
                        Id = reviewProcess.DataExtractionProcess.Id,
                        ReviewProcessId = reviewProcess.DataExtractionProcess.ReviewProcessId,
                        Status = reviewProcess.DataExtractionProcess.Status,
                        StatusText = reviewProcess.DataExtractionProcess.Status.ToString(),
                        Notes = reviewProcess.DataExtractionProcess.Notes,
                        CreatedAt = reviewProcess.DataExtractionProcess.CreatedAt,
                        ModifiedAt = reviewProcess.DataExtractionProcess.ModifiedAt
                    }
                    : null,
                SynthesisProcess = reviewProcess.SynthesisProcess != null
                    ? new SynthesisProcessResponse
                    {
                        Id = reviewProcess.SynthesisProcess.Id,
                        ReviewProcessId = reviewProcess.SynthesisProcess.ReviewProcessId,
                        Status = reviewProcess.SynthesisProcess.Status,
                        StatusText = reviewProcess.SynthesisProcess.Status.ToString(),
                        StartedAt = reviewProcess.SynthesisProcess.StartedAt,
                        CompletedAt = reviewProcess.SynthesisProcess.CompletedAt,
                        CreatedAt = reviewProcess.SynthesisProcess.CreatedAt,
                        ModifiedAt = reviewProcess.SynthesisProcess.ModifiedAt
                    }
                    : null
            };
        }

        private async Task<PrismaStatisticsResponse> GetPrismaStatisticsForIdentificationAsync(
            Guid identificationProcessId,
            CancellationToken cancellationToken)
        {
            return await _identificationService.GetPrismaStatisticsAsync(identificationProcessId, cancellationToken);
        }
    }
}
