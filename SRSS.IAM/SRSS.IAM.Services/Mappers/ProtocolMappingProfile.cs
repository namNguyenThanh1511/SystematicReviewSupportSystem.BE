using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.DataExtraction;
using SRSS.IAM.Services.DTOs.QualityAssessment;
using SRSS.IAM.Services.DTOs.SearchStrategy;
using SRSS.IAM.Services.DTOs.SelectionCriteria;
using SRSS.IAM.Services.DTOs.Synthesis;
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
				Status = entity.Status.ToString(),
				IsDeleted = entity.IsDeleted,
				CreatedAt = entity.CreatedAt,
				ApprovedAt = entity.ApprovedAt,
				Versions = entity.Versions?.Select(v => v.ToDto()).ToList() ?? new List<VersionHistoryDto>(),

				// Map related details
				SearchSources = entity.SearchSources?.Select(s => s.ToDto()).ToList() ?? new(),
				SelectionCriterias = entity.SelectionCriterias?.Select(s => s.ToDto()).ToList() ?? new(),
				SelectionProcedures = entity.SelectionProcedures?.Select(p => p.ToDto()).ToList() ?? new(),
				QualityStrategies = entity.QualityStrategies?.Select(q => q.ToDto()).ToList() ?? new(),
				ExtractionStrategies = entity.ExtractionStrategies?.Select(e => e.ToDto()).ToList() ?? new(),
				ExtractionTemplates = entity.ExtractionTemplates?.Select(t => t.ToDto()).ToList() ?? new(),
				SynthesisStrategies = entity.SynthesisStrategies?.Select(s => s.ToDto()).ToList() ?? new(),
				DisseminationStrategies = entity.DisseminationStrategies?.Select(d => d.ToDto()).ToList() ?? new()
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