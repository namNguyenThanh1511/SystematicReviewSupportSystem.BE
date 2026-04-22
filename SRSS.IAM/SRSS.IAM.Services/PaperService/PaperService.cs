using Microsoft.EntityFrameworkCore;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.Common;
using SRSS.IAM.Services.DTOs.Paper;
using SRSS.IAM.Services.DTOs.StudySelection;
using SRSS.IAM.Services.NotificationService;
using SRSS.IAM.Services.StudySelectionService;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SRSS.IAM.Services.DTOs.Identification;

namespace SRSS.IAM.Services.PaperService
{
    public class PaperService : IPaperService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly MetadataMergeService.IMetadataMergeService _metadataMergeService;
        private readonly ILogger<PaperService> _logger;
        private readonly IStudySelectionService _studySelectionService;

        public PaperService(
            IUnitOfWork unitOfWork,
            INotificationService notificationService,
            MetadataMergeService.IMetadataMergeService metadataMergeService,
            IStudySelectionService studySelectionService,
            ILogger<PaperService> logger)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _metadataMergeService = metadataMergeService;
            _studySelectionService = studySelectionService;
            _logger = logger;
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
                request.AssignmentStatus,
                request.Stage,
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            // Map to response DTOs
            var paperResponses = new List<PaperResponse>();
            foreach (var p in papers)
            {
                paperResponses.Add(await MapToPaperResponseAsync(p, null, null, cancellationToken));
            }

