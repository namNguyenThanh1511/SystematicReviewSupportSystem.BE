using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.Protocol;

namespace SRSS.IAM.Services.Mappers
{
	public static class ProtocolMappingExtension
	{
		public static ProtocolDetailResponse ToDetailResponse(this ReviewProtocol entity)
		{
			return new ProtocolDetailResponse
			{
				ProtocolId = entity.Id,
				ProjectId = entity.ProjectId,
				ProtocolVersion = entity.ProtocolVersion,
				Status = entity.Status,
				CreatedAt = entity.CreatedAt,
				ApprovedAt = entity.ApprovedAt,
				Versions = entity.Versions?.Select(v => v.ToDto()).ToList() ?? new List<VersionHistoryDto>()
			};
		}

		public static VersionHistoryDto ToDto(this ProtocolVersion entity)
		{
			return new VersionHistoryDto
			{
				VersionId = entity.Id,
				VersionNumber = entity.VersionNumber,
				ChangeSummary = entity.ChangeSummary,
				CreatedAt = entity.CreatedAt
			};
		}

		public static List<VersionHistoryDto> ToDtoList(this IEnumerable<ProtocolVersion> entities)
		{
			return entities.Select(e => e.ToDto()).ToList();
		}
	}
}