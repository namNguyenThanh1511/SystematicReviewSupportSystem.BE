using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.Common;
using SRSS.IAM.Services.DTOs.StudySelection;
using SRSS.IAM.Services.GrobidClient;
using SRSS.IAM.Services.GrobidClient.DTOs;
using SRSS.IAM.Services.MetadataMergeService;
using SRSS.IAM.Services.NotificationService;
using SRSS.IAM.Services.StudySelectionProcessPaperService;
using SRSS.IAM.Services.Utils;
using SRSS.IAM.Services.PaperFullTextService;
using SRSS.IAM.Services.SupabaseService;
using SRSS.IAM.Services.UserService;
using SRSS.IAM.Services.DTOs.Paper;

namespace SRSS.IAM.Services.StudySelectionService
{
    public class StudySelectionService : IStudySelectionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGrobidService _grobidService;
        private readonly IMetadataMergeService _metadataMergeService;
        private readonly INotificationService _notificationService;
        private readonly IStudySelectionProcessPaperService _studySelectionProcessPaperService;
        private readonly IPaperFullTextQueue _fullTextQueue;
        private readonly IGrobidProcessingQueue _grobidQueue;
        private readonly ISupabaseStorageService _storageService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<StudySelectionService> _logger;
        private readonly SRSS.IAM.Services.RagService.IRagIngestionQueue _ragQueue;

