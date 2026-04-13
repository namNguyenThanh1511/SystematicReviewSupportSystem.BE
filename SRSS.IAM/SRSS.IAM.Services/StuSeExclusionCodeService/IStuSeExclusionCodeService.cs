using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Services.DTOs.StuSeExclusionCode;

namespace SRSS.IAM.Services.StuSeExclusionCodeService
{
    public interface IStuSeExclusionCodeService
    {
        Task<IEnumerable<StuSeExclusionCodeResponse>> GetByProcessIdAsync(
            Guid processId, 
            bool onlyActive = false, 
            ExclusionReasonSourceFilter source = ExclusionReasonSourceFilter.All,
            string? search = null);
        Task<IEnumerable<StuSeExclusionCodeResponse>> AddBatchAsync(Guid processId, AddExclusionReasonsRequest request);
        Task<StuSeExclusionCodeResponse> UpdateAsync(Guid id, UpdateExclusionReasonRequest request);
        Task<StuSeExclusionCodeResponse> ToggleActiveAsync(Guid id);
        Task DeleteAsync(Guid id);
    }
}
