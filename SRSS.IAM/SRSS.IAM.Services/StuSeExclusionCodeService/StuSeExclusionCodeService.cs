using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.StuSeExclusionCode;

namespace SRSS.IAM.Services.StuSeExclusionCodeService
{
    public class StuSeExclusionCodeService : IStuSeExclusionCodeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StuSeExclusionCodeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<StuSeExclusionCodeResponse>> GetByProcessIdAsync(
            Guid processId,
            bool onlyActive = false,
            ExclusionReasonSourceFilter source = ExclusionReasonSourceFilter.All,
            string? search = null)
        {
            var reasons = await _unitOfWork.StuSeExclusionCodes.FindAllAsync(x =>
                x.StudySelectionProcessId == processId &&
                (!onlyActive || x.IsActive) &&
                (source == ExclusionReasonSourceFilter.All ||
                 (source == ExclusionReasonSourceFilter.Library && x.LibraryReasonId != null) ||
                 (source == ExclusionReasonSourceFilter.Custom && x.LibraryReasonId == null)) &&
                (string.IsNullOrWhiteSpace(search) ||
                 x.Name.ToLower().Contains(search.ToLower()) ||
                 x.Code.ToString().Contains(search)));

            return reasons.OrderBy(x => x.Code).Select(MapToResponse);
        }

        public async Task<IEnumerable<StuSeExclusionCodeResponse>> AddBatchAsync(Guid processId, AddExclusionReasonsRequest request)
        {
            if (request == null || ((request.LibraryReasonIds == null || !request.LibraryReasonIds.Any()) &&
                                    (request.CustomReasons == null || !request.CustomReasons.Any())))
            {
                throw new ArgumentException("At least one exclusion reason (library or custom) must be provided.");
            }

            // 1. Validate Study Selection Process exists
            var process = await _unitOfWork.StudySelectionProcesses.GetByIdAsync(processId);
            if (process == null)
            {
                throw new InvalidOperationException($"Study Selection Process with ID {processId} not found.");
            }

            // Get existing reasons in DB for this process
            var existingInDb = (await _unitOfWork.StuSeExclusionCodes.FindAllAsync(x => x.StudySelectionProcessId == processId)).ToList();
            var existingCodes = existingInDb.Select(x => x.Code).ToHashSet();
            var existingNames = existingInDb.Select(x => x.Name.ToLowerInvariant()).ToHashSet();
            var existingLibraryIds = existingInDb.Where(x => x.LibraryReasonId.HasValue)
                                                .Select(x => x.LibraryReasonId!.Value).ToHashSet();

            var newReasons = new List<StudySelectionExclusionReason>();
            var timestamp = DateTimeOffset.UtcNow;

            // 2. Handle Library Reasons (Optimized: Single query, no N+1)
            if (request.LibraryReasonIds != null && request.LibraryReasonIds.Any())
            {
                var uniqueRequestedLibIds = request.LibraryReasonIds.Distinct().ToList();

                // Check against DB first
                foreach (var libId in uniqueRequestedLibIds)
                {
                    if (existingLibraryIds.Contains(libId))
                    {
                        throw new ArgumentException($"Library reason with ID {libId} has already been added to this process.");
                    }
                }

                // Batch fetch from library
                var libraryReasons = (await _unitOfWork.ExclusionReasonLibraries.FindAllAsync(x =>
                    uniqueRequestedLibIds.Contains(x.Id))).ToList();

                if (libraryReasons.Count != uniqueRequestedLibIds.Count)
                {
                    var foundIds = libraryReasons.Select(x => x.Id).ToHashSet();
                    var missingIds = uniqueRequestedLibIds.Where(id => !foundIds.Contains(id));
                    throw new InvalidOperationException($"One or more Library Reasons not found: {string.Join(", ", missingIds)}");
                }

                foreach (var libraryReason in libraryReasons)
                {
                    if (existingCodes.Contains(libraryReason.Code))
                    {
                        throw new ArgumentException($"Exclusion reason code {libraryReason.Code} already exists in this process.");
                    }
                    if (existingNames.Contains(libraryReason.Name.ToLowerInvariant()))
                    {
                        throw new ArgumentException($"Exclusion reason name '{libraryReason.Name}' already exists in this process.");
                    }

                    var entity = new StudySelectionExclusionReason
                    {
                        Id = Guid.NewGuid(),
                        StudySelectionProcessId = processId,
                        LibraryReasonId = libraryReason.Id,
                        Code = libraryReason.Code,
                        Name = libraryReason.Name,
                        IsActive = true,
                        CreatedAt = timestamp,
                        ModifiedAt = timestamp
                    };

                    newReasons.Add(entity);
                    existingCodes.Add(entity.Code);
                    existingNames.Add(entity.Name.ToLowerInvariant());
                }
            }

            // 3. Handle Custom Reasons
            if (request.CustomReasons != null && request.CustomReasons.Any())
            {
                foreach (var custom in request.CustomReasons)
                {
                    if (string.IsNullOrWhiteSpace(custom.Name))
                    {
                        throw new ArgumentException("Custom exclusion reason name cannot be empty.");
                    }

                    if (existingCodes.Contains(custom.Code))
                    {
                        throw new ArgumentException($"Exclusion reason code {custom.Code} already exists in this process (either in DB or request).");
                    }
                    if (existingNames.Contains(custom.Name.ToLowerInvariant()))
                    {
                        throw new ArgumentException($"Exclusion reason name '{custom.Name}' already exists in this process (either in DB or request).");
                    }

                    var entity = new StudySelectionExclusionReason
                    {
                        Id = Guid.NewGuid(),
                        StudySelectionProcessId = processId,
                        LibraryReasonId = null,
                        Code = custom.Code,
                        Name = custom.Name,
                        IsActive = true,
                        CreatedAt = timestamp,
                        ModifiedAt = timestamp
                    };

                    newReasons.Add(entity);
                    existingCodes.Add(entity.Code);
                    existingNames.Add(entity.Name.ToLowerInvariant());
                }
            }

            // 4. Bulk Insert
            await _unitOfWork.StuSeExclusionCodes.AddRangeAsync(newReasons);
            await _unitOfWork.SaveChangesAsync();

            return newReasons.OrderBy(x => x.Code).Select(MapToResponse);
        }

