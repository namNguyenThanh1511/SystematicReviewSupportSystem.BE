using System.IO;
using ClosedXML.Excel;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.User;

namespace SRSS.IAM.Services.UserExportService
{
    public class UserExportService : IUserExportService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserExportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<byte[]> ExportUsersToExcelAsync(UserExportRequest request)
        {
            // Fetch all users without filtering or pagination as per requirement
            var users = await _unitOfWork.Users.FindAllAsync(isTracking: false);
            
            // Sort by Username for consistent output
            users = users.OrderBy(u => u.Username);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("User Accounts");

            // Header row
            var headers = new[]
            {
                "User ID",
                "Full Name",
                "Email",
                "Username",
                "Role",
                "Status",
                "Created Date"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#E0E0E0");
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Data rows
            int rowCount = 2;
            foreach (var user in users)
            {
                worksheet.Cell(rowCount, 1).Value = user.Id.ToString();
                worksheet.Cell(rowCount, 2).Value = user.FullName;
                worksheet.Cell(rowCount, 3).Value = user.Email;
                worksheet.Cell(rowCount, 4).Value = user.Username;
                worksheet.Cell(rowCount, 5).Value = user.Role.ToString();
                worksheet.Cell(rowCount, 6).Value = user.IsActive ? "Active" : "Inactive";
                worksheet.Cell(rowCount, 7).Value = user.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
                rowCount++;
            }

            // Formatting
            worksheet.Columns().AdjustToContents();
            worksheet.SheetView.FreezeRows(1);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
