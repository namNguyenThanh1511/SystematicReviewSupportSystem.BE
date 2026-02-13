using Microsoft.EntityFrameworkCore;
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
            Guid projectId,
            GeneratePrismaReportRequest request,
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

            // Begin transaction to ensure atomicity
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // Calculate PRISMA counts from Paper table
                var counts = await CalculatePrismaCounts(projectId, cancellationToken);

                // Create PRISMA Report
                var prismaReport = new PrismaReport
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
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

        public async Task<PrismaReportResponse?> GetReportByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var reports = await _unitOfWork.PrismaReports.GetReportsByProjectAsync(Guid.Empty, cancellationToken);
            var report = reports.FirstOrDefault(r => r.Id == id);

            if (report == null)
            {
                return null;
            }

            return MapToResponse(report);
        }

        public async Task<List<PrismaReportListResponse>> GetReportsByProjectAsync(
            Guid projectId,
            CancellationToken cancellationToken = default)
        {
            var reports = await _unitOfWork.PrismaReports.GetReportsByProjectAsync(projectId, cancellationToken);

            return reports.Select(MapToListResponse).ToList();
        }

        public async Task<PrismaReportResponse?> GetLatestReportByProjectAsync(
            Guid projectId,
            CancellationToken cancellationToken = default)
        {
            var report = await _unitOfWork.PrismaReports.GetLatestReportByProjectAsync(projectId, cancellationToken);

            if (report == null)
            {
                return null;
            }

            return MapToResponse(report);
        }

        private async Task<PrismaCounts> CalculatePrismaCounts(
            Guid projectId,
            CancellationToken cancellationToken)
        {
            var papers = await _unitOfWork.Papers.FindAllAsync(
                p => p.ProjectId == projectId,
                cancellationToken: cancellationToken);

            var papersList = papers.ToList();

            return new PrismaCounts
            {
                RecordsIdentified = papersList.Count,
                DuplicateRecordsRemoved = papersList.Count(p => p.IsDuplicate),
                RecordsScreened = papersList.Count(p => !p.IsDuplicate),
                RecordsExcluded = papersList.Count(p => p.CurrentSelectionStatus == SelectionStatus.Excluded && !p.IsDuplicate),
                StudiesIncluded = papersList.Count(p => p.IsIncludedFinal && !p.IsDuplicate)
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
                ProjectId = report.ProjectId,
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
                ProjectId = report.ProjectId,
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
