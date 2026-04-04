using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.DataExtraction;
using SRSS.IAM.Services.DTOs.Protocol;
using SRSS.IAM.Services.DTOs.QualityAssessment;
using SRSS.IAM.Services.DTOs.SearchStrategy;
using SRSS.IAM.Services.DTOs.SelectionCriteria;
using SRSS.IAM.Services.DTOs.Synthesis;

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
                SearchSources = entity.SearchSources?.Select(ss => ss.ToDto()).ToList() ?? new List<SearchSourceDto>(),
                SelectionCriterias = entity.SelectionCriterias?.Select(sc => sc.ToDto()).ToList() ?? new List<StudySelectionCriteriaDto>(),
                SelectionProcedures = entity.SelectionProcedures?.Select(sp => sp.ToDto()).ToList() ?? new List<StudySelectionProcedureDto>(),
                QualityStrategies = entity.QualityStrategies?.Select(qs => qs.ToDto()).ToList() ?? new List<QualityAssessmentStrategyDto>(),
                // ExtractionStrategies = entity.ExtractionStrategies?.Select(es => es.ToDto()).ToList() ?? new List<DataExtractionStrategyDto>(),
                ExtractionTemplates = entity.ExtractionTemplates?.Select(et => et.ToDto()).ToList() ?? new List<ExtractionTemplateDto>(),
                SynthesisStrategies = entity.SynthesisStrategies?.Select(ss => ss.ToDto()).ToList() ?? new List<DataSynthesisStrategyDto>(),
                DisseminationStrategies = entity.DisseminationStrategies?.Select(ds => ds.ToDto()).ToList() ?? new List<DisseminationStrategyDto>()
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