        public StudySelectionService(
            IUnitOfWork unitOfWork,
            IGrobidService grobidService,
            IMetadataMergeService metadataMergeService,
            INotificationService notificationService,
            IStudySelectionProcessPaperService studySelectionProcessPaperService,
            IPaperFullTextQueue fullTextQueue,
            IGrobidProcessingQueue grobidQueue,
            ISupabaseStorageService storageService,
            ICurrentUserService currentUserService,
            ILogger<StudySelectionService> logger,
            SRSS.IAM.Services.RagService.IRagIngestionQueue ragQueue)
        {
            _unitOfWork = unitOfWork;
            _grobidService = grobidService;
            _metadataMergeService = metadataMergeService;
            _notificationService = notificationService;
            _studySelectionProcessPaperService = studySelectionProcessPaperService;
            _fullTextQueue = fullTextQueue;
            _grobidQueue = grobidQueue;
            _storageService = storageService;
            _currentUserService = currentUserService;
            _logger = logger;
            _ragQueue = ragQueue;
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

            // Maintain legacy field for backward compatibility - now represents Overall statistics
            var overallIncluded = studySelectionProcessResponse.PhaseStatistics.FullText.IncludedCount;
            var overallExcluded = studySelectionProcessResponse.PhaseStatistics.TitleAbstract.ExcludedCount + studySelectionProcessResponse.PhaseStatistics.FullText.ExcludedCount;
            var overallConflict = studySelectionProcessResponse.PhaseStatistics.TitleAbstract.ConflictCount + studySelectionProcessResponse.PhaseStatistics.FullText.ConflictCount;
            var overallPending = studySelectionProcessResponse.PhaseStatistics.TitleAbstract.PendingCount + studySelectionProcessResponse.PhaseStatistics.FullText.PendingCount;

            var decided = overallIncluded + overallExcluded + overallConflict;
            var total = studySelectionProcessResponse.PhaseStatistics.TitleAbstract.TotalPapers;

            studySelectionProcessResponse.SelectionStatistics = new SelectionStatisticsResponse
            {
                StudySelectionProcessId = id,
                TotalPapers = total,
                IncludedCount = overallIncluded,
                ExcludedCount = overallExcluded,
                ConflictCount = overallConflict,
                PendingCount = overallPending,
                CompletionPercentage = total > 0 ? Math.Round((double)decided / total * 100, 2) : 0,
                ExclusionReasonBreakdown = studySelectionProcessResponse.PhaseStatistics.TitleAbstract.ExclusionReasonBreakdown
                    .Concat(studySelectionProcessResponse.PhaseStatistics.FullText.ExclusionReasonBreakdown)
                    .GroupBy(x => new { x.ReasonCode, x.ReasonText })
                    .Select(g => new ExclusionReasonBreakdownItem
                    {
                        ReasonCode = g.Key.ReasonCode,
                        ReasonText = g.Key.ReasonText,
                        Count = g.Sum(x => x.Count)
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList()
            };

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

            // Check if study selection criteria exists for this process
            var hasCriteria = await _unitOfWork.SelectionCriterias.AnyAsync(
                c => c.StudySelectionProcessId == id,
                cancellationToken: cancellationToken);

            if (!hasCriteria)
            {
                return new StudySelectionProcessResponse
                {
                    IsHaveCriteria = false
                };
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

            var response = MapToResponse(process);
            response.IsHaveCriteria = true;
            return response;
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

            // ==========================================
            // KÍCH HOẠT RAG INGESTION TẠI ĐÂY
            // Lấy danh sách Paper đã PASS vòng Full-Text Screening
            // ==========================================
            var eligiblePapers = await _unitOfWork.StudySelectionProcessPapers.GetWithPaperByProcessAsync(id, cancellationToken);
            foreach (var item in eligiblePapers)
            {
                if (!string.IsNullOrWhiteSpace(item.Paper?.PdfUrl))
                {
                    await _ragQueue.QueuePaperForIngestionAsync(item.PaperId, item.Paper.PdfUrl, cancellationToken);
                }
            }

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
            if (request.Decision == ScreeningDecisionType.Exclude && request.ExclusionReasonId == null)
            {
                throw new ArgumentException("ExclusionReasonId is required when decision is Exclude.");
            }

            // Find checklist submission for this context if not provided
            var checklistSubmissionId = request.ChecklistSubmissionId;
            if (checklistSubmissionId == null)
            {
                var submission = await _unitOfWork.StudySelectionChecklistSubmissions.GetByContextWithAnswersAsync(
                    studySelectionProcessId,
                    paperId,
                    request.ReviewerId,
                    request.Phase,
                    cancellationToken);
                checklistSubmissionId = submission?.Id;
            }

            if (checklistSubmissionId == null)
            {
                throw new InvalidOperationException("Cannot make a screening decision without a checklist submission. Please complete the checklist first.");
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
                ExclusionReasonId = request.ExclusionReasonId,
                Reason = request.Reason,
                DecidedAt = DateTimeOffset.UtcNow,
                ChecklistSubmissionId = checklistSubmissionId,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.ScreeningDecisions.AddAsync(decision, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Auto-resolution: check if all reviewers have decided and create resolution automatically (G-05, G-06)
            await TryAutoResolveAsync(studySelectionProcessId, paperId, request.Phase, cancellationToken);

            return await MapToDecisionResponse(decision, paper?.Title ?? string.Empty, cancellationToken: cancellationToken);
        }



        private async Task ValidateChecklistCompletionAsync(Guid submissionId, CancellationToken cancellationToken)
        {
            var submission = await _unitOfWork.StudySelectionChecklistSubmissions.FindSingleAsync(s => s.Id == submissionId, cancellationToken: cancellationToken);
            if (submission == null) return;

            // Since StudySelectionChecklistAnswer was removed, we no longer validate individual answers here.
            // The existence of the submission itself indicates the checklist was viewed/submitted.
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
                result.Add(await MapToDecisionResponse(d, paperTitle, userNames, cancellationToken: cancellationToken));
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
                        conflictingDecisions.Add(await MapToDecisionResponse(d, paperTitle, userNames, cancellationToken: cancellationToken));
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

        public async Task<List<PaperConflictStatusResponse>> GetPaperConflictStatusesAsync(
            Guid studySelectionProcessId,
            ScreeningPhase phase,
            CancellationToken cancellationToken = default)
        {
            // 1. Fetch resolved paper IDs separately to avoid correlated subqueries
            var resolvedPaperIds = (await _unitOfWork.ScreeningResolutions.GetQueryable(
                sr => sr.StudySelectionProcessId == studySelectionProcessId && sr.Phase == phase,
                isTracking: false)
                .Select(sr => sr.PaperId)
                .ToListAsync(cancellationToken))
                .ToHashSet();

            // 2. Query decision groupings (Include/Exclude flags per paper)
            var decisionConflicts = await _unitOfWork.ScreeningDecisions.GetQueryable(
                sd => sd.StudySelectionProcessId == studySelectionProcessId && sd.Phase == phase,
                isTracking: false)
                .GroupBy(sd => sd.PaperId)
                .Select(g => new
                {
                    PaperId = g.Key,
                    HasInclude = g.Any(d => d.Decision == ScreeningDecisionType.Include),
                    HasExclude = g.Any(d => d.Decision == ScreeningDecisionType.Exclude)
                })
                .ToListAsync(cancellationToken);

            // 3. Compute final conflict status using in-memory lookup
            return decisionConflicts.Select(dc => new PaperConflictStatusResponse
            {
                PaperId = dc.PaperId,
                HasConflict = !resolvedPaperIds.Contains(dc.PaperId) && dc.HasInclude && dc.HasExclude
            }).ToList();
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
                ExclusionReasonId = request.ExclusionReasonId,
                ResolutionNotes = request.ResolutionNotes,
                ResolvedBy = request.ResolvedBy,
                ResolvedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.ScreeningResolutions.AddAsync(resolution, cancellationToken);

            // Create StudySelectionProcessPaper if included in Full-Text phase
            if (request.Phase == ScreeningPhase.FullText && request.FinalDecision == ScreeningDecisionType.Include)
            {
                var existingProcessPaper = await _unitOfWork.StudySelectionProcessPapers.FindSingleAsync(
                    pp => pp.StudySelectionProcessId == studySelectionProcessId && pp.PaperId == paperId,
                    cancellationToken: cancellationToken);
                if (existingProcessPaper == null)
                {
                    var processPaper = new StudySelectionProcessPaper
                    {
                        Id = Guid.NewGuid(),
                        StudySelectionProcessId = studySelectionProcessId,
                        PaperId = paperId,
                        IsAddedToDataset = true,
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = DateTimeOffset.UtcNow
                    };
                    await _unitOfWork.StudySelectionProcessPapers.AddAsync(processPaper, cancellationToken);
                }
            }

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
                if (ssp == null) return await MapToResolutionResponse(resolution, paper?.Title ?? string.Empty, new Dictionary<Guid, string>(), cancellationToken: cancellationToken);

                var rp = await _unitOfWork.ReviewProcesses.FindSingleAsync(r => r.Id == ssp.ReviewProcessId, cancellationToken: cancellationToken);
                if (rp == null) return await MapToResolutionResponse(resolution, paper?.Title ?? string.Empty, new Dictionary<Guid, string>(), cancellationToken: cancellationToken);

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

            return await MapToResolutionResponse(resolution, paper?.Title ?? string.Empty, new Dictionary<Guid, string>(), cancellationToken: cancellationToken);
        }

        public async Task<List<ScreeningResolutionResponse>> BulkResolveConflictsAsync(
            Guid studySelectionProcessId,
            BulkResolveConflictsRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request.PaperIds == null || !request.PaperIds.Any())
            {
                throw new ArgumentException("PaperIds list cannot be empty.");
            }

            // Check if any paper has existing assignments (Prevent bulk resolve for assigned papers)
            var hasAssignments = await _unitOfWork.PaperAssignments.AnyAsync(
                pa => pa.StudySelectionProcessId == studySelectionProcessId
                      && request.PaperIds.Contains(pa.PaperId)
                      && pa.Phase == request.Phase,
                isTracking: false,
                cancellationToken: cancellationToken);

            if (hasAssignments)
            {
                throw new InvalidOperationException("Cannot bulk resolve papers that already have reviewer assignments.");
            }

            var results = new List<ScreeningResolutionResponse>();

            // Fetch all existing resolutions for these papers and phase to avoid duplicates
            var existingResolutions = await _unitOfWork.ScreeningResolutions.FindAllAsync(
                sr => sr.StudySelectionProcessId == studySelectionProcessId
                      && request.PaperIds.Contains(sr.PaperId)
                      && sr.Phase == request.Phase,
                isTracking: false,
                cancellationToken: cancellationToken);

            var existingPaperIds = existingResolutions.Select(sr => sr.PaperId).ToHashSet();
            var paperIdsToProcess = request.PaperIds.Where(id => !existingPaperIds.Contains(id)).Distinct().ToList();

            if (!paperIdsToProcess.Any())
            {
                return results; // All already resolved
            }

            // Fetch papers for titles
            var papers = await _unitOfWork.Papers.FindAllAsync(
                p => paperIdsToProcess.Contains(p.Id),
                isTracking: false,
                cancellationToken: cancellationToken);
            var paperMap = papers.ToDictionary(p => p.Id);

            foreach (var paperId in paperIdsToProcess)
            {
                var resolution = new ScreeningResolution
                {
                    Id = Guid.NewGuid(),
                    StudySelectionProcessId = studySelectionProcessId,
                    PaperId = paperId,
                    FinalDecision = request.FinalDecision,
                    Phase = request.Phase,
                    ExclusionReasonId = request.ExclusionReasonId,
                    ResolutionNotes = request.ResolutionNotes,
                    ResolvedBy = request.ResolvedBy,
                    ResolvedAt = DateTimeOffset.UtcNow,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                };

                await _unitOfWork.ScreeningResolutions.AddAsync(resolution, cancellationToken);
                results.Add(await MapToResolutionResponse(resolution, paperMap.TryGetValue(paperId, out var p) ? p.Title : string.Empty, new Dictionary<Guid, string>(), cancellationToken: cancellationToken));
            }

            // Create StudySelectionProcessPaper for bulk included papers in Full-Text phase
            if (request.Phase == ScreeningPhase.FullText && request.FinalDecision == ScreeningDecisionType.Include)
            {
                var existingProcessPapers = await _unitOfWork.StudySelectionProcessPapers.FindAllAsync(
                    pp => pp.StudySelectionProcessId == studySelectionProcessId && paperIdsToProcess.Contains(pp.PaperId),
                    isTracking: false,
                    cancellationToken: cancellationToken);

                var existingPaperIdsInProcess = existingProcessPapers.Select(pp => pp.PaperId).ToHashSet();
                var papersToAdd = paperIdsToProcess.Where(pid => !existingPaperIdsInProcess.Contains(pid)).ToList();

                foreach (var pid in papersToAdd)
                {
                    await _unitOfWork.StudySelectionProcessPapers.AddAsync(new StudySelectionProcessPaper
                    {
                        Id = Guid.NewGuid(),
                        StudySelectionProcessId = studySelectionProcessId,
                        PaperId = pid,
                        IsAddedToDataset = true,
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = DateTimeOffset.UtcNow
                    }, cancellationToken);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Handle notifications in bulk if possible, but for now, the UI logic expects some feedback.
            // Sending notifications for each paper might be heavy, but we follow the existing pattern.
            // Optimization: Process notifications asynchronously or in batch.
            _ = Task.Run(async () =>
            {
                try
                {
                    var resolverName = await GetUserNameAsync(request.ResolvedBy, cancellationToken);
                    if (string.IsNullOrEmpty(resolverName)) resolverName = "a manager";
                    var phaseName = request.Phase == ScreeningPhase.TitleAbstract ? "Title/Abstract" : "Full-Text";

                    foreach (var paperId in paperIdsToProcess)
                    {
                        var paper = paperMap.GetValueOrDefault(paperId);
                        var assignments = await _unitOfWork.PaperAssignments.FindAllAsync(
                            pa => pa.StudySelectionProcessId == studySelectionProcessId
                                  && pa.PaperId == paperId
                                  && pa.Phase == request.Phase,
                            isTracking: false,
                            cancellationToken: cancellationToken);

                        // This is quite heavy in a loop. For bulk, we might want to optimize.
                        // But following ResolveConflictAsync logic:
                        var ssp = await _unitOfWork.StudySelectionProcesses.FindSingleAsync(s => s.Id == studySelectionProcessId, cancellationToken: cancellationToken);
                        if (ssp == null) continue;
                        var rp = await _unitOfWork.ReviewProcesses.FindSingleAsync(r => r.Id == ssp.ReviewProcessId, cancellationToken: cancellationToken);
                        if (rp == null) continue;
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
                            var title = "Paper Conflict Resolved (Bulk)";
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
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send bulk conflict resolution notifications.");
                }
            }, cancellationToken);

            return results;
        }

        public async Task<PaginatedResponse<ScreeningResolutionPaperResponse>> GetResolutionsAsync(
            Guid studySelectionProcessId,
            GetResolutionsRequest request,
            CancellationToken cancellationToken = default)
        {
            var phase = request.Phase ?? ScreeningPhase.TitleAbstract;
            List<Guid> eligiblePaperIds;

            if (phase == ScreeningPhase.FullText)
            {
                eligiblePaperIds = await GetFullTextEligiblePapersAsync(studySelectionProcessId, cancellationToken);
            }
            else
            {
                eligiblePaperIds = await GetEligiblePapersAsync(studySelectionProcessId, cancellationToken);
            }

            if (!eligiblePaperIds.Any())
            {
                return new PaginatedResponse<ScreeningResolutionPaperResponse>
                {
                    Items = new List<ScreeningResolutionPaperResponse>(),
                    TotalCount = 0,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
            }

            // Fetch papers for these IDs
            var papers = (await _unitOfWork.Papers.FindAllAsync(
                p => eligiblePaperIds.Contains(p.Id),
                isTracking: false,
                cancellationToken: cancellationToken)).ToList();

            var process = await _unitOfWork.StudySelectionProcesses.GetPhaseStatusAsync(studySelectionProcessId, cancellationToken);
            int requiredReviewers = phase == ScreeningPhase.TitleAbstract
                ? (process?.TitleAbstractScreening?.MinReviewersPerPaper ?? 2)
                : (process?.FullTextScreening?.MinReviewersPerPaper ?? 2);

            // Fetch decisions and resolutions for this phase in batch
            var phaseDecisions = await _unitOfWork.ScreeningDecisions.FindAllAsync(
                d => d.StudySelectionProcessId == studySelectionProcessId && d.Phase == phase,
                isTracking: false,
                cancellationToken: cancellationToken);

            var phaseResolutions = await _unitOfWork.ScreeningResolutions.FindAllAsync(
                r => r.StudySelectionProcessId == studySelectionProcessId && r.Phase == phase,
                isTracking: false,
                cancellationToken: cancellationToken);

            var decisionMap = phaseDecisions.GroupBy(d => d.PaperId).ToDictionary(g => g.Key, g => g.ToList());
            var resolutionMap = phaseResolutions.ToDictionary(r => r.PaperId);

            var paperStatusMap = new Dictionary<Guid, PaperSelectionStatus>();
            foreach (var paper in papers)
            {
                var dList = decisionMap.TryGetValue(paper.Id, out var list) ? list : new List<ScreeningDecision>();
                var res = resolutionMap.TryGetValue(paper.Id, out var r) ? r : null;
                paperStatusMap[paper.Id] = ComputePaperStatus(dList, res, requiredReviewers, phase);
            }

            // Filtering
            IEnumerable<Paper> filtered = papers;

            // Search filter
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim().ToLowerInvariant();
                filtered = filtered.Where(p =>
                    p.Title.ToLowerInvariant().Contains(search) ||
                    (p.Authors != null && p.Authors.ToLowerInvariant().Contains(search)) ||
                    (p.DOI != null && p.DOI.ToLowerInvariant().Contains(search)));
            }

            // Status filter (Resolution-based)
            if (request.Status != ResolutionFilterStatus.All)
            {
                filtered = request.Status switch
                {
                    ResolutionFilterStatus.NotDecided => filtered.Where(p => !resolutionMap.ContainsKey(p.Id)),
                    ResolutionFilterStatus.Include => filtered.Where(p => resolutionMap.TryGetValue(p.Id, out var r) && r.FinalDecision == ScreeningDecisionType.Include),
                    ResolutionFilterStatus.Exclude => filtered.Where(p => resolutionMap.TryGetValue(p.Id, out var r) && r.FinalDecision == ScreeningDecisionType.Exclude),
                    _ => filtered
                };
            }

            var filteredList = filtered.OrderBy(p => p.Title).ToList();
            var totalCount = filteredList.Count;

            // Pagination
            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize < 1 ? 20 : request.PageSize;

            var pagedPapers = filteredList
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Mapping to Response
            var userIds = phaseResolutions.Select(r => r.ResolvedBy).Distinct().ToList();
            var userNames = await GetUserNamesAsync(userIds, cancellationToken);
            var results = new List<ScreeningResolutionPaperResponse>();

            foreach (var paper in pagedPapers)
            {
                var resolution = resolutionMap.GetValueOrDefault(paper.Id);
                var status = paperStatusMap[paper.Id];

                // If there's no resolution record but the status is Included/Excluded (shouldn't happen with TryAutoResolve but safe-guard)
                // we "derive" a base response or handle it.

                ScreeningResolutionPaperResponse item;
                if (resolution != null)
                {
                    var baseRes = await MapToResolutionResponse(resolution, paper.Title, userNames, cancellationToken: cancellationToken);
                    item = new ScreeningResolutionPaperResponse
                    {
                        Id = baseRes.Id,
                        StudySelectionProcessId = baseRes.StudySelectionProcessId,
                        PaperId = baseRes.PaperId,
                        PaperTitle = baseRes.PaperTitle,
                        FinalDecision = baseRes.FinalDecision,
                        FinalDecisionText = baseRes.FinalDecisionText,
                        Phase = baseRes.Phase,
                        PhaseText = baseRes.PhaseText,
                        ResolutionNotes = baseRes.ResolutionNotes,
                        ResolvedBy = baseRes.ResolvedBy,
                        ResolverName = baseRes.ResolverName,
                        ResolvedAt = baseRes.ResolvedAt,
                        Authors = paper.Authors,
                        DOI = paper.DOI,
                        PublicationYear = paper.PublicationYear,
                        Source = paper.Source
                    };
                }
                else
                {
                    // For Included/Excluded papers without an explicit resolution record (e.g. if auto-resolve failed or was skipped)
                    // we can provide a minimal response based on status.
                    var finalDecision = status == PaperSelectionStatus.Included ? ScreeningDecisionType.Include : ScreeningDecisionType.Exclude;

                    item = new ScreeningResolutionPaperResponse
                    {
                        Id = Guid.Empty,
                        StudySelectionProcessId = studySelectionProcessId,
                        PaperId = paper.Id,
                        PaperTitle = paper.Title,
                        FinalDecision = finalDecision,
                        FinalDecisionText = finalDecision.ToString(),
                        Phase = phase,
                        PhaseText = phase.ToString(),
                        ResolutionNotes = "Derived from unanimous decisions.",
                        ResolvedBy = Guid.Empty,
                        ResolverName = "System",
                        ResolvedAt = DateTimeOffset.UtcNow,
                        Authors = paper.Authors,
                        DOI = paper.DOI,
                        PublicationYear = paper.PublicationYear,
                        Source = paper.Source
                    };
                }
                results.Add(item);
            }

            return new PaginatedResponse<ScreeningResolutionPaperResponse>
            {
                Items = results,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
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
            var taStats = await GetPhaseStatisticsAsync(studySelectionProcessId, ScreeningPhase.TitleAbstract, cancellationToken);
            var ftStats = await GetPhaseStatisticsAsync(studySelectionProcessId, ScreeningPhase.FullText, cancellationToken);

            var overallIncluded = ftStats.IncludedCount;
            var overallExcluded = taStats.ExcludedCount + ftStats.ExcludedCount;
            var overallConflict = taStats.ConflictCount + ftStats.ConflictCount;
            var overallPending = taStats.PendingCount + ftStats.PendingCount;

            var decidedCount = overallIncluded + overallExcluded + overallConflict;
            var completionPercentage = taStats.TotalPapers > 0
                ? (double)decidedCount / taStats.TotalPapers * 100
                : 0;

            var exclusionBreakdown = taStats.ExclusionReasonBreakdown
                .Concat(ftStats.ExclusionReasonBreakdown)
                .GroupBy(x => new { x.ReasonCode, x.ReasonText })
                .Select(g => new ExclusionReasonBreakdownItem
                {
                    ReasonCode = g.Key.ReasonCode,
                    ReasonText = g.Key.ReasonText,
                    Count = g.Sum(x => x.Count)
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            return new SelectionStatisticsResponse
            {
                StudySelectionProcessId = studySelectionProcessId,
                TotalPapers = taStats.TotalPapers,
                IncludedCount = overallIncluded,
                ExcludedCount = overallExcluded,
                ConflictCount = overallConflict,
                PendingCount = overallPending,
                CompletionPercentage = Math.Round(completionPercentage, 2),
                ExclusionReasonBreakdown = exclusionBreakdown
            };
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
            var allProcessReasons = await _unitOfWork.StuSeExclusionCodes.FindAllAsync(x => x.StudySelectionProcessId == studySelectionProcessId, isTracking: false, cancellationToken: cancellationToken);
            var reasonMap = allProcessReasons.ToDictionary(x => x.Id);

            var exclusionBreakdown = allDecisions
                .Where(d => d.Decision == ScreeningDecisionType.Exclude && d.ExclusionReasonId.HasValue)
                .GroupBy(d => d.ExclusionReasonId!.Value)
                .Select(g =>
                {
                    var reasonName = reasonMap.TryGetValue(g.Key, out var r) ? r.Name : "Unknown";
                    var reasonCode = reasonMap.TryGetValue(g.Key, out var r2) ? r2.Code : 0;
                    return new ExclusionReasonBreakdownItem
                    {
                        ReasonCode = reasonCode,
                        ReasonText = reasonName,
                        Count = g.Count()
                    };
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
                decisionResponses.Add(await MapToDecisionResponse(d, paper.Title, userNames, cancellationToken: cancellationToken));
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
                FullTextRetrievalStatus = paper.FullTextRetrievalStatus,
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
                    ? await MapToResolutionResponse(resolution, paper.Title, userNames, cancellationToken: cancellationToken)
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

            var papers = (await _unitOfWork.Papers.FindAllAsync(
                p => paperIds.Contains(p.Id),
                isTracking: false,
                cancellationToken: cancellationToken)).ToList();

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
                filtered = filtered.Where(p =>
                    p.Title.ToLowerInvariant().Contains(search) ||
                    (p.Authors != null && p.Authors.ToLowerInvariant().Contains(search)) ||
                    (p.PublicationYear != null && p.PublicationYear.ToLowerInvariant().Contains(search)));
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
            var allProcessReasons = await _unitOfWork.StuSeExclusionCodes.FindAllAsync(x => x.StudySelectionProcessId == studySelectionProcessId, isTracking: false, cancellationToken: cancellationToken);
            var reasonMap = allProcessReasons.ToDictionary(x => x.Id);

            var items = new List<PaperWithDecisionsResponse>();
            foreach (var paper in pagedPapers)
            {
                var pDecisions = decisionMap.TryGetValue(paper.Id, out var list) ? list : new List<ScreeningDecision>();
                var pResolution = resolutionMap.TryGetValue(paper.Id, out var r) ? r : null;
                var citationCount = citationCounts.TryGetValue(paper.Id, out var cc) ? cc : 0;
                var referenceCount = referenceCounts.TryGetValue(paper.Id, out var rc) ? rc : 0;

                items.Add(await MapToPaperWithDecisionsResponseBatchAsync(paper, studySelectionProcessId, paperStatusMap[paper.Id], pDecisions, pResolution, citationCount, referenceCount, reasonMap, cancellationToken));
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
            Dictionary<Guid, StudySelectionExclusionReason> reasonMap,
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
                decisionResponses.Add(await MapToDecisionResponse(d, paper.Title, userNames, reasonMap, cancellationToken));
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
                FullTextRetrievalStatus = paper.FullTextRetrievalStatus,

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
                    ? await MapToResolutionResponse(resolution, paper.Title, userNames, reasonMap, cancellationToken)
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
                decisionResponses.Add(await MapToDecisionResponse(d, paper.Title, userNames, cancellationToken: cancellationToken));
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
                    ? await MapToResolutionResponse(resolution, paper.Title, userNames, cancellationToken: cancellationToken)
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
            var distinctIds = userIds.Where(id => id != Guid.Empty).Distinct().ToList();
            if (!distinctIds.Any()) return new Dictionary<Guid, string>();

            var users = await _unitOfWork.Users.FindAllAsync(
                u => distinctIds.Contains(u.Id),
                isTracking: false,
                cancellationToken);

            return users.ToDictionary(u => u.Id, u => u.FullName ?? string.Empty);
        }

        private async Task<ScreeningDecisionResponse> MapToDecisionResponse(
            ScreeningDecision d,
            string paperTitle,
            Dictionary<Guid, string>? userNames = null,
            Dictionary<Guid, StudySelectionExclusionReason>? reasonMap = null,
            CancellationToken cancellationToken = default)
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
                ExclusionReasonId = d.ExclusionReasonId,
                ExclusionReasonCode = d.ExclusionReasonId.HasValue
                    ? (reasonMap != null && reasonMap.TryGetValue(d.ExclusionReasonId.Value, out var r) ? r.Code : (await _unitOfWork.StuSeExclusionCodes.FindSingleAsync(x => x.Id == d.ExclusionReasonId.Value, cancellationToken: cancellationToken))?.Code)
                    : null,
                ExclusionReasonName = d.ExclusionReasonId.HasValue
                    ? (reasonMap != null && reasonMap.TryGetValue(d.ExclusionReasonId.Value, out var r2) ? r2.Name : (await _unitOfWork.StuSeExclusionCodes.FindSingleAsync(x => x.Id == d.ExclusionReasonId.Value, cancellationToken: cancellationToken))?.Name)
                    : null,
                Reason = d.Reason,
                DecidedAt = d.DecidedAt
            };
        }

        private async Task<ScreeningResolutionResponse> MapToResolutionResponse(
            ScreeningResolution resolution,
            string paperTitle,
            Dictionary<Guid, string>? userNames = null,
            Dictionary<Guid, StudySelectionExclusionReason>? reasonMap = null,
            CancellationToken cancellationToken = default)
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
                ExclusionReasonId = resolution.ExclusionReasonId,
                ExclusionReasonCode = resolution.ExclusionReasonId.HasValue
                    ? (reasonMap != null && reasonMap.TryGetValue(resolution.ExclusionReasonId.Value, out var r) ? r.Code : (await _unitOfWork.StuSeExclusionCodes.FindSingleAsync(x => x.Id == resolution.ExclusionReasonId.Value, cancellationToken: cancellationToken))?.Code)
                    : null,
                ExclusionReasonName = resolution.ExclusionReasonId.HasValue
                    ? (reasonMap != null && reasonMap.TryGetValue(resolution.ExclusionReasonId.Value, out var r2) ? r2.Name : (await _unitOfWork.StuSeExclusionCodes.FindSingleAsync(x => x.Id == resolution.ExclusionReasonId.Value, cancellationToken: cancellationToken))?.Name)
                    : null,
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
                response.TitleAbstractScreening = MapToTAResponse(process.TitleAbstractScreening, process.PaperAssignments?.Any(pa => pa.Phase == ScreeningPhase.TitleAbstract) ?? false);
            }

            return response;
        }

        private static TitleAbstractScreeningResponse MapToTAResponse(TitleAbstractScreening ta, bool isAssigned = false)
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
                ModifiedAt = ta.ModifiedAt,
                IsAssigned = isAssigned
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

            return MapToTAResponse(taScreening, false);
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

            process.ReviewProcess = reviewProcess;

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

            var isAssigned = await _unitOfWork.PaperAssignments.AnyAsync(
                pa => pa.StudySelectionProcessId == studySelectionProcessId && pa.Phase == ScreeningPhase.TitleAbstract,
                cancellationToken: cancellationToken);

            return MapToTAResponse(taScreening, isAssigned);
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

            var isAssigned = await _unitOfWork.PaperAssignments.AnyAsync(
                pa => pa.StudySelectionProcessId == studySelectionProcessId && pa.Phase == ScreeningPhase.TitleAbstract,
                cancellationToken: cancellationToken);

            return MapToTAResponse(taScreening, isAssigned);
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

            var isAssigned = await _unitOfWork.PaperAssignments.AnyAsync(
                pa => pa.StudySelectionProcessId == studySelectionProcessId && pa.Phase == ScreeningPhase.TitleAbstract,
                cancellationToken: cancellationToken);

            return MapToTAResponse(taScreening, isAssigned);
        }

        // ============================================
        // Issue 2: Full-Text Upload/Link Management
        // ============================================

        public async Task<PaperDetailsResponse> UpdatePaperFullTextAsync(
            Guid paperId,
            UpdatePaperFullTextRequest request,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.PdfUrl) && string.IsNullOrWhiteSpace(request.Url))
            {
                throw new ArgumentException("At least one of PdfUrl or Url must be provided.");
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

            // Update the core link fields first so the upload is visible even if background work is delayed.
            if (!string.IsNullOrWhiteSpace(request.PdfUrl)) paper.PdfUrl = request.PdfUrl;
            if (!string.IsNullOrWhiteSpace(request.PdfFileName)) paper.PdfFileName = request.PdfFileName;
            if (!string.IsNullOrWhiteSpace(request.Url)) paper.Url = request.Url;

            var hasPdfEvidence = !string.IsNullOrWhiteSpace(request.PdfUrl) || request.PdfStream != null;
            if (hasPdfEvidence)
            {
                paper.FullTextRetrievalStatus = FullTextRetrievalStatus.Retrieved;
            }

            paper.ModifiedAt = DateTimeOffset.UtcNow;

            ExtractionSuggestionResponse? extractionSuggestion = null;

            if (request.PdfStream != null)
            {
                var fileHash = HashHelper.ComputeSha256Hash(request.PdfStream);
                request.PdfStream.Position = 0;

                var paperPdf = await _unitOfWork.PaperPdfs.FindSingleAsync(
                    p => p.PaperId == paperId,
                    isTracking: true,
                    cancellationToken: cancellationToken);

                if (paperPdf == null)
                {
                    paperPdf = new PaperPdf
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = paper.ProjectId,
                        PaperId = paperId,
                        FilePath = request.PdfUrl ?? string.Empty,
                        FileName = request.PdfFileName ?? string.Empty,
                        UploadedAt = DateTimeOffset.UtcNow,
                        FileHash = fileHash,
                        ValidationStatus = PdfValidationStatus.Pending,
                        ProcessingStatus = PdfProcessingStatus.Uploaded,
                        GrobidProcessed = false,
                        FullTextProcessed = false,
                        RefsExtracted = false,
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = DateTimeOffset.UtcNow
                    };

                    await _unitOfWork.PaperPdfs.AddAsync(paperPdf, cancellationToken);
                }
                else
                {
                    var isCurrentHash = string.Equals(paper.CurrentFileHash, fileHash, StringComparison.OrdinalIgnoreCase);

                    paperPdf.FilePath = request.PdfUrl ?? paperPdf.FilePath;
                    paperPdf.FileName = request.PdfFileName ?? paperPdf.FileName;
                    paperPdf.UploadedAt = DateTimeOffset.UtcNow;
                    paperPdf.FileHash = fileHash;
                    paperPdf.ModifiedAt = DateTimeOffset.UtcNow;

                    if (!isCurrentHash || paperPdf.ValidationStatus != PdfValidationStatus.Valid)
                    {
                        // Reset workflow state when the file version changes so old jobs cannot keep stale success flags.
                        paperPdf.ExtractedDoi = null;
                        paperPdf.ValidationStatus = PdfValidationStatus.Pending;
                        paperPdf.ProcessingStatus = PdfProcessingStatus.Uploaded;
                        paperPdf.GrobidProcessed = false;
                        paperPdf.FullTextProcessed = false;
                        paperPdf.MetadataProcessedAt = null;
                        paperPdf.MetadataValidatedAt = null;
                        paperPdf.FullTextProcessedAt = null;
                    }
                }

                paper.CurrentFileHash = fileHash;

                if (request.ExtractWithGrobid)
                {
                    paperPdf.ProcessingStatus = PdfProcessingStatus.MetadataProcessing;
                    paperPdf.MetadataProcessedAt = DateTimeOffset.UtcNow;
                    paperPdf.GrobidProcessed = false;
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                if (request.ExtractWithGrobid)
                {
                    _logger.LogInformation("Enqueuing GROBID extraction for PaperPdf {PaperPdfId} with hash {FileHash}.", paperPdf.Id, fileHash);

                    var userIdStr = _currentUserService.GetUserId();
                    Guid.TryParse(userIdStr, out var userId);

                    _grobidQueue.TryWrite(new GrobidWorkItem
                    {
                        PaperPdfId = paperPdf.Id,
                        FileHash = fileHash,
                        PaperId = paperId,
                        UserId = userId
                    });
                }
            }
            else
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }


            return MapToPaperResponseDetails(paper);
        }

        private PaperDetailsResponse MapToPaperResponseDetails(Paper paper)
        {

            var response = new PaperDetailsResponse
            {
                Id = paper.Id,
                Title = paper.Title,
                Authors = paper.Authors,
                Abstract = paper.Abstract,
                DOI = paper.DOI,
                PublicationType = paper.PublicationType,
                PublicationYear = paper.PublicationYear,
                PublicationYearInt = paper.PublicationYearInt,
                PublicationDate = paper.PublicationDate,
                Volume = paper.Volume,
                Issue = paper.Issue,
                Pages = paper.Pages,
                Publisher = paper.Publisher,
                Language = paper.Language,
                Keywords = paper.Keywords,
                Url = paper.Url,
                ConferenceName = paper.ConferenceName,
                ConferenceLocation = paper.ConferenceLocation,
                ConferenceCountry = paper.ConferenceCountry,
                ConferenceYear = paper.ConferenceYear,
                Journal = paper.Journal,
                JournalIssn = paper.JournalIssn,
                JournalEIssn = paper.JournalEIssn,
                Md5 = paper.Md5,
                Source = paper.Source,
                SearchSourceId = paper.SearchSourceId,
                ImportedAt = paper.ImportedAt,
                ImportedBy = paper.ImportedBy,
                PdfUrl = paper.PdfUrl,
                FullTextAvailable = paper.FullTextAvailable,
                FullTextRetrievalStatus = paper.FullTextRetrievalStatus,
                FullTextRetrievalStatusText = paper.FullTextRetrievalStatus.ToString(),
                CreatedAt = paper.CreatedAt,
                ModifiedAt = paper.ModifiedAt
            };

            // Get Extraction Suggestion (G-11, G-12)
            // response.ExtractionSuggestion = await _studySelectionService.GetExtractionSuggestionAsync(paper, cancellationToken);
            response.ExtractionSuggestion = null;
            return response;
        }

        public async Task ProcessGrobidExtractionAsync(GrobidWorkItem workItem, CancellationToken ct)
        {
            _logger.LogInformation("Processing background GROBID extraction for Paper {PaperId}", workItem.PaperId);

            try
            {
                var paper = await _unitOfWork.Papers.FindSingleAsync(p => p.Id == workItem.PaperId, isTracking: true, cancellationToken: ct);
                var paperPdf = await _unitOfWork.PaperPdfs.FindSingleAsync(p => p.Id == workItem.PaperPdfId, isTracking: true, cancellationToken: ct);

                if (paper == null || paperPdf == null)
                {
                    _logger.LogWarning("Paper or PaperPdf not found for background extraction. PaperId: {PaperId}, PaperPdfId: {PaperPdfId}",
                        workItem.PaperId, workItem.PaperPdfId);
                    return;
                }

                // Reject stale jobs before touching external services. The current hash is the source of truth.
                if (!string.Equals(paperPdf.FileHash, workItem.FileHash, StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(paper.CurrentFileHash, workItem.FileHash, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation(
                        "Skipping GROBID extraction for PaperPdf {PaperPdfId} because the queued hash is stale.",
                        workItem.PaperPdfId);
                    return;
                }

                if (string.IsNullOrWhiteSpace(paperPdf.FilePath))
                {
                    _logger.LogWarning("PaperPdf {PaperPdfId} has no file path. Skipping extraction.", workItem.PaperPdfId);
                    return;
                }

                if (paperPdf.ValidationStatus == PdfValidationStatus.DoiMismatch)
                {
                    _logger.LogInformation("PaperPdf {PaperPdfId} is already marked invalid. Skipping GROBID processing.", workItem.PaperPdfId);
                    return;
                }

                paperPdf.ProcessingStatus = PdfProcessingStatus.MetadataProcessing;
                paperPdf.MetadataProcessedAt = DateTimeOffset.UtcNow;
                paperPdf.ModifiedAt = DateTimeOffset.UtcNow;
                await _unitOfWork.SaveChangesAsync(ct);

                // 1. Download PDF from Supabase
                byte[] pdfBytes = await _storageService.DownloadFileAsync(paperPdf.FilePath);
                using var pdfStream = new MemoryStream(pdfBytes);

                // 2. Perform Extraction
                var extractionSuggestion = await PerformGrobidExtractionAsync(paper, paperPdf, pdfStream, paperPdf.FileName ?? "upload.pdf", ct);

                // Persist metadata-validation state before reloading latest entities for enqueue checks.
                await _unitOfWork.SaveChangesAsync(ct);

                if (paperPdf.ValidationStatus == PdfValidationStatus.DoiMismatch)
                {
                    _logger.LogInformation("PaperPdf {PaperPdfId} failed DOI validation. Full-text will not be queued.", workItem.PaperPdfId);
                    return;
                }

                // 3. Mark as processed only if the job still targets the latest PDF version.
                var latestPaper = await _unitOfWork.Papers.FindSingleAsync(p => p.Id == workItem.PaperId, isTracking: false, cancellationToken: ct);
                var latestPaperPdf = await _unitOfWork.PaperPdfs.FindSingleAsync(p => p.Id == workItem.PaperPdfId, isTracking: false, cancellationToken: ct);

                if (latestPaper == null || latestPaperPdf == null ||
                    !string.Equals(latestPaper.CurrentFileHash, workItem.FileHash, StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(latestPaperPdf.FileHash, workItem.FileHash, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation(
                        "Skipping GROBID persistence for PaperPdf {PaperPdfId} because a newer upload replaced the file during processing.",
                        workItem.PaperPdfId);
                    return;
                }

                // 4. Send Notification
                if (workItem.UserId != Guid.Empty && extractionSuggestion != null)
                {
                    // Send system notification
                    await _notificationService.SendAsync(
                        workItem.UserId,
                        "Extraction Complete",
                        $"Metadata extraction for '{paper.Title}' has been completed.",
                        NotificationType.System,
                        workItem.PaperId,
                        NotificationEntityType.Paper);

                    // Send real-time metadata update via SignalR
                    if (extractionSuggestion != null)
                    {
                        await _notificationService.SendMetadataExtractedAsync(workItem.UserId, extractionSuggestion);
                    }
                }

                // Only enqueue full-text after the metadata pass has validated the PDF and the file is still current.
                if (extractionSuggestion != null &&
                    latestPaperPdf.ValidationStatus == PdfValidationStatus.Valid &&
                    latestPaperPdf.ProcessingStatus == PdfProcessingStatus.MetadataValidated)
                {
                    var enqueued = _fullTextQueue.TryWrite(new PaperFullTextWorkItem
                    {
                        PaperPdfId = latestPaperPdf.Id,
                        FileHash = workItem.FileHash
                    });

                    if (enqueued)
                    {
                        _logger.LogInformation(
                            "Enqueued full-text extraction for PaperPdf {PaperPdfId} (Hash: {FileHash}).",
                            latestPaperPdf.Id,
                            workItem.FileHash);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Failed to enqueue full-text extraction for PaperPdf {PaperPdfId}. Queue may be full.",
                            latestPaperPdf.Id);
                    }
                }
                else
                {
                    _logger.LogInformation(
                        "Skipping full-text enqueue for PaperPdf {PaperPdfId}. extractionSuggestionNull={ExtractionSuggestionNull}, validationStatus={ValidationStatus}, processingStatus={ProcessingStatus}",
                        latestPaperPdf.Id,
                        extractionSuggestion == null,
                        latestPaperPdf.ValidationStatus,
                        latestPaperPdf.ProcessingStatus);
                }

                _logger.LogInformation("Successfully completed background GROBID extraction for Paper {PaperId}", workItem.PaperId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed background GROBID extraction for Paper {PaperId}", workItem.PaperId);
                // We don't throw here to avoid crashing the background worker
            }
        }

        private async Task<ExtractionSuggestionResponse?> PerformGrobidExtractionAsync(
            Paper paper,
            PaperPdf paperPdf,
            Stream pdfStream,
            string fileName,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting GROBID metadata extraction for Paper {PaperId}", paper.Id);
            var grobidDto = await _grobidService.ExtractHeaderAsync(pdfStream, fileName, cancellationToken);

            var grobidDoi = DoiHelper.Normalize(grobidDto.DOI);
            var paperDoi = DoiHelper.Normalize(paper.DOI);

            // DOI strict validation keeps a late job from attaching metadata from the wrong PDF version.
            if (!string.IsNullOrWhiteSpace(grobidDoi) &&
                !string.IsNullOrWhiteSpace(paperDoi) &&
                !string.Equals(grobidDoi, paperDoi, StringComparison.OrdinalIgnoreCase))
            {
                paperPdf.ExtractedDoi = grobidDto.DOI;
                paperPdf.ValidationStatus = PdfValidationStatus.DoiMismatch;
                paperPdf.ProcessingStatus = PdfProcessingStatus.MetadataInvalid;
                paperPdf.GrobidProcessed = false;
                paperPdf.FullTextProcessed = false;
                paperPdf.MetadataValidatedAt = DateTimeOffset.UtcNow;
                paperPdf.ModifiedAt = DateTimeOffset.UtcNow;

                paper.FullTextRetrievalStatus = FullTextRetrievalStatus.NotRetrieved;
                paper.ModifiedAt = DateTimeOffset.UtcNow;
                paper.PdfFileName = null;
                paper.PdfUrl = null;



                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var userIdStr = _currentUserService.GetUserId();
                if (Guid.TryParse(userIdStr, out var userId))
                {
                    await _notificationService.SendAsync(
                        userId,
                        "PDF validation failed",
                        $"The uploaded PDF DOI ({grobidDto.DOI}) does not match the paper DOI ({paper.DOI}). Full-text extraction was skipped.",
                        NotificationType.System,
                        paper.Id,
                        NotificationEntityType.Paper);
                }


                return null;
            }

            if (string.IsNullOrWhiteSpace(grobidDto.RawXml)) return null;

            // 1. Idempotency for GrobidHeaderResult
            var existingHeader = await _unitOfWork.GrobidHeaderResults.FindSingleAsync(
                ghr => ghr.PaperPdfId == paperPdf.Id,
                isTracking: true,
                cancellationToken: cancellationToken);

            if (existingHeader != null)
            {
                existingHeader.Title = grobidDto.Title;
                existingHeader.Authors = grobidDto.Authors;
                existingHeader.Abstract = grobidDto.Abstract;
                existingHeader.DOI = grobidDto.DOI;
                existingHeader.Journal = grobidDto.Journal;
                existingHeader.Volume = grobidDto.Volume;
                existingHeader.Issue = grobidDto.Issue;
                existingHeader.Pages = grobidDto.Pages;
                existingHeader.RawXml = grobidDto.RawXml;
                existingHeader.ExtractedAt = DateTimeOffset.UtcNow;
                existingHeader.ModifiedAt = DateTimeOffset.UtcNow;
            }
            else
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
            }

            // 2. Idempotency and Field Merging for PaperSourceMetadata
            var sourceMeta = await _unitOfWork.PaperSourceMetadatas.GetLatestWithGrobidHeaderByPaperIdAsync(
                paper.Id,
                cancellationToken);

            if (sourceMeta != null)
            {
                // Update only non-null fields (Field Merging) - keep existing if new extraction is null or empty
                if (!string.IsNullOrWhiteSpace(grobidDto.Title)) sourceMeta.Title = grobidDto.Title;
                if (!string.IsNullOrWhiteSpace(grobidDto.Authors)) sourceMeta.Authors = grobidDto.Authors;
                if (!string.IsNullOrWhiteSpace(grobidDto.Abstract)) sourceMeta.Abstract = grobidDto.Abstract;
                if (!string.IsNullOrWhiteSpace(grobidDto.DOI)) sourceMeta.DOI = grobidDto.DOI;
                if (!string.IsNullOrWhiteSpace(grobidDto.Journal)) sourceMeta.Journal = grobidDto.Journal;
                if (!string.IsNullOrWhiteSpace(grobidDto.Volume)) sourceMeta.Volume = grobidDto.Volume;
                if (!string.IsNullOrWhiteSpace(grobidDto.Issue)) sourceMeta.Issue = grobidDto.Issue;
                if (!string.IsNullOrWhiteSpace(grobidDto.Pages)) sourceMeta.Pages = grobidDto.Pages;
                if (!string.IsNullOrWhiteSpace(grobidDto.Publisher)) sourceMeta.Publisher = grobidDto.Publisher;
                if (grobidDto.PublishedDate != null) sourceMeta.PublishedDate = grobidDto.PublishedDate?.ToString("yyyy-MM-dd");
                if (grobidDto.Year != null) sourceMeta.Year = grobidDto.Year;
                if (!string.IsNullOrWhiteSpace(grobidDto.ISSN)) sourceMeta.ISSN = grobidDto.ISSN;
                if (!string.IsNullOrWhiteSpace(grobidDto.EISSN)) sourceMeta.EISSN = grobidDto.EISSN;
                if (!string.IsNullOrWhiteSpace(grobidDto.Keywords)) sourceMeta.Keywords = grobidDto.Keywords;
                if (!string.IsNullOrWhiteSpace(grobidDto.Language)) sourceMeta.Language = grobidDto.Language;
                if (!string.IsNullOrWhiteSpace(grobidDto.Md5)) sourceMeta.Md5 = grobidDto.Md5;

                sourceMeta.ExtractedAt = DateTimeOffset.UtcNow;
                sourceMeta.ModifiedAt = DateTimeOffset.UtcNow;
            }
            else
            {
                sourceMeta = new PaperSourceMetadata
                {
                    Id = Guid.NewGuid(),
                    PaperId = paper.Id,
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
            }

            // 3. Smart SuggestedFields Tracking (Compare Consolidated Metadata vs Main Paper)
            // Recalculate what can be still updated based on current Paper state
            var suggestedFields = GetUpdatedMetadataFields(paper, sourceMeta);
            sourceMeta.SuggestedFields = suggestedFields;

            paperPdf.ExtractedDoi = grobidDto.DOI;
            paperPdf.ValidationStatus = PdfValidationStatus.Valid;
            paperPdf.ProcessingStatus = PdfProcessingStatus.MetadataValidated;
            paperPdf.GrobidProcessed = true;
            paperPdf.MetadataValidatedAt = DateTimeOffset.UtcNow;
            paperPdf.ModifiedAt = DateTimeOffset.UtcNow;

            // 4. Map to Suggestion Response
            return new ExtractionSuggestionResponse
            {
                SourceMetadataId = sourceMeta.Id,
                PaperId = paper.Id,
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
                EISSN = sourceMeta.EISSN,
                UpdatedFields = sourceMeta.AppliedFields,
                SuggestedFields = suggestedFields
            };
        }

        public async Task<PaperWithDecisionsResponse> RetryMetadataExtractionAsync(
            Guid paperId,
            RetryExtractionRequest request,
            CancellationToken cancellationToken = default)
        {
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

            if (paperPdf.ValidationStatus == PdfValidationStatus.DoiMismatch)
            {
                return new ExtractionStatusResponse
                {
                    Requested = true,
                    Provider = "GROBID",
                    Status = "failed",
                    Message = "PDF validation failed because the DOI does not match the paper record."
                };
            }

            if (paperPdf.ProcessingStatus == PdfProcessingStatus.Completed || paperPdf.FullTextProcessed)
            {
                return new ExtractionStatusResponse
                {
                    Requested = true,
                    Provider = "GROBID",
                    Status = "succeeded"
                };
            }

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

            // 3. Chỉ cho phép auto khi có ít nhất 1 assignment
            if (assignedCount < 1) return;

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

            // 8. Chỉ auto khi unanimous
            ScreeningDecisionType? autoDecision = null;
            string? autoNotes = null;

            if (includeCount == assignedCount)
            {
                autoDecision = ScreeningDecisionType.Include;
                autoNotes = $"Auto-resolved: unanimous Include ({includeCount}/{assignedCount} reviewers).";
            }
            else if (excludeCount == assignedCount)
            {
                autoDecision = ScreeningDecisionType.Exclude;
                autoNotes = $"Auto-resolved: unanimous Exclude ({excludeCount}/{assignedCount} reviewers).";
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
                ExclusionReasonId = decisions.FirstOrDefault(d => d.Decision == ScreeningDecisionType.Exclude)?.ExclusionReasonId,
                Phase = phase,
                ResolutionNotes = autoNotes,
                ResolvedBy = Guid.Empty, // System
                ResolvedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.ScreeningResolutions.AddAsync(resolution, cancellationToken);

            // 10. Create StudySelectionProcessPaper if included in Full-Text phase
            if (phase == ScreeningPhase.FullText && resolution.FinalDecision == ScreeningDecisionType.Include)
            {
                var existingProcessPaper = await _unitOfWork.StudySelectionProcessPapers.FindSingleAsync(
                    pp => pp.StudySelectionProcessId == studySelectionProcessId && pp.PaperId == paperId,
                    cancellationToken: cancellationToken);

                if (existingProcessPaper == null)
                {
                    var processPaper = new StudySelectionProcessPaper
                    {
                        Id = Guid.NewGuid(),
                        StudySelectionProcessId = studySelectionProcessId,
                        PaperId = paperId,
                        IsAddedToDataset = true,
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = DateTimeOffset.UtcNow
                    };
                    await _unitOfWork.StudySelectionProcessPapers.AddAsync(processPaper, cancellationToken);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 11. Send notifications
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
            var extractionSuggestion = await GetExtractionSuggestionAsync(paper, cancellationToken);

            return await MapToPaperWithDecisionsResponseAsync(
                paper,
                studySelectionProcessId,
                status,
                cancellationToken,
                extractionSuggestion);
        }

        private string? NormalizeDoi(string? doi)
        {
            return DoiHelper.Normalize(doi);
        }

        public async Task<ExtractionSuggestionResponse?> GetExtractionSuggestionAsync(Paper paper, CancellationToken cancellationToken = default)
        {
            var sourceMeta = await _unitOfWork.PaperSourceMetadatas.GetLatestWithGrobidHeaderByPaperIdAsync(
                paper.Id,
                cancellationToken);

            if (sourceMeta == null) return null;

            var suggestedFields = GetUpdatedMetadataFields(paper, sourceMeta);

            return new ExtractionSuggestionResponse
            {
                SourceMetadataId = sourceMeta.Id,
                PaperId = paper.Id,
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
                EISSN = sourceMeta.EISSN,
                UpdatedFields = sourceMeta.AppliedFields,
                SuggestedFields = suggestedFields
            };
        }

        public List<string> GetUpdatedMetadataFields(Paper paper, PaperSourceMetadata sourceMeta)
        {
            var updatedFields = new List<string>();

            if (CompareFields(paper.Title, sourceMeta.Title)) updatedFields.Add("Title");
            if (CompareFields(paper.Authors, sourceMeta.Authors)) updatedFields.Add("Authors");
            if (CompareFields(paper.Abstract, sourceMeta.Abstract)) updatedFields.Add("Abstract");
            if (CompareFields(NormalizeDoi(paper.DOI), NormalizeDoi(sourceMeta.DOI))) updatedFields.Add("DOI");
            if (CompareFields(paper.Journal, sourceMeta.Journal)) updatedFields.Add("Journal");
            if (CompareFields(paper.Volume, sourceMeta.Volume)) updatedFields.Add("Volume");
            if (CompareFields(paper.Issue, sourceMeta.Issue)) updatedFields.Add("Issue");
            if (CompareFields(paper.Pages, sourceMeta.Pages)) updatedFields.Add("Pages");
            if (CompareFields(paper.Keywords, sourceMeta.Keywords)) updatedFields.Add("Keywords");
            if (CompareFields(paper.Language, sourceMeta.Language)) updatedFields.Add("Language");
            if (CompareFields(paper.Md5, sourceMeta.Md5)) updatedFields.Add("Md5");
            if (CompareFields(paper.Publisher, sourceMeta.Publisher)) updatedFields.Add("Publisher");

            // Compare dates (Paper: DateTimeOffset?, SourceMeta: string "yyyy-MM-dd")
            // Clean comparison: only if suggested date is not empty
            if (!string.IsNullOrWhiteSpace(sourceMeta.PublishedDate))
            {
                var paperDateStr = paper.PublicationDate?.ToString("yyyy-MM-dd");
                if (!string.Equals(paperDateStr, sourceMeta.PublishedDate, StringComparison.OrdinalIgnoreCase))
                    updatedFields.Add("PublishedDate");
            }

            // Compare years
            // Clean comparison: only if suggested year is not null
            if (sourceMeta.Year != null)
            {
                var paperYear = paper.PublicationYearInt ?? (int.TryParse(paper.PublicationYear, out var y) ? y : (int?)null);
                if (paperYear != sourceMeta.Year)
                    updatedFields.Add("Year");
            }

            if (CompareFields(paper.JournalIssn, sourceMeta.ISSN)) updatedFields.Add("ISSN");
            if (CompareFields(paper.JournalEIssn, sourceMeta.EISSN)) updatedFields.Add("EISSN");

            return updatedFields;
        }

        private bool CompareFields(string? dbValue, string? extractedValue)
        {
            var normalizedDb = dbValue?.Trim() ?? string.Empty;
            var normalizedExtracted = extractedValue?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(normalizedExtracted)) return false;

            return !string.Equals(normalizedDb, normalizedExtracted, StringComparison.OrdinalIgnoreCase);
        }

        public async Task<List<ReviewerDecisionDetailResponse>> GetReviewerDecisionsAsync(
            Guid studySelectionProcessId,
            Guid paperId,
            ScreeningPhase phase,
            CancellationToken cancellationToken = default)
        {
            // 1. Fetch Process
            var process = await _unitOfWork.StudySelectionProcesses.GetPhaseStatusAsync(studySelectionProcessId, cancellationToken);
            if (process == null)
            {
                throw new InvalidOperationException($"StudySelectionProcess with ID {studySelectionProcessId} not found.");
            }

            // 2. Fetch Paper Title (for mapping)
            var paper = await _unitOfWork.Papers.FindSingleAsync(p => p.Id == paperId, isTracking: false, cancellationToken: cancellationToken);
            if (paper == null)
            {
                throw new InvalidOperationException($"Paper with ID {paperId} not found.");
            }

            // 3. Fetch Assignments for this paper and phase
            var assignments = await _unitOfWork.PaperAssignments.FindAllAsync(
                pa => pa.StudySelectionProcessId == studySelectionProcessId
                      && pa.PaperId == paperId
                      && pa.Phase == phase,
                isTracking: false,
                cancellationToken: cancellationToken);

            var assignmentList = assignments.ToList();

            // 4. Fetch Decisions for this paper and phase
            var decisions = await _unitOfWork.ScreeningDecisions.GetByPaperAsync(studySelectionProcessId, paperId, phase, cancellationToken);
            var decisionMap = decisions.ToDictionary(d => d.ReviewerId);

            // 5. Build user ID list for name resolution
            var reviewProcess = await _unitOfWork.ReviewProcesses.FindSingleAsync(
                rp => rp.Id == process.ReviewProcessId,
                cancellationToken: cancellationToken);

            var projectMembers = await _unitOfWork.SystematicReviewProjects.GetMembersByProjectIdAsync(reviewProcess!.ProjectId);
            var memberToUserMap = projectMembers.ToDictionary(m => m.Id, m => m.UserId);

            var assignedUserIds = assignmentList
                .Select(a => memberToUserMap.TryGetValue(a.ProjectMemberId, out var uid) ? uid : (Guid?)null)
                .Where(uid => uid.HasValue)
                .Select(uid => uid!.Value)
                .ToList();

            var deciderUserIds = decisions.Select(d => d.ReviewerId).ToList();
            var allUserIds = assignedUserIds.Concat(deciderUserIds).Distinct().ToList();

            var userNames = await GetUserNamesAsync(allUserIds, cancellationToken);

            // 6. Map to DTOs
            var results = new List<ReviewerDecisionDetailResponse>();
            var assignedSet = assignedUserIds.ToHashSet();

            // Include assigned reviewers (with or without decisions)
            foreach (var userId in assignedUserIds)
            {
                var response = new ReviewerDecisionDetailResponse
                {
                    ReviewerId = userId,
                    ReviewerName = userNames.GetValueOrDefault(userId, "Unknown")
                };

                if (decisionMap.TryGetValue(userId, out var decision))
                {
                    response.Decision = await MapToDecisionResponse(decision, paper.Title, userNames, cancellationToken: cancellationToken);
                }

                results.Add(response);
            }

            // Include decisions from people NOT currently assigned (historical or non-assigned decisions)
            foreach (var decision in decisions)
            {
                if (!assignedSet.Contains(decision.ReviewerId))
                {
                    results.Add(new ReviewerDecisionDetailResponse
                    {
                        ReviewerId = decision.ReviewerId,
                        ReviewerName = userNames.GetValueOrDefault(decision.ReviewerId, "Unknown"),
                        Decision = await MapToDecisionResponse(decision, paper.Title, userNames, cancellationToken: cancellationToken)
                    });
                }
            }

            return results;
        }

        public async Task<List<ReviewerAssignmentTableItemResponse>> GetReviewerAssignmentTableAsync(
            Guid studySelectionProcessId,
            Guid reviewerId,
            CancellationToken cancellationToken = default)
        {
            // 1. Fetch Process
            var process = await _unitOfWork.StudySelectionProcesses.FindSingleAsync(
                ssp => ssp.Id == studySelectionProcessId,
                isTracking: false,
                cancellationToken: cancellationToken);

            if (process == null)
            {
                throw new InvalidOperationException($"StudySelectionProcess with ID {studySelectionProcessId} not found.");
            }

            // 2. Fetch ReviewProcess and Project Members to map UserId to ProjectMemberId
            var reviewProcess = await _unitOfWork.ReviewProcesses.FindSingleAsync(
                rp => rp.Id == process.ReviewProcessId,
                isTracking: false,
                cancellationToken: cancellationToken);

            var projectMembers = await _unitOfWork.SystematicReviewProjects.GetMembersByProjectIdAsync(reviewProcess!.ProjectId);
            var member = projectMembers.FirstOrDefault(m => m.UserId == reviewerId);

            if (member == null)
            {
                // If the user is not a project member, they have no assignments
                return new List<ReviewerAssignmentTableItemResponse>();
            }

            var projectMemberId = member.Id;

            // 3. Fetch all assignments for this ProjectMember in this Process
            var assignments = await _unitOfWork.PaperAssignments.GetQueryable(
                pa => pa.StudySelectionProcessId == studySelectionProcessId && pa.ProjectMemberId == projectMemberId,
                isTracking: false)
                .Include(pa => pa.Paper)
                .ToListAsync(cancellationToken);

            var assignmentList = assignments.ToList();
            if (!assignmentList.Any())
            {
                return new List<ReviewerAssignmentTableItemResponse>();
            }

            // 4. Fetch all decisions by this reviewer for this process
            var decisions = await _unitOfWork.ScreeningDecisions.FindAllAsync(
                d => d.StudySelectionProcessId == studySelectionProcessId && d.ReviewerId == reviewerId,
                isTracking: false,
                cancellationToken: cancellationToken);
            var decisionMap = decisions.GroupBy(d => d.PaperId).ToDictionary(g => g.Key, g => g.ToList());

            // 5. Fetch all checklist submissions by this reviewer for this process
            var submissions = await _unitOfWork.StudySelectionChecklistSubmissions.FindAllAsync(
                s => s.StudySelectionProcessId == studySelectionProcessId && s.ReviewerId == reviewerId,
                isTracking: false,
                cancellationToken: cancellationToken);
            var submissionMap = submissions.GroupBy(s => s.PaperId).ToDictionary(g => g.Key, g => g.ToList());

            // 6. Group assignments by Paper
            var paperGroups = assignmentList.GroupBy(a => a.PaperId);
            var results = new List<ReviewerAssignmentTableItemResponse>();

            foreach (var group in paperGroups)
            {
                var paperId = group.Key;
                var paper = group.First().Paper;
                var paperTitle = paper?.Title ?? "Unknown Title";

                var taAssignment = group.FirstOrDefault(a => a.Phase == ScreeningPhase.TitleAbstract);
                var ftAssignment = group.FirstOrDefault(a => a.Phase == ScreeningPhase.FullText);

                var paperDecisions = decisionMap.GetValueOrDefault(paperId, new List<ScreeningDecision>());
                var paperSubmissions = submissionMap.GetValueOrDefault(paperId, new List<StudySelectionChecklistSubmission>());

                var titleAbstractDisplay = BuildPhaseDisplay(taAssignment != null, paperDecisions.FirstOrDefault(d => d.Phase == ScreeningPhase.TitleAbstract), paperSubmissions.Any(s => s.Phase == ScreeningPhase.TitleAbstract));
                var fullTextDisplay = BuildPhaseDisplay(ftAssignment != null, paperDecisions.FirstOrDefault(d => d.Phase == ScreeningPhase.FullText), paperSubmissions.Any(s => s.Phase == ScreeningPhase.FullText));

                // Overall status rules:
                // Completed: All assigned phases already have decisions.
                // In Progress: At least one assigned phase has started/completed, but not all assigned phases are completed.
                // Not Started: All assigned phases have NO decision and NO checklist.

                var assignedPhases = new List<ScreeningPhase>();
                if (taAssignment != null) assignedPhases.Add(ScreeningPhase.TitleAbstract);
                if (ftAssignment != null) assignedPhases.Add(ScreeningPhase.FullText);

                string overallStatus = "Not Started";
                if (assignedPhases.Any())
                {
                    bool allCompleted = assignedPhases.All(p => paperDecisions.Any(d => d.Phase == p));
                    bool anyStarted = assignedPhases.Any(p => paperDecisions.Any(d => d.Phase == p) || paperSubmissions.Any(s => s.Phase == p));

                    if (allCompleted)
                    {
                        overallStatus = "Completed";
                    }
                    else if (anyStarted)
                    {
                        overallStatus = "In Progress";
                    }
                }

                results.Add(new ReviewerAssignmentTableItemResponse
                {
                    PaperId = paperId,
                    PaperTitle = paperTitle,
                    TitleAbstractDisplay = titleAbstractDisplay,
                    FullTextDisplay = fullTextDisplay,
                    OverallStatus = overallStatus
                });
            }

            return results.OrderBy(r => r.PaperTitle).ToList();
        }

        private string BuildPhaseDisplay(bool isAssigned, ScreeningDecision? decision, bool hasChecklist)
        {
            if (!isAssigned) return "Not Assigned";

            string decisionPart = "Pending";
            if (decision != null)
            {
                decisionPart = decision.Decision.ToString(); // Include or Exclude
            }

            string checklistPart = hasChecklist ? "Checklist Submitted" : "No Checklist";

            return $"{decisionPart} · {checklistPart}";
        }

        public async Task<PaginatedResponse<DatasetPaperResponse>> GetIncludedFullTextPapersAsync(
            Guid studySelectionProcessId,
            GetResolutionsRequest request,
            CancellationToken cancellationToken = default)
        {
            var query = _unitOfWork.ScreeningResolutions.GetQueryable(
                r => r.StudySelectionProcessId == studySelectionProcessId &&
                     r.Phase == ScreeningPhase.FullText &&
                     r.FinalDecision == ScreeningDecisionType.Include &&
                     !r.StudySelectionProcess.StudySelectionProcessPapers.Any(ip => ip.PaperId == r.PaperId && ip.IsAddedToDataset),
                isTracking: false);

            var paperQuery = query.Select(r => new
            {
                r.PaperId,
                r.Paper.Title,
                r.Paper.Authors,
                r.Paper.PublicationYear,
                r.Paper.DOI,
                r.Paper.Abstract,
                Domain = r.StudySelectionProcess.ReviewProcess.Project.Domain
            });

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim().ToLower();
                paperQuery = paperQuery.Where(p =>
                    p.Title.ToLower().Contains(search) ||
                    (p.Authors != null && p.Authors.ToLower().Contains(search)) ||
                    (p.DOI != null && p.DOI.ToLower().Contains(search)));
            }

            var totalCount = await paperQuery.CountAsync(cancellationToken);

            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize < 1 ? 20 : request.PageSize;

            var items = await paperQuery
                .OrderBy(p => p.Title)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new DatasetPaperResponse
                {
                    PaperId = p.PaperId,
                    Title = p.Title,
                    Authors = p.Authors,
                    PublicationYear = p.PublicationYear,
                    Domain = p.Domain,
                    Abstract = p.Abstract
                })
                .ToListAsync(cancellationToken);

            return new PaginatedResponse<DatasetPaperResponse>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
        public async Task<FinalResolutionProgressResponse> GetFinalResolutionPaperProgressAsync(
            Guid studySelectionProcessId,
            FinalResolutionProgressRequest request,
            CancellationToken cancellationToken = default)
        {
            var process = await _unitOfWork.StudySelectionProcesses.GetPhaseStatusAsync(studySelectionProcessId, cancellationToken);
            if (process == null)
            {
                throw new InvalidOperationException($"StudySelectionProcess with ID {studySelectionProcessId} not found.");
            }

            // 1. Get all eligible papers
            var eligiblePaperIds = await GetEligiblePapersAsync(studySelectionProcessId, cancellationToken);
            if (!eligiblePaperIds.Any())
            {
                return new FinalResolutionProgressResponse { PageSize = request.PageSize, PageNumber = request.PageNumber };
            }

            // 2. Fetch papers with metadata
            var papers = await _unitOfWork.Papers.FindAllAsync(
                p => eligiblePaperIds.Contains(p.Id),
                isTracking: false,
                cancellationToken: cancellationToken);

            // 3. Fetch all decisions and resolutions for both phases
            var allDecisions = await _unitOfWork.ScreeningDecisions.FindAllAsync(
                d => d.StudySelectionProcessId == studySelectionProcessId,
                isTracking: false,
                cancellationToken: cancellationToken);

            var allResolutions = await _unitOfWork.ScreeningResolutions.FindAllAsync(
                r => r.StudySelectionProcessId == studySelectionProcessId,
                isTracking: false,
                cancellationToken: cancellationToken);

            // Fetch exclusion reasons to map codes/names
            var exclusionReasons = await _unitOfWork.StuSeExclusionCodes.FindAllAsync(
                r => r.StudySelectionProcessId == studySelectionProcessId,
                isTracking: false,
                cancellationToken: cancellationToken);
            var reasonMap = exclusionReasons.ToDictionary(r => r.Id);

            // 4. Group data by PaperId and Phase
            var decisionMap = allDecisions.GroupBy(d => new { d.PaperId, d.Phase })
                .ToDictionary(g => g.Key, g => g.ToList());
            var resolutionMap = allResolutions.ToDictionary(r => new { r.PaperId, r.Phase });

            // 5. Determine required reviewers for each phase
            int taRequired = process.TitleAbstractScreening?.MinReviewersPerPaper ?? 2;
            int ftRequired = process.FullTextScreening?.MinReviewersPerPaper ?? 2;

            var allItems = new List<PaperResolutionProgressItem>();

            foreach (var paper in papers.OrderBy(p => p.Title))
            {
                var item = new PaperResolutionProgressItem
                {
                    PaperId = paper.Id,
                    Title = paper.Title,
                    Authors = paper.Authors ?? string.Empty,
                    Journal = paper.Journal ?? string.Empty,
                    PublicationYear = paper.PublicationYear ?? string.Empty,
                    DOI = paper.DOI
                };

                // Compute Title/Abstract Phase Status
                var taDecisions = decisionMap.GetValueOrDefault(new { PaperId = paper.Id, Phase = ScreeningPhase.TitleAbstract });
                var taResolution = resolutionMap.GetValueOrDefault(new { PaperId = paper.Id, Phase = ScreeningPhase.TitleAbstract });
                var taStatus = ComputePaperStatus(taDecisions, taResolution, taRequired, ScreeningPhase.TitleAbstract);

                item.TitleAbstractStatus = MapToPhaseStatusResponse(taStatus, ScreeningPhase.TitleAbstract, taDecisions, taResolution != null);

                // Compute Full-Text Phase Status
                if (taStatus == PaperSelectionStatus.Included)
                {
                    var ftDecisions = decisionMap.GetValueOrDefault(new { PaperId = paper.Id, Phase = ScreeningPhase.FullText });
                    var ftResolution = resolutionMap.GetValueOrDefault(new { PaperId = paper.Id, Phase = ScreeningPhase.FullText });
                    var ftStatus = ComputePaperStatus(ftDecisions, ftResolution, ftRequired, ScreeningPhase.FullText);

                    item.FullTextStatus = MapToPhaseStatusResponse(ftStatus, ScreeningPhase.FullText, ftDecisions, ftResolution != null);

                    // Final Decision and Exclusion Reason from FT
                    if (ftStatus == PaperSelectionStatus.Included)
                    {
                        item.FinalDecision = "INCLUDED";
                    }
                    else if (ftStatus == PaperSelectionStatus.Excluded)
                    {
                        item.FinalDecision = "EXCLUDED";
                        var exclusionReasonId = ftResolution?.ExclusionReasonId ?? ftDecisions?.FirstOrDefault(d => d.Decision == ScreeningDecisionType.Exclude)?.ExclusionReasonId;
                        if (exclusionReasonId != null && reasonMap.TryGetValue(exclusionReasonId.Value, out var reason))
                        {
                            item.ExclusionReason = new ExclusionReasonDetailResponse { Code = reason.Code, Name = reason.Name };
                        }
                    }
                    else
                    {
                        item.FinalDecision = "PENDING";
                    }
                }
                else if (taStatus == PaperSelectionStatus.Excluded)
                {
                    item.FullTextStatus = new PhaseStatusResponse { Status = "NOT_REACHED" };
                    item.FinalDecision = "EXCLUDED";
                    
                    var exclusionReasonId = taResolution?.ExclusionReasonId ?? taDecisions?.FirstOrDefault(d => d.Decision == ScreeningDecisionType.Exclude)?.ExclusionReasonId;
                    if (exclusionReasonId != null && reasonMap.TryGetValue(exclusionReasonId.Value, out var reason))
                    {
                        item.ExclusionReason = new ExclusionReasonDetailResponse { Code = reason.Code, Name = reason.Name };
                    }
                }
                else
                {
                    item.FullTextStatus = new PhaseStatusResponse { Status = "NOT_REACHED" };
                    item.FinalDecision = "PENDING";
                }

                allItems.Add(item);
            }

            var result = new FinalResolutionProgressResponse
            {
                TotalPapers = allItems.Count,
                IncludedCount = allItems.Count(p => p.FinalDecision == "INCLUDED"),
                ExcludedCount = allItems.Count(p => p.FinalDecision == "EXCLUDED"),
                PendingCount = allItems.Count(p => p.FinalDecision == "PENDING"),
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            // Calculate Top Exclusion Reason from all items
            result.TopExclusionReason = allItems
                .Where(p => p.ExclusionReason != null)
                .GroupBy(p => new { p.ExclusionReason!.Code, p.ExclusionReason!.Name })
                .Select(g => new TopExclusionReasonResponse
                {
                    Code = g.Key.Code,
                    Name = g.Key.Name,
                    Count = g.Count()
                })
                .OrderByDescending(r => r.Count)
                .FirstOrDefault();

            // Apply Filters to the list
            IEnumerable<PaperResolutionProgressItem> filteredItems = allItems;

            if (request.Status != FinalResolutionStatusFilter.All)
            {
                var statusStr = request.Status.ToString().ToUpper();
                filteredItems = filteredItems.Where(p => p.FinalDecision.Equals(statusStr, StringComparison.OrdinalIgnoreCase));
            }

            if (request.ExclusionReasonCode.HasValue)
            {
                filteredItems = filteredItems.Where(p => p.ExclusionReason?.Code == request.ExclusionReasonCode.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var searchTerm = request.Search.Trim().ToLowerInvariant();
                filteredItems = filteredItems.Where(p =>
                    p.Title.ToLowerInvariant().Contains(searchTerm) ||
                    p.Authors.ToLowerInvariant().Contains(searchTerm) ||
                    (p.DOI != null && p.DOI.ToLowerInvariant().Contains(searchTerm)));
            }

            if (request.FromYear.HasValue)
            {
                filteredItems = filteredItems.Where(p =>
                    int.TryParse(p.PublicationYear, out var year) && year >= request.FromYear.Value);
            }

            if (request.ToYear.HasValue)
            {
                filteredItems = filteredItems.Where(p =>
                    int.TryParse(p.PublicationYear, out var year) && year <= request.ToYear.Value);
            }

            result.TotalCount = filteredItems.Count();

            // Apply Pagination
            result.Papers = filteredItems
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return result;
        }

        private PhaseStatusResponse MapToPhaseStatusResponse(PaperSelectionStatus status, ScreeningPhase phase, List<ScreeningDecision>? decisions, bool hasResolution)
        {
            var response = new PhaseStatusResponse();

            // If resolved, always use the status from resolution (Included/Excluded)
            if (hasResolution)
            {
                response.Status = status.ToString().ToUpper();
                return response;
            }

            // If not resolved, check for conflict
            if (status == PaperSelectionStatus.Conflict)
            {
                response.Status = "CONFLICTED";
                return response;
            }

            // "Manual" conflict detection for cases where status might be Pending but decisions already conflict
            if (decisions != null)
            {
                var hasInclude = decisions.Any(d => d.Decision == ScreeningDecisionType.Include);
                var hasExclude = decisions.Any(d => d.Decision == ScreeningDecisionType.Exclude);
                if (hasInclude && hasExclude)
                {
                    response.Status = "CONFLICTED";
                    return response;
                }
            }

            response.Status = status.ToString().ToUpper();
            return response;
        }
    }
}
