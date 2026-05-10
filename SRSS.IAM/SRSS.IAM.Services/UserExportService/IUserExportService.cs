using SRSS.IAM.Services.DTOs.User;

namespace SRSS.IAM.Services.UserExportService
{
    public interface IUserExportService
    {
        Task<byte[]> ExportUsersToExcelAsync(UserExportRequest request);
    }
}
