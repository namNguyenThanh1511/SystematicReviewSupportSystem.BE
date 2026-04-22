using Microsoft.EntityFrameworkCore;
using Shared.Exceptions;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.PrismaReport;
using SRSS.IAM.Services.IdentificationService;
using SRSS.IAM.Services.UserService;
using System.Linq;
using System.Text.Json;

namespace SRSS.IAM.Services.PrismaReportService
{
    public class PrismaReportService : IPrismaReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdentificationService _identificationService;
        private readonly ICurrentUserService _currentUserService;

        public PrismaReportService(
            IUnitOfWork unitOfWork,
            IIdentificationService identificationService,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _identificationService = identificationService;
            _currentUserService = currentUserService;
        }

        public async Task<PrismaReportResponse> GenerateReportAsync(
            Guid reviewProcessId,
            GeneratePrismaReportRequest request,
            CancellationToken cancellationToken = default)
        {
            // Validate review process exists
            var reviewProcess = await _unitOfWork.ReviewProcesses.GetByIdWithProcessesAsync(
                reviewProcessId,
                cancellationToken: cancellationToken);

            if (reviewProcess == null)
            {
                throw new InvalidOperationException($"ReviewProcess with ID {reviewProcessId} not found.");
            }

            await EnsureCurrentUserIsProjectLeaderAsync(reviewProcess.ProjectId, cancellationToken);

            // Begin transaction to ensure atomicity
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // Calculate PRISMA details
                var details = await CalculatePrismaDetailsAsync(reviewProcess, cancellationToken);

                // Create PRISMA Report
                var prismaReport = new PrismaReport
                {
                    Id = Guid.NewGuid(),
                    ReviewProcessId = reviewProcessId,
                    Version = request.Version,
                    GeneratedAt = DateTimeOffset.UtcNow,
                    Notes = request.Notes,
                    GeneratedBy = request.GeneratedBy,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                };

                // Create flow records
                var flowRecords = CreateFlowRecords(prismaReport.Id, details);
                foreach (var flowRecord in flowRecords)
                {
                    prismaReport.FlowRecords.Add(flowRecord);
                }

                // Add report with all flow records to context
                await _unitOfWork.PrismaReports.AddAsync(prismaReport, cancellationToken);

                // Save everything in one transaction
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                // Return the created report
                return MapToResponse(prismaReport);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task<PrismaReportResponse> GetReportByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var reports = await _unitOfWork.PrismaReports.GetReportsByIdAsync(id, cancellationToken);
            var report = reports.FirstOrDefault(r => r.Id == id);

            if (report == null)
            {
                throw new NotFoundException("Prisma report not found.");
            }

            return MapToResponse(report);
        }

        public async Task<List<PrismaReportListResponse>> GetReportsByReviewProcessAsync(
            Guid reviewProcessId,
            CancellationToken cancellationToken = default)
        {
            var reports = await _unitOfWork.PrismaReports.GetReportsByReviewProcessAsync(reviewProcessId, cancellationToken);

            return reports.Select(MapToListResponse).ToList();
        }

        public async Task<PrismaReportResponse> GetLatestReportByReviewProcessAsync(
            Guid reviewProcessId,
            CancellationToken cancellationToken = default)
        {
            var report = await _unitOfWork.PrismaReports.GetLatestReportByReviewProcessAsync(reviewProcessId, cancellationToken);

            if (report == null)
            {
                throw new NotFoundException("No PRISMA report found for the specified review process.");
            }

            return MapToResponse(report);
        }

        private async Task EnsureCurrentUserIsProjectLeaderAsync(Guid projectId, CancellationToken cancellationToken)
        {
            var userIdValue = _currentUserService.GetUserId();
            if (!Guid.TryParse(userIdValue, out var currentUserId))
            {
                throw new UnauthorizedException("User not authenticated.");
            }

            var membership = await _unitOfWork.SystematicReviewProjects.GetMembershipQueryable(currentUserId)
                .FirstOrDefaultAsync(m => m.ProjectId == projectId, cancellationToken);

            if (membership == null || membership.Role != ProjectRole.Leader)
            {
                throw new UnauthorizedException("Only project leaders can generate PRISMA reports.");
            }
        }

