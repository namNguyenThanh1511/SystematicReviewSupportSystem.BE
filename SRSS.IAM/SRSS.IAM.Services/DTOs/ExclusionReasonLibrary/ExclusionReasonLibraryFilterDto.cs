using SRSS.IAM.Services.DTOs.Common;

namespace SRSS.IAM.Services.DTOs.ExclusionReasonLibrary
{
    public class ExclusionReasonLibraryFilterDto : PaginationRequest
    {
        public string? Search { get; set; }
        public bool? OnlyActive { get; set; }
    }
}
