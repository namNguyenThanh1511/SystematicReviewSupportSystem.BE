using Microsoft.EntityFrameworkCore;
using Shared.Exceptions;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.Identification;
using SRSS.IAM.Services.DTOs.Protocol;
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

            // Check if current user is the leader of the project
            var (userId, _) = _currentUserService.GetCurrentUser();
            var isLeader = await _unitOfWork.SystematicReviewProjects.IsProjectLeaderAsync(projectId, Guid.Parse(userId));
            if (!isLeader)
            {
                throw new InvalidOperationException("Only the project leader can create a new review process.");
            }

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

            Console.WriteLine($"Check name: {request.Name}");

            try
            {
                // Validate protocol if provided
                ReviewProtocol? protocol = null;
                Console.WriteLine($"Protocol ID: {request.ProtocolId}");
                if (request.ProtocolId.HasValue)
                {
                    protocol = await _unitOfWork.Protocols.FindSingleAsync(p => p.Id == request.ProtocolId.Value, cancellationToken: cancellationToken);
                    if (protocol == null)
                    {
                        throw new InvalidOperationException($"Protocol with ID {request.ProtocolId.Value} not found.");
                    }
                    if (protocol.ProjectId != projectId)
                    {
                        throw new InvalidOperationException("The protocol must belong to the same project as the review process.");
                    }
                }

                // Create ReviewProcess
                var reviewProcess = project.AddReviewProcess(request.Name, request.Notes, protocol);
                Console.WriteLine($"Review process created: {reviewProcess}");

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
                //Auto create IdentificationProcessPaper snap
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
                // Optionally, you could add additional statistics for the StudySelectionProcess here
                response.StudySelectionProcess!.SelectionStatistics = await _studySelectionService.GetSelectionStatisticsAsync(reviewProcess.StudySelectionProcess.Id, cancellationToken);
            }

            if (reviewProcess.QualityAssessmentProcess != null)
            {
                response.QualityAssessmentProcess!.QualityStatistics = await _qualityAssessmentService.GetQualityStatisticsAsync(reviewProcess.QualityAssessmentProcess.Id);
            }

            // if (reviewProcess.DataExtractionProcess != null)
            // {
            //     response.DataExtractionProcess!.DataExtractionStatistics = await _dataExtractionService.GetDataExtractionStatisticsAsync(reviewProcess.DataExtractionProcess.Id, cancellationToken);
            // }

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

            //Check if current user is the leader of the project
            var (userId, _) = _currentUserService.GetCurrentUser();
            var isLeader = await _unitOfWork.SystematicReviewProjects.IsProjectLeaderAsync(reviewProcess.ProjectId, Guid.Parse(userId));
            if (!isLeader)
            {
                throw new InvalidOperationException("Only the project leader can update a review process.");
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

            //Check if current user is the leader of the project
            var (userId, _) = _currentUserService.GetCurrentUser();
            var isLeader = await _unitOfWork.SystematicReviewProjects.IsProjectLeaderAsync(reviewProcess.ProjectId, Guid.Parse(userId));
            if (!isLeader)
            {
                throw new InvalidOperationException("Only the project leader can start a review process.");
            }

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

            //Check if current user is the leader of the project
            var (userId, _) = _currentUserService.GetCurrentUser();
            var isLeader = await _unitOfWork.SystematicReviewProjects.IsProjectLeaderAsync(reviewProcess.ProjectId, Guid.Parse(userId));
            if (!isLeader)
            {
                throw new InvalidOperationException("Only the project leader can complete a review process.");
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

            //Check if current user is the leader of the project
            var (userId, _) = _currentUserService.GetCurrentUser();
            var isLeader = await _unitOfWork.SystematicReviewProjects.IsProjectLeaderAsync(reviewProcess.ProjectId, Guid.Parse(userId));
            if (!isLeader)
            {
                throw new InvalidOperationException("Only the project leader can delete a review process.");
            }

            await _unitOfWork.ReviewProcesses.RemoveAsync(reviewProcess, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<ReviewProcessResponse> AssignProtocolAsync(
            Guid processId,
            Guid protocolId,
            CancellationToken cancellationToken = default)
        {
            var reviewProcess = await _unitOfWork.ReviewProcesses
                .FindSingleAsync(rp => rp.Id == processId, isTracking: true, cancellationToken);

            if (reviewProcess == null)
            {
                throw new NotFoundException($"ReviewProcess with ID {processId} not found.");
            }

            // Check if current user is the leader of the project
            var (userId, _) = _currentUserService.GetCurrentUser();
            var isLeader = await _unitOfWork.SystematicReviewProjects.IsProjectLeaderAsync(reviewProcess.ProjectId, Guid.Parse(userId));
            if (!isLeader)
            {
                throw new InvalidOperationException("Only the project leader can assign a protocol to a review process.");
            }

            var protocol = await _unitOfWork.Protocols.FindSingleAsync(p => p.Id == protocolId, cancellationToken: cancellationToken);
            if (protocol == null)
            {
                throw new NotFoundException($"Protocol with ID {protocolId} not found.");
            }

            reviewProcess.SetProtocol(protocol);

            await _unitOfWork.ReviewProcesses.UpdateAsync(reviewProcess, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToResponse(reviewProcess);
        }

        public async Task<ProtocolDetailResponse?> GetProtocolByProcessIdAsync(
            Guid processId,
            CancellationToken cancellationToken = default)
        {
            var reviewProcess = await _unitOfWork.ReviewProcesses
                .FindSingleAsync(rp => rp.Id == processId, cancellationToken: cancellationToken);

            if (reviewProcess == null)
            {
                throw new NotFoundException($"ReviewProcess with ID {processId} not found.");
            }

            if (reviewProcess.ProtocolId == null)
            {
                return null;
            }

            var protocol = await _unitOfWork.Protocols
                .GetByIdWithVersionsAsync(reviewProcess.ProtocolId.Value, cancellationToken);

            return protocol?.ToDetailResponse();
        }

        private static ReviewProcessResponse MapToResponse(Repositories.Entities.ReviewProcess reviewProcess)
        {
            return new ReviewProcessResponse
            {
                Id = reviewProcess.Id,
                Name = reviewProcess.Name,
                ProjectId = reviewProcess.ProjectId,
                ProtocolId = reviewProcess.ProtocolId,
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