        private async Task<PrismaReportDetails> CalculatePrismaDetailsAsync(
            ReviewProcess reviewProcess,
            CancellationToken cancellationToken)
        {
            var details = new PrismaReportDetails();

            if (reviewProcess.IdentificationProcess == null)
            {
                return details;
            }

            var identificationProcess = reviewProcess.IdentificationProcess;

            // 1, 2, 3. Identification: (Databases + Snowballing + Duplicates)
            // Use common logic from IdentificationService for consistency
            var idStats = await _identificationService.GetPrismaStatisticsAsync(
                reviewProcess.Id,
                cancellationToken);

            details.IdentifiedBreakdown = idStats.IdentifiedBreakdown;

            // Identification: Snowballing - counted separately in identify box?
            // Actually, identifiedBreakdown already has 'Manual' or sources.
            // In the original code, snowballing was added to IdentifiedBreakdown.
            // IdentificationService now includes "Manual" for those by default if no source is present.

            details.RecordsIdentified = idStats.TotalRecordsImported;
            details.DuplicateRecordsRemoved = idStats.DuplicateRecords;
            details.PendingSelectionCount = idStats.PendingSelectionCount;
            details.RecordsScreened = idStats.UniqueRecords;

            // 4. Screening & Eligibility (from StudySelectionProcess)
            if (reviewProcess.StudySelectionProcess != null)
            {
                var sspId = reviewProcess.StudySelectionProcess.Id;

                // Load all decisions for breakdown
                var allDecisions = await _unitOfWork.ScreeningDecisions.FindAllAsync(
                    d => d.StudySelectionProcessId == sspId,
                    isTracking: false,
                    cancellationToken: cancellationToken);

                // Load all resolutions for totals
                var allResolutions = await _unitOfWork.ScreeningResolutions.FindAllAsync(
                    r => r.StudySelectionProcessId == sspId,
                    isTracking: false,
                    cancellationToken: cancellationToken);

                var papersForRetrieval = allResolutions
                                                    .Where(r => r.Phase == ScreeningPhase.TitleAbstract 
                                                            && r.FinalDecision != ScreeningDecisionType.Exclude)
                                                    .Select(r => r.PaperId)
                                                    .ToHashSet();
                                                    
                var papers = await _unitOfWork.Papers.FindAllAsync(
                                                    p => papersForRetrieval.Contains(p.Id),
                                                    isTracking: false,
                                                    cancellationToken: cancellationToken);

                // Phase 1: Title/Abstract Exclusions
                var taExclusions = allResolutions.Where(r => r.Phase == ScreeningPhase.TitleAbstract && r.FinalDecision == ScreeningDecisionType.Exclude).ToList();
                details.RecordsExcluded = taExclusions.Count;

                // Phase 2: Retrieval
                details.ReportsSoughtForRetrieval = details.RecordsScreened - details.RecordsExcluded;
                // TODO: Implement logic for "Reports not retrieved" if needed
                details.ReportsNotRetrieved = papers.Count(p => p.FullTextRetrievalStatus == FullTextRetrievalStatus.NotRetrieved);
                details.ReportsAssessedForEligibility = details.ReportsSoughtForRetrieval - details.ReportsNotRetrieved;

                // Phase 3: Full-Text Exclusions
                var ftExclusions = allResolutions.Where(r => r.Phase == ScreeningPhase.FullText && r.FinalDecision == ScreeningDecisionType.Exclude).ToList();
                details.ReportsExcluded = ftExclusions.Count;

                // Load exclusion reasons for labels
                var reasons = await _unitOfWork.StuSeExclusionCodes.FindAllAsync(x => x.StudySelectionProcessId == sspId, isTracking: false, cancellationToken: cancellationToken);
                var reasonMap = reasons.ToDictionary(x => x.Id);

                details.ExclusionReasonsTA = GetExclusionReasonBreakdown(taExclusions.Select(x => x.PaperId).ToHashSet(), ScreeningPhase.TitleAbstract, allResolutions, reasonMap);
                details.ExclusionReasonsFT = GetExclusionReasonBreakdown(ftExclusions.Select(x => x.PaperId).ToHashSet(), ScreeningPhase.FullText, allResolutions, reasonMap);

                // Final Included
                details.StudiesIncluded = allResolutions.Count(r => r.Phase == ScreeningPhase.FullText && r.FinalDecision == ScreeningDecisionType.Include);

            }

            return details;
        }

