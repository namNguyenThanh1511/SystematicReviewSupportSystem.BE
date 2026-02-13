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

        public async Task<PaginatedResponse<PaperResponse>> GetDuplicatePapersByProjectAsync(
            Guid projectId,
            DuplicatePapersRequest request,
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

            // Get duplicate papers with filtering and pagination
            var (papers, totalCount) = await _unitOfWork.Papers.GetDuplicatePapersByProjectAsync(
                projectId,
                request.Search,
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
                CurrentSelectionStatus = paper.CurrentSelectionStatus,
                CurrentSelectionStatusText = paper.CurrentSelectionStatus.ToString(),
                IsIncludedFinal = paper.IsIncludedFinal,
                LastDecisionAt = paper.LastDecisionAt,
                IsDuplicate = paper.IsDuplicate,
                DuplicateOfId = paper.DuplicateOfId,
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
