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
using SRSS.IAM.Repositories.Entities.Enums;

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
                    await GetPrismaStatisticsForIdentificationAsync(reviewProcess.Id, cancellationToken);
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
                        await GetPrismaStatisticsForIdentificationAsync(reviewProcess.Id, cancellationToken);
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

        public async Task<List<ReviewProcessSnapshotResponse>> GetReviewProcessSnapshotsByProjectIdAsync(
            Guid projectId,
            CancellationToken cancellationToken = default)
        {
            var reviewProcesses = await _unitOfWork.ReviewProcesses.GetByProjectIdAsync(projectId, cancellationToken);
            var responses = new List<ReviewProcessSnapshotResponse>();

            foreach (var process in reviewProcesses.OrderByDescending(x => x.CreatedAt))
            {
                var importedCount = 0;
                var includeCount = 0;
                var excludeCount = 0;

                if (process.IdentificationProcess != null)
                {
                    var prismaStats = await _identificationService.GetPrismaStatisticsAsync(process.Id, cancellationToken);
                    importedCount = prismaStats.UniqueRecords;
                }

                if (process.StudySelectionProcess != null)
                {
                    var selectionStats = await _studySelectionService.GetPhaseStatisticsAsync(process.StudySelectionProcess.Id, ScreeningPhase.FullText, cancellationToken);
                    includeCount = selectionStats.IncludedCount;
                    excludeCount = selectionStats.ExcludedCount;
                }

                responses.Add(new ReviewProcessSnapshotResponse
                {
                    ProcessId = process.Id,
                    ProcessName = process.Name,
                    StatusText = process.Status.ToString(),
                    StartAt = process.StartedAt,
                    CompletedAt = process.CompletedAt,
                    ProgressPercent = CalculateProgressPercent(process),
                    TotalPapersImported = importedCount,
                    TotalIncludedPapers = includeCount,
                    TotalExcludedPapers = excludeCount
                });
            }

            return responses;
        }

        public async Task<AddPapersToReviewProcessResponse> AddSelectedPapersAsync(
            Guid reviewProcessId,
            AddSelectedPapersRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request.PaperIds == null || request.PaperIds.Count == 0)
            {
                throw new ArgumentException("paperIds is required.");
            }

            var reviewProcess = await _unitOfWork.ReviewProcesses.GetByIdWithProjectAsync(reviewProcessId, cancellationToken);
            if (reviewProcess == null)
            {
                throw new NotFoundException($"Review process with ID {reviewProcessId} not found.");
            }

            var identificationProcess = reviewProcess.IdentificationProcess
                ?? throw new NotFoundException("Identification process not found for this review process.");

            var distinctPaperIds = request.PaperIds.Distinct().ToList();

            var existingPapers = await _unitOfWork.Papers.FindAllAsync(
                x => distinctPaperIds.Contains(x.Id) && x.ProjectId == reviewProcess.ProjectId,
                isTracking: false,
                cancellationToken: cancellationToken);

            var foundPaperIds = existingPapers.Select(x => x.Id).ToHashSet();
            var missingPaperIds = distinctPaperIds.Where(x => !foundPaperIds.Contains(x)).ToList();
            if (missingPaperIds.Count > 0)
            {
                throw new NotFoundException($"Some papers were not found in this project: {string.Join(", ", missingPaperIds)}");
            }

            var existingSnapshotPaperIds = await _unitOfWork.IdentificationProcessPapers.GetQueryable()
                .AsNoTracking()
                .Where(x => x.IdentificationProcessId == identificationProcess.Id && distinctPaperIds.Contains(x.PaperId))
                .Select(x => x.PaperId)
                .ToListAsync(cancellationToken);

            var existingSet = existingSnapshotPaperIds.ToHashSet();
            var newPaperIds = distinctPaperIds.Where(x => !existingSet.Contains(x)).ToList();
            var now = DateTimeOffset.UtcNow;
            var newRecords = newPaperIds.Select(paperId => new Repositories.Entities.IdentificationProcessPaper
            {
                Id = Guid.NewGuid(),
                IdentificationProcessId = identificationProcess.Id,
                PaperId = paperId,
                IncludedAfterDedup = true,
                SourceType = PaperSourceType.DatabaseSearch,
                CreatedAt = now,
                ModifiedAt = now
            }).ToList();

            if (newRecords.Count > 0)
            {
                await _unitOfWork.IdentificationProcessPapers.AddRangeAsync(newRecords, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return new AddPapersToReviewProcessResponse
            {
                Inserted = newRecords.Count,
                SkippedAsDuplicate = distinctPaperIds.Count - newRecords.Count,
                ReviewProcessSnapshot = new ReviewProcessProgressSnapshotResponse
                {
                    ReviewProcessId = reviewProcess.Id,
                    ReviewProcessName = reviewProcess.Name,
                    StatusText = reviewProcess.Status.ToString(),
                    ProgressPercent = CalculateProgressPercent(reviewProcess)
                }
            };
        }

        public async Task<AddPapersFromFilterResponse> AddPapersFromFilterSettingAsync(
            Guid reviewProcessId,
            AddFromFilterSettingRequest request,
            CancellationToken cancellationToken = default)
        {
            var reviewProcess = await _unitOfWork.ReviewProcesses.GetByIdWithProjectAsync(reviewProcessId, cancellationToken);
            if (reviewProcess == null)
            {
                throw new NotFoundException($"Review process with ID {reviewProcessId} not found.");
            }

            var identificationProcess = reviewProcess.IdentificationProcess
                ?? throw new NotFoundException("Identification process not found for this review process.");

            var filterSetting = await _unitOfWork.FilterSettings.FindSingleAsync(
                x => x.Id == request.FilterSettingId && x.ProjectId == reviewProcess.ProjectId,
                isTracking: false,
                cancellationToken: cancellationToken);

            if (filterSetting == null)
            {
                throw new NotFoundException("Filter setting not found in this project.");
            }

            var (matchedPapers, matchedTotal) = await _unitOfWork.Papers.GetPaperPoolByProjectAsync(
                reviewProcess.ProjectId,
                filterSetting.SearchText,
                filterSetting.Keyword,
                filterSetting.YearFrom,
                filterSetting.YearTo,
                filterSetting.SearchSourceId,
                filterSetting.ImportBatchId,
                filterSetting.DoiState,
                filterSetting.FullTextState,
                filterSetting.OnlyUnused,
                filterSetting.RecentlyImported,
                pageNumber: 1,
                pageSize: int.MaxValue,
                cancellationToken: cancellationToken);

            if (matchedTotal == 0)
            {
                throw new InvalidOperationException("No papers matched");
            }

            var matchedPaperIds = matchedPapers.Select(x => x.Id).Distinct().ToList();
            var existingPaperIds = await _unitOfWork.IdentificationProcessPapers.GetQueryable()
                .AsNoTracking()
                .Where(x => x.IdentificationProcessId == identificationProcess.Id && matchedPaperIds.Contains(x.PaperId))
                .Select(x => x.PaperId)
                .ToListAsync(cancellationToken);

            var existingSet = existingPaperIds.ToHashSet();
            var newPaperIds = matchedPaperIds.Where(x => !existingSet.Contains(x)).ToList();
            var now = DateTimeOffset.UtcNow;

            var newRecords = newPaperIds.Select(paperId => new Repositories.Entities.IdentificationProcessPaper
            {
                Id = Guid.NewGuid(),
                IdentificationProcessId = identificationProcess.Id,
                PaperId = paperId,
                IncludedAfterDedup = true,
                SourceType = PaperSourceType.DatabaseSearch,
                CreatedAt = now,
                ModifiedAt = now
            }).ToList();

            if (newRecords.Count > 0)
            {
                await _unitOfWork.IdentificationProcessPapers.AddRangeAsync(newRecords, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return new AddPapersFromFilterResponse
            {
                Inserted = newRecords.Count,
                SkippedAsDuplicate = matchedPaperIds.Count - newRecords.Count,
                MatchedTotal = matchedPaperIds.Count,
                ProcessSnapshot = new ProcessSnapshotWithExistingPapersResponse
                {
                    ProcessId = reviewProcess.Id,
                    ProcessName = reviewProcess.Name,
                    StatusText = reviewProcess.Status.ToString(),
                    ProgressPercent = CalculateProgressPercent(reviewProcess),
                    ExistingPaperIds = existingPaperIds
                }
            };
        }

        private static double CalculateProgressPercent(Repositories.Entities.ReviewProcess reviewProcess)
        {
            var completedFlags = new[]
            {
                reviewProcess.IdentificationProcess?.Status == IdentificationStatus.Completed,
                reviewProcess.StudySelectionProcess?.Status == SelectionProcessStatus.Completed,
                reviewProcess.QualityAssessmentProcess?.Status == QualityAssessmentProcessStatus.Completed,
                reviewProcess.DataExtractionProcess?.Status == ExtractionProcessStatus.Completed,
                reviewProcess.SynthesisProcess?.Status == SynthesisProcessStatus.Completed
            };

            var completedCount = completedFlags.Count(x => x);
            return Math.Round((completedCount / 5d) * 100d, 2);
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
            Guid reviewProcessId,
            CancellationToken cancellationToken)
        {
            return await _identificationService.GetPrismaStatisticsAsync(reviewProcessId, cancellationToken);
        }
    }
}
