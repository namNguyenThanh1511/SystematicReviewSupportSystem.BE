using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Services.DTOs.QualityAssessment;
using SRSS.IAM.Services.DTOs.SelectionCriteria;

namespace SRSS.IAM.Services.Mappers
{
	public static class QualityAssessmentMappingExtension
	{
		// ==================== QualityAssessmentStrategy ====================
		public static QualityAssessmentStrategyDto ToDto(this QualityAssessmentStrategy entity)
		{
			return new QualityAssessmentStrategyDto
			{
				QaStrategyId = entity.Id,
				ProtocolId = entity.ProtocolId,
				Description = entity.Description
			};
		}

		public static QualityAssessmentStrategy ToEntity(this QualityAssessmentStrategyDto dto)
		{
			return new QualityAssessmentStrategy
			{
				Id = dto.QaStrategyId ?? Guid.Empty,
				ProtocolId = dto.ProtocolId,
				Description = dto.Description
			};
		}

		public static void UpdateEntity(this QualityAssessmentStrategyDto dto, QualityAssessmentStrategy entity)
		{
			entity.ProtocolId = dto.ProtocolId;
			entity.Description = dto.Description;
		}

		// ==================== QualityChecklist ====================
		public static QualityChecklistDto ToDto(this QualityChecklist entity)
		{
			return new QualityChecklistDto
			{
				ChecklistId = entity.Id,
				QaStrategyId = entity.QaStrategyId,
				Name = entity.Name
			};
		}

		public static QualityChecklist ToEntity(this QualityChecklistDto dto)
		{
			return new QualityChecklist
			{
				Id = dto.ChecklistId ?? Guid.Empty,
				QaStrategyId = dto.QaStrategyId,
				Name = dto.Name
			};
		}

		public static void UpdateEntity(this QualityChecklistDto dto, QualityChecklist entity)
		{
			entity.QaStrategyId = dto.QaStrategyId;
			entity.Name = dto.Name;
		}

		// ==================== QualityCriterion ====================
		public static QualityCriterionDto ToDto(this QualityCriterion entity)
		{
			return new QualityCriterionDto
			{
				QualityCriterionId = entity.Id,
				ChecklistId = entity.ChecklistId,
				Question = entity.Question,
				Weight = entity.Weight
			};
		}

		public static QualityCriterion ToEntity(this QualityCriterionDto dto)
		{
			return new QualityCriterion
			{
				Id = dto.QualityCriterionId ?? Guid.Empty,
				ChecklistId = dto.ChecklistId,
				Question = dto.Question,
				Weight = dto.Weight
			};
		}

		public static void UpdateEntity(this QualityCriterionDto dto, QualityCriterion entity)
		{
			entity.ChecklistId = dto.ChecklistId;
			entity.Question = dto.Question;
			entity.Weight = dto.Weight;
		}

		// ==================== List Mapping ====================
		public static List<QualityAssessmentStrategyDto> ToDtoList(this IEnumerable<QualityAssessmentStrategy> entities)
		{
			return entities.Select(e => e.ToDto()).ToList();
		}

		public static List<QualityChecklistDto> ToDtoList(this IEnumerable<QualityChecklist> entities)
		{
			return entities.Select(e => e.ToDto()).ToList();
		}

		public static List<QualityCriterionDto> ToDtoList(this IEnumerable<QualityCriterion> entities)
		{
			return entities.Select(e => e.ToDto()).ToList();
		}

		public static List<StudySelectionCriteriaDto> ToDtoList(this IEnumerable<StudySelectionCriteria> entities)
		{
			return entities.Select(e => e.ToDto()).ToList();
		}

		public static List<InclusionCriterionDto> ToDtoList(this IEnumerable<InclusionCriterion> entities)
		{
			return entities.Select(e => e.ToDto()).ToList();
		}

		public static List<ExclusionCriterionDto> ToDtoList(this IEnumerable<ExclusionCriterion> entities)
		{
			return entities.Select(e => e.ToDto()).ToList();
		}

		public static List<StudySelectionProcedureDto> ToDtoList(this IEnumerable<StudySelectionProcedure> entities)
		{
			return entities.Select(e => e.ToDto()).ToList();
		}

        // ==================== QualityAssessmentProcess ====================
        public static void UpdateEntity(this UpdateQualityAssessmentProcessDto dto, QualityAssessmentProcess entity)
        {
            entity.Notes = dto.Notes;
            // Status transitions handled by domain entity methods
        }

