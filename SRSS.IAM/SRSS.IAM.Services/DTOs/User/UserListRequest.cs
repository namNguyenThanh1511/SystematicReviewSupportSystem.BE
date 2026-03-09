using SRSS.IAM.Services.DTOs.Common;

namespace SRSS.IAM.Services.DTOs.User
{
    public class UserListRequest : PaginationRequest
    {
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
    }
}