        public async Task<StuSeExclusionCodeResponse> UpdateAsync(Guid id, UpdateExclusionReasonRequest request)
        {
            var reason = await _unitOfWork.StuSeExclusionCodes.FindSingleAsync(x => x.Id == id);
            if (reason == null)
            {
                throw new InvalidOperationException($"Exclusion reason with ID {id} not found.");
            }

            // Validate duplicate code/name in same process (excluding itself)
            var others = await _unitOfWork.StuSeExclusionCodes.FindAllAsync(x =>
                x.StudySelectionProcessId == reason.StudySelectionProcessId && x.Id != id);

            if (others.Any(x => x.Code == request.Code))
            {
                throw new ArgumentException($"Exclusion reason with code {request.Code} already exists in this process.");
            }
            if (others.Any(x => x.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException($"Exclusion reason with name '{request.Name}' already exists in this process.");
            }

            reason.Code = request.Code;
            reason.Name = request.Name;
            reason.IsActive = request.IsActive;
            reason.ModifiedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.StuSeExclusionCodes.UpdateAsync(reason);
            await _unitOfWork.SaveChangesAsync();

            return MapToResponse(reason);
        }

        public async Task<StuSeExclusionCodeResponse> ToggleActiveAsync(Guid id)
        {
            var reason = await _unitOfWork.StuSeExclusionCodes.FindSingleAsync(x => x.Id == id);
            if (reason == null)
            {
                throw new InvalidOperationException($"Exclusion reason with ID {id} not found.");
            }

            reason.IsActive = !reason.IsActive;
            reason.ModifiedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.StuSeExclusionCodes.UpdateAsync(reason);
            await _unitOfWork.SaveChangesAsync();

            return MapToResponse(reason);
        }

        public async Task DeleteAsync(Guid id)
        {
            var reason = await _unitOfWork.StuSeExclusionCodes.FindSingleAsync(x => x.Id == id);
            if (reason == null)
            {
                throw new InvalidOperationException($"Exclusion reason with ID {id} not found.");
            }

            // Check if used in ScreeningDecision or ScreeningResolution
            var decisions = await _unitOfWork.ScreeningDecisions.FindAllAsync(x =>
                x.ExclusionReasonId == id);

            if (decisions.Any())
            {
                throw new InvalidOperationException("Cannot delete exclusion reason because it has already been used in screening decisions.");
            }

            var resolutions = await _unitOfWork.ScreeningResolutions.FindAllAsync(x =>
                x.ExclusionReasonId == id);

            if (resolutions.Any())
            {
                throw new InvalidOperationException("Cannot delete exclusion reason because it has already been used in screening resolutions.");
            }

            await _unitOfWork.StuSeExclusionCodes.RemoveAsync(reason);
            await _unitOfWork.SaveChangesAsync();
        }

        private StuSeExclusionCodeResponse MapToResponse(StudySelectionExclusionReason entity)
        {
            return new StuSeExclusionCodeResponse
            {
                Id = entity.Id,
                StudySelectionProcessId = entity.StudySelectionProcessId,
                LibraryReasonId = entity.LibraryReasonId,
                Code = entity.Code,
                Name = entity.Name,
                Source = entity.LibraryReasonId.HasValue ? ExclusionReasonSource.Library : ExclusionReasonSource.Custom,
                IsActive = entity.IsActive
            };
        }
    }
}
