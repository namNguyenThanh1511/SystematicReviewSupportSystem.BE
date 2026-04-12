using SRSS.IAM.Services.DTOs.MasterSource;

namespace SRSS.IAM.Services.AdminMasterSourceService
{
    public interface IMasterSearchSourceService
    {
        Task<IEnumerable<MasterSearchSourceResponse>> GetAllAsync(bool? isActive = null, string? sourceName = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<AvailableMasterSearchSourceResponse>> GetAvailableAsync(CancellationToken cancellationToken = default);
        Task<MasterSearchSourceResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<MasterSearchSourceResponse> CreateAsync(CreateMasterSearchSourceRequest request, CancellationToken cancellationToken = default);
        Task<MasterSearchSourceResponse> UpdateAsync(Guid id, UpdateMasterSearchSourceRequest request, CancellationToken cancellationToken = default);
        Task<MasterSearchSourceResponse> ToggleStatusAsync(Guid id, CancellationToken cancellationToken = default);
        Task<int> GetUsageCountAsync(Guid id, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
