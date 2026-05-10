using SRSS.IAM.Services.DTOs.SystematicReviewProject;

namespace SRSS.IAM.Services.ProjectExportService
{
    public interface IProjectExportService
    {
        Task<byte[]> ExportProjectsToExcelAsync(ProjectExportRequest request);
    }
}
