using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.Common;
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

            studySelectionProcessResponse.SelectionStatistics = await GetSelectionStatisticsAsync(id, cancellationToken);
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
                Phase = ScreeningPhase.TitleAbstract,
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
            await TryAutoResolveAsync(studySelectionProcessId, paperId, cancellationToken);

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

            return await MapToResolutionResponse(resolution, paper?.Title ?? string.Empty, cancellationToken: cancellationToken);
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

            // Check for conflicts (decisions disagree)
            var distinctDecisions = decisions.Select(d => d.Decision).Distinct().Count();
            if (distinctDecisions > 1)
            {
                return PaperSelectionStatus.Conflict;
            }

            // All decisions agree — but only return Included/Excluded if min reviewers reached
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

            // Compute status for each paper to get accurate statistics
            var includedCount = 0;
            var excludedCount = 0;
            var conflictCount = 0;
            var pendingCount = 0;

            foreach (var paperId in eligiblePapers)
            {
                var status = await GetPaperSelectionStatusAsync(studySelectionProcessId, paperId, cancellationToken);
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

            var decidedCount = includedCount + excludedCount + conflictCount;
            var completionPercentage = totalPapers > 0
                ? (double)decidedCount / totalPapers * 100
                : 0;

            // Exclusion reason breakdown (G-10)
            var allDecisions = await _unitOfWork.ScreeningDecisions.GetByProcessAsync(studySelectionProcessId, cancellationToken);
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
            // Validate pagination parameters
            if (request.PageNumber < 1) request.PageNumber = 1;
            if (request.PageSize < 1) request.PageSize = 20;
            if (request.PageSize > 100) request.PageSize = 100;

            // Phase-aware paper retrieval
            var phase = request.Phase ?? ScreeningPhase.TitleAbstract;
            List<Guid> eligiblePaperIds;

            if (phase == ScreeningPhase.FullText)
            {
                // Full-Text: only papers that were INCLUDED in Title/Abstract screening
                eligiblePaperIds = await _unitOfWork.ScreeningResolutions.GetResolvedPaperIdsByPhaseAsync(
                    studySelectionProcessId,
                    ScreeningPhase.TitleAbstract,
                    ScreeningDecisionType.Include,
                    cancellationToken);
            }
            else
            {
                // Title/Abstract: all eligible papers from identification
                eligiblePaperIds = await GetEligiblePapersAsync(studySelectionProcessId, cancellationToken);
            }

            // Fetch all eligible papers for filtering/sorting
            var papers = new List<Paper>();
            foreach (var paperId in eligiblePaperIds)
            {
                var paper = await _unitOfWork.Papers.FindSingleAsync(p => p.Id == paperId, cancellationToken: cancellationToken);
                if (paper != null) papers.Add(paper);
            }

            // Compute status for each paper
            var paperStatusMap = new Dictionary<Guid, PaperSelectionStatus>();
            foreach (var paper in papers)
            {
                paperStatusMap[paper.Id] = await GetPaperSelectionStatusAsync(studySelectionProcessId, paper.Id, cancellationToken);
            }

            // Apply search filter (by title)
            IEnumerable<Paper> filtered = papers;
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim().ToLowerInvariant();
                filtered = filtered.Where(p => p.Title.ToLowerInvariant().Contains(search));
            }

            // Pre-load all decisions for sorting/filtering (used by Issue 3 & 4)
            var allDecisions = await _unitOfWork.ScreeningDecisions.GetByProcessAsync(studySelectionProcessId, cancellationToken);

            // Apply status filter
            if (request.Status.HasValue)
            {
                filtered = filtered.Where(p => paperStatusMap[p.Id] == request.Status.Value);
            }

            // Issue 4: HasFullText filter
            if (request.HasFullText.HasValue)
            {
                filtered = request.HasFullText.Value
                    ? filtered.Where(p => !string.IsNullOrWhiteSpace(p.PdfUrl) || !string.IsNullOrWhiteSpace(p.Url))
                    : filtered.Where(p => string.IsNullOrWhiteSpace(p.PdfUrl) && string.IsNullOrWhiteSpace(p.Url));
            }

            // Issue 4: HasConflict filter
            if (request.HasConflict.HasValue)
            {
                filtered = request.HasConflict.Value
                    ? filtered.Where(p => paperStatusMap[p.Id] == PaperSelectionStatus.Conflict)
                    : filtered.Where(p => paperStatusMap[p.Id] != PaperSelectionStatus.Conflict);
            }

            // Issue 4: DecidedByReviewerId filter
            if (request.DecidedByReviewerId.HasValue)
            {
                var decidedPaperIds = allDecisions
                    .Where(d => d.ReviewerId == request.DecidedByReviewerId.Value)
                    .Select(d => d.PaperId)
                    .ToHashSet();
                filtered = filtered.Where(p => decidedPaperIds.Contains(p.Id));
            }

            // Apply sorting (Issue 3: add RelevanceDesc)
            filtered = request.SortBy switch
            {
                PaperSortBy.TitleDesc => filtered.OrderByDescending(p => p.Title),
                PaperSortBy.YearNewest => filtered.OrderByDescending(p => p.PublicationYearInt ?? 0).ThenBy(p => p.Title),
                PaperSortBy.YearOldest => filtered.OrderBy(p => p.PublicationYearInt ?? int.MaxValue).ThenBy(p => p.Title),
                PaperSortBy.RelevanceDesc => filtered.OrderByDescending(p =>
                {
                    // Sort by number of decisions (most-reviewed first), then conflicts first
                    var decisionCount = allDecisions.Count(d => d.PaperId == p.Id);
                    var hasConflict = paperStatusMap[p.Id] == PaperSelectionStatus.Conflict ? 1 : 0;
                    return hasConflict * 10000 + decisionCount;
                }).ThenBy(p => p.Title),
                _ => filtered.OrderBy(p => p.Title),
            };

            var filteredList = filtered.ToList();
            var totalCount = filteredList.Count;

            // Apply pagination
            var pagedPapers = filteredList
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // Build response items only for the current page
            var items = new List<PaperWithDecisionsResponse>();

            foreach (var paper in pagedPapers)
            {
                var decisions = await _unitOfWork.ScreeningDecisions.GetByPaperAsync(
                    studySelectionProcessId,
                    paper.Id,
                    cancellationToken);

                var resolution = await _unitOfWork.ScreeningResolutions.GetByProcessAndPaperAsync(
                    studySelectionProcessId,
                    paper.Id,
                    cancellationToken);

                var status = paperStatusMap[paper.Id];

                // Batch-resolve user names for decisions + resolution
                var allUserIds = decisions.Select(d => d.ReviewerId).ToList();
                if (resolution != null) allUserIds.Add(resolution.ResolvedBy);
                var userNames = await GetUserNamesAsync(allUserIds, cancellationToken);

                var paperTitle = paper.Title;

                var decisionResponses = new List<ScreeningDecisionResponse>();
                foreach (var d in decisions)
                {
                    decisionResponses.Add(await MapToDecisionResponse(d, paperTitle, userNames, cancellationToken));
                }

                // Issue 5: Deterministic final decision
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

                items.Add(new PaperWithDecisionsResponse
                {
                    PaperId = paper.Id,
                    Title = paperTitle,
                    DOI = paper.DOI,
                    Authors = paper.Authors,
                    PublicationYear = paper.PublicationYearInt,
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
                    ConferenceName = paper.ConferenceName,
                    ConferenceLocation = paper.ConferenceLocation,
                    JournalIssn = paper.JournalIssn,
                    Status = status,
                    StatusText = status.ToString(),
                    FinalDecision = finalDecision,
                    FinalDecisionText = finalDecision?.ToString(),
                    Decisions = decisionResponses,
                    Resolution = resolution != null
                        ? await MapToResolutionResponse(resolution, paperTitle, userNames, cancellationToken)
                        : null
                });
            }

            return new PaginatedResponse<PaperWithDecisionsResponse>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
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
                studySelectionProcessId, cancellationToken);
            var resolvedPaperIds = (await _unitOfWork.ScreeningResolutions.GetByProcessAsync(
                studySelectionProcessId, cancellationToken))
                .Where(sr => sr.Phase == ScreeningPhase.TitleAbstract)
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

            if (!string.IsNullOrWhiteSpace(request.Url))
            {
                paper.Url = request.Url;
            }

            paper.ModifiedAt = DateTimeOffset.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Build response
            var decisions = await _unitOfWork.ScreeningDecisions.GetByPaperAsync(
                studySelectionProcessId, paperId, cancellationToken);
            var resolution = await _unitOfWork.ScreeningResolutions.GetByProcessAndPaperAsync(
                studySelectionProcessId, paperId, cancellationToken);
            var status = await GetPaperSelectionStatusAsync(studySelectionProcessId, paperId, cancellationToken);

            var allUserIds = decisions.Select(d => d.ReviewerId).ToList();
            if (resolution != null) allUserIds.Add(resolution.ResolvedBy);
            var userNames = await GetUserNamesAsync(allUserIds, cancellationToken);

            var decisionResponses = new List<ScreeningDecisionResponse>();
            foreach (var d in decisions)
            {
                decisionResponses.Add(await MapToDecisionResponse(d, paper.Title, userNames, cancellationToken));
            }

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
                PublicationYear = paper.PublicationYearInt,
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
                ConferenceName = paper.ConferenceName,
                ConferenceLocation = paper.ConferenceLocation,
                JournalIssn = paper.JournalIssn,
                Status = status,
                StatusText = status.ToString(),
                FinalDecision = finalDecision,
                FinalDecisionText = finalDecision?.ToString(),
                Decisions = decisionResponses,
                Resolution = resolution != null
                    ? await MapToResolutionResponse(resolution, paper.Title, userNames, cancellationToken)
                    : null
            };
        }

        // ============================================
        // Auto-Resolution Logic (G-05, G-06)
        // ============================================

        /// <summary>
        /// Checks whether all expected reviewers have submitted decisions for a paper.
        /// If unanimous, auto-creates a ScreeningResolution (G-06).
        /// If majority (2 vs 1 with 3 reviewers), auto-creates a ScreeningResolution (G-05).
        /// </summary>
        private async Task TryAutoResolveAsync(
            Guid studySelectionProcessId,
            Guid paperId,
            CancellationToken cancellationToken = default)
        {
            // Check if resolution already exists
            var existingResolution = await _unitOfWork.ScreeningResolutions.GetByProcessAndPaperAsync(
                studySelectionProcessId, paperId, cancellationToken);

            if (existingResolution != null) return;

            // Get TA screening to know the reviewer count config
            var taScreening = await _unitOfWork.TitleAbstractScreenings.GetByProcessIdAsync(
                studySelectionProcessId, cancellationToken);

            var minReviewers = taScreening?.MinReviewersPerPaper ?? 2;

            // Get all decisions for this paper
            var decisions = await _unitOfWork.ScreeningDecisions.GetByPaperAsync(
                studySelectionProcessId, paperId, cancellationToken);

            if (decisions.Count < minReviewers) return;

            var includeCount = decisions.Count(d => d.Decision == ScreeningDecisionType.Include);
            var excludeCount = decisions.Count(d => d.Decision == ScreeningDecisionType.Exclude);

            ScreeningDecisionType? autoDecision = null;
            string? autoNotes = null;

            // Unanimous decision (G-06)
            if (includeCount == decisions.Count)
            {
                autoDecision = ScreeningDecisionType.Include;
                autoNotes = $"Auto-resolved: unanimous Include ({decisions.Count}/{decisions.Count} reviewers).";
            }
            else if (excludeCount == decisions.Count)
            {
                autoDecision = ScreeningDecisionType.Exclude;
                autoNotes = $"Auto-resolved: unanimous Exclude ({decisions.Count}/{decisions.Count} reviewers).";
            }
            // Majority decision with 3+ reviewers (G-05)
            else if (decisions.Count >= 3)
            {
                if (includeCount > excludeCount)
                {
                    autoDecision = ScreeningDecisionType.Include;
                    autoNotes = $"Auto-resolved: majority Include ({includeCount}/{decisions.Count} reviewers).";
                }
                else if (excludeCount > includeCount)
                {
                    autoDecision = ScreeningDecisionType.Exclude;
                    autoNotes = $"Auto-resolved: majority Exclude ({excludeCount}/{decisions.Count} reviewers).";
                }
            }

            if (autoDecision.HasValue)
            {
                var resolution = new ScreeningResolution
                {
                    Id = Guid.NewGuid(),
                    StudySelectionProcessId = studySelectionProcessId,
                    PaperId = paperId,
                    FinalDecision = autoDecision.Value,
                    Phase = ScreeningPhase.TitleAbstract,
                    ResolutionNotes = autoNotes,
                    ResolvedBy = Guid.Empty, // System auto-resolution
                    ResolvedAt = DateTimeOffset.UtcNow,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                };

                await _unitOfWork.ScreeningResolutions.AddAsync(resolution, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
