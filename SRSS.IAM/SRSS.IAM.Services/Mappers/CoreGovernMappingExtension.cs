using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.CoreGovern;

namespace SRSS.IAM.Services.Mappers
{
	public static class CoreGovernMappingExtension
	{
		// ══════════════════════════ ReviewNeed ══════════════════════════════

		public static ReviewNeed ToEntity(this CreateReviewNeedRequest r) => new()
		{
			ProjectId = r.ProjectId,
			Description = r.Description,
			Justification = r.Justification,
			IdentifiedBy = r.IdentifiedBy
		};

		public static void ApplyTo(this UpdateReviewNeedRequest r, ReviewNeed entity)
		{
			entity.Description = r.Description;
			entity.Justification = r.Justification;
			entity.IdentifiedBy = r.IdentifiedBy;
		}

		public static ReviewNeedResponse ToResponse(this ReviewNeed e) => new()
		{
			Id = e.Id,
			ProjectId = e.ProjectId,
			Description = e.Description,
			Justification = e.Justification,
			IdentifiedBy = e.IdentifiedBy,
			CreatedAt = e.CreatedAt,
			ModifiedAt = e.ModifiedAt
		};

		// ══════════════════════ CommissioningDocument ════════════════════════

		public static CommissioningDocument ToEntity(this CreateCommissioningDocumentRequest r) => new()
		{
			ProjectId = r.ProjectId,
			Sponsor = r.Sponsor,
			Scope = r.Scope,
			Budget = r.Budget,
			DocumentUrl = r.DocumentUrl
		};

		public static void ApplyTo(this UpdateCommissioningDocumentRequest r, CommissioningDocument entity)
		{
			entity.Sponsor = r.Sponsor;
			entity.Scope = r.Scope;
			entity.Budget = r.Budget;
			entity.DocumentUrl = r.DocumentUrl;
		}

		public static CommissioningDocumentResponse ToResponse(this CommissioningDocument e) => new()
		{
			Id = e.Id,
			ProjectId = e.ProjectId,
			Sponsor = e.Sponsor,
			Scope = e.Scope,
			Budget = e.Budget,
			DocumentUrl = e.DocumentUrl,
			CreatedAt = e.CreatedAt,
			ModifiedAt = e.ModifiedAt
		};

		// ══════════════════════════ ReviewObjective ══════════════════════════

		public static ReviewObjective ToEntity(this CreateReviewObjectiveRequest r) => new()
		{
			ProjectId = r.ProjectId,
			ObjectiveStatement = r.ObjectiveStatement
		};

		public static void ApplyTo(this UpdateReviewObjectiveRequest r, ReviewObjective entity)
		{
			entity.ObjectiveStatement = r.ObjectiveStatement;
		}

		public static ReviewObjectiveResponse ToResponse(this ReviewObjective e) => new()
		{
			Id = e.Id,
			ProjectId = e.ProjectId,
			ObjectiveStatement = e.ObjectiveStatement,
			CreatedAt = e.CreatedAt,
			ModifiedAt = e.ModifiedAt
		};

		// ══════════════════════════ QuestionType ════════════════════════════

		public static QuestionType ToEntity(this CreateQuestionTypeRequest r) => new()
		{
			Name = r.Name,
			Description = r.Description
		};

		public static void ApplyTo(this UpdateQuestionTypeRequest r, QuestionType entity)
		{
			entity.Name = r.Name;
			entity.Description = r.Description;
		}

		public static QuestionTypeResponse ToResponse(this QuestionType e) => new()
		{
			Id = e.Id,
			Name = e.Name,
			Description = e.Description,
			CreatedAt = e.CreatedAt,
			ModifiedAt = e.ModifiedAt
		};
	}
}
