using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.StudySelection;

namespace SRSS.IAM.Services.StudySelectionService
{
    public class StudySelectionService : IStudySelectionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StudySelectionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<StudySelectionProcessResponse> CreateStudySelectionProcessAsync(
            CreateStudySelectionProcessRequest request,
            CancellationToken cancellationToken = default)
        {
            // Validate ReviewProcess exists
            var reviewProcess = await _unitOfWork.ReviewProcesses.GetByIdWithProcessesAsync(request.ReviewProcessId, cancellationToken);

            if (reviewProcess == null)
            {
                throw new InvalidOperationException($"ReviewProcess with ID {request.ReviewProcessId} not found.");
            }

            // Use domain guard to enforce business rules
            reviewProcess.EnsureCanCreateStudySelectionProcess();

            // Create new process
            var process = new StudySelectionProcess
            {
                Id = Guid.NewGuid(),
                ReviewProcessId = request.ReviewProcessId,
                Notes = request.Notes,
                Status = SelectionProcessStatus.NotStarted,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.StudySelectionProcesses.AddAsync(process, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToResponse(process);
        }

        public async Task<StudySelectionProcessResponse> GetStudySelectionProcessAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var process = await _unitOfWork.StudySelectionProcesses.GetByIdAsync(id,
                cancellationToken: cancellationToken);

            if (process == null)
            {
                throw new InvalidOperationException($"StudySelectionProcess with ID {id} not found.");
            }

            var studySelectionProcessResponse = MapToResponse(process);
            studySelectionProcessResponse.ScreenedStudy = await _unitOfWork.ScreeningDecisions.CountScreenedPapersAsync(id,cancellationToken: cancellationToken);
            var eligiblePapers = await GetEligiblePapersAsync(id, cancellationToken);
            studySelectionProcessResponse.StudyToScreen = eligiblePapers.Count - studySelectionProcessResponse.ScreenedStudy;

            return studySelectionProcessResponse;
        }

        public async Task<StudySelectionProcessResponse> StartStudySelectionProcessAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var process = await _unitOfWork.StudySelectionProcesses.FindSingleAsync(
                ssp => ssp.Id == id,
                isTracking: true,
                cancellationToken);

            if (process == null)
            {
                throw new InvalidOperationException($"StudySelectionProcess with ID {id} not found.");
            }

            // Load ReviewProcess with IdentificationProcess for validation
            var reviewProcess = await _unitOfWork.ReviewProcesses.FindSingleAsync(
                rp => rp.Id == process.ReviewProcessId,
                isTracking: false,
                cancellationToken);

            if (reviewProcess == null)
            {
                throw new InvalidOperationException("ReviewProcess not found.");
            }

            // Manually load IdentificationProcess
            var identificationProcess = await _unitOfWork.IdentificationProcesses.FindSingleAsync(
                ip => ip.ReviewProcessId == reviewProcess.Id,
                isTracking: false,
                cancellationToken);

            reviewProcess.IdentificationProcess = identificationProcess;
            process.ReviewProcess = reviewProcess;

            // Use domain method for validation and state transition
            process.Start();

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToResponse(process);
        }

        public async Task<StudySelectionProcessResponse> CompleteStudySelectionProcessAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var process = await _unitOfWork.StudySelectionProcesses.FindSingleAsync(
                ssp => ssp.Id == id,
                isTracking: true,
                cancellationToken);

            if (process == null)
            {
                throw new InvalidOperationException($"StudySelectionProcess with ID {id} not found.");
            }

            // Check for unresolved conflicts
            var conflictedPapers = await _unitOfWork.ScreeningDecisions.GetPapersWithConflictsAsync(id, cancellationToken);
            var resolvedPaperIds = (await _unitOfWork.ScreeningResolutions.GetByProcessAsync(id, cancellationToken))
                .Select(sr => sr.PaperId)
                .ToList();

            var unresolvedConflicts = conflictedPapers.Except(resolvedPaperIds).ToList();

            if (unresolvedConflicts.Any())
            {
                throw new InvalidOperationException($"Cannot complete process with {unresolvedConflicts.Count} unresolved conflicts.");
            }

