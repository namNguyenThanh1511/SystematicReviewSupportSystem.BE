using SRSS.IAM.Repositories.Entities;
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

            // Use the same unique papers logic as the unique papers endpoint
            // (filters by ImportBatch → SearchExecution chain, excludes IsRemovedAsDuplicate and pending dedup)
            var (_, totalCount) = await _unitOfWork.Papers.GetUniquePapersByIdentificationProcessAsync(
                identificationProcess.Id,
                search: null,
                year: null,
                pageNumber: 1,
                pageSize: 1,
                cancellationToken);

            if (totalCount == 0)
            {
                return new List<Guid>();
            }

            // Fetch all unique paper IDs
            var (papers, _) = await _unitOfWork.Papers.GetUniquePapersByIdentificationProcessAsync(
                identificationProcess.Id,
                search: null,
                year: null,
                pageNumber: 1,
                pageSize: totalCount,
                cancellationToken);

            return papers.Select(p => p.Id).ToList();
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

        public async Task<PaginatedResponse<PaperWithDecisionsResponse>> GetPapersWithDecisionsAsync(
            Guid studySelectionProcessId,
            PapersWithDecisionsRequest request,
            CancellationToken cancellationToken = default)
        {
            // Validate pagination parameters
            if (request.PageNumber < 1) request.PageNumber = 1;
            if (request.PageSize < 1) request.PageSize = 20;
            if (request.PageSize > 100) request.PageSize = 100;

            var eligiblePaperIds = await GetEligiblePapersAsync(studySelectionProcessId, cancellationToken);

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

            // Apply status filter
            if (request.Status.HasValue)
            {
                filtered = filtered.Where(p => paperStatusMap[p.Id] == request.Status.Value);
            }

            // Apply sorting
            filtered = request.SortBy switch
            {
                PaperSortBy.TitleDesc => filtered.OrderByDescending(p => p.Title),
                PaperSortBy.YearNewest => filtered.OrderByDescending(p => p.PublicationYearInt ?? 0).ThenBy(p => p.Title),
                PaperSortBy.YearOldest => filtered.OrderBy(p => p.PublicationYearInt ?? int.MaxValue).ThenBy(p => p.Title),
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
                Reason = d.Reason,
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
                ResolutionNotes = resolution.ResolutionNotes,
                ResolvedBy = resolution.ResolvedBy,
                ResolverName = resolverName,
                ResolvedAt = resolution.ResolvedAt
            };
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
