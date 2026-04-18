using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.Common;
using SRSS.IAM.Services.DTOs.ExclusionReasonLibrary;
using SRSS.IAM.Services.Mappers;

namespace SRSS.IAM.Services.ExclusionReasonLibraryService
{
    public class ExclusionReasonLibraryService : IExclusionReasonLibraryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExclusionReasonLibraryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ExclusionReasonLibraryDto> CreateAsync(CreateExclusionReasonRequest request)
        {
            if (request == null) throw new ArgumentException("Request cannot be null.");

            // Check if code already exists
            var exists = await _unitOfWork.ExclusionReasonLibraries.AnyAsync(e => e.Code == request.Code);
            if (exists)
            {
                throw new InvalidOperationException($"Exclusion reason code {request.Code} already exists.");
            }

            var entity = new ExclusionReasonLibrary
            {
                Id = Guid.NewGuid(),
                Code = request.Code,
                Name = request.Name,
                IsActive = true
            };

            await _unitOfWork.ExclusionReasonLibraries.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return entity.ToDto();
        }

        public async Task<IEnumerable<ExclusionReasonLibraryDto>> BulkCreateAsync(List<CreateExclusionReasonRequest> requests)
        {
            if (requests == null || requests.Count == 0)
                throw new ArgumentException("Request list cannot be null or empty.");

            var entities = new List<ExclusionReasonLibrary>();

            // Fetch all existing codes to avoid multiple DB hits in a loop
            var existingCodes = (await _unitOfWork.ExclusionReasonLibraries.FindAllAsync(isTracking: false))
                                .Select(e => e.Code)
                                .ToHashSet();

            foreach (var req in requests)
            {
                if (existingCodes.Contains(req.Code))
                {
                    throw new InvalidOperationException($"Exclusion reason code {req.Code} already exists in the library.");
                }

                var entity = new ExclusionReasonLibrary
                {
                    Id = Guid.NewGuid(),
                    Code = req.Code,
                    Name = req.Name,
                    IsActive = true
                };
                entities.Add(entity);
                existingCodes.Add(req.Code); // Prevent duplicates within the same bulk request
            }

            await _unitOfWork.ExclusionReasonLibraries.AddRangeAsync(entities);
            await _unitOfWork.SaveChangesAsync();

            return entities.Select(e => e.ToDto());
        }

        public async Task<PaginatedResponse<ExclusionReasonLibraryDto>> GetAllAsync(ExclusionReasonLibraryFilterDto filter)
        {
            var (items, totalCount) = await _unitOfWork.ExclusionReasonLibraries.GetPaginatedAsync(
                filter.Search,
                filter.OnlyActive,
                filter.PageNumber,
                filter.PageSize
            );

            return new PaginatedResponse<ExclusionReasonLibraryDto>
            {
                Items = items.Select(e => e.ToDto()).ToList(),
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task<ExclusionReasonLibraryDto> ToggleActiveAsync(Guid id)
        {
            var entity = await _unitOfWork.ExclusionReasonLibraries.FindSingleAsync(e => e.Id == id, isTracking: true);
            if (entity == null)
            {
                throw new InvalidOperationException($"Exclusion reason with ID {id} not found.");
            }

            entity.IsActive = !entity.IsActive;
            entity.ModifiedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.ExclusionReasonLibraries.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return entity.ToDto();
        }

        public async Task HardDeleteAsync(Guid id)
        {
            var entity = await _unitOfWork.ExclusionReasonLibraries.FindSingleAsync(e => e.Id == id);
            if (entity == null)
            {
                throw new InvalidOperationException($"Exclusion reason with ID {id} not found.");
            }

            // Check if reason is already in use by any project
            var isInUse = await _unitOfWork.StudySelectionExclusionReasons.AnyAsync(r => r.LibraryReasonId == id);
            if (isInUse)
            {
                throw new InvalidOperationException("Cannot delete this exclusion reason because it is currently in use by one or more projects.");
            }

            // Check if reason is already in use by any screening decisions (future-proofing)
            // For now, per requirement, we just hard delete.

            await _unitOfWork.ExclusionReasonLibraries.RemoveAsync(entity);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
