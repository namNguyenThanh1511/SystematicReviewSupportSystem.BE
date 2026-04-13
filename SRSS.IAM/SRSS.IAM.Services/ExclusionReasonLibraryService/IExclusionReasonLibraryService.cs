using SRSS.IAM.Services.DTOs.Common;
using SRSS.IAM.Services.DTOs.ExclusionReasonLibrary;

namespace SRSS.IAM.Services.ExclusionReasonLibraryService
{
    public interface IExclusionReasonLibraryService
    {
        Task<ExclusionReasonLibraryDto> CreateAsync(CreateExclusionReasonRequest request);
        Task<IEnumerable<ExclusionReasonLibraryDto>> BulkCreateAsync(List<CreateExclusionReasonRequest> requests);
        Task<PaginatedResponse<ExclusionReasonLibraryDto>> GetAllAsync(ExclusionReasonLibraryFilterDto filter);
        Task<ExclusionReasonLibraryDto> ToggleActiveAsync(Guid id);
        Task HardDeleteAsync(Guid id);
    }
}
