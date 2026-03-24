using Azure.Core;
using Microsoft.Extensions.Logging;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.Common;
using SRSS.IAM.Services.DTOs.StudySelection;
using SRSS.IAM.Services.GrobidClient;
using SRSS.IAM.Services.MetadataMergeService;
using SRSS.IAM.Services.NotificationService;

namespace SRSS.IAM.Services.StudySelectionService
{
    public class StudySelectionService : IStudySelectionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGrobidService _grobidService;
        private readonly IMetadataMergeService _metadataMergeService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<StudySelectionService> _logger;

        public StudySelectionService(
            IUnitOfWork unitOfWork,
            IGrobidService grobidService,
            IMetadataMergeService metadataMergeService,
            INotificationService notificationService,
            ILogger<StudySelectionService> logger)
        {
            _unitOfWork = unitOfWork;
            _grobidService = grobidService;
            _metadataMergeService = metadataMergeService;
            _notificationService = notificationService;
            _logger = logger;
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

            studySelectionProcessResponse.PhaseStatistics = new PhaseStatisticsResponse
            {
                TitleAbstract = await GetPhaseStatisticsAsync(id, ScreeningPhase.TitleAbstract, cancellationToken),
                FullText = await GetPhaseStatisticsAsync(id, ScreeningPhase.FullText, cancellationToken)
            };

            // Maintain legacy field for backward compatibility
            studySelectionProcessResponse.SelectionStatistics = studySelectionProcessResponse.PhaseStatistics.TitleAbstract;

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
            reviewProcess.CurrentPhase = ProcessPhase.StudySelection;
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
            var conflictedPapers = await _unitOfWork.ScreeningDecisions.GetPapersWithConflictsAsync(id, null, cancellationToken);
            var resolvedPaperIds = (await _unitOfWork.ScreeningResolutions.GetByProcessAsync(id, null, cancellationToken))
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

            // Get ReviewProcess to find the IdentificationProcess
            var reviewProcess = await _unitOfWork.ReviewProcesses.FindSingleAsync(
                rp => rp.Id == process.ReviewProcessId,
                cancellationToken: cancellationToken);

            if (reviewProcess == null)
            {
                throw new InvalidOperationException($"ReviewProcess not found.");
            }

            // Get the IdentificationProcess for this review process
            var identificationProcess = await _unitOfWork.IdentificationProcesses.FindSingleAsync(
                ip => ip.ReviewProcessId == reviewProcess.Id,
                cancellationToken: cancellationToken);

            if (identificationProcess == null)
            {
                return new List<Guid>();
            }

            // Use the frozen snapshot dataset generated when identification was completed
            return await _unitOfWork.IdentificationProcessPapers.GetIncludedPaperIdsByProcessAsync(
                identificationProcess.Id,
                cancellationToken);
        }

