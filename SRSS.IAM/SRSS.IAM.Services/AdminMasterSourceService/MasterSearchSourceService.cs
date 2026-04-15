using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.MasterSource;

namespace SRSS.IAM.Services.AdminMasterSourceService
{
    public class MasterSearchSourceService : IMasterSearchSourceService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MasterSearchSourceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<MasterSearchSourceResponse>> GetAllAsync(bool? isActive = null, string? sourceName = null, CancellationToken cancellationToken = default)
        {
            var sources = await _unitOfWork.MasterSearchSources.FindAllAsync(
                m => (!isActive.HasValue || m.IsActive == isActive.Value) &&
                     (string.IsNullOrEmpty(sourceName) || m.SourceName.Contains(sourceName)),
                isTracking: false,
                cancellationToken: cancellationToken);

            var sourceList = sources.ToList();
            var ids = sourceList.Select(s => s.Id).ToList();
            var usageCounts = await _unitOfWork.MasterSearchSources.GetUsageCountsAsync(ids, cancellationToken);

            return sourceList.Select(s => MapToResponse(s, usageCounts.GetValueOrDefault(s.Id, 0)));
        }

        public async Task<IEnumerable<AvailableMasterSearchSourceResponse>> GetAvailableAsync(CancellationToken cancellationToken = default)
        {
            var sources = await _unitOfWork.MasterSearchSources.FindAllAsync(
                m => m.IsActive,
                isTracking: false,
                cancellationToken: cancellationToken);

            return sources.Select(s => new AvailableMasterSearchSourceResponse
            {
                Id = s.Id,
                Name = s.SourceName,
                Url = s.BaseUrl
            });
        }

        public async Task<MasterSearchSourceResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var source = await _unitOfWork.MasterSearchSources.FindSingleOrDefaultAsync(m => m.Id == id, isTracking: false, cancellationToken);
            if (source == null)
            {
                throw new InvalidOperationException($"Master source with ID {id} not found.");
            }

            var usageCount = await _unitOfWork.MasterSearchSources.GetUsageCountAsync(id, cancellationToken);
            return MapToResponse(source, usageCount);
        }

        public async Task<MasterSearchSourceResponse> CreateAsync(CreateMasterSearchSourceRequest request, CancellationToken cancellationToken = default)
        {
            var existing = await _unitOfWork.MasterSearchSources.GetByNameAsync(request.SourceName, cancellationToken);
            if (existing != null)
            {
                throw new ArgumentException($"A master source with name '{request.SourceName}' already exists.");
            }

            var source = new MasterSearchSources
            {
                Id = Guid.NewGuid(),
                SourceName = request.SourceName,
                BaseUrl = request.BaseUrl,
                IsActive = request.IsActive,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.MasterSearchSources.AddAsync(source, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToResponse(source, 0);
        }

        public async Task<MasterSearchSourceResponse> UpdateAsync(Guid id, UpdateMasterSearchSourceRequest request, CancellationToken cancellationToken = default)
        {
            var source = await _unitOfWork.MasterSearchSources.FindSingleOrDefaultAsync(m => m.Id == id, cancellationToken: cancellationToken);
            if (source == null)
            {
                throw new InvalidOperationException($"Master source with ID {id} not found.");
            }

            // Check if name is being changed and if new name already exists
            if (!string.Equals(source.SourceName, request.SourceName, StringComparison.OrdinalIgnoreCase))
            {
                var existing = await _unitOfWork.MasterSearchSources.GetByNameAsync(request.SourceName, cancellationToken);
                if (existing != null)
                {
                    throw new ArgumentException($"A master source with name '{request.SourceName}' already exists.");
                }
            }

            source.SourceName = request.SourceName;
            source.BaseUrl = request.BaseUrl;
            source.IsActive = request.IsActive;
            source.ModifiedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.MasterSearchSources.UpdateAsync(source, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var usageCount = await _unitOfWork.MasterSearchSources.GetUsageCountAsync(id, cancellationToken);
            return MapToResponse(source, usageCount);
        }

        public async Task<MasterSearchSourceResponse> ToggleStatusAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var source = await _unitOfWork.MasterSearchSources.FindSingleOrDefaultAsync(m => m.Id == id, cancellationToken: cancellationToken);
            if (source == null)
            {
                throw new InvalidOperationException($"Master source with ID {id} not found.");
            }

            source.IsActive = !source.IsActive;
            source.ModifiedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.MasterSearchSources.UpdateAsync(source, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var usageCount = await _unitOfWork.MasterSearchSources.GetUsageCountAsync(id, cancellationToken);
            return MapToResponse(source, usageCount);
        }

        public async Task<int> GetUsageCountAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _unitOfWork.MasterSearchSources.GetUsageCountAsync(id, cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var source = await _unitOfWork.MasterSearchSources.FindSingleOrDefaultAsync(m => m.Id == id, cancellationToken: cancellationToken);
            if (source == null)
            {
                throw new InvalidOperationException($"Master source with ID {id} not found.");
            }

            var usageCount = await _unitOfWork.MasterSearchSources.GetUsageCountAsync(id, cancellationToken);
            if (usageCount > 0)
            {
                throw new InvalidOperationException($"Cannot delete master source as it is currently in use by {usageCount} search sources.");
            }

            await _unitOfWork.MasterSearchSources.RemoveAsync(source, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private static MasterSearchSourceResponse MapToResponse(MasterSearchSources source, int usageCount)
        {
            return new MasterSearchSourceResponse
            {
                Id = source.Id,
                SourceName = source.SourceName,
                BaseUrl = source.BaseUrl,
                IsActive = source.IsActive,
                UsageCount = usageCount,
                CreatedAt = source.CreatedAt,
                ModifiedAt = source.ModifiedAt
            };
        }
    }
}
