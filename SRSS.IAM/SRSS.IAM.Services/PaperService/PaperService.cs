using Microsoft.EntityFrameworkCore;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.Common;
using SRSS.IAM.Services.DTOs.Paper;

namespace SRSS.IAM.Services.PaperService
{
    public class PaperService : IPaperService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PaperService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PaginatedResponse<PaperResponse>> GetPapersByProjectAsync(
            Guid projectId,
            PaperListRequest request,
            CancellationToken cancellationToken = default)
        {
            // Validate project exists
            var project = await _unitOfWork.SystematicReviewProjects.FindSingleAsync(
                p => p.Id == projectId,
                cancellationToken: cancellationToken);

            if (project == null)
            {
                throw new InvalidOperationException($"Project with ID {projectId} not found.");
            }

            // Validate pagination parameters
            if (request.PageNumber < 1)
            {
                request.PageNumber = 1;
            }

            if (request.PageSize < 1)
            {
                request.PageSize = 20;
            }

            if (request.PageSize > 100)
            {
                request.PageSize = 100;
            }

            // Get papers with filtering and pagination
            var (papers, totalCount) = await _unitOfWork.Papers.GetPapersByProjectAsync(
                projectId,
                request.Search,
                request.Status,
                request.Year,
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            // Map to response DTOs
            var paperResponses = papers.Select(MapToPaperResponse).ToList();

            return new PaginatedResponse<PaperResponse>
            {
                Items = paperResponses,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        /// <summary>
        /// Get duplicate papers for a specific identification process
        /// Queries DeduplicationResult table for process-scoped results
        /// </summary>
        public async Task<PaginatedResponse<DuplicatePaperResponse>> GetDuplicatePapersByIdentificationProcessAsync(
            Guid identificationProcessId,
            DuplicatePapersRequest request,
            CancellationToken cancellationToken = default)
        {
            // Validate identification process exists
            var identificationProcess = await _unitOfWork.IdentificationProcesses.FindSingleAsync(
                ip => ip.Id == identificationProcessId,
                cancellationToken: cancellationToken);

            if (identificationProcess == null)
            {
                throw new InvalidOperationException($"IdentificationProcess with ID {identificationProcessId} not found.");
            }

            // Validate pagination parameters
            if (request.PageNumber < 1)
            {
                request.PageNumber = 1;
            }

            if (request.PageSize < 1)
            {
                request.PageSize = 20;
            }

            if (request.PageSize > 100)
            {
                request.PageSize = 100;
            }

            // Get duplicate papers with deduplication metadata
            var (papers, deduplicationResults, totalCount) = await _unitOfWork.Papers.GetDuplicatePapersByIdentificationProcessAsync(
                identificationProcessId,
                request.Search,
                request.Year,
                request.SortBy,
                request.SortOrder,
                request.ReviewStatus,
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            // Map to response DTOs with deduplication metadata
            var duplicateResponses = new List<DuplicatePaperResponse>();
            for (int i = 0; i < papers.Count; i++)
            {
                var paper = papers[i];
                var deduplicationResult = deduplicationResults[i];

                duplicateResponses.Add(new DuplicatePaperResponse
                {
                    // Paper metadata
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
                    Source = paper.Source,
                    ImportedAt = paper.ImportedAt,
                    ImportedBy = paper.ImportedBy,
                    // Selection status NOT stored in Paper - must query from ScreeningResolution
                    SelectionStatus = null,
                    SelectionStatusText = null,
                    PdfUrl = paper.PdfUrl,
                    FullTextAvailable = paper.FullTextAvailable,
                    AccessType = paper.AccessType,
                    AccessTypeText = paper.AccessType?.ToString(),
                    CreatedAt = paper.CreatedAt,
                    ModifiedAt = paper.ModifiedAt,

                    // Deduplication metadata
                    DuplicateOfPaperId = deduplicationResult.DuplicateOfPaperId,
                    DuplicateOfTitle = deduplicationResult.DuplicateOfPaper?.Title,
                    DuplicateOfAuthors = deduplicationResult.DuplicateOfPaper?.Authors,
                    DuplicateOfYear = deduplicationResult.DuplicateOfPaper?.PublicationYear,
                    DuplicateOfDoi = deduplicationResult.DuplicateOfPaper?.DOI,
                    DuplicateOfSource = deduplicationResult.DuplicateOfPaper?.Source,
                    DuplicateOfAbstract = deduplicationResult.DuplicateOfPaper?.Abstract,
                    Method = deduplicationResult.Method,
                    MethodText = deduplicationResult.Method.ToString(),
                    ConfidenceScore = deduplicationResult.ConfidenceScore,
                    DeduplicationNotes = deduplicationResult.Notes,
                    DetectedAt = deduplicationResult.CreatedAt,
                    ReviewStatus = deduplicationResult.ReviewStatus,
                    ReviewStatusText = deduplicationResult.ReviewStatus.ToString(),
                    ReviewedBy = deduplicationResult.ReviewedBy,
                    ReviewedAt = deduplicationResult.ReviewedAt
                });
            }

            return new PaginatedResponse<DuplicatePaperResponse>
            {
                Items = duplicateResponses,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<PaperResponse> GetPaperByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var paper = await _unitOfWork.Papers.FindSingleAsync(
                p => p.Id == id,
                cancellationToken: cancellationToken);

            if (paper == null)
            {
                throw new InvalidOperationException($"Paper with ID {id} not found.");
            }

            return MapToPaperResponse(paper);
        }

        public async Task<PaginatedResponse<PaperResponse>> GetUniquePapersByIdentificationProcessAsync(
            Guid identificationProcessId,
            PaperListRequest request,
            CancellationToken cancellationToken = default)
        {
            var identificationProcess = await _unitOfWork.IdentificationProcesses.FindSingleAsync(
                ip => ip.Id == identificationProcessId,
                cancellationToken: cancellationToken);

            if (identificationProcess == null)
            {
                throw new InvalidOperationException($"IdentificationProcess with ID {identificationProcessId} not found.");
            }

            if (request.PageNumber < 1)
            {
                request.PageNumber = 1;
            }

            if (request.PageSize < 1)
            {
                request.PageSize = 20;
            }

            if (request.PageSize > 100)
            {
                request.PageSize = 100;
            }

            var (papers, totalCount) = await _unitOfWork.Papers.GetUniquePapersByIdentificationProcessAsync(
                identificationProcessId,
                request.Search,
                request.Year,
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            var paperResponses = papers.Select(MapToPaperResponse).ToList();

            return new PaginatedResponse<PaperResponse>
            {
                Items = paperResponses,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<PaginatedResponse<PaperResponse>> SearchPapersAsync(
            Guid projectId,
            PaperSearchRequest request,
            CancellationToken cancellationToken = default)
        {
            // TODO: Implement advanced search
            throw new NotImplementedException("SearchPapersAsync not yet implemented.");
        }

        public async Task<DuplicatePaperResponse> ResolveDuplicateAsync(
            Guid identificationProcessId,
            Guid deduplicationResultId,
            ResolveDuplicateRequest request,
            CancellationToken cancellationToken = default)
        {
            var deduplicationResult = await _unitOfWork.DeduplicationResults.FindSingleAsync(
                dr => dr.Id == deduplicationResultId && dr.IdentificationProcessId == identificationProcessId,
                isTracking: true,
                cancellationToken);

            if (deduplicationResult == null)
            {
                throw new InvalidOperationException(
                    $"DeduplicationResult with ID {deduplicationResultId} not found for IdentificationProcess {identificationProcessId}.");
            }

            deduplicationResult.ReviewStatus = request.Resolution;
            deduplicationResult.ReviewedBy = request.ReviewedBy;
            deduplicationResult.ReviewedAt = DateTimeOffset.UtcNow;

            // Map enum-based resolution to decision string for consistent filtering
            deduplicationResult.ResolvedDecision = request.Resolution switch
            {
                DeduplicationReviewStatus.Confirmed => "keep-original",
                DeduplicationReviewStatus.Rejected => "keep-both",
                _ => null
            };

            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                deduplicationResult.Notes = request.Notes;
            }

            deduplicationResult.ModifiedAt = DateTimeOffset.UtcNow;

            // Set IsRemovedAsDuplicate: Confirmed means keep-original → remove the duplicate paper
            if (request.Resolution == DeduplicationReviewStatus.Confirmed)
            {
                var removedPaperId = deduplicationResult.PaperId;
                var survivingPaperId = deduplicationResult.DuplicateOfPaperId;

                var duplicatePaper = await _unitOfWork.Papers.FindSingleAsync(
                    p => p.Id == removedPaperId,
                    isTracking: true,
                    cancellationToken);
                if (duplicatePaper != null)
                {
                    duplicatePaper.IsRemovedAsDuplicate = true;
                    duplicatePaper.ModifiedAt = DateTimeOffset.UtcNow;
                }

                // Cascade: re-point pending pairs that reference the removed paper as "original"
                await CascadeOriginalPaperChangeAsync(removedPaperId, survivingPaperId, deduplicationResultId, cancellationToken);
            }
            // Rejected = keep-both → neither paper is removed, no cascade needed

            await _unitOfWork.DeduplicationResults.UpdateAsync(deduplicationResult, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Reload with navigation properties for response mapping
            var paper = await _unitOfWork.Papers.FindSingleAsync(
                p => p.Id == deduplicationResult.PaperId,
                cancellationToken: cancellationToken);

            var originalPaper = await _unitOfWork.Papers.FindSingleAsync(
                p => p.Id == deduplicationResult.DuplicateOfPaperId,
                cancellationToken: cancellationToken);

            return new DuplicatePaperResponse
            {
                Id = paper!.Id,
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
                Source = paper.Source,
                ImportedAt = paper.ImportedAt,
                ImportedBy = paper.ImportedBy,
                SelectionStatus = null,
                SelectionStatusText = null,
                PdfUrl = paper.PdfUrl,
                FullTextAvailable = paper.FullTextAvailable,
                AccessType = paper.AccessType,
                AccessTypeText = paper.AccessType?.ToString(),
                CreatedAt = paper.CreatedAt,
                ModifiedAt = paper.ModifiedAt,
                DuplicateOfPaperId = deduplicationResult.DuplicateOfPaperId,
                DuplicateOfTitle = originalPaper?.Title,
                DuplicateOfAuthors = originalPaper?.Authors,
                DuplicateOfYear = originalPaper?.PublicationYear,
                DuplicateOfDoi = originalPaper?.DOI,
                DuplicateOfSource = originalPaper?.Source,
                DuplicateOfAbstract = originalPaper?.Abstract,
                Method = deduplicationResult.Method,
                MethodText = deduplicationResult.Method.ToString(),
                ConfidenceScore = deduplicationResult.ConfidenceScore,
                DeduplicationNotes = deduplicationResult.Notes,
                DetectedAt = deduplicationResult.CreatedAt,
                ReviewStatus = deduplicationResult.ReviewStatus,
                ReviewStatusText = deduplicationResult.ReviewStatus.ToString(),
                ReviewedBy = deduplicationResult.ReviewedBy,
                ReviewedAt = deduplicationResult.ReviewedAt
            };
        }

        public async Task<PaginatedResponse<DuplicatePairResponse>> GetDuplicatePairsAsync(
            Guid identificationProcessId,
            DuplicatePairsRequest request,
            CancellationToken cancellationToken = default)
        {
            var identificationProcess = await _unitOfWork.IdentificationProcesses.FindSingleAsync(
                ip => ip.Id == identificationProcessId,
                cancellationToken: cancellationToken);

            if (identificationProcess == null)
            {
                throw new InvalidOperationException($"IdentificationProcess with ID {identificationProcessId} not found.");
            }

            if (request.PageNumber < 1) request.PageNumber = 1;
            if (request.PageSize < 1) request.PageSize = 20;
            if (request.PageSize > 100) request.PageSize = 100;

            var (results, totalCount) = await _unitOfWork.DeduplicationResults.GetDuplicatePairsAsync(
                identificationProcessId,
                request.Search,
                request.Status,
                request.MinConfidence,
                request.Method,
                request.SortBy,
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            var pairResponses = results.Select(dr => new DuplicatePairResponse
            {
                Id = dr.Id,
                OriginalPaper = MapToPairPaperDto(dr.DuplicateOfPaper),
                DuplicatePaper = MapToPairPaperDto(dr.Paper),
                Method = dr.Method,
                MethodText = dr.Method.ToString(),
                ConfidenceScore = dr.ConfidenceScore,
                DeduplicationNotes = dr.Notes,
                ReviewStatus = dr.ReviewStatus,
                ReviewStatusText = dr.ReviewStatus.ToString(),
                ReviewedBy = dr.ReviewedBy,
                ReviewedAt = dr.ReviewedAt,
                DetectedAt = dr.CreatedAt,
                ResolvedDecision = dr.ResolvedDecision
            }).ToList();

            return new PaginatedResponse<DuplicatePairResponse>
            {
                Items = pairResponses,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<ResolveDuplicatePairResponse> ResolveDuplicatePairAsync(
            Guid identificationProcessId,
            Guid pairId,
            ResolveDuplicatePairRequest request,
            CancellationToken cancellationToken = default)
        {
            var validDecisions = new[] { "keep-original", "keep-duplicate", "keep-both" };
            if (!validDecisions.Contains(request.Decision))
            {
                throw new ArgumentException(
                    $"Invalid decision '{request.Decision}'. Must be one of: {string.Join(", ", validDecisions)}.");
            }

            var deduplicationResult = await _unitOfWork.DeduplicationResults.FindSingleAsync(
                dr => dr.Id == pairId && dr.IdentificationProcessId == identificationProcessId,
                isTracking: true,
                cancellationToken);

            if (deduplicationResult == null)
            {
                throw new InvalidOperationException(
                    $"DeduplicationResult with ID {pairId} not found for IdentificationProcess {identificationProcessId}.");
            }

            if (deduplicationResult.ReviewStatus != DeduplicationReviewStatus.Pending)
            {
                throw new InvalidOperationException(
                    $"Duplicate pair {pairId} has already been resolved with status '{deduplicationResult.ReviewStatus}'.");
            }

            deduplicationResult.ReviewStatus = request.Decision == "keep-both"
                ? DeduplicationReviewStatus.Rejected
                : DeduplicationReviewStatus.Confirmed;
            deduplicationResult.ResolvedDecision = request.Decision;
            deduplicationResult.ReviewedAt = DateTimeOffset.UtcNow;
            deduplicationResult.ModifiedAt = DateTimeOffset.UtcNow;

            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                deduplicationResult.Notes = request.Notes;
            }

            // Set IsRemovedAsDuplicate on the paper that should be removed
            // and cascade: re-point other pending pairs to the new survivor
            if (request.Decision == "keep-original")
            {
                var removedPaperId = deduplicationResult.PaperId;
                var survivingPaperId = deduplicationResult.DuplicateOfPaperId;

                // Remove the duplicate paper (PaperId)
                var duplicatePaper = await _unitOfWork.Papers.FindSingleAsync(
                    p => p.Id == removedPaperId,
                    isTracking: true,
                    cancellationToken);
                if (duplicatePaper != null)
                {
                    duplicatePaper.IsRemovedAsDuplicate = true;
                    duplicatePaper.ModifiedAt = DateTimeOffset.UtcNow;
                }

                // Cascade: re-point pending pairs that reference the removed paper as "original"
                await CascadeOriginalPaperChangeAsync(removedPaperId, survivingPaperId, pairId, cancellationToken);
            }
            else if (request.Decision == "keep-duplicate")
            {
                var removedPaperId = deduplicationResult.DuplicateOfPaperId;
                var survivingPaperId = deduplicationResult.PaperId;

                // Remove the original paper (DuplicateOfPaperId)
                var originalPaper = await _unitOfWork.Papers.FindSingleAsync(
                    p => p.Id == removedPaperId,
                    isTracking: true,
                    cancellationToken);
                if (originalPaper != null)
                {
                    originalPaper.IsRemovedAsDuplicate = true;
                    originalPaper.ModifiedAt = DateTimeOffset.UtcNow;
                }

                // Cascade: re-point pending pairs that reference the removed paper as "original"
                await CascadeOriginalPaperChangeAsync(removedPaperId, survivingPaperId, pairId, cancellationToken);
            }
            // "keep-both" → neither paper is removed, no cascade needed

            await _unitOfWork.DeduplicationResults.UpdateAsync(deduplicationResult, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ResolveDuplicatePairResponse
            {
                Id = deduplicationResult.Id,
                ReviewStatus = deduplicationResult.ReviewStatus,
                ReviewStatusText = deduplicationResult.ReviewStatus.ToString(),
                ResolvedDecision = deduplicationResult.ResolvedDecision,
                ReviewedAt = deduplicationResult.ReviewedAt,
                ReviewedBy = deduplicationResult.ReviewedBy
            };
        }

        /// <summary>
        /// When a paper is removed as duplicate, re-point all other pending dedup pairs
        /// that reference it as the "original" (DuplicateOfPaperId) to the new surviving paper.
        /// This prevents stale references where a pending pair points to a removed paper.
        /// </summary>
        private async Task CascadeOriginalPaperChangeAsync(
            Guid removedPaperId,
            Guid survivingPaperId,
            Guid currentPairId,
            CancellationToken cancellationToken)
        {
            var pendingPairs = await _unitOfWork.DeduplicationResults.FindAllAsync(
                dr => dr.DuplicateOfPaperId == removedPaperId
                    && dr.Id != currentPairId
                    && dr.ReviewStatus == DeduplicationReviewStatus.Pending,
                isTracking: true,
                cancellationToken);

            foreach (var pair in pendingPairs)
            {
                pair.DuplicateOfPaperId = survivingPaperId;
                pair.ModifiedAt = DateTimeOffset.UtcNow;
            }
        }

        private static DuplicatePairPaperDto MapToPairPaperDto(Paper paper)
        {
            return new DuplicatePairPaperDto
            {
                Id = paper.Id,
                Title = paper.Title,
                Authors = paper.Authors,
                Abstract = paper.Abstract,
                DOI = paper.DOI,
                PublicationType = paper.PublicationType,
                PublicationYear = paper.PublicationYear,
                PublicationYearInt = paper.PublicationYearInt,
                Source = paper.Source,
                Journal = paper.Journal,
                Keywords = paper.Keywords,
                Url = paper.Url,
                ImportedAt = paper.ImportedAt
            };
        }

        private static PaperResponse MapToPaperResponse(Paper paper)
        {
            return new PaperResponse
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
                Source = paper.Source,
                ImportedAt = paper.ImportedAt,
                ImportedBy = paper.ImportedBy,
                // SelectionStatus is NOT stored in Paper - must be queried from ScreeningResolution
                SelectionStatus = null,
                SelectionStatusText = null,
                PdfUrl = paper.PdfUrl,
                FullTextAvailable = paper.FullTextAvailable,
                AccessType = paper.AccessType,
                AccessTypeText = paper.AccessType?.ToString(),
                CreatedAt = paper.CreatedAt,
                ModifiedAt = paper.ModifiedAt
            };
        }
    }
}