        private List<PrismaBreakdownResponse> GetExclusionReasonBreakdown(
            HashSet<Guid> excludedPaperIds,
            ScreeningPhase phase,
            IEnumerable<ScreeningResolution> allResolutions,
            Dictionary<Guid, StudySelectionExclusionReason> reasonMap)
        {
            return allResolutions
                .Where(r => r.Phase == phase
                            && excludedPaperIds.Contains(r.PaperId)
                            && r.FinalDecision == ScreeningDecisionType.Exclude)
                .GroupBy(r => r.ExclusionReasonId)
                .Select(g =>
                {
                    var label = "Other";
                    if (g.Key.HasValue && reasonMap.TryGetValue(g.Key.Value, out var reason))
                    {
                        label = reason.Name;
                    }

                    return new PrismaBreakdownResponse
                    {
                        Label = label,
                        Count = g.Count()
                    };
                })
                .OrderByDescending(x => x.Count)
                .ToList();
        }

        private List<PrismaFlowRecord> CreateFlowRecords(Guid reportId, PrismaReportDetails details)
        {
            var records = new List<PrismaFlowRecord>();

            // 1. RecordsIdentified
            records.Add(new PrismaFlowRecord
            {
                Id = Guid.NewGuid(),
                PrismaReportId = reportId,
                Stage = PrismaStage.RecordsIdentified,
                Label = "Records identified : ",
                Count = details.RecordsIdentified,
                MetadataJson = JsonSerializer.Serialize(new { breakdown = details.IdentifiedBreakdown }),
                DisplayOrder = 1,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            });

            // 2. DuplicateRecordsRemoved
            records.Add(new PrismaFlowRecord
            {
                Id = Guid.NewGuid(),
                PrismaReportId = reportId,
                Stage = PrismaStage.DuplicateRecordsRemoved,
                Label = "Records removed before screening",
                Count = details.DuplicateRecordsRemoved + details.PendingSelectionCount,
                MetadataJson = JsonSerializer.Serialize(new
                {
                    breakdown = new[] {
                        new { label = "Duplicate records removed", count = details.DuplicateRecordsRemoved },
                        // new { label = "Records marked ineligible", count = 0 },
                        new { label = "Records removed other reasons", count = details.PendingSelectionCount }
                    }
                }),
                DisplayOrder = 2,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            });

            // 3. RecordsScreened
            records.Add(new PrismaFlowRecord
            {
                Id = Guid.NewGuid(),
                PrismaReportId = reportId,
                Stage = PrismaStage.RecordsScreened,
                Label = "Records screened",
                Count = details.RecordsScreened,
                DisplayOrder = 3,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            });

            // 4. RecordsExcluded (Side box for 3)
            records.Add(new PrismaFlowRecord
            {
                Id = Guid.NewGuid(),
                PrismaReportId = reportId,
                Stage = PrismaStage.RecordsExcluded,
                Label = "Records excluded",
                Count = details.RecordsExcluded,
                MetadataJson = JsonSerializer.Serialize(new { reasons = details.ExclusionReasonsTA }),
                DisplayOrder = 4,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            });

            // 5. ReportsSoughtForRetrieval
            records.Add(new PrismaFlowRecord
            {
                Id = Guid.NewGuid(),
                PrismaReportId = reportId,
                Stage = PrismaStage.ReportsSoughtForRetrieval,
                Label = "Reports sought for retrieval",
                Count = details.ReportsSoughtForRetrieval,
                DisplayOrder = 5,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            });

            // 6. ReportsNotRetrieved (Side box for 5)
            records.Add(new PrismaFlowRecord
            {
                Id = Guid.NewGuid(),
                PrismaReportId = reportId,
                Stage = PrismaStage.ReportsNotRetrieved,
                Label = "Reports not retrieved",
                Count = details.ReportsNotRetrieved,
                DisplayOrder = 6,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            });

            // 7. ReportsAssessed
            records.Add(new PrismaFlowRecord
            {
                Id = Guid.NewGuid(),
                PrismaReportId = reportId,
                Stage = PrismaStage.ReportsAssessed,
                Label = "Reports assessed for eligibility",
                Count = details.ReportsAssessedForEligibility,
                DisplayOrder = 7,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            });

            // 8. ReportsExcluded (Side box for 7)
            records.Add(new PrismaFlowRecord
            {
                Id = Guid.NewGuid(),
                PrismaReportId = reportId,
                Stage = PrismaStage.ReportsExcluded,
                Label = "Reports excluded during eligibility assessment",
                Count = details.ReportsExcluded,
                MetadataJson = JsonSerializer.Serialize(new { reasons = details.ExclusionReasonsFT }),
                DisplayOrder = 8,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            });

            // 9. StudiesIncludedInReview
            records.Add(new PrismaFlowRecord
            {
                Id = Guid.NewGuid(),
                PrismaReportId = reportId,
                Stage = PrismaStage.StudiesIncludedInReview,
                Label = "Studies included in review",
                Count = details.StudiesIncluded,
                DisplayOrder = 9,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            });

            return records;
        }