            return new PaginatedResponse<PaperResponse>
            {
                Items = paperResponses,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<PaginatedResponse<PaperResponse>> SearchPapersByProjectAsync(
            Guid projectId,
            PaperSearchQuery query,
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
            if (query.PageNumber < 1)
            {
                query.PageNumber = 1;
            }

            if (query.PageSize < 1)
            {
                query.PageSize = 20;
            }

            if (query.PageSize > 100)
            {
                query.PageSize = 100;
            }

            // Get papers with advanced filtering and pagination
            var (papers, totalCount) = await _unitOfWork.Papers.SearchPapersByProjectAsync(
                projectId,
                query.Search,
                query.SearchStrategyId,
                query.SearchSourceId,
                query.Year,
                query.PageNumber,
                query.PageSize,
                cancellationToken);

            // Map to response DTOs
            var paperResponses = new List<PaperResponse>();
            foreach (var p in papers)
            {
                paperResponses.Add(await MapToPaperResponseAsync(p, null, null, cancellationToken));
            }

            return new PaginatedResponse<PaperResponse>
            {
                Items = paperResponses,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<PaginatedResponse<PaperResponse>> GetPaperPoolAsync(
            Guid projectId,
            PaperPoolQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var project = await _unitOfWork.SystematicReviewProjects.FindSingleAsync(
                p => p.Id == projectId,
                cancellationToken: cancellationToken);

            if (project == null)
            {
                throw new InvalidOperationException($"Project with ID {projectId} not found.");
            }

            if (request.PageNumber < 1) request.PageNumber = 1;
            if (request.PageSize < 1) request.PageSize = 20;
            if (request.PageSize > 100) request.PageSize = 100;

            Guid? searchSourceId = null;
            if (!string.IsNullOrWhiteSpace(request.SearchSourceId) &&
                !request.SearchSourceId.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                searchSourceId = Guid.Parse(request.SearchSourceId);
            }

            Guid? importBatchId = null;
            if (!string.IsNullOrWhiteSpace(request.ImportBatchId) &&
                !request.ImportBatchId.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                importBatchId = Guid.Parse(request.ImportBatchId);
            }

            var (papers, totalCount) = await _unitOfWork.Papers.GetPaperPoolByProjectAsync(
                projectId,
                request.SearchText,
                request.Keyword,
                request.YearFrom,
                request.YearTo,
                searchSourceId,
                importBatchId,
                request.DoiState,
                request.FullTextState,
                request.OnlyUnused,
                request.RecentlyImported,
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            var items = new List<PaperResponse>();
            foreach (var paper in papers)
            {
                items.Add(await MapToPaperResponseAsync(paper, null, null, cancellationToken));
            }

            return new PaginatedResponse<PaperResponse>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<PaperPoolFilterMetadataResponse> GetFilterMetadataAsync(
            Guid projectId,
            CancellationToken cancellationToken = default)
        {
            var project = await _unitOfWork.SystematicReviewProjects.FindSingleAsync(
                p => p.Id == projectId,
                cancellationToken: cancellationToken);

            if (project == null)
            {
                throw new InvalidOperationException($"Project with ID {projectId} not found.");
            }

            var searchSources = await _unitOfWork.SearchSources.FindAllAsync(
                x => x.ProjectId == projectId,
                isTracking: false,
                cancellationToken: cancellationToken);

            var importBatches = await _unitOfWork.ImportBatches.FindAllAsync(
                x => x.Project.Id == projectId,
                isTracking: false,
                cancellationToken: cancellationToken);

            return new PaperPoolFilterMetadataResponse
            {
                SearchSources = searchSources
                    .OrderBy(x => x.Name)
                    .Select(x => new FilterOptionResponse { Id = x.Id, Name = x.Name })
                    .ToList(),
                ImportBatches = importBatches
                    .OrderByDescending(x => x.ImportedAt)
                    .Select(x => new FilterOptionResponse
                    {
                        Id = x.Id,
                        Name = string.IsNullOrWhiteSpace(x.FileName) ? $"Batch {x.ImportedAt:yyyy-MM-dd HH:mm}" : x.FileName
                    })
                    .ToList()
            };
        }

        public async Task<List<FilterSettingResponse>> GetFilterSettingsAsync(
            Guid projectId,
            CancellationToken cancellationToken = default)
        {
            var filterSettings = await _unitOfWork.FilterSettings.FindAllAsync(
                x => x.ProjectId == projectId,
                isTracking: false,
                cancellationToken: cancellationToken);

            return filterSettings
                .OrderByDescending(x => x.CreatedAt)
                .Select(MapFilterSettingResponse)
                .ToList();
        }

        public async Task<FilterSettingResponse> GetFilterSettingByIdAsync(
            Guid projectId,
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var filterSetting = await _unitOfWork.FilterSettings.FindSingleAsync(
                x => x.ProjectId == projectId && x.Id == id,
                isTracking: false,
                cancellationToken: cancellationToken);

            if (filterSetting == null)
            {
                throw new InvalidOperationException("Filter setting not found.");
            }

            return MapFilterSettingResponse(filterSetting);
        }

        public async Task<FilterSettingResponse> CreateFilterSettingAsync(
            Guid projectId,
            FilterSettingRequest request,
            CancellationToken cancellationToken = default)
        {
            ValidateFilterSettingRequest(request);

            var duplicate = await _unitOfWork.FilterSettings.FindSingleAsync(
                x => x.ProjectId == projectId && x.Name.ToLower() == request.Name.Trim().ToLower(),
                cancellationToken: cancellationToken);

            if (duplicate != null)
            {
                throw new InvalidOperationException("Filter name already exists in this project.");
            }

            var filterSetting = new FilterSetting
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                Name = request.Name.Trim(),
                SearchText = request.SearchText?.Trim(),
                Keyword = request.Filters.Keyword?.Trim(),
                YearFrom = request.Filters.YearFrom,
                YearTo = request.Filters.YearTo,
                SearchSourceId = ParseOptionalGuid(request.Filters.SearchSourceId),
                ImportBatchId = ParseOptionalGuid(request.Filters.ImportBatchId),
                DoiState = NormalizeState(request.Filters.DoiState),
                FullTextState = NormalizeState(request.Filters.FullTextState),
                OnlyUnused = request.Filters.OnlyUnused,
                RecentlyImported = request.Filters.RecentlyImported
            };

            await _unitOfWork.FilterSettings.AddAsync(filterSetting, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapFilterSettingResponse(filterSetting);
        }

        public async Task<FilterSettingResponse> UpdateFilterSettingAsync(
            Guid projectId,
            Guid id,
            FilterSettingRequest request,
            CancellationToken cancellationToken = default)
        {
            ValidateFilterSettingRequest(request);

            var filterSetting = await _unitOfWork.FilterSettings.FindSingleAsync(
                x => x.Id == id && x.ProjectId == projectId,
                isTracking: true,
                cancellationToken: cancellationToken);

            if (filterSetting == null)
            {
                throw new InvalidOperationException("Filter setting not found.");
            }

            var duplicate = await _unitOfWork.FilterSettings.FindSingleAsync(
                x => x.ProjectId == projectId && x.Id != id && x.Name.ToLower() == request.Name.Trim().ToLower(),
                cancellationToken: cancellationToken);

            if (duplicate != null)
            {
                throw new InvalidOperationException("Filter name already exists in this project.");
            }

            filterSetting.Name = request.Name.Trim();
            filterSetting.SearchText = request.SearchText?.Trim();
            filterSetting.Keyword = request.Filters.Keyword?.Trim();
            filterSetting.YearFrom = request.Filters.YearFrom;
            filterSetting.YearTo = request.Filters.YearTo;
            filterSetting.SearchSourceId = ParseOptionalGuid(request.Filters.SearchSourceId);
            filterSetting.ImportBatchId = ParseOptionalGuid(request.Filters.ImportBatchId);
            filterSetting.DoiState = NormalizeState(request.Filters.DoiState);
            filterSetting.FullTextState = NormalizeState(request.Filters.FullTextState);
            filterSetting.OnlyUnused = request.Filters.OnlyUnused;
            filterSetting.RecentlyImported = request.Filters.RecentlyImported;

            await _unitOfWork.FilterSettings.UpdateAsync(filterSetting, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapFilterSettingResponse(filterSetting);
        }

        public async Task DeleteFilterSettingAsync(
            Guid projectId,
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var filterSetting = await _unitOfWork.FilterSettings.FindSingleAsync(
                x => x.Id == id && x.ProjectId == projectId,
                isTracking: true,
                cancellationToken: cancellationToken);

            if (filterSetting == null)
            {
                throw new InvalidOperationException("Filter setting not found.");
            }

            await _unitOfWork.FilterSettings.RemoveAsync(filterSetting, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Get duplicate papers for a specific project
        /// Queries DeduplicationResult table for project-scoped results
        /// </summary>
        public async Task<PaginatedResponse<DuplicatePaperResponse>> GetDuplicatePapersByProjectAsync(
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

            // Get duplicate papers with deduplication metadata
            var (papers, deduplicationResults, totalCount) = await _unitOfWork.Papers.GetDuplicatePapersByProjectAsync(
                projectId,
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
                    SearchSourceId = paper.SearchSourceId,
                    ImportedAt = paper.ImportedAt,
                    ImportedBy = paper.ImportedBy,
                    // Selection status NOT stored in Paper - must query from ScreeningResolution
                    SelectionStatus = null,
                    SelectionStatusText = null,
                    Stage = 0,
                    StageText = "TitleAbstract",
                    AssignmentStatus = paper.PaperAssignments?.Any() == true ? 1 : 0,
                    AssignmentStatusText = paper.PaperAssignments?.Any() == true ? "Assigned" : "Unassigned",
                    AssignedReviewers = paper.PaperAssignments?.Select(pa => new AssignedReviewerDto
                    {
                        Id = pa.ProjectMember.UserId,
                        Name = pa.ProjectMember.User?.FullName ?? "Unknown"
                    }).ToList() ?? new List<AssignedReviewerDto>(),
                    PdfUrl = paper.PdfUrl,
                    FullTextRetrievalStatus = paper.FullTextRetrievalStatus,
                    FullTextRetrievalStatusText = paper.FullTextRetrievalStatus.ToString(),
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

        private static string NormalizeState(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? "all" : value.Trim().ToLowerInvariant();
        }

        private static Guid? ParseOptionalGuid(string? value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return Guid.Parse(value);
        }

        private static void ValidateFilterSettingRequest(FilterSettingRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("Filter name is required.");
            }

            if (request.Filters.YearFrom.HasValue &&
                request.Filters.YearTo.HasValue &&
                request.Filters.YearFrom.Value > request.Filters.YearTo.Value)
            {
                throw new ArgumentException("yearFrom must be less than or equal to yearTo.");
            }

            var validStates = new[] { "all", "has", "missing" };
            if (!validStates.Contains(NormalizeState(request.Filters.DoiState)))
            {
                throw new ArgumentException("doiState must be one of: all, has, missing.");
            }

            if (!validStates.Contains(NormalizeState(request.Filters.FullTextState)))
            {
                throw new ArgumentException("fullTextState must be one of: all, has, missing.");
            }
        }

        private static FilterSettingResponse MapFilterSettingResponse(FilterSetting x)
        {
            return new FilterSettingResponse
            {
                Id = x.Id,
                Name = x.Name,
                SearchText = x.SearchText,
                CreatedAt = x.CreatedAt,
                Filters = new FilterStateDto
                {
                    Keyword = x.Keyword,
                    YearFrom = x.YearFrom,
                    YearTo = x.YearTo,
                    SearchSourceId = x.SearchSourceId?.ToString() ?? "all",
                    ImportBatchId = x.ImportBatchId?.ToString() ?? "all",
                    DoiState = x.DoiState,
                    FullTextState = x.FullTextState,
                    OnlyUnused = x.OnlyUnused,
                    RecentlyImported = x.RecentlyImported
                }
            };
        }

        public async Task<PaperDetailsResponse> GetPaperByIdAsync(
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

            return await MapToPaperResponseDetailsAsync(paper, cancellationToken);
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

            var paperResponses = new List<PaperResponse>();
            foreach (var p in papers)
            {
                paperResponses.Add(await MapToPaperResponseAsync(p, null, null, cancellationToken));
            }

            return new PaginatedResponse<PaperResponse>
            {
                Items = paperResponses,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<PaginatedResponse<PaperResponse>> GetUniquePapersByDataExtractionProcessAsync(
            Guid dataExtractionProcessId,
            PaperListRequest request,
            CancellationToken cancellationToken = default)
        {
            var dataExtractionProcess = await _unitOfWork.DataExtractionProcesses.FindSingleAsync(
                dep => dep.Id == dataExtractionProcessId,
                cancellationToken: cancellationToken);

            if (dataExtractionProcess == null)
            {
                throw new InvalidOperationException($"DataExtractionProcess with ID {dataExtractionProcessId} not found.");
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

            var (papers, totalCount) = await _unitOfWork.Papers.GetUniquePapersByDataExtractionProcessAsync(
                dataExtractionProcessId,
                request.Search,
                request.Year,
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            var paperResponses = new List<PaperResponse>();
            foreach (var p in papers)
            {
                paperResponses.Add(await MapToPaperResponseAsync(p, null, null, cancellationToken));
            }

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
            Guid projectId,
            Guid deduplicationResultId,
            ResolveDuplicateRequest request,
            CancellationToken cancellationToken = default)
        {
            var project = await _unitOfWork.SystematicReviewProjects.FindSingleAsync(
                p => p.Id == projectId,
                cancellationToken: cancellationToken);

            if (project == null)
            {
                throw new InvalidOperationException($"Project with ID {projectId} not found.");
            }

            var deduplicationResult = await _unitOfWork.DeduplicationResults.FindSingleAsync(
                dr => dr.Id == deduplicationResultId && dr.ProjectId == projectId,
                isTracking: true,
                cancellationToken);

            if (deduplicationResult == null)
            {
                throw new InvalidOperationException(
                    $"DeduplicationResult with ID {deduplicationResultId} not found for Project {projectId}.");
            }

            deduplicationResult.ResolvedDecision = request.Decision;
            deduplicationResult.ReviewedBy = request.ReviewedBy;
            deduplicationResult.ReviewedAt = DateTimeOffset.UtcNow;
            deduplicationResult.ModifiedAt = DateTimeOffset.UtcNow;

            var duplicatePaper = await _unitOfWork.Papers.FindSingleAsync(
                p => p.Id == deduplicationResult.PaperId,
                isTracking: true,
                cancellationToken: cancellationToken);

            if (duplicatePaper == null)
            {
                throw new InvalidOperationException($"Paper with ID {deduplicationResult.PaperId} not found.");
            }

            if (request.Decision == DuplicateResolutionDecision.CANCEL)
            {
                deduplicationResult.ReviewStatus = DeduplicationReviewStatus.Confirmed;
                duplicatePaper.IsDeleted = true;
                duplicatePaper.ModifiedAt = DateTimeOffset.UtcNow;
            }
            else
            {
                deduplicationResult.ReviewStatus = DeduplicationReviewStatus.Confirmed;
                duplicatePaper.IsDeleted = false;
                duplicatePaper.ModifiedAt = DateTimeOffset.UtcNow;
            }

            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                deduplicationResult.Notes = request.Notes;
            }

            await _unitOfWork.DeduplicationResults.UpdateAsync(deduplicationResult, cancellationToken);
            await _unitOfWork.Papers.UpdateAsync(duplicatePaper, cancellationToken);
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
                SearchSourceId = paper.SearchSourceId,
                ImportedAt = paper.ImportedAt,
                ImportedBy = paper.ImportedBy,
                SelectionStatus = null,
                SelectionStatusText = null,
                PdfUrl = paper.PdfUrl,
                FullTextRetrievalStatus = paper.FullTextRetrievalStatus,
                FullTextRetrievalStatusText = paper.FullTextRetrievalStatus.ToString(),
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
            Guid projectId,
            DuplicatePairsRequest request,
            CancellationToken cancellationToken = default)
        {
            var project = await _unitOfWork.SystematicReviewProjects.FindSingleAsync(
                p => p.Id == projectId,
                cancellationToken: cancellationToken);

            if (project == null)
            {
                throw new InvalidOperationException($"Project with ID {projectId} not found.");
            }

            if (request.PageNumber < 1) request.PageNumber = 1;
            if (request.PageSize < 1) request.PageSize = 20;
            if (request.PageSize > 100) request.PageSize = 100;

            var (results, totalCount) = await _unitOfWork.DeduplicationResults.GetDuplicatePairsAsync(
                projectId,
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
            Guid projectId,
            Guid pairId,
            ResolveDuplicatePairRequest request,
            CancellationToken cancellationToken = default)
        {
            var project = await _unitOfWork.SystematicReviewProjects.FindSingleAsync(
                p => p.Id == projectId,
                cancellationToken: cancellationToken);

            if (project == null)
            {
                throw new InvalidOperationException($"Project with ID {projectId} not found.");
            }

            var deduplicationResult = await _unitOfWork.DeduplicationResults.FindSingleAsync(
                dr => dr.Id == pairId && dr.ProjectId == projectId,
                isTracking: true,
                cancellationToken);

            if (deduplicationResult == null)
            {
                throw new InvalidOperationException(
                    $"DeduplicationResult with ID {pairId} not found for Project {projectId}.");
            }

            if (deduplicationResult.ReviewStatus != DeduplicationReviewStatus.Pending)
            {
                throw new InvalidOperationException(
                    $"Duplicate pair {pairId} has already been resolved with status '{deduplicationResult.ReviewStatus}'.");
            }

            deduplicationResult.ResolvedDecision = request.Decision;
            deduplicationResult.ReviewedAt = DateTimeOffset.UtcNow;
            deduplicationResult.ModifiedAt = DateTimeOffset.UtcNow;

            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                deduplicationResult.Notes = request.Notes;
            }

            var duplicatePaper = await _unitOfWork.Papers.FindSingleAsync(
                p => p.Id == deduplicationResult.PaperId,
                isTracking: true,
                cancellationToken: cancellationToken);

            if (duplicatePaper == null)
            {
                throw new InvalidOperationException($"Paper with ID {deduplicationResult.PaperId} not found.");
            }

            if (request.Decision == DuplicateResolutionDecision.CANCEL)
            {
                // Confirmed duplicate — soft-delete the duplicate paper.
                deduplicationResult.ReviewStatus = DeduplicationReviewStatus.Confirmed;
                duplicatePaper.IsDeleted = true;
                duplicatePaper.ModifiedAt = DateTimeOffset.UtcNow;
            }
            else
            {
                // Not a duplicate — both papers remain visible.
                deduplicationResult.ReviewStatus = DeduplicationReviewStatus.Confirmed;
                duplicatePaper.IsDeleted = false;
                duplicatePaper.ModifiedAt = DateTimeOffset.UtcNow;
            }

            await _unitOfWork.DeduplicationResults.UpdateAsync(deduplicationResult, cancellationToken);
            await _unitOfWork.Papers.UpdateAsync(duplicatePaper, cancellationToken);
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

        public async Task MarkAsDuplicateAsync(
            Guid projectId,
            Guid paperId,
            MarkAsDuplicateRequest request,
            CancellationToken cancellationToken = default)
        {
            var project = await _unitOfWork.SystematicReviewProjects.FindSingleAsync(
                p => p.Id == projectId,
                cancellationToken: cancellationToken);

            if (project == null)
            {
                throw new InvalidOperationException($"Project with ID {projectId} not found.");
            }

            var paper = await _unitOfWork.Papers.FindSingleAsync(
                p => p.Id == paperId && p.ProjectId == projectId,
                isTracking: true,
                cancellationToken: cancellationToken);

            if (paper == null)
            {
                throw new InvalidOperationException($"Paper with ID {paperId} not found in Project {projectId}.");
            }

            var duplicateOfPaper = await _unitOfWork.Papers.FindSingleAsync(
                p => p.Id == request.DuplicateOfPaperId && p.ProjectId == projectId,
                cancellationToken: cancellationToken);

            if (duplicateOfPaper == null)
            {
                throw new InvalidOperationException($"Original paper with ID {request.DuplicateOfPaperId} not found in Project {projectId}.");
            }

            var now = DateTimeOffset.UtcNow;
            var deduplicationResult = new DeduplicationResult
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                PaperId = paperId,
                DuplicateOfPaperId = request.DuplicateOfPaperId,
                Method = DeduplicationMethod.MANUAL,
                ReviewStatus = DeduplicationReviewStatus.Confirmed,
                ResolvedDecision = DuplicateResolutionDecision.CANCEL,
                ConfidenceScore = 1.0m,
                Notes = request.Reason,
                CreatedAt = now,
                ModifiedAt = now
            };

            // Soft-delete the duplicate paper
            paper.IsDeleted = true;
            paper.ModifiedAt = now;

            await _unitOfWork.DeduplicationResults.AddAsync(deduplicationResult, cancellationToken);
            await _unitOfWork.Papers.UpdateAsync(paper, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
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

        private async Task<PaperResponse> MapToPaperResponseAsync(Paper paper, ScreeningPhase? phase = null, Guid? studySelectionProcessId = null, CancellationToken cancellationToken = default)
        {
            var effectivePhase = phase ?? ScreeningPhase.TitleAbstract;

            // Only filter assignments by phase if an explicit phase is provided
            var filteredAssignments = phase.HasValue
                ? paper.PaperAssignments?.Where(pa => pa.Phase == phase.Value).ToList()
                : paper.PaperAssignments?.ToList();

            var resolution = studySelectionProcessId.HasValue && paper.ScreeningResolutions != null
                ? paper.ScreeningResolutions.FirstOrDefault(r => r.StudySelectionProcessId == studySelectionProcessId.Value && r.Phase == effectivePhase)
                : null;

            var response = new PaperResponse
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

                // SelectionStatus is NOT stored in Paper - must be queried from ScreeningResolution
                SelectionStatus = resolution?.FinalDecision == ScreeningDecisionType.Include ? SelectionStatus.Included
                                : resolution?.FinalDecision == ScreeningDecisionType.Exclude ? SelectionStatus.Excluded : null,
                SelectionStatusText = resolution?.FinalDecision.ToString(),

                // Stage - Use TitleAbstract as default status but don't force it if we are in general view
                Stage = (int)effectivePhase,
                StageText = effectivePhase.ToString(),

                // Assignment Status
                AssignmentStatus = (filteredAssignments != null && filteredAssignments.Any()) ? 1 : 0,
                AssignmentStatusText = (filteredAssignments != null && filteredAssignments.Any()) ? "Assigned" : "Unassigned",

                // Assigned Reviewers
                AssignedReviewers = filteredAssignments?.Select(pa => new AssignedReviewerDto
                {
                    Id = pa.ProjectMember.UserId,
                    Name = pa.ProjectMember.User?.FullName ?? "Unknown"
                }).ToList() ?? new List<AssignedReviewerDto>(),

                PdfUrl = paper.PdfUrl,
                FullTextAvailable = paper.FullTextAvailable,
                FullTextRetrievalStatus = paper.FullTextRetrievalStatus,
                FullTextRetrievalStatusText = paper.FullTextRetrievalStatus.ToString(),
                AccessType = paper.AccessType,
                AccessTypeText = paper.AccessType?.ToString(),
                CreatedAt = paper.CreatedAt,
                ModifiedAt = paper.ModifiedAt,

                DecidedStatus = resolution != null ? resolution.FinalDecision.ToString() : "None"
            };

            // Get Extraction Suggestion (G-11, G-12)
            response.ExtractionSuggestion = await _studySelectionService.GetExtractionSuggestionAsync(paper, cancellationToken);

            return response;
        }

        private async Task<PaperDetailsResponse> MapToPaperResponseDetailsAsync(Paper paper, CancellationToken cancellationToken = default)
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
            response.ExtractionSuggestion = await _studySelectionService.GetExtractionSuggestionAsync(paper, cancellationToken);

            return response;
        }

        public async Task AssignPapersAsync(
            AssignPapersRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!request.PaperIds.Any() || !request.MemberIds.Any())
            {
                return;
            }

            // 1. Get ProjectId from the first paper and validate all papers belong to the same project
            // Optimize: Get all papers in request to check their ProjectId
            var papers = await _unitOfWork.Papers.FindAllAsync(
                p => request.PaperIds.Contains(p.Id),
                isTracking: false,
                cancellationToken: cancellationToken);

            if (!papers.Any())
            {
                throw new ArgumentException("None of the specified papers were found.");
            }

            var projectIds = papers.Select(p => p.ProjectId).Distinct().ToList();
            if (projectIds.Count > 1)
            {
                throw new ArgumentException("Papers must all belong to the same project for assignment.");
            }

            var projectId = projectIds.First();
            var foundPaperIds = papers.Select(p => p.Id).ToHashSet();
            var missingPaperIds = request.PaperIds.Where(id => !foundPaperIds.Contains(id)).ToList();

            if (missingPaperIds.Any())
            {
                throw new ArgumentException($"The following papers were not found: {string.Join(", ", missingPaperIds)}");
            }

            // 2. Validate project exists
            var project = await _unitOfWork.SystematicReviewProjects.FindSingleAsync(
                p => p.Id == projectId,
                cancellationToken: cancellationToken);

            if (project == null)
            {
                throw new InvalidOperationException($"Project with ID {projectId} not found.");
            }

            // 3. Resolve and validate all members belong to this project
            // Note: ProjectMemberDto exposes UserId, so clients likely send UserIds
            var allProjectMembers = await _unitOfWork.SystematicReviewProjects.GetMembersByProjectIdAsync(projectId);
            var userIdToMemberIdMap = allProjectMembers.ToDictionary(m => m.UserId, m => m.Id);
            var projectMemberIdsSet = allProjectMembers.Select(m => m.Id).ToHashSet();

            var resolvedMemberIds = new List<Guid>();
            var invalidIds = new List<Guid>();

            foreach (var id in request.MemberIds)
            {
                if (userIdToMemberIdMap.TryGetValue(id, out var memberId))
                {
                    resolvedMemberIds.Add(memberId);
                }
                else if (projectMemberIdsSet.Contains(id))
                {
                    resolvedMemberIds.Add(id);
                }
                else
                {
                    invalidIds.Add(id);
                }
            }

            if (invalidIds.Any())
            {
                throw new ArgumentException($"The following IDs are not part of this project: {string.Join(", ", invalidIds)}");
            }

            // 4. Get existing assignments to prevent duplicates
            var existingAssignments = await _unitOfWork.PaperAssignments.FindAllAsync(
                pa => request.PaperIds.Contains(pa.PaperId)
                    && resolvedMemberIds.Contains(pa.ProjectMemberId)
                    && pa.StudySelectionProcessId == request.StudySelectionProcessId
                    && pa.Phase == request.Phase,
                isTracking: false,
                cancellationToken: cancellationToken);

            var existingPairs = existingAssignments
                .Select(pa => (pa.PaperId, pa.ProjectMemberId))
                .ToHashSet();

            // 5. Create new assignments
            var newAssignments = new List<PaperAssignment>();

            foreach (var paperId in request.PaperIds)
            {
                foreach (var memberId in resolvedMemberIds)
                {
                    if (!existingPairs.Contains((paperId, memberId)))
                    {
                        newAssignments.Add(new PaperAssignment(paperId, memberId, request.StudySelectionProcessId, request.Phase));
                    }
                }
            }

            // 6. Bulk insert new assignments if any
            if (newAssignments.Any())
            {
                await _unitOfWork.PaperAssignments.AddRangeAsync(newAssignments, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // 7. Send notifications to assigned reviewers
                // Using a try-catch as a fail-safe so notification failure doesn't roll back the assignment
                try
                {
                    var assignedMemberIds = newAssignments.Select(pa => pa.ProjectMemberId).Distinct().ToList();
                    var members = await _unitOfWork.SystematicReviewProjects.GetMembersByProjectIdAsync(projectId);
                    var membersDict = members.ToDictionary(m => m.Id);

                    foreach (var memberId in assignedMemberIds)
                    {
                        if (membersDict.TryGetValue(memberId, out var member))
                        {
                            var paperCount = newAssignments.Count(pa => pa.ProjectMemberId == memberId);
                            var title = "Paper Assignments";
                            var message = $"You have been assigned {paperCount} paper(s) for review in project '{project.Title}'.";

                            var metadata = JsonSerializer.Serialize(new
                            {
                                projectId = projectId,
                                paperCount = paperCount,
                                assignedAt = DateTimeOffset.UtcNow
                            });

                            await _notificationService.SendAsync(
                                member.UserId,
                                title,
                                message,
                                NotificationType.Review,
                                projectId,
                                NotificationEntityType.PaperAssignment,
                                metadata);
                        }
                    }
                }
                catch (Exception)
                {
                    // Fail-safe: Notification errors should not disrupt the application flow
                }
            }
        }

        public async Task<PaperResponse> ApplyMetadataAsync(
            Guid paperId,
            ApplyMetadataRequest request,
            CancellationToken cancellationToken = default)
        {
            var paper = await _unitOfWork.Papers.FindSingleAsync(
                p => p.Id == paperId,
                isTracking: true,
                cancellationToken: cancellationToken);

            if (paper == null)
            {
                throw new InvalidOperationException($"Paper with ID {paperId} not found.");
            }

            var sourceMetadata = await _unitOfWork.PaperSourceMetadatas.FindSingleAsync(
                sm => sm.Id == request.SourceMetadataId && sm.PaperId == paperId,
                cancellationToken: cancellationToken);

            if (sourceMetadata == null)
            {
                throw new InvalidOperationException($"PaperSourceMetadata with ID {request.SourceMetadataId} not found for this Paper.");
            }

            // Apply selected fields
            await _metadataMergeService.MergeSelectedFieldsAsync(paper, sourceMetadata, request.Fields);


            // Track provenance: merge new fields with previously applied ones
            sourceMetadata.AppliedFields = sourceMetadata.AppliedFields
                .Union(request.Fields)
                .Distinct()
                .ToList();

            sourceMetadata.ModifiedAt = DateTimeOffset.UtcNow;

            _logger.LogInformation("Applied {FieldCount} metadata fields from SourceMetadata {SourceMetadataId} to Paper {PaperId}",
                request.Fields.Count, request.SourceMetadataId, paperId);

            paper.ModifiedAt = DateTimeOffset.UtcNow;


            await _unitOfWork.Papers.UpdateAsync(paper, cancellationToken);
            await _unitOfWork.PaperSourceMetadatas.UpdateAsync(sourceMetadata, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return await MapToPaperResponseAsync(paper, null, null, cancellationToken);
        }



        public async Task<CheckedDuplicatePapersResponse> GetTitleAbstractEligiblePapersAsync(
            Guid studySelectionProcessId,
            EligiblePapersRequest request,
            CancellationToken cancellationToken = default)
        {
            // 1. Papers that passed deduplication (eligible for Step 1: Title/Abstract)
            var eligiblePaperIds = await _studySelectionService.GetEligiblePapersAsync(studySelectionProcessId, cancellationToken);

            if (!eligiblePaperIds.Any())
            {
                return new CheckedDuplicatePapersResponse
                {
                    Items = new List<PaperResponse>(),
                    TotalCount = 0,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    CurrentPhase = ScreeningPhase.TitleAbstract,
                    CurrentPhaseText = ScreeningPhase.TitleAbstract.ToString()
                };
            }

            // 2. Fetch papers with pagination and filtering
            var (papers, totalCount) = await _unitOfWork.Papers.GetPapersByIdsAsync(
                eligiblePaperIds,
                request.Search,
                request.Year,
                request.SearchSourceId,
                request.AssignmentStatus,
                request.DecisionStatus,
                ScreeningPhase.TitleAbstract,
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            var paperResponses = new List<PaperResponse>();
            foreach (var p in papers)
            {
                paperResponses.Add(await MapToPaperResponseAsync(p, ScreeningPhase.TitleAbstract, studySelectionProcessId, cancellationToken));
            }

            return new CheckedDuplicatePapersResponse
            {
                Items = paperResponses,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                CurrentPhase = ScreeningPhase.TitleAbstract,
                CurrentPhaseText = ScreeningPhase.TitleAbstract.ToString()
            };
        }

        public async Task<CheckedDuplicatePapersResponse> GetFullTextEligiblePapersAsync(
            Guid studySelectionProcessId,
            EligiblePapersRequest request,
            CancellationToken cancellationToken = default)
        {
            // 1. Papers included after Title/Abstract screening (eligible for Step 2: Full-Text)
            var eligiblePaperIds = await _studySelectionService.GetFullTextEligiblePapersAsync(
                studySelectionProcessId,
                cancellationToken);

            // Ensure distinct IDs
            eligiblePaperIds = eligiblePaperIds.Distinct().ToList();

            if (!eligiblePaperIds.Any())
            {
                return new CheckedDuplicatePapersResponse
                {
                    Items = new List<PaperResponse>(),
                    TotalCount = 0,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    CurrentPhase = ScreeningPhase.FullText,
                    CurrentPhaseText = ScreeningPhase.FullText.ToString()
                };
            }

            // 2. Fetch papers with pagination and filtering
            var (papers, totalCount) = await _unitOfWork.Papers.GetPapersByIdsAsync(
                eligiblePaperIds,
                request.Search,
                request.Year,
                request.SearchSourceId,
                request.AssignmentStatus,
                request.DecisionStatus,
                ScreeningPhase.FullText,
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            var paperResponses = new List<PaperResponse>();
            foreach (var p in papers)
            {
                paperResponses.Add(await MapToPaperResponseAsync(p, ScreeningPhase.FullText, studySelectionProcessId, cancellationToken));
            }

            return new CheckedDuplicatePapersResponse
            {
                Items = paperResponses,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                CurrentPhase = ScreeningPhase.FullText,
                CurrentPhaseText = ScreeningPhase.FullText.ToString()
            };
        }

        public async Task<PaginatedResponse<PaperResponse>> GetAssignedPapersByPhaseAsync(
            Guid studySelectionProcessId,
            Guid userId,
            ScreeningPhase phase,
            PaperListRequest request,
            CancellationToken cancellationToken = default)
        {
            // 1. Get assignments for this user in this phase within the study selection process
            // EF Core translates the navigation property access into a JOIN in the SQL query.
            var assignments = await _unitOfWork.PaperAssignments
                .FindAllAsync(pa => pa.StudySelectionProcessId == studySelectionProcessId
                                    && pa.ProjectMember.UserId == userId
                                    && pa.Phase == phase,
                isTracking: false,
                cancellationToken: cancellationToken);

            var paperIds = assignments.Select(pa => pa.PaperId).ToList();

            if (!paperIds.Any())
            {
                return new PaginatedResponse<PaperResponse>
                {
                    Items = new List<PaperResponse>(),
                    TotalCount = 0,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
            }

            // 2. Fetch papers with pagination and search
            var (papers, totalCount) = await _unitOfWork.Papers.GetPapersByIdsAsync(
                paperIds,
                request.Search,
                request.Year,
                request.SearchSourceId,
                AssignmentFilterStatus.All,
                ResolutionFilterStatus.All,
                phase,
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            var paperResponses = new List<PaperResponse>();
            foreach (var p in papers)
            {
                paperResponses.Add(await MapToPaperResponseAsync(p, phase, studySelectionProcessId, cancellationToken));
            }

            return new PaginatedResponse<PaperResponse>
            {
                Items = paperResponses,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
