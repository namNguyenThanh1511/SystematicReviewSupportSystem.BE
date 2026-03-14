using Microsoft.EntityFrameworkCore;
using Shared.Exceptions;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.PrismaReport;

namespace SRSS.IAM.Services.PrismaReportService
{
    public class PrismaReportService : IPrismaReportService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PrismaReportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PrismaReportResponse> GenerateReportAsync(
            Guid reviewProcessId,
            GeneratePrismaReportRequest request,
            CancellationToken cancellationToken = default)
        {
            // Validate review process exists
            var reviewProcess = await _unitOfWork.ReviewProcesses.FindSingleAsync(
                rp => rp.Id == reviewProcessId,
                cancellationToken: cancellationToken);

            if (reviewProcess == null)
            {
                throw new InvalidOperationException($"ReviewProcess with ID {reviewProcessId} not found.");
            }

            // Begin transaction to ensure atomicity
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // Calculate PRISMA counts scoped to this review process
                var counts = await CalculatePrismaCounts(reviewProcessId, cancellationToken);

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

                // Create flow records and add them to report BEFORE saving
                var flowRecords = CreateFlowRecords(prismaReport.Id, counts);
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
        private async Task<PrismaCounts> CalculatePrismaCounts(
            Guid reviewProcessId,
            CancellationToken cancellationToken)
        {
            // Scope to the review process's identification process
            var identificationProcess = await _unitOfWork.IdentificationProcesses.FindSingleAsync(
                ip => ip.ReviewProcessId == reviewProcessId,
                cancellationToken: cancellationToken);

            if (identificationProcess == null)
            {
                return new PrismaCounts();
            }

            // Get total records imported via ImportBatch → SearchExecution → IdentificationProcess chain
            var searchExecutions = await _unitOfWork.SearchExecutions.FindAllAsync(
                se => se.IdentificationProcessId == identificationProcess.Id,
                cancellationToken: cancellationToken);

            var searchExecutionIds = searchExecutions.Select(se => se.Id).ToHashSet();

            var allImportBatches = await _unitOfWork.ImportBatches.FindAllAsync(
                ib => ib.SearchExecutionId != null && searchExecutionIds.Contains(ib.SearchExecutionId.Value),
                cancellationToken: cancellationToken);

            var importBatchList = allImportBatches.ToList();
            var totalRecordsImported = importBatchList.Sum(ib => ib.TotalRecords);

            // Query actual unique paper count using the same logic as the unique papers endpoint
            var (_, uniqueRecords) = await _unitOfWork.Papers.GetUniquePapersByIdentificationProcessAsync(
                identificationProcess.Id,
                search: null,
                year: null,
                pageNumber: 1,
                pageSize: 1,
                cancellationToken);

            var duplicateCount = totalRecordsImported - uniqueRecords;

            // TODO: Selection status should be calculated from ScreeningResolution, not Paper
            // This requires knowing the StudySelectionProcessId
            // For now, return basic counts
            return new PrismaCounts
            {
                RecordsIdentified = totalRecordsImported,
                DuplicateRecordsRemoved = duplicateCount,
                RecordsScreened = uniqueRecords,
                RecordsExcluded = 0, // TODO: Query ScreeningResolution with FinalDecision = Exclude
                StudiesIncluded = 0  // TODO: Query ScreeningResolution with FinalDecision = Include
            };
        }

        private List<PrismaFlowRecord> CreateFlowRecords(Guid reportId, PrismaCounts counts)
        {
            return new List<PrismaFlowRecord>
            {
                new PrismaFlowRecord
                {
                    Id = Guid.NewGuid(),
                    PrismaReportId = reportId,
                    Stage = PrismaStage.RecordsIdentified,
                    Label = "Records identified from databases and registers",
                    Count = counts.RecordsIdentified,
                    Description = $"Total papers identified from all sources (n = {counts.RecordsIdentified})",
                    DisplayOrder = 1,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                },
                new PrismaFlowRecord
                {
                    Id = Guid.NewGuid(),
                    PrismaReportId = reportId,
                    Stage = PrismaStage.DuplicateRecordsRemoved,
                    Label = "Duplicate records removed",
                    Count = counts.DuplicateRecordsRemoved,
                    Description = $"Papers marked as duplicates (n = {counts.DuplicateRecordsRemoved})",
                    DisplayOrder = 2,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                },
                new PrismaFlowRecord
                {
                    Id = Guid.NewGuid(),
                    PrismaReportId = reportId,
                    Stage = PrismaStage.RecordsScreened,
                    Label = "Records screened",
                    Count = counts.RecordsScreened,
                    Description = $"Unique papers screened after duplicate removal (n = {counts.RecordsScreened})",
                    DisplayOrder = 3,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                },
                new PrismaFlowRecord
                {
                    Id = Guid.NewGuid(),
                    PrismaReportId = reportId,
                    Stage = PrismaStage.RecordsExcluded,
                    Label = "Records excluded",
                    Count = counts.RecordsExcluded,
                    Description = $"Papers excluded during screening (n = {counts.RecordsExcluded})",
                    DisplayOrder = 4,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                },
                new PrismaFlowRecord
                {
                    Id = Guid.NewGuid(),
                    PrismaReportId = reportId,
                    Stage = PrismaStage.StudiesIncludedInReview,
                    Label = "Studies included in review",
                    Count = counts.StudiesIncluded,
                    Description = $"Final papers included in the systematic review (n = {counts.StudiesIncluded})",
                    DisplayOrder = 5,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                }
            };
        }

        private static PrismaReportResponse MapToResponse(PrismaReport report)
        {
            return new PrismaReportResponse
            {
                Id = report.Id,
                ReviewProcessId = report.ReviewProcessId,
                Version = report.Version,
                GeneratedAt = report.GeneratedAt,
                Notes = report.Notes,
                GeneratedBy = report.GeneratedBy,
                FlowRecords = report.FlowRecords
                    .OrderBy(fr => fr.DisplayOrder)
                    .Select(fr => new PrismaFlowRecordResponse
                    {
                        Id = fr.Id,
                        Stage = fr.Stage,
                        StageText = fr.Stage.ToString(),
                        Label = fr.Label,
                        Count = fr.Count,
                        Description = fr.Description,
                        DisplayOrder = fr.DisplayOrder
                    })
                    .ToList(),
                CreatedAt = report.CreatedAt,
                ModifiedAt = report.ModifiedAt
            };
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

        private class PrismaCounts
        {
            public int RecordsIdentified { get; set; }
            public int DuplicateRecordsRemoved { get; set; }
            public int RecordsScreened { get; set; }
            public int RecordsExcluded { get; set; }
            public int StudiesIncluded { get; set; }
        }
    }
}