            // Use domain method for state transition
            process.Complete();

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToResponse(process);
        }

        public async Task<List<Guid>> GetEligiblePapersAsync(
            Guid studySelectionProcessId,
            CancellationToken cancellationToken = default)
        {
            // Get the process with ReviewProcess
            var process = await _unitOfWork.StudySelectionProcesses.FindSingleAsync(
                ssp => ssp.Id == studySelectionProcessId,
                cancellationToken: cancellationToken);

            if (process == null)
            {
                throw new InvalidOperationException($"StudySelectionProcess with ID {studySelectionProcessId} not found.");
            }

            // Get ReviewProcess to find the project
            var reviewProcess = await _unitOfWork.ReviewProcesses.FindSingleAsync(
                rp => rp.Id == process.ReviewProcessId,
                cancellationToken: cancellationToken);

            if (reviewProcess == null)
            {
                throw new InvalidOperationException($"ReviewProcess not found.");
            }

            // Get all papers from the project
            var projectPapers = await _unitOfWork.Papers.FindAllAsync(
                p => p.ProjectId == reviewProcess.ProjectId,
                cancellationToken: cancellationToken);

            var paperIds = projectPapers.Select(p => p.Id).ToList();

            // Get duplicate papers from the IdentificationProcess
            var identificationProcess = await _unitOfWork.IdentificationProcesses.FindSingleAsync(
                ip => ip.ReviewProcessId == reviewProcess.Id,
                cancellationToken: cancellationToken);

            List<Guid> duplicatePaperIds = new();
            if (identificationProcess != null)
            {
                var duplicates = await _unitOfWork.DeduplicationResults.FindAllAsync(
                    dr => dr.IdentificationProcessId == identificationProcess.Id,
                    cancellationToken: cancellationToken);

                duplicatePaperIds = duplicates.Select(dr => dr.PaperId).ToList();
            }

            // Eligible papers = All project papers - Duplicates
            var eligiblePapers = paperIds.Except(duplicatePaperIds).ToList();

            return eligiblePapers;
        }

        public async Task<ScreeningDecisionResponse> SubmitScreeningDecisionAsync(
            Guid studySelectionProcessId,
            Guid paperId,
            SubmitScreeningDecisionRequest request,
            CancellationToken cancellationToken = default)
        {
            // Validate process exists and is in progress
            var process = await _unitOfWork.StudySelectionProcesses.FindSingleAsync(
                ssp => ssp.Id == studySelectionProcessId,
                cancellationToken: cancellationToken);

            if (process == null)
            {
                throw new InvalidOperationException($"StudySelectionProcess with ID {studySelectionProcessId} not found.");
            }

            if (process.Status != SelectionProcessStatus.InProgress)
            {
                throw new InvalidOperationException($"Cannot submit decisions for process in {process.Status} status.");
            }

            // Check if reviewer already has a decision for this paper
            var existingDecision = await _unitOfWork.ScreeningDecisions.GetByReviewerAndPaperAsync(
                studySelectionProcessId,
                paperId,
                request.ReviewerId,
                cancellationToken);

            if (existingDecision != null)
            {
                throw new InvalidOperationException("Reviewer has already submitted a decision for this paper.");
            }

            // Load paper for response
            var paper = await _unitOfWork.Papers.FindSingleAsync(p => p.Id == paperId, cancellationToken: cancellationToken);

            if (paper == null)
            {
                throw new InvalidOperationException($"Paper with ID {paperId} not found.");
            }

            // Create new decision
            var decision = new ScreeningDecision
            {
                Id = Guid.NewGuid(),
                StudySelectionProcessId = studySelectionProcessId,
                PaperId = paperId,
                ReviewerId = request.ReviewerId,
                Decision = request.Decision,
                Reason = request.Reason,
                DecidedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.ScreeningDecisions.AddAsync(decision, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);



            return new ScreeningDecisionResponse
            {
                Id = decision.Id,
                StudySelectionProcessId = decision.StudySelectionProcessId,
                PaperId = decision.PaperId,
                PaperTitle = paper?.Title ?? string.Empty,
                ReviewerId = decision.ReviewerId,
                Decision = decision.Decision,
                DecisionText = decision.Decision.ToString(),
                Reason = decision.Reason,
                DecidedAt = decision.DecidedAt
            };
        }

        public async Task<List<ScreeningDecisionResponse>> GetDecisionsByPaperAsync(
            Guid studySelectionProcessId,
            Guid paperId,
            CancellationToken cancellationToken = default)
        {
            var decisions = await _unitOfWork.ScreeningDecisions.GetByPaperAsync(
                studySelectionProcessId,
                paperId,
                cancellationToken);

            var paper = await _unitOfWork.Papers.FindSingleAsync(p => p.Id == paperId, cancellationToken: cancellationToken);

            return decisions.Select(d => new ScreeningDecisionResponse
            {
                Id = d.Id,
                StudySelectionProcessId = d.StudySelectionProcessId,
                PaperId = d.PaperId,
                PaperTitle = paper?.Title ?? string.Empty,
                ReviewerId = d.ReviewerId,
                Decision = d.Decision,
                DecisionText = d.Decision.ToString(),
                Reason = d.Reason,
                DecidedAt = d.DecidedAt
            }).ToList();
        }

        public async Task<List<ConflictedPaperResponse>> GetConflictedPapersAsync(
            Guid studySelectionProcessId,
            CancellationToken cancellationToken = default)
        {
            var conflictedPaperIds = await _unitOfWork.ScreeningDecisions.GetPapersWithConflictsAsync(
                studySelectionProcessId,
                cancellationToken);

            var result = new List<ConflictedPaperResponse>();

            foreach (var paperId in conflictedPaperIds)
            {
                var paper = await _unitOfWork.Papers.FindSingleAsync(p => p.Id == paperId, cancellationToken: cancellationToken);
                var decisions = await _unitOfWork.ScreeningDecisions.GetByPaperAsync(
                    studySelectionProcessId,
                    paperId,
                    cancellationToken);

                // Check if already resolved
                var resolution = await _unitOfWork.ScreeningResolutions.GetByProcessAndPaperAsync(
                    studySelectionProcessId,
                    paperId,
                    cancellationToken);

                if (resolution == null) // Only include unresolved conflicts
                {
                    result.Add(new ConflictedPaperResponse
                    {
                        PaperId = paperId,
                        Title = paper?.Title ?? string.Empty,
                        DOI = paper?.DOI,
                        ConflictingDecisions = decisions.Select(d => new ScreeningDecisionResponse
                        {
                            Id = d.Id,
                            StudySelectionProcessId = d.StudySelectionProcessId,
                            PaperId = d.PaperId,
                            PaperTitle = paper?.Title ?? string.Empty,
                            ReviewerId = d.ReviewerId,
                            Decision = d.Decision,
                            DecisionText = d.Decision.ToString(),
                            Reason = d.Reason,
                            DecidedAt = d.DecidedAt
                        }).ToList()
                    });
                }
            }

            return result;
        }

        public async Task<ScreeningResolutionResponse> ResolveConflictAsync(
            Guid studySelectionProcessId,
            Guid paperId,
            ResolveScreeningConflictRequest request,
            CancellationToken cancellationToken = default)
        {
            // Check if resolution already exists
            var existing = await _unitOfWork.ScreeningResolutions.GetByProcessAndPaperAsync(
                studySelectionProcessId,
                paperId,
                cancellationToken);

            if (existing != null)
            {
                throw new InvalidOperationException("Resolution already exists for this paper.");
            }

            // Create resolution
            var resolution = new ScreeningResolution
            {
                Id = Guid.NewGuid(),
                StudySelectionProcessId = studySelectionProcessId,
                PaperId = paperId,
                FinalDecision = request.FinalDecision,
                ResolutionNotes = request.ResolutionNotes,
                ResolvedBy = request.ResolvedBy,
                ResolvedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.ScreeningResolutions.AddAsync(resolution, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var paper = await _unitOfWork.Papers.FindSingleAsync(p => p.Id == paperId, cancellationToken: cancellationToken);

            return new ScreeningResolutionResponse
            {
                Id = resolution.Id,
                StudySelectionProcessId = resolution.StudySelectionProcessId,
                PaperId = resolution.PaperId,
                PaperTitle = paper?.Title ?? string.Empty,
                FinalDecision = resolution.FinalDecision,
                FinalDecisionText = resolution.FinalDecision.ToString(),
                ResolutionNotes = resolution.ResolutionNotes,
                ResolvedBy = resolution.ResolvedBy,
                ResolvedAt = resolution.ResolvedAt
            };
        }

        public async Task<PaperSelectionStatus> GetPaperSelectionStatusAsync(
            Guid studySelectionProcessId,
            Guid paperId,
            CancellationToken cancellationToken = default)
        {
            // Check if resolution exists
            var resolution = await _unitOfWork.ScreeningResolutions.GetByProcessAndPaperAsync(
                studySelectionProcessId,
                paperId,
                cancellationToken);

            if (resolution != null)
            {
                return resolution.FinalDecision == ScreeningDecisionType.Include
                    ? PaperSelectionStatus.Included
                    : PaperSelectionStatus.Excluded;
            }

            // Check decisions
            var decisions = await _unitOfWork.ScreeningDecisions.GetByPaperAsync(
                studySelectionProcessId,
                paperId,
                cancellationToken);

            if (!decisions.Any())
            {
                return PaperSelectionStatus.Pending;
            }

            // Check for conflicts
            var distinctDecisions = decisions.Select(d => d.Decision).Distinct().Count();
            if (distinctDecisions > 1)
            {
                return PaperSelectionStatus.Conflict;
            }

            // Unanimous decision
            return decisions.First().Decision == ScreeningDecisionType.Include
                ? PaperSelectionStatus.Included
                : PaperSelectionStatus.Excluded;
        }

        public async Task<SelectionStatisticsResponse> GetSelectionStatisticsAsync(
            Guid studySelectionProcessId,
            CancellationToken cancellationToken = default)
        {
            // Get eligible papers
            var eligiblePapers = await GetEligiblePapersAsync(studySelectionProcessId, cancellationToken);
            var totalPapers = eligiblePapers.Count;

            // Get all resolutions
            var resolutions = await _unitOfWork.ScreeningResolutions.GetByProcessAsync(
                studySelectionProcessId,
                cancellationToken);

            var includedCount = resolutions.Count(r => r.FinalDecision == ScreeningDecisionType.Include);
            var excludedCount = resolutions.Count(r => r.FinalDecision == ScreeningDecisionType.Exclude);

            // Get conflicted papers (not yet resolved)
            var conflictedPaperIds = await _unitOfWork.ScreeningDecisions.GetPapersWithConflictsAsync(
                studySelectionProcessId,
                cancellationToken);

            var resolvedPaperIds = resolutions.Select(r => r.PaperId).ToHashSet();
            var unresolvedConflicts = conflictedPaperIds.Where(id => !resolvedPaperIds.Contains(id)).ToList();

            var conflictCount = unresolvedConflicts.Count;

            // Pending = Total - Resolved - Conflicts
            var pendingCount = totalPapers - resolutions.Count - conflictCount;

            var completionPercentage = totalPapers > 0
                ? (double)(resolutions.Count + conflictCount) / totalPapers * 100
                : 0;

            return new SelectionStatisticsResponse
            {
                StudySelectionProcessId = studySelectionProcessId,
                TotalPapers = totalPapers,
                IncludedCount = includedCount,
                ExcludedCount = excludedCount,
                ConflictCount = conflictCount,
                PendingCount = pendingCount,
                CompletionPercentage = Math.Round(completionPercentage, 2)
            };
        }

        public async Task<List<PaperWithDecisionsResponse>> GetPapersWithDecisionsAsync(
            Guid studySelectionProcessId,
            CancellationToken cancellationToken = default)
        {
            var eligiblePaperIds = await GetEligiblePapersAsync(studySelectionProcessId, cancellationToken);
            var result = new List<PaperWithDecisionsResponse>();

            foreach (var paperId in eligiblePaperIds)
            {
                var paper = await _unitOfWork.Papers.FindSingleAsync(p => p.Id == paperId, cancellationToken: cancellationToken);
                var decisions = await _unitOfWork.ScreeningDecisions.GetByPaperAsync(
                    studySelectionProcessId,
                    paperId,
                    cancellationToken);

                var resolution = await _unitOfWork.ScreeningResolutions.GetByProcessAndPaperAsync(
                    studySelectionProcessId,
                    paperId,
                    cancellationToken);

                var status = await GetPaperSelectionStatusAsync(studySelectionProcessId, paperId, cancellationToken);

                result.Add(new PaperWithDecisionsResponse
                {
                    PaperId = paperId,
                    Title = paper?.Title ?? string.Empty,
                    DOI = paper?.DOI,
                    Authors = paper?.Authors,
                    PublicationYear = paper?.PublicationYearInt,
                    Status = status,
                    StatusText = status.ToString(),
                    Decisions = decisions.Select(d => new ScreeningDecisionResponse
                    {
                        Id = d.Id,
                        StudySelectionProcessId = d.StudySelectionProcessId,
                        PaperId = d.PaperId,
                        PaperTitle = paper?.Title ?? string.Empty,
                        ReviewerId = d.ReviewerId,
                        Decision = d.Decision,
                        DecisionText = d.Decision.ToString(),
                        Reason = d.Reason,
                        DecidedAt = d.DecidedAt
                    }).ToList(),
                    Resolution = resolution != null ? new ScreeningResolutionResponse
                    {
                        Id = resolution.Id,
                        StudySelectionProcessId = resolution.StudySelectionProcessId,
                        PaperId = resolution.PaperId,
                        PaperTitle = paper?.Title ?? string.Empty,
                        FinalDecision = resolution.FinalDecision,
                        FinalDecisionText = resolution.FinalDecision.ToString(),
                        ResolutionNotes = resolution.ResolutionNotes,
                        ResolvedBy = resolution.ResolvedBy,
                        ResolvedAt = resolution.ResolvedAt
                    } : null
                });
            }

            return result;
        }

        private static StudySelectionProcessResponse MapToResponse(StudySelectionProcess process)
        {
            return new StudySelectionProcessResponse
            {
                Id = process.Id,
                ReviewProcessId = process.ReviewProcessId,
                Notes = process.Notes,
                StartedAt = process.StartedAt,
                CompletedAt = process.CompletedAt,
                Status = process.Status,
                StatusText = process.Status.ToString(),
                CreatedAt = process.CreatedAt,
                ModifiedAt = process.ModifiedAt

            };
        }
    }
}