        public static QualityAssessmentProcessResponse ToResponse(this QualityAssessmentProcess entity)
        {
            if (entity == null) return null!;

            return new QualityAssessmentProcessResponse
            {
                Id = entity.Id,
                ReviewProcessId = entity.ReviewProcessId,
                Notes = entity.Notes,
                Status = entity.Status,
                StatusText = entity.Status.ToString(),
                StartedAt = entity.StartedAt,
                CompletedAt = entity.CompletedAt
            };
        }
		
		public static void UpdateStatus(this QualityAssessmentProcess entity, QualityAssessmentProcessStatus newStatus)
        {
            if (newStatus != entity.Status)
            {
                entity.Status = newStatus;
                if (newStatus == QualityAssessmentProcessStatus.InProgress && entity.StartedAt == null)
                    entity.StartedAt = DateTimeOffset.UtcNow;
                if (newStatus == QualityAssessmentProcessStatus.Completed && entity.CompletedAt == null)
                    entity.CompletedAt = DateTimeOffset.UtcNow;
            }
        }

        // ==================== QualityAssessmentDecision ====================
        public static QualityAssessmentDecision ToEntity(this CreateQualityAssessmentDecisionDto dto, Guid reviewerId)
        {
            return new QualityAssessmentDecision
            {
                ReviewerId = reviewerId,
                PaperId = dto.PaperId,
                QualityCriterionId = dto.QualityCriterionId,
                Value = dto.Value,
                Comment = dto.Comment
            };
        }

        public static QualityAssessmentDecision ToEntity(this CreateQualityAssessmentDecisionItemDto dto, Guid reviewerId, Guid paperId)
        {
            return new QualityAssessmentDecision
            {
                ReviewerId = reviewerId,
                PaperId = paperId,
                QualityCriterionId = dto.QualityCriterionId,
                Value = dto.Value,
                Comment = dto.Comment
            };
        }

        public static void UpdateEntity(this UpdateQualityAssessmentDecisionDto dto, QualityAssessmentDecision entity)
        {
            entity.Value = dto.Value;
            entity.Comment = dto.Comment;
        }

        public static void UpdateEntity(this UpdateQualityAssessmentDecisionItemDto dto, QualityAssessmentDecision entity)
        {
            entity.Value = dto.Value;
            entity.Comment = dto.Comment;
        }

        public static QualityAssessmentDecision ToEntity(this UpdateQualityAssessmentDecisionItemDto dto, Guid reviewerId, Guid paperId)
        {
            return new QualityAssessmentDecision
            {
                ReviewerId = reviewerId,
                PaperId = paperId,
                QualityCriterionId = dto.QualityCriterionId,
                Value = dto.Value,
                Comment = dto.Comment
            };
        }

        public static QualityAssessmentDecisionDto ToDto(this QualityAssessmentDecision entity)
        {
            if (entity == null) return null!;
            
            return new QualityAssessmentDecisionDto
            {
                Id = entity.Id,
                ReviewerId = entity.ReviewerId,
                ReviewerName = entity.Reviewer?.FullName,
                PaperId = entity.PaperId,
                QualityCriterionId = entity.QualityCriterionId,
                CriterionQuestion = entity.QualityCriterion?.Question,
                Value = entity.Value,
                Comment = entity.Comment,
            };
        }

        // ==================== QualityAssessmentResolution ====================
        public static QualityAssessmentResolution ToEntity(this CreateQualityAssessmentResolutionDto dto)
        {
            return new QualityAssessmentResolution
            {
                QualityAssessmentProcessId = dto.QualityAssessmentProcessId,
                PaperId = dto.PaperId,
                ResolvedBy = dto.ResolvedBy,
                FinalDecision = dto.FinalDecision,
                FinalScore = dto.FinalScore,
                ResolutionNotes = dto.ResolutionNotes,
                ResolvedAt = DateTimeOffset.UtcNow
            };
        }

        public static void UpdateEntity(this UpdateQualityAssessmentResolutionDto dto, QualityAssessmentResolution entity)
        {
            entity.FinalDecision = dto.FinalDecision;
            entity.FinalScore = dto.FinalScore;
            entity.ResolutionNotes = dto.ResolutionNotes;
        }

        public static QualityAssessmentResolutionResponse ToResponse(this QualityAssessmentResolution entity)
        {
            if (entity == null) return null!;
            
            return new QualityAssessmentResolutionResponse
            {
                Id = entity.Id,
                QualityAssessmentProcessId = entity.QualityAssessmentProcessId,
                PaperId = entity.PaperId,
                FinalDecision = entity.FinalDecision,
                FinalScore = entity.FinalScore,
                ResolutionNotes = entity.ResolutionNotes,
                ResolvedBy = entity.ResolvedBy,
                ResolvedByName = entity.ResolvedBy.ToString(),
                ResolvedAt = entity.ResolvedAt
            };
        }
	}
}