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
                    Method = deduplicationResult.Method,
                    MethodText = deduplicationResult.Method.ToString(),
                    ConfidenceScore = deduplicationResult.ConfidenceScore,
                    DeduplicationNotes = deduplicationResult.Notes,
                    DetectedAt = deduplicationResult.CreatedAt
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