        private PrismaReportResponse MapToResponse(PrismaReport report)
        {
            var response = new PrismaReportResponse
            {
                Id = report.Id,
                ReviewProcessId = report.ReviewProcessId,
                Version = report.Version,
                GeneratedAt = report.GeneratedAt,
                Notes = report.Notes,
                GeneratedBy = report.GeneratedBy,
                CreatedAt = report.CreatedAt,
                ModifiedAt = report.ModifiedAt
            };

            var records = report.FlowRecords.ToDictionary(r => r.Stage);

            // Mapping nodes with sideboxes as per frontend requirements
            response.Nodes = new List<PrismaNodeResponse>
            {
                CreateNode(records, PrismaStage.RecordsIdentified, PrismaStage.DuplicateRecordsRemoved),
                CreateNode(records, PrismaStage.RecordsScreened, PrismaStage.RecordsExcluded),
                CreateNode(records, PrismaStage.ReportsSoughtForRetrieval, PrismaStage.ReportsNotRetrieved),
                CreateNode(records, PrismaStage.ReportsAssessed, PrismaStage.ReportsExcluded)
            };

            response.Included = CreateNode(records, PrismaStage.StudiesIncludedInReview);

            return response;
        }

        private PrismaNodeResponse CreateNode(Dictionary<PrismaStage, PrismaFlowRecord> records, PrismaStage mainStage, PrismaStage? sideStage = null)
        {
            records.TryGetValue(mainStage, out var mainRecord);

            var node = new PrismaNodeResponse
            {
                Stage = mainStage.ToString(),
                Total = mainRecord?.Count ?? 0
            };

            if (mainRecord?.MetadataJson != null)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var meta = JsonDocument.Parse(mainRecord.MetadataJson);
                if (meta.RootElement.TryGetProperty("breakdown", out var b))
                    node.Breakdown = b.Deserialize<List<PrismaBreakdownResponse>>(options);
                if (meta.RootElement.TryGetProperty("reasons", out var r))
                    node.Reasons = r.Deserialize<List<PrismaBreakdownResponse>>(options);
            }

            if (sideStage.HasValue && records.TryGetValue(sideStage.Value, out var sideRecord))
            {
                node.SideBox = new PrismaSideBoxResponse
                {
                    Stage = sideStage.Value.ToString(),
                    Total = sideRecord.Count
                };

                if (sideRecord.MetadataJson != null)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var meta = JsonDocument.Parse(sideRecord.MetadataJson);
                    if (meta.RootElement.TryGetProperty("breakdown", out var b))
                        node.SideBox.Breakdown = b.Deserialize<List<PrismaBreakdownResponse>>(options);
                    if (meta.RootElement.TryGetProperty("reasons", out var r))
                        node.SideBox.Reasons = r.Deserialize<List<PrismaBreakdownResponse>>(options);
                }
            }

            return node;
        }

        private static PrismaReportListResponse MapToListResponse(PrismaReport report)
        {
            return new PrismaReportListResponse
            {
                Id = report.Id,
                ReviewProcessId = report.ReviewProcessId,
                Version = report.Version,
                GeneratedAt = report.GeneratedAt,
                GeneratedBy = report.GeneratedBy,
                TotalRecords = report.FlowRecords
                    .FirstOrDefault(fr => fr.Stage == PrismaStage.RecordsIdentified)?.Count ?? 0,
                CreatedAt = report.CreatedAt
            };
        }

        private class PrismaReportDetails
        {
            public int RecordsIdentified { get; set; }
            public List<PrismaBreakdownResponse> IdentifiedBreakdown { get; set; } = new();
            public int DuplicateRecordsRemoved { get; set; }
            public int PendingSelectionCount { get; set; }
            public int RecordsScreened { get; set; }
            public int RecordsExcluded { get; set; }
            public List<PrismaBreakdownResponse> ExclusionReasonsTA { get; set; } = new();
            public int ReportsSoughtForRetrieval { get; set; }
            public int ReportsNotRetrieved { get; set; }
            public int ReportsAssessedForEligibility { get; set; }
            public int ReportsExcluded { get; set; }
            public List<PrismaBreakdownResponse> ExclusionReasonsFT { get; set; } = new();
            public int StudiesIncluded { get; set; }
        }
    }
}
