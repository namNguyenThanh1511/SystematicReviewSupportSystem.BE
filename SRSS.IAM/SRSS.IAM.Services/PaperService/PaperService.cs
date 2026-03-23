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
            var paperResponses = papers.Select(p => MapToPaperResponse(p)).ToList();

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

            var paperResponses = papers.Select(p => MapToPaperResponse(p)).ToList();

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

            var paperResponses = papers.Select(p => MapToPaperResponse(p)).ToList();

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

            deduplicationResult.ResolvedDecision = request.Decision;
            deduplicationResult.ReviewedBy = request.ReviewedBy;
            deduplicationResult.ReviewedAt = DateTimeOffset.UtcNow;
            deduplicationResult.ModifiedAt = DateTimeOffset.UtcNow;

            if (request.Decision == DuplicateResolutionDecision.CANCEL)
            {
                deduplicationResult.ReviewStatus = DeduplicationReviewStatus.Confirmed;
            }
            else
            {
                deduplicationResult.ReviewStatus = DeduplicationReviewStatus.Rejected;
            }

            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                deduplicationResult.Notes = request.Notes;
            }

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

            deduplicationResult.ResolvedDecision = request.Decision;
            deduplicationResult.ReviewedAt = DateTimeOffset.UtcNow;
            deduplicationResult.ModifiedAt = DateTimeOffset.UtcNow;

            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                deduplicationResult.Notes = request.Notes;
            }

            if (request.Decision == DuplicateResolutionDecision.CANCEL)
            {
                // Confirmed duplicate — PaperId is excluded from this process
                deduplicationResult.ReviewStatus = DeduplicationReviewStatus.Confirmed;
            }
            else
            {
                // Not a duplicate — both papers remain
                deduplicationResult.ReviewStatus = DeduplicationReviewStatus.Rejected;
            }

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

        private static PaperResponse MapToPaperResponse(Paper paper, ScreeningPhase? phase = null)
        {
            var effectivePhase = phase ?? ScreeningPhase.TitleAbstract;

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

                // Stage
                Stage = (int)effectivePhase,
                StageText = effectivePhase.ToString(),

                // Assignment Status
                AssignmentStatus = paper.PaperAssignments?.Any() == true ? 1 : 0,
                AssignmentStatusText = paper.PaperAssignments?.Any() == true ? "Assigned" : "Unassigned",

                // Assigned Reviewers
                AssignedReviewers = paper.PaperAssignments?.Select(pa => new AssignedReviewerDto
                {
                    Id = pa.ProjectMember.UserId,
                    Name = pa.ProjectMember.User?.FullName ?? "Unknown"
                }).ToList() ?? new List<AssignedReviewerDto>(),

                PdfUrl = paper.PdfUrl,
                FullTextAvailable = paper.FullTextAvailable,
                AccessType = paper.AccessType,
                AccessTypeText = paper.AccessType?.ToString(),
                CreatedAt = paper.CreatedAt,
                ModifiedAt = paper.ModifiedAt
            };
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

            _logger.LogInformation("Applied {FieldCount} metadata fields from SourceMetadata {SourceMetadataId} to Paper {PaperId}", 
                request.Fields.Count, request.SourceMetadataId, paperId);

            paper.ModifiedAt = DateTimeOffset.UtcNow;
            
            await _unitOfWork.Papers.UpdateAsync(paper, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToPaperResponse(paper);
        }
        

        public async Task<CheckedDuplicatePapersResponse> GetTitleAbstractEligiblePapersAsync(
            Guid studySelectionProcessId,
            CheckedDuplicatePapersRequest request,
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
                request.AssignmentStatus,
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            var paperResponses = papers.Select(p => MapToPaperResponse(p, ScreeningPhase.TitleAbstract)).ToList();

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
            CheckedDuplicatePapersRequest request,
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
                request.AssignmentStatus,
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            var paperResponses = papers.Select(p => MapToPaperResponse(p, ScreeningPhase.FullText)).ToList();

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
                null, // Filtered implicitly by paperIds
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            var paperResponses = papers.Select(p => MapToPaperResponse(p, phase)).ToList();

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
