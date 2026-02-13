using SRSS.IAM.Services.DTOs.PrismaReport;

namespace SRSS.IAM.Services.PrismaReportService
{
    public interface IPrismaReportService
    {
        Task<PrismaReportResponse> GenerateReportAsync(
            Guid projectId,
            GeneratePrismaReportRequest request,
            CancellationToken cancellationToken = default);

        Task<PrismaReportResponse?> GetReportByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<List<PrismaReportListResponse>> GetReportsByProjectAsync(
            Guid projectId,
            CancellationToken cancellationToken = default);

        Task<PrismaReportResponse?> GetLatestReportByProjectAsync(
            Guid projectId,
            CancellationToken cancellationToken = default);
    }
}
