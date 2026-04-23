using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.Repositories.PaperRepo
{
    public class PaperRepository : GenericRepository<Paper, Guid, AppDbContext>, IPaperRepository
    {
        public PaperRepository(AppDbContext context) : base(context)
        {
        }

        public IQueryable<Paper> GetPapersQueryable(List<Guid> ids)
        {
            // Nếu danh sách ids null hoặc rỗng, trả về một queryable rỗng thay vì ném lỗi hoặc lấy toàn bộ
            if (ids == null || !ids.Any())
            {
                return _context.Papers.Where(p => false);
            }

            return _context.Papers
                .AsNoTracking() // Tăng hiệu năng vì đây là truy vấn Read-only
                .Where(p => !p.IsDeleted && ids.Contains(p.Id));
        }

        public async Task<Paper?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Papers
                .FirstOrDefaultAsync(p => !p.IsDeleted && p.Id == id, cancellationToken);
        }

        public async Task<Paper?> GetByDoiAndProjectAsync(string doi, Guid projectId, CancellationToken cancellationToken = default)
        {
            return await _context.Papers
                .Where(p => !p.IsDeleted && p.DOI == doi && p.ProjectId == projectId)
                .OrderBy(p => p.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }


        public async Task<Paper?> GetByDoiAndIdentificationProcessAsync(string doi, Guid identificationProcessId, CancellationToken cancellationToken = default)
        {
            return await _context.Papers
                .Where(p => p.DOI == doi
                    && !p.IsDeleted)
                .OrderBy(p => p.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<(List<Paper> Papers, int TotalCount)> GetPapersByProjectAsync(
            Guid projectId,
            string? search,
            SelectionStatus? status,
            int? year,
            string? assignmentStatus,
            ScreeningStage? stage,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Papers
                .AsNoTracking()
                .Include(p => p.PaperAssignments)
                    .ThenInclude(pa => pa.ProjectMember)
                        .ThenInclude(pm => pm.User)
                .Include(p => p.ScreeningResolutions)
                .Where(p => p.ProjectId == projectId && !p.IsDeleted);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(p =>
                    (p.Title != null && p.Title.ToLower().Contains(searchLower)) ||
                    (p.DOI != null && p.DOI.ToLower().Contains(searchLower)) ||
                    (p.Authors != null && p.Authors.ToLower().Contains(searchLower)));
            }

            // Status filtering removed - status is process-scoped, not paper-scoped
            // Use ScreeningResolution table for status queries

            // Apply year filter
            if (year.HasValue)
            {
                query = query.Where(p => p.PublicationYearInt == year.Value);
            }

            // Apply assignment status filter
            if (!string.IsNullOrWhiteSpace(assignmentStatus))
            {
                if (assignmentStatus.Equals("assigned", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(p => p.PaperAssignments.Any());
                }
                else if (assignmentStatus.Equals("unassigned", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(p => !p.PaperAssignments.Any());
                }
            }

            // Apply stage filter
            if (stage.HasValue)
            {
                if (stage == ScreeningStage.FullText)
                {
                    // FullText papers are those that have been "Included" in a screening resolution
                    query = query.Where(p => _context.ScreeningResolutions
                        .Any(sr => sr.PaperId == p.Id && sr.FinalDecision == ScreeningDecisionType.Include));
                }
                else if (stage == ScreeningStage.TitleAbstract)
                {
                    // Papers that have NOT yet moved to FullText stage (not resolved as Included)
                    query = query.Where(p => !_context.ScreeningResolutions
                        .Any(sr => sr.PaperId == p.Id && sr.FinalDecision == ScreeningDecisionType.Include));
                }
            }

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply ordering and pagination
            var papers = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (papers, totalCount);
        }

        public async Task<(List<Paper> Papers, int TotalCount)> SearchPapersByProjectAsync(
            Guid projectId,
            string? search,
            Guid? searchStrategyId,
            Guid? searchSourceId,
            int? year,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Papers
                .AsNoTracking()
                .Include(p => p.PaperAssignments)
                    .ThenInclude(pa => pa.ProjectMember)
                        .ThenInclude(pm => pm.User)
                .Include(p => p.ScreeningResolutions)
                .Where(p => p.ProjectId == projectId && !p.IsDeleted);

            // Apply search filter (search text across title, DOI, and authors)
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(p =>
                    (p.Title != null && p.Title.ToLower().Contains(searchLower)) ||
                    (p.DOI != null && p.DOI.ToLower().Contains(searchLower)) ||
                    (p.Authors != null && p.Authors.ToLower().Contains(searchLower)));
            }



            // Apply search source filter
            if (searchSourceId.HasValue)
            {
                query = query.Where(p => p.SearchSourceId == searchSourceId);
            }

            // Apply year filter
            if (year.HasValue)
            {
                query = query.Where(p => p.PublicationYearInt == year.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply ordering and pagination
            var papers = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (papers, totalCount);
        }

        public async Task<(List<Paper> Papers, int TotalCount)> GetPaperPoolByProjectAsync(
            Guid projectId,
            string? searchText,
            string? keyword,
            int? yearFrom,
            int? yearTo,
            Guid? searchSourceId,
            Guid? importBatchId,
            string doiState,
            string fullTextState,
            bool onlyUnused,
            bool recentlyImported,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Papers
                .AsNoTracking()
                .Include(p => p.PaperAssignments)
                    .ThenInclude(pa => pa.ProjectMember)
                        .ThenInclude(pm => pm.User)
                .Include(p => p.ScreeningResolutions)
                .Where(p => p.ProjectId == projectId && !p.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var searchTextLower = searchText.Trim().ToLower();
                query = query.Where(p =>
                    (p.Title != null && p.Title.ToLower().Contains(searchTextLower)) ||
                    (p.Abstract != null && p.Abstract.ToLower().Contains(searchTextLower)) ||
                    (p.Authors != null && p.Authors.ToLower().Contains(searchTextLower)) ||
                    (p.DOI != null && p.DOI.ToLower().Contains(searchTextLower)) ||
                    (p.Keywords != null && p.Keywords.ToLower().Contains(searchTextLower)));
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var keywordLower = keyword.Trim().ToLower();
                query = query.Where(p => p.Keywords != null && p.Keywords.ToLower().Contains(keywordLower));
            }

            if (yearFrom.HasValue)
            {
                query = query.Where(p => p.PublicationYearInt.HasValue && p.PublicationYearInt.Value >= yearFrom.Value);
            }

            if (yearTo.HasValue)
            {
                query = query.Where(p => p.PublicationYearInt.HasValue && p.PublicationYearInt.Value <= yearTo.Value);
            }

            if (searchSourceId.HasValue)
            {
                var sourceId = searchSourceId.Value;
                query = query.Where(p =>
                    p.SearchSourceId == sourceId);
            }

            if (importBatchId.HasValue)
            {
                query = query.Where(p => p.ImportBatchId == importBatchId.Value);
            }

            if (string.Equals(doiState, "has", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => !string.IsNullOrWhiteSpace(p.DOI));
            }
            else if (string.Equals(doiState, "missing", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => string.IsNullOrWhiteSpace(p.DOI));
            }

            if (string.Equals(fullTextState, "has", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.FullTextAvailable == true);
            }
            else if (string.Equals(fullTextState, "missing", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.FullTextAvailable != true);
            }

            if (onlyUnused)
            {
                query = query.Where(p =>
                    !_context.IdentificationProcessPapers.Any(x => x.PaperId == p.Id) &&
                    !_context.StudySelectionProcessPapers.Any(x => x.PaperId == p.Id) &&
                    !_context.QualityAssessmentDecisions.Any(x => x.PaperId == p.Id) &&
                    !_context.ExtractionPaperTasks.Any(x => x.PaperId == p.Id));
            }

            if (recentlyImported)
            {
                var recentThreshold = DateTimeOffset.UtcNow.AddDays(-7);
                query = query.Where(p => p.ImportedAt.HasValue && p.ImportedAt.Value >= recentThreshold);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var papers = await query
                .OrderByDescending(p => p.ImportedAt ?? p.CreatedAt)
                .ThenByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (papers, totalCount);
        }

        /// <summary>
        /// Get duplicate papers for a specific project
        /// Queries DeduplicationResult table for project-scoped duplicates
        /// </summary>
        public async Task<(List<Paper> Papers, List<DeduplicationResult> Results, int TotalCount)> GetDuplicatePapersByProjectAsync(
            Guid projectId,
            string? search,
            int? year,
            string? sortBy,
            string? sortOrder,
            DeduplicationReviewStatus? reviewStatus,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            // Query deduplication results for this project
            var query = _context.DeduplicationResults
                .AsNoTracking()
                .Include(dr => dr.Paper)
                    .ThenInclude(p => p.PaperAssignments)
                        .ThenInclude(pa => pa.ProjectMember)
                            .ThenInclude(pm => pm.User)
                .Include(dr => dr.Paper)
                    .ThenInclude(p => p.ScreeningResolutions)
                .Include(dr => dr.DuplicateOfPaper)
                .Where(dr => dr.ProjectId == projectId && !dr.Paper.IsDeleted && !dr.DuplicateOfPaper.IsDeleted);

            // Apply review status filter
            if (reviewStatus.HasValue)
            {
                query = query.Where(dr => dr.ReviewStatus == reviewStatus.Value);
            }

            // Apply search filter on paper metadata
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(dr =>
                    (dr.Paper.Title != null && dr.Paper.Title.ToLower().Contains(searchLower)) ||
                    (dr.Paper.DOI != null && dr.Paper.DOI.ToLower().Contains(searchLower)) ||
                    (dr.Paper.Authors != null && dr.Paper.Authors.ToLower().Contains(searchLower)));
            }

            // Apply year filter
            if (year.HasValue)
            {
                query = query.Where(dr => dr.Paper.PublicationYearInt == year.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply sorting
            var isDescending = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);

            query = sortBy?.ToLower() switch
            {
                "confidencescore" => isDescending
                    ? query.OrderByDescending(dr => dr.ConfidenceScore)
                    : query.OrderBy(dr => dr.ConfidenceScore),
                "title" => isDescending
                    ? query.OrderByDescending(dr => dr.Paper.Title)
                    : query.OrderBy(dr => dr.Paper.Title),
                "method" => isDescending
                    ? query.OrderByDescending(dr => dr.Method)
                    : query.OrderBy(dr => dr.Method),
                "reviewstatus" => isDescending
                    ? query.OrderByDescending(dr => dr.ReviewStatus)
                    : query.OrderBy(dr => dr.ReviewStatus),
                _ => query.OrderByDescending(dr => dr.CreatedAt) // default: detectedAt DESC
            };

            // Apply pagination
            var deduplicationResults = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            // Extract papers from results
            var papers = deduplicationResults.Select(dr => dr.Paper).ToList();

            return (papers, deduplicationResults, totalCount);
        }

        /// <summary>
        /// Get unique (non-duplicate) papers for a specific identification process.
        /// Papers linked via ImportBatch → SearchExecution → IdentificationProcess
        /// EXCEPT papers confirmed as duplicates (CANCEL decision) in this process. and paper wait for resolve duplicate
        /// </summary>
        public async Task<(List<Paper> Papers, int TotalCount)> GetUniquePapersByIdentificationProcessAsync(
            Guid identificationProcessId,
            string? search,
            int? year,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var includedPaperIdsQuery = _context.IdentificationProcessPapers
                .AsNoTracking()
                .Where(ipp => ipp.IdentificationProcessId == identificationProcessId && ipp.IncludedAfterDedup)
                .Select(ipp => ipp.PaperId);

            var query = _context.Papers
                .AsNoTracking()
                .Include(p => p.PaperAssignments)
                    .ThenInclude(pa => pa.ProjectMember)
                        .ThenInclude(pm => pm.User)
                .Include(p => p.ScreeningResolutions)
                .Where(p =>
                    !p.IsDeleted &&
                    includedPaperIdsQuery.Contains(p.Id) &&
                    // Exclude papers confirmed as duplicates (CANCEL decision) and pending resolve in this process
                    !_context.DeduplicationResults.Any(dr =>
                        dr.PaperId == p.Id &&
                        dr.ProjectId == p.ProjectId && (
                        dr.ResolvedDecision == DuplicateResolutionDecision.CANCEL || dr.ReviewStatus == DeduplicationReviewStatus.Pending)));

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(p =>
                    (p.Title != null && p.Title.ToLower().Contains(searchLower)) ||
                    (p.DOI != null && p.DOI.ToLower().Contains(searchLower)) ||
                    (p.Authors != null && p.Authors.ToLower().Contains(searchLower)));
            }

            // Apply year filter
            if (year.HasValue)
            {
                query = query.Where(p => p.PublicationYearInt == year.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply ordering and pagination
            var papers = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (papers, totalCount);
        }

        public async Task<(List<Paper> Papers, int TotalCount)> GetPapersMissingExternalDataAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Papers
                .AsNoTracking()
                .Where(p => !p.IsDeleted && !p.ExternalDataFetched && !string.IsNullOrWhiteSpace(p.DOI));

            var totalCount = await query.CountAsync(cancellationToken);

            var papers = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (papers, totalCount);
        }

        public async Task<(List<Paper> Papers, int TotalCount)> GetUniquePapersByDataExtractionProcessAsync(
            Guid dataExtractionProcessId,
            string? search,
            int? year,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Papers
                .AsNoTracking()
                .Include(p => p.PaperAssignments)
                    .ThenInclude(pa => pa.ProjectMember)
                        .ThenInclude(pm => pm.User)
                .Include(p => p.ScreeningResolutions)
                .Where(p => !p.IsDeleted && _context.ExtractionPaperTasks.Any(ept => ept.PaperId == p.Id && ept.DataExtractionProcessId == dataExtractionProcessId));

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(p =>
                    (p.Title != null && p.Title.ToLower().Contains(searchLower)) ||
                    (p.DOI != null && p.DOI.ToLower().Contains(searchLower)) ||
                    (p.Authors != null && p.Authors.ToLower().Contains(searchLower)));
            }

            // Apply year filter
            if (year.HasValue)
            {
                query = query.Where(p => p.PublicationYearInt == year.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply ordering and pagination
            var papers = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (papers, totalCount);
        }

        public async Task<(List<Paper> Papers, int TotalCount)> GetPapersByIdsAsync(
            List<Guid> paperIds,
            string? search,
            int? year,
            Guid? searchSourceId,
            AssignmentFilterStatus assignmentStatus,
            ResolutionFilterStatus resolutionStatus,
            ScreeningPhase? phase,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Papers
                .AsNoTracking()
                .Include(p => phase.HasValue
                    ? p.PaperAssignments.Where(pa => pa.Phase == phase.Value)
                    : p.PaperAssignments)
                    .ThenInclude(pa => pa.ProjectMember)
                        .ThenInclude(pm => pm.User)
                .Include(p => phase.HasValue
                    ? p.ScreeningResolutions.Where(sr => sr.Phase == phase.Value)
                    : p.ScreeningResolutions)
                .Where(p => !p.IsDeleted && paperIds.Contains(p.Id));

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(p =>
                    (p.Title != null && p.Title.ToLower().Contains(searchLower)) ||
                    (p.DOI != null && p.DOI.ToLower().Contains(searchLower)) ||
                    (p.Authors != null && p.Authors.ToLower().Contains(searchLower)));
            }

            if (year.HasValue)
            {
                query = query.Where(p => p.PublicationYearInt == year.Value);
            }

            if (searchSourceId.HasValue)
            {
                var sourceId = searchSourceId.Value;
                query = query.Where(p =>
                    p.SearchSourceId == sourceId ||
                    (p.SearchSourceId == null &&
                     p.ImportBatch != null));
            }

            // Apply assignment status filter
            if (assignmentStatus != AssignmentFilterStatus.All)
            {
                if (assignmentStatus == AssignmentFilterStatus.Assigned)
                {
                    query = query.Where(p => phase.HasValue
                        ? p.PaperAssignments.Any(pa => pa.Phase == phase.Value)
                        : p.PaperAssignments.Any());
                }
                else if (assignmentStatus == AssignmentFilterStatus.Unassigned)
                {
                    query = query.Where(p => phase.HasValue
                        ? !p.PaperAssignments.Any(pa => pa.Phase == phase.Value)
                        : !p.PaperAssignments.Any());
                }
            }

            // Apply resolution status filter
            if (resolutionStatus != ResolutionFilterStatus.All)
            {
                if (resolutionStatus == ResolutionFilterStatus.NotDecided)
                {
                    query = query.Where(p => phase.HasValue
                        ? !p.ScreeningResolutions.Any(sr => sr.Phase == phase.Value)
                        : !p.ScreeningResolutions.Any());
                }
                else if (resolutionStatus == ResolutionFilterStatus.Include)
                {
                    query = query.Where(p => phase.HasValue
                        ? p.ScreeningResolutions.Any(sr => sr.Phase == phase.Value && sr.FinalDecision == ScreeningDecisionType.Include)
                        : p.ScreeningResolutions.Any(sr => sr.FinalDecision == ScreeningDecisionType.Include));
                }
                else if (resolutionStatus == ResolutionFilterStatus.Exclude)
                {
                    query = query.Where(p => phase.HasValue
                        ? p.ScreeningResolutions.Any(sr => sr.Phase == phase.Value && sr.FinalDecision == ScreeningDecisionType.Exclude)
                        : p.ScreeningResolutions.Any(sr => sr.FinalDecision == ScreeningDecisionType.Exclude));
                }
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var papers = await query
                .OrderBy(p => p.Title)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (papers, totalCount);
        }

        public async Task<(List<Paper> Papers, int TotalCount)> GetPapersByIdsAsync(
            List<Guid> paperIds,
            Guid? searchSourceId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Papers
                .AsNoTracking()
                .Include(p => p.PaperAssignments)
                    .ThenInclude(pa => pa.ProjectMember)
                        .ThenInclude(pm => pm.User)
                .Where(p => !p.IsDeleted && paperIds.Contains(p.Id));

            if (searchSourceId.HasValue)
            {
                var sourceId = searchSourceId.Value;
                query = query.Where(p =>
                    p.SearchSourceId == sourceId ||
                    (p.SearchSourceId == null &&
                     p.ImportBatch != null));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var papers = await query
                .OrderBy(p => p.Title)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (papers, totalCount);
        }



        public async Task<List<Paper>> GetTopCitedPapersAsync(int topN, CancellationToken cancellationToken = default)
        {
            return await _context.Papers
                .AsNoTracking()
                .Include(p => p.IncomingCitations)
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.IncomingCitations.Count)
                .Take(topN)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Paper>> GetPapersWithCitationCountByIdsAsync(IEnumerable<Guid> paperIds, CancellationToken cancellationToken = default)
        {
            return await _context.Papers
                .AsNoTracking()
                .Include(p => p.IncomingCitations)
                .Where(p => !p.IsDeleted && paperIds.Contains(p.Id))
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Paper>> FindAllWithEmbeddingAsync(
            System.Linq.Expressions.Expression<Func<Paper, bool>>? predicate = null,
            bool isTracking = true,
            CancellationToken cancellationToken = default)
        {
            IQueryable<Paper> query = _context.Papers.Include(p => p.TitleEmbedding);

            query = query.Where(p => !p.IsDeleted);

            if (!isTracking)
                query = query.AsNoTracking();

            if (predicate != null)
                query = query.Where(predicate);

            return await query.ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Get final dataset papers eligible for enrichment after identification completion.
        /// Reuses the same filtering logic as GetUniquePapersByIdentificationProcessAsync,
        /// plus enrichment eligibility: has DOI, not already enriched, not currently processing.
        /// </summary>
        public async Task<List<Paper>> GetFinalDatasetPapersForEnrichmentAsync(
            Guid identificationProcessId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Papers
                .Where(p =>
                    !p.IsDeleted &&
                    // Same unique papers logic (from GetUniquePapersByIdentificationProcessAsync)

                    !_context.DeduplicationResults.Any(dr =>
                        dr.PaperId == p.Id &&
                        dr.ProjectId == p.ProjectId && (
                        dr.ResolvedDecision == DuplicateResolutionDecision.CANCEL || dr.ReviewStatus == DeduplicationReviewStatus.Pending)) &&
                    // Enrichment eligibility filters
                    !string.IsNullOrWhiteSpace(p.DOI) &&
                    !p.ExternalDataFetched &&
                    p.EnrichmentStatus != Entities.Enums.EnrichmentStatus.Processing &&
                    p.EnrichmentStatus != Entities.Enums.EnrichmentStatus.Completed)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Paper>> FindAllWithLimitAsync(
            System.Linq.Expressions.Expression<Func<Paper, bool>> predicate,
            int limit,
            bool isTracking = true,
            CancellationToken cancellationToken = default)
        {
            IQueryable<Paper> query = _context.Papers;

            query = query.Where(p => !p.IsDeleted);

            if (!isTracking)
                query = query.AsNoTracking();

            return await query
                .Where(predicate)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Paper>> GetPapersWithQaDetailsByIdsAsync(
            IEnumerable<Guid> paperIds,
            Guid qaProcessId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Papers
                .Where(p => !p.IsDeleted)
                .Include(p => p.QualityAssessmentAssignments.Where(a => a.QualityAssessmentProcessId == qaProcessId))
                    .ThenInclude(a => a.User)
                .Include(p => p.QualityAssessmentDecisions)
                    .ThenInclude(d => d.Reviewer)
                .Include(p => p.QualityAssessmentDecisions)
                    .ThenInclude(d => d.DecisionItems)
                .Where(p => paperIds.Contains(p.Id))
                .ToListAsync(cancellationToken);
        }

        public async Task<Paper?> GetForAiEvaluationAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Papers
                .AsNoTracking()
                .Include(p => p.StudySelectionProcessPapers)
                .Include(p => p.PaperAssignments)
                    .ThenInclude(pa => pa.ProjectMember)
                .Include(p => p.PaperPdfs)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);
        }
    }
}
