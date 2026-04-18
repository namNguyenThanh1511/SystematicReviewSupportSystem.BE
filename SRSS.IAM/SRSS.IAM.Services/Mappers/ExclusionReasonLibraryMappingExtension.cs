using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.ExclusionReasonLibrary;

namespace SRSS.IAM.Services.Mappers
{
    public static class ExclusionReasonLibraryMappingExtension
    {
        public static ExclusionReasonLibraryDto ToDto(this ExclusionReasonLibrary entity)
        {
            if (entity == null) return null!;

            return new ExclusionReasonLibraryDto
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt
            };
        }
    }
}