        public async Task<List<Guid>> GetFullTextEligiblePapersAsync(
            Guid studySelectionProcessId,
            CancellationToken cancellationToken = default)
        {
            var process = await _unitOfWork.StudySelectionProcesses.GetPhaseStatusAsync(studySelectionProcessId, cancellationToken);

            if (process == null) return new List<Guid>();

            var eligiblePapers = await GetEligiblePapersAsync(studySelectionProcessId, cancellationToken);
            if (!eligiblePapers.Any()) return new List<Guid>();

            int requiredReviewers = process.TitleAbstractScreening?.MinReviewersPerPaper ?? 2;

            // Fetch decisions and resolutions in bulk for Phase = TitleAbstract
            var decisions = await _unitOfWork.ScreeningDecisions.FindAllAsync(
                d => d.StudySelectionProcessId == studySelectionProcessId && d.Phase == ScreeningPhase.TitleAbstract,
                isTracking: false,
                cancellationToken: cancellationToken);

            var resolutions = await _unitOfWork.ScreeningResolutions.FindAllAsync(
                r => r.StudySelectionProcessId == studySelectionProcessId && r.Phase == ScreeningPhase.TitleAbstract,
                isTracking: false,
                cancellationToken: cancellationToken);

            var decisionMap = decisions.GroupBy(d => d.PaperId).ToDictionary(g => g.Key, g => g.ToList());
            var resolutionMap = resolutions.ToDictionary(r => r.PaperId);

            var fullTextEligible = new List<Guid>();

            foreach (var paperId in eligiblePapers)
            {
                var status = ComputePaperStatus(
                    decisionMap.TryGetValue(paperId, out var dList) ? dList : null,
                    resolutionMap.TryGetValue(paperId, out var res) ? res : null,
                    requiredReviewers,
                    ScreeningPhase.TitleAbstract);

                if (status == PaperSelectionStatus.Included)
                {
                    fullTextEligible.Add(paperId);
                }
            }

            return fullTextEligible;
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

            //if (process.Status != SelectionProcessStatus.InProgress)
            //{
            //    throw new InvalidOperationException($"Cannot submit decisions for process in {process.Status} status.");
            //}

            // Check if reviewer already has a decision for this paper in THIS phase
            var existingDecision = await _unitOfWork.ScreeningDecisions.GetByReviewerAndPaperAsync(
                studySelectionProcessId,
                paperId,
                request.ReviewerId,
                request.Phase,
                cancellationToken);

            if (existingDecision != null)
            {
                throw new InvalidOperationException($"Reviewer has already submitted a {request.Phase} decision for this paper.");
            }

            // Load paper for response
            var paper = await _unitOfWork.Papers.FindSingleAsync(p => p.Id == paperId, cancellationToken: cancellationToken);

            if (paper == null)
            {
                throw new InvalidOperationException($"Paper with ID {paperId} not found.");
            }

            // Validate exclusion reason when decision is Exclude
            if (request.Decision == ScreeningDecisionType.Exclude && request.ExclusionReasonCode == null)
            {
                throw new ArgumentException("ExclusionReasonCode is required when decision is Exclude.");
            }

            // Create new decision
            var decision = new ScreeningDecision
            {
                Id = Guid.NewGuid(),
                StudySelectionProcessId = studySelectionProcessId,
                PaperId = paperId,
                ReviewerId = request.ReviewerId,
                Decision = request.Decision,
                Phase = request.Phase,
                ExclusionReasonCode = request.ExclusionReasonCode,
                Reason = request.Reason,
                ReviewerNotes = request.ReviewerNotes,
                DecidedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.ScreeningDecisions.AddAsync(decision, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Auto-resolution: check if all reviewers have decided and create resolution automatically (G-05, G-06)
            await TryAutoResolveAsync(studySelectionProcessId, paperId, request.Phase, cancellationToken);

            return await MapToDecisionResponse(decision, paper?.Title ?? string.Empty, cancellationToken: cancellationToken);
        }

        public async Task<List<ScreeningDecisionResponse>> GetDecisionsByPaperAsync(
            Guid studySelectionProcessId,
            Guid paperId,
            CancellationToken cancellationToken = default)
        {
            var decisions = await _unitOfWork.ScreeningDecisions.GetByPaperAsync(
                studySelectionProcessId,
                paperId,
                null,
                cancellationToken);

            var paper = await _unitOfWork.Papers.FindSingleAsync(p => p.Id == paperId, cancellationToken: cancellationToken);

            var reviewerIds = decisions.Select(d => d.ReviewerId);
            var userNames = await GetUserNamesAsync(reviewerIds, cancellationToken);
            var paperTitle = paper?.Title ?? string.Empty;

            var result = new List<ScreeningDecisionResponse>();
            foreach (var d in decisions)
            {
                result.Add(await MapToDecisionResponse(d, paperTitle, userNames, cancellationToken));
            }
            return result;
        }

        public async Task<List<ConflictedPaperResponse>> GetConflictedPapersAsync(
            Guid studySelectionProcessId,
            CancellationToken cancellationToken = default)
        {
            var conflictedPaperIds = await _unitOfWork.ScreeningDecisions.GetPapersWithConflictsAsync(
                studySelectionProcessId,
                null,
                cancellationToken);

            var result = new List<ConflictedPaperResponse>();

            foreach (var paperId in conflictedPaperIds)
            {
                var paper = await _unitOfWork.Papers.FindSingleAsync(p => p.Id == paperId, cancellationToken: cancellationToken);
                var decisions = await _unitOfWork.ScreeningDecisions.GetByPaperAsync(
                    studySelectionProcessId,
                    paperId,
                    null,
                    cancellationToken);

                // Check if already resolved
                var resolution = await _unitOfWork.ScreeningResolutions.GetByProcessAndPaperAsync(
                    studySelectionProcessId,
                    paperId,
                    null,
                    cancellationToken);

                if (resolution == null) // Only include unresolved conflicts
                {
                    var reviewerIds = decisions.Select(d => d.ReviewerId);
                    var userNames = await GetUserNamesAsync(reviewerIds, cancellationToken);
                    var paperTitle = paper?.Title ?? string.Empty;

                    var conflictingDecisions = new List<ScreeningDecisionResponse>();
                    foreach (var d in decisions)
                    {
                        conflictingDecisions.Add(await MapToDecisionResponse(d, paperTitle, userNames, cancellationToken));
                    }

                    result.Add(new ConflictedPaperResponse
                    {
                        PaperId = paperId,
                        Title = paperTitle,
                        DOI = paper?.DOI,
                        ConflictingDecisions = conflictingDecisions
                    });
                }
            }

            return result;
        }

        public async Task<PaginatedResponse<PhaseConflictedPaperResponse>> GetConflictedPapersByPhaseAsync(
            Guid studySelectionProcessId,
            ConflictedPapersRequest request,
            CancellationToken cancellationToken = default)
        {
            // 1. Fetch all decisions for the process (includes Paper) in one go to avoid N+1
            var allDecisions = await _unitOfWork.ScreeningDecisions.GetByProcessAsync(studySelectionProcessId, cancellationToken);

            // 2. Fetch all resolutions (includes phase info)
            var allResolutions = await _unitOfWork.ScreeningResolutions.GetByProcessAsync(studySelectionProcessId, null, cancellationToken);



            // 4. Group decisions by (PaperId, Phase)
            var decisionGroups = allDecisions
                .GroupBy(d => new { d.PaperId, d.Phase });

            var allPhaseConflicts = new List<PhaseConflictedPaperResponse>();

            foreach (var group in decisionGroups)
            {
                var paperId = group.Key.PaperId;
                var currentPhase = group.Key.Phase;

                // Apply phase filter if provided
                if (request.Phase.HasValue && currentPhase != request.Phase.Value)
                    continue;

                // Conflict baseline: paper having both Include and Exclude decisions from different reviewers
                var hasInclude = group.Any(d => d.Decision == ScreeningDecisionType.Include);
                var hasExclude = group.Any(d => d.Decision == ScreeningDecisionType.Exclude);

                if (hasInclude && hasExclude)
                {
                    // Find resolution for THIS paper and phase
                    var phaseResolution = allResolutions.FirstOrDefault(r => r.PaperId == paperId && r.Phase == currentPhase);
                    var currentStatus = phaseResolution != null ? PaperSelectionStatus.Resolved : PaperSelectionStatus.Conflict;

                    // Apply status filter
                    if (request.Status.HasValue && currentStatus != request.Status.Value)
                        continue;

                    var firstDecision = group.First();
                    var paper = firstDecision.Paper;
                    var paperTitle = paper?.Title ?? string.Empty;
                    var paperAuthors = paper?.Authors ?? string.Empty;
                    var paperDOI = paper?.DOI ?? string.Empty;

                    // Apply Search Filter (Title, Authors, DOI)
                    if (!string.IsNullOrWhiteSpace(request.Search))
                    {
                        var searchTerm = request.Search.Trim().ToLowerInvariant();
                        if (!paperTitle.ToLowerInvariant().Contains(searchTerm) &&
                            !paperAuthors.ToLowerInvariant().Contains(searchTerm) &&
                            !paperDOI.ToLowerInvariant().Contains(searchTerm))
                        {
                            continue;
                        }
                    }

                    allPhaseConflicts.Add(new PhaseConflictedPaperResponse
                    {
                        PaperId = paperId,
                        Title = paperTitle,
                        Authors = paperAuthors,
                        DOI = paperDOI,
                        Year = paper?.PublicationYear,
                        Source = paper?.Source,
                        Phase = currentPhase,
                        PhaseText = currentPhase.ToString(),
                        Status = currentStatus,
                        StatusText = currentStatus.ToString()
                    });
                }
            }

            // Apply Sorting (Phase ascending, then Title ascending)
            var sortedConflicts = allPhaseConflicts.OrderBy(r => r.Phase).ThenBy(r => r.Title).ToList();

            // Pagination logic
            var totalCount = sortedConflicts.Count;
            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize < 1 ? 20 : request.PageSize;

            var items = sortedConflicts
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PaginatedResponse<PhaseConflictedPaperResponse>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<ScreeningResolutionResponse> ResolveConflictAsync(
            Guid studySelectionProcessId,
            Guid paperId,
            ResolveScreeningConflictRequest request,
            CancellationToken cancellationToken = default)
        {
            // Check if resolution already exists for this phase
            var existing = await _unitOfWork.ScreeningResolutions.GetByProcessAndPaperAsync(
                studySelectionProcessId,
                paperId,
                request.Phase,
                cancellationToken);

            if (existing != null)
            {
                throw new InvalidOperationException($"Resolution already exists for this paper in phase {request.Phase}.");
            }

            // Create resolution
            var resolution = new ScreeningResolution
            {
                Id = Guid.NewGuid(),
                StudySelectionProcessId = studySelectionProcessId,
                PaperId = paperId,
                FinalDecision = request.FinalDecision,
                Phase = request.Phase,
                ResolutionNotes = request.ResolutionNotes,
                ResolvedBy = request.ResolvedBy,
                ResolvedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.ScreeningResolutions.AddAsync(resolution, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var paper = await _unitOfWork.Papers.FindSingleAsync(p => p.Id == paperId, cancellationToken: cancellationToken);

            // Send notification to assigned members
            try
            {
                var assignments = await _unitOfWork.PaperAssignments.FindAllAsync(
                    pa => pa.StudySelectionProcessId == studySelectionProcessId
                          && pa.PaperId == paperId
                          && pa.Phase == request.Phase,
                    isTracking: false,
                    cancellationToken: cancellationToken);

                var ssp = await _unitOfWork.StudySelectionProcesses.FindSingleAsync(s => s.Id == studySelectionProcessId, cancellationToken: cancellationToken);
                if (ssp == null) return await MapToResolutionResponse(resolution, paper?.Title ?? string.Empty, cancellationToken: cancellationToken);

                var rp = await _unitOfWork.ReviewProcesses.FindSingleAsync(r => r.Id == ssp.ReviewProcessId, cancellationToken: cancellationToken);
                if (rp == null) return await MapToResolutionResponse(resolution, paper?.Title ?? string.Empty, cancellationToken: cancellationToken);

                var projectMembers = await _unitOfWork.SystematicReviewProjects.GetMembersByProjectIdAsync(rp.ProjectId);

                var memberToUserMap = projectMembers.ToDictionary(m => m.Id, m => m.UserId);

                var assignedUserIds = assignments
                    .Select(a => memberToUserMap.TryGetValue(a.ProjectMemberId, out var uid) ? uid : (Guid?)null)
                    .Where(uid => uid.HasValue)
                    .Select(uid => uid!.Value)
                    .Distinct()
                    .ToList();

                if (assignedUserIds.Any())
                {
                    var resolverName = await GetUserNameAsync(request.ResolvedBy, cancellationToken);
                    if (string.IsNullOrEmpty(resolverName)) resolverName = "a manager";

                    var phaseName = request.Phase == ScreeningPhase.TitleAbstract ? "Title/Abstract" : "Full-Text";
                    var title = "Paper Conflict Resolved";
                    var message = $"The conflict for paper '{paper?.Title}' in the {phaseName} phase has been resolved by {resolverName}. Final decision: {request.FinalDecision}.";

                    await _notificationService.SendToManyAsync(
                        assignedUserIds,
                        title,
                        message,
                        NotificationType.Review,
                        paperId,
                        NotificationEntityType.PaperAssignment);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send conflict resolution notifications for paper {PaperId}", paperId);
                // Don't throw - notification failure shouldn't break resolution success
            }

            return await MapToResolutionResponse(resolution, paper?.Title ?? string.Empty, cancellationToken: cancellationToken);
        }

        public async Task<PaperSelectionStatus> GetPaperSelectionStatusAsync(
            Guid studySelectionProcessId,
            Guid paperId,
            CancellationToken cancellationToken = default)
        {
            var process = await _unitOfWork.StudySelectionProcesses.GetPhaseStatusAsync(studySelectionProcessId, cancellationToken);

            if (process == null) return PaperSelectionStatus.Pending;

            // Check if resolution exists
            var resolution = await _unitOfWork.ScreeningResolutions.GetByProcessAndPaperAsync(
                studySelectionProcessId,
                paperId,
                null,
                cancellationToken);

            var decisions = await _unitOfWork.ScreeningDecisions.GetByPaperAsync(
                studySelectionProcessId,
                paperId,
                null,
                cancellationToken);

            // Determine current phase from latest decision or fallback to TitleAbstract
            var phase = decisions.OrderByDescending(d => d.DecidedAt).FirstOrDefault()?.Phase
                        ?? ScreeningPhase.TitleAbstract;

            int requiredReviewers = phase == ScreeningPhase.TitleAbstract
                ? (process.TitleAbstractScreening?.MinReviewersPerPaper ?? 2)
                : (process.FullTextScreening?.MinReviewersPerPaper ?? 2);

            var phaseDecisions = decisions.Where(d => d.Phase == phase).ToList();

            return ComputePaperStatus(phaseDecisions, resolution, requiredReviewers, phase);
        }
        public async Task<SelectionStatisticsResponse> GetSelectionStatisticsAsync(
            Guid studySelectionProcessId,
            CancellationToken cancellationToken = default)
        {
            // Legacy wrapper - defaults to TitleAbstract
            return await GetPhaseStatisticsAsync(studySelectionProcessId, ScreeningPhase.TitleAbstract, cancellationToken);
        }

        public async Task<SelectionStatisticsResponse> GetPhaseStatisticsAsync(
            Guid studySelectionProcessId,
            ScreeningPhase phase,
            CancellationToken cancellationToken = default)
        {
            var process = await _unitOfWork.StudySelectionProcesses.GetPhaseStatusAsync(studySelectionProcessId, cancellationToken);

            if (process == null)
            {
                throw new InvalidOperationException($"StudySelectionProcess with ID {studySelectionProcessId} not found.");
            }

            int requiredReviewers = phase == ScreeningPhase.TitleAbstract
                ? (process.TitleAbstractScreening?.MinReviewersPerPaper ?? 2)
                : (process.FullTextScreening?.MinReviewersPerPaper ?? 2);

            var eligiblePaperIds = phase == ScreeningPhase.TitleAbstract
                ? await GetEligiblePapersAsync(studySelectionProcessId, cancellationToken)
                : await GetFullTextEligiblePapersAsync(studySelectionProcessId, cancellationToken);

            // Batch Fetching
            var allDecisions = await _unitOfWork.ScreeningDecisions.FindAllAsync(
                d => d.StudySelectionProcessId == studySelectionProcessId && d.Phase == phase,
                isTracking: false,
                cancellationToken: cancellationToken);

            var allResolutions = await _unitOfWork.ScreeningResolutions.FindAllAsync(
                r => r.StudySelectionProcessId == studySelectionProcessId && r.Phase == phase,
                isTracking: false,
                cancellationToken: cancellationToken);

            // Grouping
            var decisionMap = allDecisions.GroupBy(d => d.PaperId).ToDictionary(g => g.Key, g => g.ToList());
            var resolutionMap = allResolutions.ToDictionary(r => r.PaperId);

            var includedCount = 0;
            var excludedCount = 0;
            var conflictCount = 0;
            var pendingCount = 0;

            foreach (var paperId in eligiblePaperIds)
            {
                var status = ComputePaperStatus(
                    decisionMap.TryGetValue(paperId, out var dList) ? dList : null,
                    resolutionMap.TryGetValue(paperId, out var res) ? res : null,
                    requiredReviewers,
                    phase);

                switch (status)
                {
                    case PaperSelectionStatus.Included:
                        includedCount++;
                        break;
                    case PaperSelectionStatus.Excluded:
                        excludedCount++;
                        break;
                    case PaperSelectionStatus.Conflict:
                        conflictCount++;
                        break;
                    case PaperSelectionStatus.Pending:
                    default:
                        pendingCount++;
                        break;
                }
            }

            var totalPapers = eligiblePaperIds.Count;
            var decidedCount = includedCount + excludedCount + conflictCount;
            var completionPercentage = totalPapers > 0
                ? (double)decidedCount / totalPapers * 100
                : 0;

            // Exclusion reason breakdown (phase-aware)
            var exclusionBreakdown = allDecisions
                .Where(d => d.Decision == ScreeningDecisionType.Exclude && d.ExclusionReasonCode.HasValue)
                .GroupBy(d => d.ExclusionReasonCode!.Value)
                .Select(g => new ExclusionReasonBreakdownItem
                {
                    ReasonCode = g.Key,
                    ReasonText = g.Key.ToString(),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            return new SelectionStatisticsResponse
            {
                StudySelectionProcessId = studySelectionProcessId,
                TotalPapers = totalPapers,
                IncludedCount = includedCount,
                ExcludedCount = excludedCount,
                ConflictCount = conflictCount,
                PendingCount = pendingCount,
                CompletionPercentage = Math.Round(completionPercentage, 2),
                ExclusionReasonBreakdown = exclusionBreakdown
            };
        }

        private PaperSelectionStatus ComputePaperStatus(
            List<ScreeningDecision>? decisions,
            ScreeningResolution? resolution,
            int requiredReviewers,
            ScreeningPhase phase)
        {
            // 1. Manual resolution luôn ưu tiên cho cả 2 phase
            if (resolution != null)
            {
                return resolution.FinalDecision == ScreeningDecisionType.Include
                    ? PaperSelectionStatus.Included
                    : PaperSelectionStatus.Excluded;
            }

            // 2. Chưa đủ reviewer → Pending 
            if (decisions == null || decisions.Count < requiredReviewers)
            {
                return PaperSelectionStatus.Pending;
            }

            // 3. Với phase Full-Text: Chỉ cho phép status khi có resolution
            // Nếu không có resolution, luôn coi là Conflict hoặc Pending quá trình resolution
            if (phase == ScreeningPhase.FullText)
            {
                return PaperSelectionStatus.Conflict;
            }

            // 4. Với phase Title-Abstract: Áp dụng quy tắc unanimous (tất cả các quyết định thống nhất)
            var includeCount = decisions.Count(d => d.Decision == ScreeningDecisionType.Include);
            var excludeCount = decisions.Count(d => d.Decision == ScreeningDecisionType.Exclude);

            // Consensus Include
            if (includeCount > 0 && excludeCount == 0 && decisions.Count >= requiredReviewers)
            {
                return PaperSelectionStatus.Included;
            }

            // Consensus Exclude
            if (excludeCount > 0 && includeCount == 0 && decisions.Count >= requiredReviewers)
            {
                return PaperSelectionStatus.Excluded;
            }

            // Disagreement = Conflict
            return PaperSelectionStatus.Conflict;
        }

        public async Task<ConflictPaperDetailResponse> GetConflictPaperDetailAsync(
            Guid studySelectionProcessId,
            Guid paperId,
            ScreeningPhase phase,
            CancellationToken cancellationToken = default)
        {
            // Validate process exists
            var process = await _unitOfWork.StudySelectionProcesses.GetPhaseStatusAsync(studySelectionProcessId, cancellationToken);
            if (process == null)
            {
                throw new InvalidOperationException($"StudySelectionProcess with ID {studySelectionProcessId} not found.");
            }

            // Validate paper exists
            var paper = await _unitOfWork.Papers.FindSingleAsync(p => p.Id == paperId, isTracking: false, cancellationToken: cancellationToken);
            if (paper == null)
            {
                throw new InvalidOperationException($"Paper with ID {paperId} not found.");
            }

            // Get decisions and resolution for THIS SPECIFIC PHASE
            var decisions = await _unitOfWork.ScreeningDecisions.GetByPaperAsync(studySelectionProcessId, paperId, phase, cancellationToken);
            var resolution = await _unitOfWork.ScreeningResolutions.GetByProcessAndPaperAsync(studySelectionProcessId, paperId, phase, cancellationToken);

            // Fetch assignments to check if all reviewers finished (requested field: isFinishReview)
            var assignments = await _unitOfWork.PaperAssignments.FindAllAsync(
                pa => pa.StudySelectionProcessId == studySelectionProcessId
                      && pa.PaperId == paperId
                      && pa.Phase == phase,
                isTracking: false,
                cancellationToken: cancellationToken);

            var assignmentList = assignments.ToList();

            var reviewProcess = await _unitOfWork.ReviewProcesses.FindSingleAsync(
                rp => rp.Id == process.ReviewProcessId,
                cancellationToken: cancellationToken);

            var projectMembers = await _unitOfWork.SystematicReviewProjects.GetMembersByProjectIdAsync(reviewProcess!.ProjectId);
            var memberToUserMap = projectMembers.ToDictionary(m => m.Id, m => m.UserId);

            var assignedUserIds = assignmentList
                .Select(a => memberToUserMap.TryGetValue(a.ProjectMemberId, out var uid) ? uid : (Guid?)null)
                .Where(uid => uid.HasValue)
                .Select(uid => uid!.Value)
                .ToHashSet();

            var decisionUserIds = decisions.Select(d => d.ReviewerId).ToHashSet();
            bool isFinishReview = assignedUserIds.Count > 0 && assignedUserIds.All(uid => decisionUserIds.Contains(uid));

            int requiredReviewers = phase == ScreeningPhase.TitleAbstract
                ? (process.TitleAbstractScreening?.MinReviewersPerPaper ?? 2)
                : (process.FullTextScreening?.MinReviewersPerPaper ?? 2);

            // Compute status for this specific phase
            var status = ComputePaperStatus(decisions, resolution, requiredReviewers, phase);

            // Fetch user names for mapping (decisions, resolution, AND assignments)
            var allUserIds = decisions.Select(d => d.ReviewerId).ToList();
            if (resolution != null) allUserIds.Add(resolution.ResolvedBy);
            allUserIds.AddRange(assignedUserIds);

            var userNames = await GetUserNamesAsync(allUserIds.Distinct().ToList(), cancellationToken);

            var assignedMembers = assignmentList
                .Select(a =>
                {
                    var uid = memberToUserMap.TryGetValue(a.ProjectMemberId, out var id) ? id : Guid.Empty;
                    return new ReviewerAssignmentResponse
                    {
                        ProjectMemberId = a.ProjectMemberId,
                        ReviewerId = uid,
                        ReviewerName = uid != Guid.Empty ? userNames.GetValueOrDefault(uid, "Unknown") : "Unknown"
                    };
                })
                .ToList();

            var decisionResponses = new List<ScreeningDecisionResponse>();
            foreach (var d in decisions)
            {
                decisionResponses.Add(await MapToDecisionResponse(d, paper.Title, userNames, cancellationToken));
            }

            // Deterministic final decision for THIS PHASE
            ScreeningDecisionType? finalDecision = null;
            if (resolution != null)
            {
                finalDecision = resolution.FinalDecision;
            }
            else if (status == PaperSelectionStatus.Included)
            {
                finalDecision = ScreeningDecisionType.Include;
            }
            else if (status == PaperSelectionStatus.Excluded)
            {
                finalDecision = ScreeningDecisionType.Exclude;
            }

            return new ConflictPaperDetailResponse
            {
                PaperId = paper.Id,
                Title = paper.Title,
                DOI = paper.DOI,
                Authors = paper.Authors,
                PublicationYear = paper.PublicationYear,
                PublicationDate = paper.PublicationYear,
                Abstract = paper.Abstract,
                Journal = paper.Journal,
                Source = paper.Source,
                Keywords = paper.Keywords,
                PublicationType = paper.PublicationType,
                Volume = paper.Volume,
                Issue = paper.Issue,
                Pages = paper.Pages,
                Publisher = paper.Publisher,
                Language = paper.Language,
                Url = paper.Url,
                PdfUrl = paper.PdfUrl,
                PdfFileName = paper.PdfFileName,
                ConferenceName = paper.ConferenceName,
                ConferenceLocation = paper.ConferenceLocation,
                JournalIssn = paper.JournalIssn,
                JournalEIssn = paper.JournalEIssn,
                Md5 = paper.Md5,
                Status = status,
                StatusText = status.ToString(),
                FinalDecision = finalDecision,
                FinalDecisionText = finalDecision?.ToString(),
                Decisions = decisionResponses,
                Resolution = resolution != null
                    ? await MapToResolutionResponse(resolution, paper.Title, userNames, cancellationToken)
                    : null,
                Extraction = await GetExtractionStatusAsync(paper.Id, cancellationToken),
                MetadataSources = await GetMetadataSourcesAsync(paper.Id, cancellationToken),
                ExtractionResult = await GetExtractionResultAsync(paper, cancellationToken),
                ExtractionSuggestion = null,
                IsFinishReview = isFinishReview,
                AssignedMembers = assignedMembers
            };
        }

        public async Task<StudySelectionPhaseStatusResponse> GetPhaseStatusAsync(
            Guid studySelectionProcessId,
            CancellationToken cancellationToken = default)
        {
            var process = await _unitOfWork.StudySelectionProcesses.GetPhaseStatusAsync(studySelectionProcessId, cancellationToken);

            if (process == null)
            {
                throw new InvalidOperationException($"StudySelectionProcess with ID {studySelectionProcessId} not found.");
            }

            return new StudySelectionPhaseStatusResponse
            {
                CurrentPhase = process.CurrentPhase,
                CurrentPhaseText = process.CurrentPhase.ToString(),
                TitleAbstractStarted = process.TitleAbstractScreening?.Status != ScreeningPhaseStatus.NotStarted,
                TitleAbstractCompleted = process.TitleAbstractScreening?.Status == ScreeningPhaseStatus.Completed,
                FullTextStarted = process.FullTextScreening?.Status != ScreeningPhaseStatus.NotStarted,
                FullTextCompleted = process.FullTextScreening?.Status == ScreeningPhaseStatus.Completed
            };
        }

        public async Task<PaginatedResponse<PaperWithDecisionsResponse>> GetPapersWithDecisionsAsync(
            Guid studySelectionProcessId,
            PapersWithDecisionsRequest request,
            CancellationToken cancellationToken = default)
        {
            // Phase-aware paper retrieval
            var phase = request.Phase ?? ScreeningPhase.TitleAbstract;
            List<Guid> eligiblePaperIds;

            if (phase == ScreeningPhase.FullText)
            {
                // Full-Text: papers computed as Included in Title/Abstract screening
                eligiblePaperIds = await GetFullTextEligiblePapersAsync(studySelectionProcessId, cancellationToken);
            }
            else
            {
                // Title/Abstract: all eligible papers from identification
                eligiblePaperIds = await GetEligiblePapersAsync(studySelectionProcessId, cancellationToken);
            }

            return await ProcessPapersWithDecisionsInternalAsync(studySelectionProcessId, eligiblePaperIds, request, cancellationToken);
        }

        public async Task<PaginatedResponse<PaperWithDecisionsResponse>> GetAssignedPapersAsync(
            Guid studySelectionProcessId,
            Guid userId,
            PapersWithDecisionsRequest request,
            CancellationToken cancellationToken = default)
        {
            // Filter assignments by process and user id through project member navigation
            var assignments = await _unitOfWork.PaperAssignments.FindAllAsync(
                pa => pa.StudySelectionProcessId == studySelectionProcessId && pa.ProjectMember.UserId == userId,
                isTracking: false,
                cancellationToken: cancellationToken);

            // Filter by phase if provided in request
            if (request.Phase.HasValue)
            {
                assignments = assignments.Where(pa => pa.Phase == request.Phase.Value);
            }

            var assignedPaperIds = assignments.Select(pa => pa.PaperId).Distinct().ToList();

            return await ProcessPapersWithDecisionsInternalAsync(studySelectionProcessId, assignedPaperIds, request, cancellationToken);
        }

        private async Task<PaginatedResponse<PaperWithDecisionsResponse>> ProcessPapersWithDecisionsInternalAsync(
            Guid studySelectionProcessId,
            List<Guid> paperIds,
            PapersWithDecisionsRequest request,
            CancellationToken cancellationToken)
        {
            if (request.PageNumber < 1) request.PageNumber = 1;
            if (request.PageSize < 1) request.PageSize = 20;
            if (request.PageSize > 100) request.PageSize = 100;

            var papers = new List<Paper>();
            foreach (var paperId in paperIds)
            {
                var paper = await _unitOfWork.Papers.FindSingleAsync(p => p.Id == paperId, cancellationToken: cancellationToken);
                if (paper != null) papers.Add(paper);
            }

            var process = await _unitOfWork.StudySelectionProcesses.GetPhaseStatusAsync(studySelectionProcessId, cancellationToken);

            var phase = request.Phase ?? ScreeningPhase.TitleAbstract;
            int requiredReviewers = phase == ScreeningPhase.TitleAbstract
                ? (process?.TitleAbstractScreening?.MinReviewersPerPaper ?? 2)
                : (process?.FullTextScreening?.MinReviewersPerPaper ?? 2);

            // Fetch all decisions and resolutions in batch to avoid N+1
            var allDecisions = await _unitOfWork.ScreeningDecisions.GetByProcessAsync(studySelectionProcessId, cancellationToken);
            var phaseDecisions = allDecisions.Where(d => d.Phase == phase).ToList();
            var allResolutions = await _unitOfWork.ScreeningResolutions.GetByProcessAsync(studySelectionProcessId, phase, cancellationToken);
            var phaseResolutions = allResolutions; // filtered by repo now

            var decisionMap = phaseDecisions.GroupBy(d => d.PaperId).ToDictionary(g => g.Key, g => g.ToList());
            var resolutionMap = phaseResolutions.ToDictionary(r => r.PaperId);

            var paperStatusMap = new Dictionary<Guid, PaperSelectionStatus>();
            foreach (var paper in papers)
            {
                var dList = decisionMap.TryGetValue(paper.Id, out var list) ? list : new List<ScreeningDecision>();
                var res = resolutionMap.TryGetValue(paper.Id, out var r) ? r : null;
                paperStatusMap[paper.Id] = ComputePaperStatus(dList, res, requiredReviewers, phase);
            }

            IEnumerable<Paper> filtered = papers;
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim().ToLowerInvariant();
                filtered = filtered.Where(p => p.Title.ToLowerInvariant().Contains(search));
            }

            if (request.Status.HasValue)
            {
                filtered = filtered.Where(p => paperStatusMap[p.Id] == request.Status.Value);
            }

            if (request.HasFullText.HasValue)
            {
                filtered = request.HasFullText.Value
                    ? filtered.Where(p => !string.IsNullOrWhiteSpace(p.PdfUrl) || !string.IsNullOrWhiteSpace(p.Url))
                    : filtered.Where(p => string.IsNullOrWhiteSpace(p.PdfUrl) && string.IsNullOrWhiteSpace(p.Url));
            }

            if (request.HasConflict.HasValue)
            {
                filtered = request.HasConflict.Value
                    ? filtered.Where(p => paperStatusMap[p.Id] == PaperSelectionStatus.Conflict)
                    : filtered.Where(p => paperStatusMap[p.Id] != PaperSelectionStatus.Conflict);
            }

            if (request.DecidedByReviewerId.HasValue)
            {
                var decidedPaperIds = phaseDecisions
                    .Where(d => d.ReviewerId == request.DecidedByReviewerId.Value)
                    .Select(d => d.PaperId)
                    .ToHashSet();
                filtered = filtered.Where(p => decidedPaperIds.Contains(p.Id));
            }

            filtered = request.SortBy switch
            {
                PaperSortBy.TitleDesc => filtered.OrderByDescending(p => p.Title),
                PaperSortBy.YearNewest => filtered.OrderByDescending(p => p.PublicationYearInt ?? 0).ThenBy(p => p.Title),
                PaperSortBy.YearOldest => filtered.OrderBy(p => p.PublicationYearInt ?? int.MaxValue).ThenBy(p => p.Title),
                PaperSortBy.RelevanceDesc => filtered.OrderByDescending(p =>
                {
                    var decisionCount = phaseDecisions.Count(d => d.PaperId == p.Id);
                    var hasConflict = paperStatusMap[p.Id] == PaperSelectionStatus.Conflict ? 1 : 0;
                    return hasConflict * 10000 + decisionCount;
                }).ThenBy(p => p.Title),
                _ => filtered.OrderBy(p => p.Title),
            };

            var filteredList = filtered.ToList();
            var totalCount = filteredList.Count;

            var pagedPapers = filteredList
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var pagedPaperIds = pagedPapers.Select(p => p.Id).ToList();
            var citationCounts = await _unitOfWork.PaperCitations.CountByTargetsAsync(pagedPaperIds, cancellationToken);
            var referenceCounts = await _unitOfWork.PaperCitations.CountBySourcesAsync(pagedPaperIds, cancellationToken);

            var items = new List<PaperWithDecisionsResponse>();
            foreach (var paper in pagedPapers)
            {
                var pDecisions = decisionMap.TryGetValue(paper.Id, out var list) ? list : new List<ScreeningDecision>();
                var pResolution = resolutionMap.TryGetValue(paper.Id, out var r) ? r : null;
                var citationCount = citationCounts.TryGetValue(paper.Id, out var cc) ? cc : 0;
                var referenceCount = referenceCounts.TryGetValue(paper.Id, out var rc) ? rc : 0;

                items.Add(await MapToPaperWithDecisionsResponseBatchAsync(paper, studySelectionProcessId, paperStatusMap[paper.Id], pDecisions, pResolution, citationCount, referenceCount, cancellationToken));
            }

            return new PaginatedResponse<PaperWithDecisionsResponse>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        private async Task<PaperWithDecisionsResponse> MapToPaperWithDecisionsResponseBatchAsync(
            Paper paper,
            Guid studySelectionProcessId,
            PaperSelectionStatus status,
            List<ScreeningDecision> decisions,
            ScreeningResolution? resolution,
            int citationCount,
            int referenceCount,
            CancellationToken cancellationToken,
            ExtractionSuggestionResponse? extractionSuggestion = null)
        {
            // Batch-resolve user names for decisions + resolution
            var allUserIds = decisions.Select(d => d.ReviewerId).ToList();
            if (resolution != null) allUserIds.Add(resolution.ResolvedBy);
            var userNames = await GetUserNamesAsync(allUserIds, cancellationToken);

            var decisionResponses = new List<ScreeningDecisionResponse>();
            foreach (var d in decisions)
            {
                decisionResponses.Add(await MapToDecisionResponse(d, paper.Title, userNames, cancellationToken));
            }

            // Deterministic final decision
            ScreeningDecisionType? finalDecision = null;
            if (resolution != null)
            {
                finalDecision = resolution.FinalDecision;
            }
            else if (status == PaperSelectionStatus.Included)
            {
                finalDecision = ScreeningDecisionType.Include;
            }
            else if (status == PaperSelectionStatus.Excluded)
            {
                finalDecision = ScreeningDecisionType.Exclude;
            }

            return new PaperWithDecisionsResponse
            {
                PaperId = paper.Id,
                Title = paper.Title,
                DOI = paper.DOI,
                Authors = paper.Authors,
                PublicationYear = paper.PublicationYear,
                PublicationDate = paper.PublicationYear, // Check mapping logic
                Abstract = paper.Abstract,
                Journal = paper.Journal,
                Source = paper.Source,
                Keywords = paper.Keywords,
                PublicationType = paper.PublicationType,
                Volume = paper.Volume,
                Issue = paper.Issue,
                Pages = paper.Pages,
                Publisher = paper.Publisher,
                Language = paper.Language,
                Url = paper.Url,
                PdfUrl = paper.PdfUrl,
                PdfFileName = paper.PdfFileName,
                ConferenceName = paper.ConferenceName,
                ConferenceLocation = paper.ConferenceLocation,
                JournalIssn = paper.JournalIssn,
                JournalEIssn = paper.JournalEIssn,
                Md5 = paper.Md5,
                Status = status,
                StatusText = status.ToString(),
                FinalDecision = finalDecision,
                FinalDecisionText = finalDecision?.ToString(),
                CitationCount = citationCount,
                ReferenceCount = referenceCount,
                Decisions = decisionResponses,
                Resolution = resolution != null
                    ? await MapToResolutionResponse(resolution, paper.Title, userNames, cancellationToken)
                    : null,
                Extraction = await GetExtractionStatusAsync(paper.Id, cancellationToken),
                MetadataSources = await GetMetadataSourcesAsync(paper.Id, cancellationToken),
                ExtractionResult = await GetExtractionResultAsync(paper, cancellationToken),
                ExtractionSuggestion = extractionSuggestion
            };
        }

        private async Task<PaperWithDecisionsResponse> MapToPaperWithDecisionsResponseAsync(
            Paper paper,
            Guid studySelectionProcessId,
            PaperSelectionStatus status,
            CancellationToken cancellationToken,
            ExtractionSuggestionResponse? extractionSuggestion = null)
        {
            var decisions = await _unitOfWork.ScreeningDecisions.GetByPaperAsync(
                studySelectionProcessId,
                paper.Id,
                null,
                cancellationToken);

            var resolution = await _unitOfWork.ScreeningResolutions.GetByProcessAndPaperAsync(
                studySelectionProcessId,
                paper.Id,
                null,
                cancellationToken);

            var citationCount = await _unitOfWork.PaperCitations.CountByTargetAsync(paper.Id, cancellationToken);
            var referenceCount = await _unitOfWork.PaperCitations.CountBySourceAsync(paper.Id, cancellationToken);

            // Batch-resolve user names for decisions + resolution
            var allUserIds = decisions.Select(d => d.ReviewerId).ToList();
            if (resolution != null) allUserIds.Add(resolution.ResolvedBy);
            var userNames = await GetUserNamesAsync(allUserIds, cancellationToken);

            var decisionResponses = new List<ScreeningDecisionResponse>();
            foreach (var d in decisions)
            {
                decisionResponses.Add(await MapToDecisionResponse(d, paper.Title, userNames, cancellationToken));
            }

            // Deterministic final decision
            ScreeningDecisionType? finalDecision = null;
            if (resolution != null)
            {
                finalDecision = resolution.FinalDecision;
            }
            else if (status == PaperSelectionStatus.Included)
            {
                finalDecision = ScreeningDecisionType.Include;
            }
            else if (status == PaperSelectionStatus.Excluded)
            {
                finalDecision = ScreeningDecisionType.Exclude;
            }

            return new PaperWithDecisionsResponse
            {
                PaperId = paper.Id,
                Title = paper.Title,
                DOI = paper.DOI,
                Authors = paper.Authors,
                PublicationYear = paper.PublicationYear,
                PublicationDate = paper.PublicationYear,
                Abstract = paper.Abstract,
                Journal = paper.Journal,
                Source = paper.Source,
                Keywords = paper.Keywords,
                PublicationType = paper.PublicationType,
                Volume = paper.Volume,
                Issue = paper.Issue,
                Pages = paper.Pages,
                Publisher = paper.Publisher,
                Language = paper.Language,
                Url = paper.Url,
                PdfUrl = paper.PdfUrl,
                PdfFileName = paper.PdfFileName,
                ConferenceName = paper.ConferenceName,
                ConferenceLocation = paper.ConferenceLocation,
                JournalIssn = paper.JournalIssn,
                JournalEIssn = paper.JournalEIssn,
                Md5 = paper.Md5,
                Status = status,
                StatusText = status.ToString(),
                FinalDecision = finalDecision,
                FinalDecisionText = finalDecision?.ToString(),
                CitationCount = citationCount,
                ReferenceCount = referenceCount,
                Decisions = decisionResponses,
                Resolution = resolution != null
                    ? await MapToResolutionResponse(resolution, paper.Title, userNames, cancellationToken)
                    : null,
                Extraction = await GetExtractionStatusAsync(paper.Id, cancellationToken),
                MetadataSources = await GetMetadataSourcesAsync(paper.Id, cancellationToken),
                ExtractionResult = await GetExtractionResultAsync(paper, cancellationToken),
                ExtractionSuggestion = extractionSuggestion
            };
        }



        private async Task<ExtractionResultResponse?> GetExtractionResultAsync(Paper paper, CancellationToken cancellationToken)
        {
            var grobidSource = await _unitOfWork.PaperSourceMetadatas.GetLatestWithGrobidHeaderByPaperIdAsync(
                paper.Id,
                cancellationToken);

            if (grobidSource == null) return null;

            return new ExtractionResultResponse
            {
                Title = grobidSource.Title,
                Authors = grobidSource.Authors,
                Abstract = grobidSource.Abstract,
                DOI = grobidSource.DOI,
                Journal = grobidSource.Journal,
                Volume = grobidSource.Volume,
                Issue = grobidSource.Issue,
                Pages = grobidSource.Pages,
                Keywords = grobidSource.Keywords,
                Publisher = grobidSource.Publisher,
                PublishedDate = grobidSource.PublishedDate,
                Year = grobidSource.Year,
                ISSN = grobidSource.ISSN,
                EISSN = grobidSource.EISSN,
                Language = grobidSource.Language,
                Md5 = grobidSource.Md5,
                UpdatedFields = grobidSource.AppliedFields ?? new List<string>()
            };
        }

        private async Task<string> GetUserNameAsync(Guid userId, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Users.FindSingleAsync(
                u => u.Id == userId,
                isTracking: false,
                cancellationToken);
            return user?.FullName ?? string.Empty;
        }

        private async Task<Dictionary<Guid, string>> GetUserNamesAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken)
        {
            var distinctIds = userIds.Distinct().ToList();
            var result = new Dictionary<Guid, string>();
            foreach (var id in distinctIds)
            {
                result[id] = await GetUserNameAsync(id, cancellationToken);
            }
            return result;
        }

        private async Task<ScreeningDecisionResponse> MapToDecisionResponse(
            ScreeningDecision d, string paperTitle, Dictionary<Guid, string>? userNames = null, CancellationToken cancellationToken = default)
        {
            var reviewerName = userNames != null && userNames.TryGetValue(d.ReviewerId, out var name)
                ? name
                : await GetUserNameAsync(d.ReviewerId, cancellationToken);

            return new ScreeningDecisionResponse
            {
                Id = d.Id,
                StudySelectionProcessId = d.StudySelectionProcessId,
                PaperId = d.PaperId,
                PaperTitle = paperTitle,
                ReviewerId = d.ReviewerId,
                ReviewerName = reviewerName,
                Decision = d.Decision,
                DecisionText = d.Decision.ToString(),
                Phase = d.Phase,
                PhaseText = d.Phase.ToString(),
                ExclusionReasonCode = d.ExclusionReasonCode,
                Reason = d.Reason,
                ReviewerNotes = d.ReviewerNotes,
                DecidedAt = d.DecidedAt
            };
        }

        private async Task<ScreeningResolutionResponse> MapToResolutionResponse(
            ScreeningResolution resolution, string paperTitle, Dictionary<Guid, string>? userNames = null, CancellationToken cancellationToken = default)
        {
            var resolverName = userNames != null && userNames.TryGetValue(resolution.ResolvedBy, out var name)
                ? name
                : await GetUserNameAsync(resolution.ResolvedBy, cancellationToken);

            return new ScreeningResolutionResponse
            {
                Id = resolution.Id,
                StudySelectionProcessId = resolution.StudySelectionProcessId,
                PaperId = resolution.PaperId,
                PaperTitle = paperTitle,
                FinalDecision = resolution.FinalDecision,
                FinalDecisionText = resolution.FinalDecision.ToString(),
                Phase = resolution.Phase,
                PhaseText = resolution.Phase.ToString(),
                ResolutionNotes = resolution.ResolutionNotes,
                ResolvedBy = resolution.ResolvedBy,
                ResolverName = resolverName,
                ResolvedAt = resolution.ResolvedAt
            };
        }

        private static StudySelectionProcessResponse MapToResponse(StudySelectionProcess process)
        {
            var response = new StudySelectionProcessResponse
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

            if (process.TitleAbstractScreening != null)
            {
                response.TitleAbstractScreening = MapToTAResponse(process.TitleAbstractScreening);
            }

            return response;
        }

        private static TitleAbstractScreeningResponse MapToTAResponse(TitleAbstractScreening ta)
        {
            return new TitleAbstractScreeningResponse
            {
                Id = ta.Id,
                StudySelectionProcessId = ta.StudySelectionProcessId,
                Status = ta.Status,
                StatusText = ta.Status.ToString(),
                StartedAt = ta.StartedAt,
                CompletedAt = ta.CompletedAt,
                MinReviewersPerPaper = ta.MinReviewersPerPaper,
                MaxReviewersPerPaper = ta.MaxReviewersPerPaper,
                CreatedAt = ta.CreatedAt,
                ModifiedAt = ta.ModifiedAt
            };
        }

        // ============================================
        // Title-Abstract Screening Lifecycle
        // ============================================

        public async Task<TitleAbstractScreeningResponse> CreateTitleAbstractScreeningAsync(
            Guid studySelectionProcessId,
            CancellationToken cancellationToken = default)
        {
            var process = await _unitOfWork.StudySelectionProcesses.FindSingleAsync(
                ssp => ssp.Id == studySelectionProcessId,
                cancellationToken: cancellationToken);

            if (process == null)
            {
                throw new InvalidOperationException($"StudySelectionProcess with ID {studySelectionProcessId} not found.");
            }

            // Check if TA screening already exists
            var existing = await _unitOfWork.TitleAbstractScreenings.GetByProcessIdAsync(
                studySelectionProcessId, cancellationToken);

            if (existing != null)
            {
                throw new InvalidOperationException("Title/Abstract screening already exists for this process.");
            }

            var taScreening = new TitleAbstractScreening
            {
                Id = Guid.NewGuid(),
                StudySelectionProcessId = studySelectionProcessId,
                Status = ScreeningPhaseStatus.NotStarted,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.TitleAbstractScreenings.AddAsync(taScreening, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToTAResponse(taScreening);
        }

        public async Task<TitleAbstractScreeningResponse> StartTitleAbstractScreeningAsync(
            Guid studySelectionProcessId,
            CancellationToken cancellationToken = default)
        {
            var taScreening = await _unitOfWork.TitleAbstractScreenings.FindSingleAsync(
                ta => ta.StudySelectionProcessId == studySelectionProcessId,
                isTracking: true,
                cancellationToken);

            if (taScreening == null)
            {
                throw new InvalidOperationException($"Title/Abstract screening not found for process {studySelectionProcessId}.");
            }

            // Load ReviewProcess with Protocol for validation
            var process = await _unitOfWork.StudySelectionProcesses.FindSingleAsync(
                ssp => ssp.Id == studySelectionProcessId,
                isTracking: false,
                cancellationToken);

            if (process == null)
            {
                throw new InvalidOperationException($"StudySelectionProcess with ID {studySelectionProcessId} not found.");
            }

            var reviewProcess = await _unitOfWork.ReviewProcesses.FindSingleAsync(
                rp => rp.Id == process.ReviewProcessId,
                isTracking: true,
                cancellationToken);

            if (reviewProcess == null)
            {
                throw new InvalidOperationException("ReviewProcess not found.");
            }

            // Load protocol for validation (G-04, G-12)
            if (reviewProcess.ProtocolId.HasValue)
            {
                var protocol = await _unitOfWork.Protocols.FindSingleAsync(
                    p => p.Id == reviewProcess.ProtocolId.Value,
                    isTracking: true,
                    cancellationToken);

                reviewProcess.Protocol = protocol;
            }

            process.ReviewProcess = reviewProcess;



            // Lock protocol once screening starts (G-04)
            if (reviewProcess.Protocol != null && reviewProcess.Protocol.Status == ProtocolStatus.Approved)
            {
                reviewProcess.Protocol.Lock();
            }

            // Validate paper metadata (G-07)
            var eligiblePaperIds = await GetEligiblePapersAsync(studySelectionProcessId, cancellationToken);
            var metadataWarnings = new List<string>();
            foreach (var paperId in eligiblePaperIds)
            {
                var paper = await _unitOfWork.Papers.FindSingleAsync(p => p.Id == paperId, cancellationToken: cancellationToken);
                if (paper != null)
                {
                    try
                    {
                        TitleAbstractScreening.ValidatePaperMetadata(paper);
                    }
                    catch (InvalidOperationException)
                    {
                        // Collect warnings but don't block screening start
                        metadataWarnings.Add($"Paper '{paper.Title}' (ID: {paper.Id}) has incomplete metadata.");
                    }
                }
            }

            // Start the TA screening phase
            taScreening.Start();

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToTAResponse(taScreening);
        }

        public async Task<TitleAbstractScreeningResponse> CompleteTitleAbstractScreeningAsync(
            Guid studySelectionProcessId,
            CancellationToken cancellationToken = default)
        {
            var taScreening = await _unitOfWork.TitleAbstractScreenings.FindSingleAsync(
                ta => ta.StudySelectionProcessId == studySelectionProcessId,
                isTracking: true,
                cancellationToken);

            if (taScreening == null)
            {
                throw new InvalidOperationException($"Title/Abstract screening not found for process {studySelectionProcessId}.");
            }

            // Check for unresolved conflicts before completing
            var conflictedPapers = await _unitOfWork.ScreeningDecisions.GetPapersWithConflictsAsync(
                studySelectionProcessId, ScreeningPhase.TitleAbstract, cancellationToken);
            var resolvedPaperIds = (await _unitOfWork.ScreeningResolutions.GetByProcessAsync(
                studySelectionProcessId, ScreeningPhase.TitleAbstract, cancellationToken))
                .Select(sr => sr.PaperId)
                .ToList();

            var unresolvedConflicts = conflictedPapers.Except(resolvedPaperIds).ToList();
            if (unresolvedConflicts.Any())
            {
                throw new InvalidOperationException(
                    $"Cannot complete TA screening with {unresolvedConflicts.Count} unresolved conflicts.");
            }

            taScreening.Complete();

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToTAResponse(taScreening);
        }

        public async Task<TitleAbstractScreeningResponse> GetTitleAbstractScreeningAsync(
            Guid studySelectionProcessId,
            CancellationToken cancellationToken = default)
        {
            var taScreening = await _unitOfWork.TitleAbstractScreenings.GetByProcessIdAsync(
                studySelectionProcessId, cancellationToken);

            if (taScreening == null)
            {
                throw new InvalidOperationException($"Title/Abstract screening not found for process {studySelectionProcessId}.");
            }

            return MapToTAResponse(taScreening);
        }

        // ============================================
        // Issue 2: Full-Text Upload/Link Management
        // ============================================

        public async Task<PaperWithDecisionsResponse> UpdatePaperFullTextAsync(
            Guid studySelectionProcessId,
            Guid paperId,
            UpdatePaperFullTextRequest request,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.PdfUrl) && string.IsNullOrWhiteSpace(request.Url))
            {
                throw new ArgumentException("At least one of PdfUrl or Url must be provided.");
            }

            // Validate process exists
            var process = await _unitOfWork.StudySelectionProcesses.FindSingleAsync(
                ssp => ssp.Id == studySelectionProcessId,
                cancellationToken: cancellationToken);

            if (process == null)
            {
                throw new InvalidOperationException($"StudySelectionProcess with ID {studySelectionProcessId} not found.");
            }

            // Validate paper exists and is eligible
            var paper = await _unitOfWork.Papers.FindSingleAsync(
                p => p.Id == paperId,
                isTracking: true,
                cancellationToken);

            if (paper == null)
            {
                throw new InvalidOperationException($"Paper with ID {paperId} not found.");
            }

            // Update full-text fields
            if (!string.IsNullOrWhiteSpace(request.PdfUrl))
            {
                paper.PdfUrl = request.PdfUrl;
            }

            if (!string.IsNullOrWhiteSpace(request.PdfFileName))
            {
                paper.PdfFileName = request.PdfFileName;
            }

            if (!string.IsNullOrWhiteSpace(request.Url))
            {
                paper.Url = request.Url;
            }

            paper.ModifiedAt = DateTimeOffset.UtcNow;

            ExtractionSuggestionResponse? extractionSuggestion = null;

            // Grobid Integration
            PaperPdf? paperPdf = null;
            if (!string.IsNullOrWhiteSpace(request.PdfUrl))
            {
                paperPdf = new PaperPdf
                {
                    Id = Guid.NewGuid(),
                    PaperId = paperId,
                    FilePath = request.PdfUrl,
                    FileName = request.PdfFileName ?? string.Empty,
                    UploadedAt = DateTimeOffset.UtcNow,
                    GrobidProcessed = request.ExtractWithGrobid,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                };
                await _unitOfWork.PaperPdfs.AddAsync(paperPdf, cancellationToken);
            }
            Console.WriteLine("ExtractWithGrobid: " + request.ExtractWithGrobid);
            Console.WriteLine("PdfStream: " + request.PdfStream);
            Console.WriteLine("PaperPdf: " + paperPdf);
            if (request.ExtractWithGrobid && request.PdfStream != null && paperPdf != null)
            {
                _logger.LogInformation("Starting GROBID metadata extraction for Paper {PaperId}", paperId);
                var grobidDto = await _grobidService.ExtractHeaderAsync(request.PdfStream, request.PdfFileName ?? "upload.pdf", cancellationToken);

                var grobidDoi = grobidDto.DOI?.Replace("https://doi.org/", "").Replace("http://doi.org/", "").Trim();
                var paperDoi = paper.DOI?.Replace("https://doi.org/", "").Replace("http://doi.org/", "").Trim();

                if (!string.IsNullOrWhiteSpace(grobidDoi) &&
                    !string.IsNullOrWhiteSpace(paperDoi) &&
                    !string.Equals(grobidDoi, paperDoi, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException($"The DOI in the PDF ({grobidDto.DOI}) does not match the paper's current DOI ({paper.DOI}).");
                }

                if (!string.IsNullOrWhiteSpace(grobidDto.RawXml))
                {
                    var headerResult = new GrobidHeaderResult
                    {
                        Id = Guid.NewGuid(),
                        PaperPdfId = paperPdf.Id,
                        Title = grobidDto.Title,
                        Authors = grobidDto.Authors,
                        Abstract = grobidDto.Abstract,
                        DOI = grobidDto.DOI,
                        Journal = grobidDto.Journal,
                        Volume = grobidDto.Volume,
                        Issue = grobidDto.Issue,
                        Pages = grobidDto.Pages,
                        RawXml = grobidDto.RawXml,
                        ExtractedAt = DateTimeOffset.UtcNow,
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = DateTimeOffset.UtcNow
                    };
                    await _unitOfWork.GrobidHeaderResults.AddAsync(headerResult, cancellationToken);

                    var sourceMeta = new PaperSourceMetadata
                    {
                        Id = Guid.NewGuid(),
                        PaperId = paperId,
                        Source = MetadataSource.GROBID_HEADER,
                        Title = grobidDto.Title,
                        Authors = grobidDto.Authors,
                        Abstract = grobidDto.Abstract,
                        DOI = grobidDto.DOI,
                        Journal = grobidDto.Journal,
                        Volume = grobidDto.Volume,
                        Issue = grobidDto.Issue,
                        Pages = grobidDto.Pages,
                        Publisher = grobidDto.Publisher,
                        PublishedDate = grobidDto.PublishedDate?.ToString("yyyy-MM-dd"),
                        Year = grobidDto.Year,
                        ISSN = grobidDto.ISSN,
                        EISSN = grobidDto.EISSN,
                        Keywords = grobidDto.Keywords,
                        Language = grobidDto.Language,
                        Md5 = grobidDto.Md5,
                        ExtractedAt = DateTimeOffset.UtcNow,
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = DateTimeOffset.UtcNow
                    };
                    await _unitOfWork.PaperSourceMetadatas.AddAsync(sourceMeta, cancellationToken);

                    // REMOVED automatic merge to preserve canonical Paper metadata
                    // await _metadataMergeService.MergeAsync(paper, sourceMeta);

                    // Issue 3: Include ExtractionResult payloads dynamically based on modifications
                    var extractionResult = new ExtractionResultResponse
                    {
                        Title = grobidDto.Title,
                        Authors = grobidDto.Authors,
                        Abstract = grobidDto.Abstract,
                        DOI = grobidDto.DOI,
                        Journal = grobidDto.Journal,
                        Volume = grobidDto.Volume,
                        Issue = grobidDto.Issue,
                        Pages = grobidDto.Pages,
                        Publisher = grobidDto.Publisher,
                        PublishedDate = grobidDto.PublishedDate?.ToString("yyyy-MM-dd"),
                        Year = grobidDto.Year,
                        ISSN = grobidDto.ISSN,
                        EISSN = grobidDto.EISSN,
                        Keywords = grobidDto.Keywords,
                        Language = grobidDto.Language,
                        Md5 = grobidDto.Md5,
                        UpdatedFields = new List<string>() // Idealy from MetadataMerge logic, keeping it simple for now
                    };

                    if (!string.IsNullOrEmpty(grobidDto.Title)) extractionResult.UpdatedFields.Add("Title");
                    if (!string.IsNullOrEmpty(grobidDto.Authors)) extractionResult.UpdatedFields.Add("Authors");
                    if (!string.IsNullOrEmpty(grobidDto.Abstract)) extractionResult.UpdatedFields.Add("Abstract");
                    if (!string.IsNullOrEmpty(grobidDto.DOI)) extractionResult.UpdatedFields.Add("DOI");
                    if (!string.IsNullOrEmpty(grobidDto.Journal)) extractionResult.UpdatedFields.Add("Journal");
                    if (!string.IsNullOrEmpty(grobidDto.Volume)) extractionResult.UpdatedFields.Add("Volume");
                    if (!string.IsNullOrEmpty(grobidDto.Issue)) extractionResult.UpdatedFields.Add("Issue");
                    if (!string.IsNullOrEmpty(grobidDto.Pages)) extractionResult.UpdatedFields.Add("Pages");
                    if (!string.IsNullOrEmpty(grobidDto.Keywords)) extractionResult.UpdatedFields.Add("Keywords");
                    if (!string.IsNullOrEmpty(grobidDto.Language)) extractionResult.UpdatedFields.Add("Language");
                    if (!string.IsNullOrEmpty(grobidDto.Md5)) extractionResult.UpdatedFields.Add("Md5");
                    if (!string.IsNullOrEmpty(grobidDto.Publisher)) extractionResult.UpdatedFields.Add("Publisher");
                    if (!string.IsNullOrEmpty(grobidDto.PublishedDate?.ToString())) extractionResult.UpdatedFields.Add("PublishedDate");
                    if (!string.IsNullOrEmpty(grobidDto.Year?.ToString())) extractionResult.UpdatedFields.Add("Year");
                    if (!string.IsNullOrEmpty(grobidDto.ISSN)) extractionResult.UpdatedFields.Add("ISSN");
                    if (!string.IsNullOrEmpty(grobidDto.EISSN)) extractionResult.UpdatedFields.Add("EISSN");

                    // Create extraction suggestion for the frontend
                    var suggestion = new ExtractionSuggestionResponse
                    {
                        SourceMetadataId = sourceMeta.Id,
                        Title = sourceMeta.Title,
                        Authors = sourceMeta.Authors,
                        Abstract = sourceMeta.Abstract,
                        DOI = sourceMeta.DOI,
                        Language = sourceMeta.Language,
                        Journal = sourceMeta.Journal,
                        Volume = sourceMeta.Volume,
                        Issue = sourceMeta.Issue,
                        Pages = sourceMeta.Pages,
                        Keywords = sourceMeta.Keywords,
                        Publisher = sourceMeta.Publisher,
                        Year = sourceMeta.Year,
                        Md5 = sourceMeta.Md5,
                        ISSN = sourceMeta.ISSN,
                        EISSN = sourceMeta.EISSN
                    };

                    _logger.LogInformation("Created extraction suggestion {SourceMetadataId} for Paper {PaperId}", sourceMeta.Id, paperId);

                    // Note: In an actual implementation with proper design, we would pass this suggestion nicely through the return DTO
                    // For now, returning it by packing it in via context or similar depending on the existing architecture.
                    // Let's modify the PaperWithDecisionsResponse return directly
                    extractionSuggestion = suggestion;

                    // Stash it in HttpContext.Items to be mapped into the response DTO
                    // Not ideal domain design, but simplest for gap report resolution
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var status = await GetPaperSelectionStatusAsync(studySelectionProcessId, paperId, cancellationToken);

            return await MapToPaperWithDecisionsResponseAsync(paper, studySelectionProcessId, status, cancellationToken, extractionSuggestion);
        }

        public async Task<PaperWithDecisionsResponse> RetryMetadataExtractionAsync(
            Guid studySelectionProcessId,
            Guid paperId,
            RetryExtractionRequest request,
            CancellationToken cancellationToken = default)
        {
            var process = await _unitOfWork.StudySelectionProcesses.FindSingleAsync(
                ssp => ssp.Id == studySelectionProcessId,
                cancellationToken: cancellationToken);

            if (process == null)
            {
                throw new InvalidOperationException($"StudySelectionProcess with ID {studySelectionProcessId} not found.");
            }

            var paper = await _unitOfWork.Papers.FindSingleAsync(
                p => p.Id == paperId,
                isTracking: true,
                cancellationToken);

            if (paper == null)
            {
                throw new InvalidOperationException($"Paper with ID {paperId} not found.");
            }

            if (request.Provider != "GROBID")
            {
                throw new ArgumentException("Only GROBID provider is currently supported for retry.", nameof(request.Provider));
            }

            var paperPdf = await _unitOfWork.PaperPdfs.FindSingleAsync(
                p => p.PaperId == paperId,
                isTracking: true,
                cancellationToken: cancellationToken);

            if (paperPdf == null || string.IsNullOrWhiteSpace(paperPdf.FilePath))
            {
                throw new InvalidOperationException("No PDF file associated with this paper to extract metadata from.");
            }

            // We normally need the physical file stream here
            // But doing this reliably across distributed storage requires downloading it from Supabase
            // Since this involves new architecture logic, we simulate completion semantics.
            throw new NotImplementedException("Retry metadata downloading feature requires Supabase integration in service tier.");
        }

        private async Task<ExtractionStatusResponse?> GetExtractionStatusAsync(Guid paperId, CancellationToken cancellationToken)
        {
            var paperPdf = await _unitOfWork.PaperPdfs.GetLatestPaperPdfAsync(paperId, cancellationToken);
            if (paperPdf == null) return null;
            var headerResult = await _unitOfWork.GrobidHeaderResults.GetLatestGrobidHeaderResultAsync(paperPdf.Id, cancellationToken);

            if (!paperPdf.GrobidProcessed)
            {
                return new ExtractionStatusResponse
                {
                    Requested = false,
                    Status = "not_requested"
                };
            }

            if (headerResult != null)
            {
                return new ExtractionStatusResponse
                {
                    Requested = true,
                    Provider = "GROBID",
                    Status = "succeeded"
                };
            }

            return new ExtractionStatusResponse
            {
                Requested = true,
                Provider = "GROBID",
                Status = "failed",
                Message = "Extraction failed or is pending."
            };
        }

        private async Task<MetadataSourcesResponse?> GetMetadataSourcesAsync(Guid paperId, CancellationToken cancellationToken)
        {
            var sources = await _unitOfWork.PaperSourceMetadatas.FindAllAsync(s => s.PaperId == paperId, isTracking: false, cancellationToken: cancellationToken);
            if (sources == null || !sources.Any()) return null;

            var grobidSource = sources.FirstOrDefault(s => s.Source == MetadataSource.GROBID_HEADER);
            var risSource = sources.FirstOrDefault(s => s.Source == MetadataSource.RIS); // Assuming RIS is the default/other source

            // Simplified logic: If we have GROBID metadata, we assume it contributed to the fields it has. 
            // The actual Merge logic in IMetadataMergeService decides this exactly.
            // A perfect implementation would track per-field provenance during the merge.
            // For now, we infer based on existence.

            return new MetadataSourcesResponse
            {
                Title = grobidSource?.Title != null ? "GROBID" : (risSource != null ? "RIS" : "MANUAL"),
                Authors = grobidSource?.Authors != null ? "GROBID" : (risSource != null ? "RIS" : "MANUAL"),
                Abstract = grobidSource?.Abstract != null ? "GROBID" : (risSource != null ? "RIS" : "MANUAL"),
                DOI = grobidSource?.DOI != null ? "GROBID" : (risSource != null ? "RIS" : "MANUAL"),
                Journal = grobidSource?.Journal != null ? "GROBID" : (risSource != null ? "RIS" : "MANUAL"),
                Publisher = grobidSource?.Publisher != null ? "GROBID" : (risSource != null ? "RIS" : "MANUAL"),
                PublishedDate = grobidSource?.PublishedDate != null ? "GROBID" : (risSource != null ? "RIS" : "MANUAL"),
                Year = grobidSource?.Year != null ? "GROBID" : (risSource != null ? "RIS" : "MANUAL"),
                ISSN = grobidSource?.ISSN != null ? "GROBID" : (risSource != null ? "RIS" : "MANUAL"),
                EISSN = grobidSource?.EISSN != null ? "GROBID" : (risSource != null ? "RIS" : "MANUAL"),
                Language = grobidSource?.Language != null ? "GROBID" : (risSource != null ? "RIS" : "MANUAL"),
                Md5 = grobidSource?.Md5 != null ? "GROBID" : (risSource != null ? "RIS" : "MANUAL"),
                Volume = grobidSource?.Volume != null ? "GROBID" : (risSource != null ? "RIS" : "MANUAL"),
                Issue = grobidSource?.Issue != null ? "GROBID" : (risSource != null ? "RIS" : "MANUAL"),
                Pages = grobidSource?.Pages != null ? "GROBID" : (risSource != null ? "RIS" : "MANUAL"),
                Keywords = grobidSource?.Keywords != null ? "GROBID" : (risSource != null ? "RIS" : "MANUAL"),
            };
        }

        // ============================================
        // Auto-Resolution Logic (G-05, G-06)
        // ============================================

        private async Task TryAutoResolveAsync(
            Guid studySelectionProcessId,
            Guid paperId,
            ScreeningPhase phase,
            CancellationToken cancellationToken = default)
        {
            // 1. Nếu đã có resolution → không xử lý nữa
            var existingResolution = await _unitOfWork.ScreeningResolutions
                .GetByProcessAndPaperAsync(studySelectionProcessId, paperId, phase, cancellationToken);

            if (existingResolution != null) return;

            // 2. Lấy assignments
            var assignments = await _unitOfWork.PaperAssignments.FindAllAsync(
                pa => pa.StudySelectionProcessId == studySelectionProcessId
                      && pa.PaperId == paperId
                      && pa.Phase == phase,
                isTracking: false,
                cancellationToken: cancellationToken);

            var assignmentList = assignments.ToList();
            int assignedCount = assignmentList.Count;

            // 3. Chỉ cho phép auto khi đúng 2 reviewers (initial round)
            if (assignedCount != 2) return;

            // 4. Lấy decisions
            var decisions = await _unitOfWork.ScreeningDecisions.GetByPaperAsync(
                studySelectionProcessId, paperId, phase, cancellationToken);

            // 5. Chưa review đủ → không auto
            if (decisions.Count < assignedCount) return;

            // 6. Đếm decision
            var includeCount = decisions.Count(d => d.Decision == ScreeningDecisionType.Include);
            var excludeCount = decisions.Count(d => d.Decision == ScreeningDecisionType.Exclude);

            // 7. Nếu có conflict → KHÔNG auto (disable forever)
            bool hasConflict = includeCount > 0 && excludeCount > 0;
            if (hasConflict) return;

            // 8. Chỉ auto khi unanimous (2/2)
            ScreeningDecisionType? autoDecision = null;
            string? autoNotes = null;

            if (includeCount == 2)
            {
                autoDecision = ScreeningDecisionType.Include;
                autoNotes = "Auto-resolved: unanimous Include (2/2 reviewers).";
            }
            else if (excludeCount == 2)
            {
                autoDecision = ScreeningDecisionType.Exclude;
                autoNotes = "Auto-resolved: unanimous Exclude (2/2 reviewers).";
            }
            else
            {
                return;
            }

            // 9. Tạo resolution
            var resolution = new ScreeningResolution
            {
                Id = Guid.NewGuid(),
                StudySelectionProcessId = studySelectionProcessId,
                PaperId = paperId,
                FinalDecision = autoDecision.Value,
                Phase = phase,
                ResolutionNotes = autoNotes,
                ResolvedBy = Guid.Empty, // System
                ResolvedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.ScreeningResolutions.AddAsync(resolution, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 10. Send notifications
            try
            {
                var paper = await _unitOfWork.Papers.FindSingleAsync(p => p.Id == paperId, cancellationToken: cancellationToken);

                var ssp = await _unitOfWork.StudySelectionProcesses.FindSingleAsync(s => s.Id == studySelectionProcessId, cancellationToken: cancellationToken);
                if (ssp != null)
                {
                    var rp = await _unitOfWork.ReviewProcesses.FindSingleAsync(r => r.Id == ssp.ReviewProcessId, cancellationToken: cancellationToken);
                    if (rp != null)
                    {
                        var projectMembers = await _unitOfWork.SystematicReviewProjects.GetMembersByProjectIdAsync(rp.ProjectId);
                        var memberToUserMap = projectMembers.ToDictionary(m => m.Id, m => m.UserId);

                        var assignedUserIds = assignmentList
                            .Select(a => memberToUserMap.TryGetValue(a.ProjectMemberId, out var uid) ? uid : (Guid?)null)
                            .Where(uid => uid.HasValue)
                            .Select(uid => uid!.Value)
                            .Distinct()
                            .ToList();

                        if (assignedUserIds.Any())
                        {
                            var phaseName = phase == ScreeningPhase.TitleAbstract ? "Title/Abstract" : "Full-Text";
                            var title = "Paper Auto-Resolved";
                            var message = $"The paper '{paper?.Title}' in the {phaseName} phase has been auto-resolved based on unanimous reviewer decisions. Final decision: {autoDecision}.";

                            await _notificationService.SendToManyAsync(
                                assignedUserIds,
                                title,
                                message,
                                NotificationType.Review,
                                paperId,
                                NotificationEntityType.PaperAssignment);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send auto-resolution notifications for paper {PaperId}", paperId);
                // Don't throw - notification failure shouldn't break resolution success
            }
        }

        public async Task<PaperWithDecisionsResponse> GetPaperDetailsAsync(Guid studySelectionProcessId, Guid paperId, CancellationToken cancellationToken = default)
        {
            var studySelectionProcess = await _unitOfWork.StudySelectionProcesses.FindSingleAsync(
                ssp => ssp.Id == studySelectionProcessId,
                isTracking: false,
                cancellationToken);

            if (studySelectionProcess == null)
            {
                throw new InvalidOperationException($"StudySelectionProcess with ID {studySelectionProcessId} not found.");
            }

            var paper = await _unitOfWork.Papers.FindSingleAsync(
                p => p.Id == paperId,
                isTracking: false,
                cancellationToken);

            if (paper == null)
            {
                throw new InvalidOperationException($"Paper with ID {paperId} not found.");
            }

            var status = await GetPaperSelectionStatusAsync(studySelectionProcessId, paperId, cancellationToken);

            return await MapToPaperWithDecisionsResponseAsync(
                paper,
                studySelectionProcessId,
                status,
                cancellationToken);
        }
    }
}