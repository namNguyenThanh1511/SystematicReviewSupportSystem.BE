using SRSS.IAM.Services.DTOs.PrismaReport;

namespace SRSS.IAM.Services.PrismaReportService
{
    public interface IPrismaReportService
    {
        Task<PrismaReportResponse> GenerateReportAsync(
            Guid reviewProcessId,
            GeneratePrismaReportRequest request,
            CancellationToken cancellationToken = default);

        Task<PrismaReportResponse> GetReportByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);


        Task<List<PrismaReportListResponse>> GetReportsByReviewProcessAsync(
            Guid reviewProcessId,
            CancellationToken cancellationToken = default);

        Task<PrismaReportResponse> GetLatestReportByReviewProcessAsync(
            Guid reviewProcessId,
            CancellationToken cancellationToken = default);
    }
}
