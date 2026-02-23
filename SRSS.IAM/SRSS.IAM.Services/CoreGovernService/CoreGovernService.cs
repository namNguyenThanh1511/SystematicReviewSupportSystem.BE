using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.CoreGovern;
using SRSS.IAM.Services.DTOs.ResearchQuestion;
using SRSS.IAM.Services.Mappers;

namespace SRSS.IAM.Services.CoreGovernService
{
	public class CoreGovernService : ICoreGovernService
	{
		private readonly IUnitOfWork _unitOfWork;

		public CoreGovernService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		// ─────────────────────────── ReviewNeed ────────────────────────────

		public async Task<ReviewNeedResponse> CreateReviewNeedAsync(CreateReviewNeedRequest request)
		{
			var entity = request.ToEntity();

			await _unitOfWork.ReviewNeeds.AddAsync(entity);
			await _unitOfWork.SaveChangesAsync();

			return entity.ToResponse();
		}

		public async Task<ReviewNeedResponse> UpdateReviewNeedAsync(UpdateReviewNeedRequest request)
		{
			var entity = await _unitOfWork.ReviewNeeds.FindSingleAsync(r => r.Id == request.Id)
				?? throw new InvalidOperationException($"ReviewNeed với ID {request.Id} không tồn tại.");

			request.ApplyTo(entity);

			await _unitOfWork.ReviewNeeds.UpdateAsync(entity);
			await _unitOfWork.SaveChangesAsync();

			return entity.ToResponse();
		}

		public async Task DeleteReviewNeedAsync(Guid id)
		{
			var entity = await _unitOfWork.ReviewNeeds.FindSingleAsync(r => r.Id == id)
				?? throw new InvalidOperationException($"ReviewNeed với ID {id} không tồn tại.");

			await _unitOfWork.ReviewNeeds.RemoveAsync(entity);
			await _unitOfWork.SaveChangesAsync();
		}

		public async Task<ReviewNeedResponse> GetReviewNeedByIdAsync(Guid id)
		{
			var entity = await _unitOfWork.ReviewNeeds.FindSingleAsync(r => r.Id == id, isTracking: false)
				?? throw new InvalidOperationException($"ReviewNeed với ID {id} không tồn tại.");

			return entity.ToResponse();
		}

		public async Task<IEnumerable<ReviewNeedResponse>> GetReviewNeedsByProjectIdAsync(Guid projectId)
		{
			var entities = await _unitOfWork.ReviewNeeds.GetByProjectIdAsync(projectId);
			return entities.Select(e => e.ToResponse());
		}

		// ─────────────────────── CommissioningDocument ─────────────────────

		public async Task<CommissioningDocumentResponse> CreateCommissioningDocumentAsync(CreateCommissioningDocumentRequest request)
		{
			var existing = await _unitOfWork.CommissioningDocuments.GetByProjectIdAsync(request.ProjectId);
			if (existing != null)
				throw new InvalidOperationException($"Dự án {request.ProjectId} đã có CommissioningDocument.");

			var entity = request.ToEntity();

			await _unitOfWork.CommissioningDocuments.AddAsync(entity);
			await _unitOfWork.SaveChangesAsync();

			return entity.ToResponse();
		}

		public async Task<CommissioningDocumentResponse> UpdateCommissioningDocumentAsync(UpdateCommissioningDocumentRequest request)
		{
			var entity = await _unitOfWork.CommissioningDocuments.FindSingleAsync(c => c.Id == request.Id)
				?? throw new InvalidOperationException($"CommissioningDocument với ID {request.Id} không tồn tại.");

			request.ApplyTo(entity);

			await _unitOfWork.CommissioningDocuments.UpdateAsync(entity);
			await _unitOfWork.SaveChangesAsync();

			return entity.ToResponse();
		}

		public async Task DeleteCommissioningDocumentAsync(Guid id)
		{
			var entity = await _unitOfWork.CommissioningDocuments.FindSingleAsync(c => c.Id == id)
				?? throw new InvalidOperationException($"CommissioningDocument với ID {id} không tồn tại.");

			await _unitOfWork.CommissioningDocuments.RemoveAsync(entity);
			await _unitOfWork.SaveChangesAsync();
		}

		public async Task<CommissioningDocumentResponse> GetCommissioningDocumentByIdAsync(Guid id)
		{
			var entity = await _unitOfWork.CommissioningDocuments.FindSingleAsync(c => c.Id == id, isTracking: false)
				?? throw new InvalidOperationException($"CommissioningDocument với ID {id} không tồn tại.");

			return entity.ToResponse();
		}

		public async Task<CommissioningDocumentResponse?> GetCommissioningDocumentByProjectIdAsync(Guid projectId)
		{
			var entity = await _unitOfWork.CommissioningDocuments.GetByProjectIdAsync(projectId);
			return entity is null ? null : entity.ToResponse();
		}

		// ─────────────────────────── ReviewObjective ───────────────────────

		public async Task<ReviewObjectiveResponse> CreateReviewObjectiveAsync(CreateReviewObjectiveRequest request)
		{
			var entity = request.ToEntity();

			await _unitOfWork.ReviewObjectives.AddAsync(entity);
			await _unitOfWork.SaveChangesAsync();

			return entity.ToResponse();
		}

		public async Task<ReviewObjectiveResponse> UpdateReviewObjectiveAsync(UpdateReviewObjectiveRequest request)
		{
			var entity = await _unitOfWork.ReviewObjectives.FindSingleAsync(r => r.Id == request.Id)
				?? throw new InvalidOperationException($"ReviewObjective với ID {request.Id} không tồn tại.");

			request.ApplyTo(entity);

			await _unitOfWork.ReviewObjectives.UpdateAsync(entity);
			await _unitOfWork.SaveChangesAsync();

			return entity.ToResponse();
		}

		public async Task DeleteReviewObjectiveAsync(Guid id)
		{
			var entity = await _unitOfWork.ReviewObjectives.FindSingleAsync(r => r.Id == id)
				?? throw new InvalidOperationException($"ReviewObjective với ID {id} không tồn tại.");

			await _unitOfWork.ReviewObjectives.RemoveAsync(entity);
			await _unitOfWork.SaveChangesAsync();
		}

		public async Task<ReviewObjectiveResponse> GetReviewObjectiveByIdAsync(Guid id)
		{
			var entity = await _unitOfWork.ReviewObjectives.FindSingleAsync(r => r.Id == id, isTracking: false)
				?? throw new InvalidOperationException($"ReviewObjective với ID {id} không tồn tại.");

			return entity.ToResponse();
		}

		public async Task<IEnumerable<ReviewObjectiveResponse>> GetReviewObjectivesByProjectIdAsync(Guid projectId)
		{
			var entities = await _unitOfWork.ReviewObjectives.GetByProjectIdAsync(projectId);
			return entities.Select(e => e.ToResponse());
		}

		// ─────────────────────────── QuestionType ──────────────────────────

		public async Task<QuestionTypeResponse> CreateQuestionTypeAsync(CreateQuestionTypeRequest request)
		{
			var existing = await _unitOfWork.QuestionTypes.GetByNameAsync(request.Name);
			if (existing != null)
				throw new InvalidOperationException($"QuestionType với tên '{request.Name}' đã tồn tại.");

			var entity = request.ToEntity();

			await _unitOfWork.QuestionTypes.AddAsync(entity);
			await _unitOfWork.SaveChangesAsync();

			return entity.ToResponse();
		}

		public async Task<QuestionTypeResponse> UpdateQuestionTypeAsync(UpdateQuestionTypeRequest request)
		{
			var entity = await _unitOfWork.QuestionTypes.FindSingleAsync(q => q.Id == request.Id)
				?? throw new InvalidOperationException($"QuestionType với ID {request.Id} không tồn tại.");

			var duplicate = await _unitOfWork.QuestionTypes.GetByNameAsync(request.Name);
			if (duplicate != null && duplicate.Id != request.Id)
				throw new InvalidOperationException($"QuestionType với tên '{request.Name}' đã tồn tại.");

			request.ApplyTo(entity);

			await _unitOfWork.QuestionTypes.UpdateAsync(entity);
			await _unitOfWork.SaveChangesAsync();

			return entity.ToResponse();
		}

		public async Task DeleteQuestionTypeAsync(Guid id)
		{
			var entity = await _unitOfWork.QuestionTypes.FindSingleAsync(q => q.Id == id)
				?? throw new InvalidOperationException($"QuestionType với ID {id} không tồn tại.");

			if (await _unitOfWork.ResearchQuestions.AnyAsync(r => r.QuestionTypeId == id))
				throw new InvalidOperationException($"Không thể xóa QuestionType vì đang được sử dụng bởi ResearchQuestion.");

			await _unitOfWork.QuestionTypes.RemoveAsync(entity);
			await _unitOfWork.SaveChangesAsync();
		}

		public async Task<QuestionTypeResponse> GetQuestionTypeByIdAsync(Guid id)
		{
			var entity = await _unitOfWork.QuestionTypes.FindSingleAsync(q => q.Id == id, isTracking: false)
				?? throw new InvalidOperationException($"QuestionType với ID {id} không tồn tại.");

			return entity.ToResponse();
		}

		public async Task<IEnumerable<QuestionTypeResponse>> GetAllQuestionTypesAsync()
		{
			var entities = await _unitOfWork.QuestionTypes.FindAllAsync(isTracking: false);
			return entities.Select(e => e.ToResponse());
		}

		// ─────────────────────────── ResearchQuestion ──────────────────────

		public async Task<ResearchQuestionDetailResponse> GetResearchQuestionByIdAsync(Guid id)
		{
			var exists = await _unitOfWork.ResearchQuestions.AnyAsync(r => r.Id == id);
			if (!exists)
				throw new InvalidOperationException($"ResearchQuestion với ID {id} không tồn tại.");

			return await BuildResearchQuestionResponseAsync(id);
		}

		public async Task<IEnumerable<ResearchQuestionDetailResponse>> GetResearchQuestionsByProjectIdAsync(Guid projectId)
		{
			var questions = await _unitOfWork.ResearchQuestions.GetByProjectIdWithDetailsAsync(projectId);

			var results = new List<ResearchQuestionDetailResponse>();
			foreach (var question in questions)
			{
				var picocDtos = new List<PicocElementDto>();
				foreach (var picoc in question.PicocElements)
					picocDtos.Add(await BuildPicocElementDtoAsync(picoc));

				results.Add(new ResearchQuestionDetailResponse
				{
					ResearchQuestionId = question.Id,
					ProjectId = question.ProjectId,
					QuestionType = question.QuestionType.Name,
					QuestionText = question.QuestionText,
					Rationale = question.Rationale,
					PicocElements = picocDtos,
					CreatedAt = question.CreatedAt
				});
			}

			return results;
		}

		public async Task<ResearchQuestionDetailResponse> UpdateResearchQuestionAsync(UpdateResearchQuestionRequest request)
		{
			var entity = await _unitOfWork.ResearchQuestions.GetByIdWithDetailsAsync(request.Id)
				?? throw new InvalidOperationException($"ResearchQuestion với ID {request.Id} không tồn tại.");

			var questionTypeExists = await _unitOfWork.QuestionTypes.AnyAsync(q => q.Id == request.QuestionTypeId);
			if (!questionTypeExists)
				throw new InvalidOperationException($"QuestionType với ID {request.QuestionTypeId} không tồn tại.");

			entity.QuestionTypeId = request.QuestionTypeId;
			entity.QuestionText = request.QuestionText;
			entity.Rationale = request.Rationale;

			await _unitOfWork.ResearchQuestions.UpdateAsync(entity);
			await _unitOfWork.SaveChangesAsync();

			return await BuildResearchQuestionResponseAsync(entity.Id);
		}

		public async Task DeleteResearchQuestionAsync(Guid id)
		{
			var entity = await _unitOfWork.ResearchQuestions.FindSingleAsync(r => r.Id == id)
				?? throw new InvalidOperationException($"ResearchQuestion với ID {id} không tồn tại.");

			await _unitOfWork.ResearchQuestions.RemoveAsync(entity);
			await _unitOfWork.SaveChangesAsync();
		}

		// ─────────────────────────── PICOC ─────────────────────────────────

		public async Task<PicocElementDto> GetPicocElementByIdAsync(Guid picocElementId)
		{
			var picoc = await _unitOfWork.PicocElements.FindSingleAsync(p => p.Id == picocElementId, isTracking: false)
				?? throw new InvalidOperationException($"PicocElement với ID {picocElementId} không tồn tại.");

			return await BuildPicocElementDtoAsync(picoc);
		}

		public async Task<IEnumerable<PicocElementDto>> GetPicocElementsByResearchQuestionIdAsync(Guid researchQuestionId)
		{
			var questionExists = await _unitOfWork.ResearchQuestions.AnyAsync(r => r.Id == researchQuestionId);
			if (!questionExists)
				throw new InvalidOperationException($"ResearchQuestion với ID {researchQuestionId} không tồn tại.");

			var elements = await _unitOfWork.PicocElements.GetByResearchQuestionIdAsync(researchQuestionId);

			var dtos = new List<PicocElementDto>();
			foreach (var picoc in elements)
				dtos.Add(await BuildPicocElementDtoAsync(picoc));

			return dtos;
		}
        
		public async Task<PicocElementDto> AddPicocElementAsync(AddPicocElementRequest request)
		{
			var questionExists = await _unitOfWork.ResearchQuestions.AnyAsync(r => r.Id == request.ResearchQuestionId);
			if (!questionExists)
				throw new InvalidOperationException($"ResearchQuestion với ID {request.ResearchQuestionId} không tồn tại.");

			var picoc = new PicocElement
			{
				ResearchQuestionId = request.ResearchQuestionId,
				ElementType = request.ElementType,
				Description = request.Description
			};

			await _unitOfWork.PicocElements.AddAsync(picoc);
			await _unitOfWork.SaveChangesAsync();

			await CreateSpecificPicocChildAsync(picoc.Id, request.ElementType,
				request.PopulationDetail?.Description,
				request.InterventionDetail?.Description,
				request.ComparisonDetail?.Description,
				request.OutcomeDetail?.Metric, request.OutcomeDetail?.Description,
				request.ContextDetail?.Environment, request.ContextDetail?.Description);

			await _unitOfWork.SaveChangesAsync();

			return await BuildPicocElementDtoAsync(picoc);
		}

		public async Task<PicocElementDto> UpdatePicocElementAsync(UpdatePicocElementRequest request)
		{
			var picoc = await _unitOfWork.PicocElements.FindSingleAsync(p => p.Id == request.Id)
				?? throw new InvalidOperationException($"PicocElement với ID {request.Id} không tồn tại.");

			picoc.Description = request.Description;
			await _unitOfWork.PicocElements.UpdateAsync(picoc);

			await UpdateSpecificPicocChildAsync(picoc.Id, picoc.ElementType,
				request.PopulationDetail?.Description,
				request.InterventionDetail?.Description,
				request.ComparisonDetail?.Description,
				request.OutcomeDetail?.Metric, request.OutcomeDetail?.Description,
				request.ContextDetail?.Environment, request.ContextDetail?.Description);

			await _unitOfWork.SaveChangesAsync();

			return await BuildPicocElementDtoAsync(picoc);
		}

		public async Task DeletePicocElementAsync(Guid picocElementId)
		{
			var picoc = await _unitOfWork.PicocElements.FindSingleAsync(p => p.Id == picocElementId)
				?? throw new InvalidOperationException($"PicocElement với ID {picocElementId} không tồn tại.");

			await _unitOfWork.PicocElements.RemoveAsync(picoc);
			await _unitOfWork.SaveChangesAsync();
		}

		// ─────────────────────────── Private Helpers ───────────────────────

		private async Task CreateSpecificPicocChildAsync(Guid picocId, string elementType,
			string? populationDesc, string? interventionDesc, string? comparisonDesc,
			string? outcomeMetric, string? outcomeDesc,
			string? contextEnv, string? contextDesc)
		{
			switch (elementType)
			{
				case "Population" when populationDesc != null:
					await _unitOfWork.Populations.AddAsync(new Population { PicocId = picocId, Description = populationDesc });
					break;
				case "Intervention" when interventionDesc != null:
					await _unitOfWork.Interventions.AddAsync(new Intervention { PicocId = picocId, Description = interventionDesc });
					break;
				case "Comparison" when comparisonDesc != null:
					await _unitOfWork.Comparisons.AddAsync(new Comparison { PicocId = picocId, Description = comparisonDesc });
					break;
				case "Outcome" when outcomeDesc != null:
					await _unitOfWork.Outcomes.AddAsync(new Outcome { PicocId = picocId, Metric = outcomeMetric, Description = outcomeDesc });
					break;
				case "Context" when contextDesc != null:
					await _unitOfWork.Contexts.AddAsync(new Context { PicocId = picocId, Environment = contextEnv, Description = contextDesc });
					break;
			}
		}

		private async Task UpdateSpecificPicocChildAsync(Guid picocId, string elementType,
			string? populationDesc, string? interventionDesc, string? comparisonDesc,
			string? outcomeMetric, string? outcomeDesc,
			string? contextEnv, string? contextDesc)
		{
			switch (elementType)
			{
				case "Population":
					var pop = await _unitOfWork.Populations.GetByPicocIdAsync(picocId);
					if (pop != null && populationDesc != null)
					{
						pop.Description = populationDesc;
						await _unitOfWork.Populations.UpdateAsync(pop);
					}
					break;
				case "Intervention":
					var inv = await _unitOfWork.Interventions.GetByPicocIdAsync(picocId);
					if (inv != null && interventionDesc != null)
					{
						inv.Description = interventionDesc;
						await _unitOfWork.Interventions.UpdateAsync(inv);
					}
					break;
				case "Comparison":
					var cmp = await _unitOfWork.Comparisons.GetByPicocIdAsync(picocId);
					if (cmp != null && comparisonDesc != null)
					{
						cmp.Description = comparisonDesc;
						await _unitOfWork.Comparisons.UpdateAsync(cmp);
					}
					break;
				case "Outcome":
					var out_ = await _unitOfWork.Outcomes.GetByPicocIdAsync(picocId);
					if (out_ != null && outcomeDesc != null)
					{
						out_.Metric = outcomeMetric;
						out_.Description = outcomeDesc;
						await _unitOfWork.Outcomes.UpdateAsync(out_);
					}
					break;
				case "Context":
					var ctx = await _unitOfWork.Contexts.GetByPicocIdAsync(picocId);
					if (ctx != null && contextDesc != null)
					{
						ctx.Environment = contextEnv;
						ctx.Description = contextDesc;
						await _unitOfWork.Contexts.UpdateAsync(ctx);
					}
					break;
			}
		}

		private async Task<ResearchQuestionDetailResponse> BuildResearchQuestionResponseAsync(Guid questionId)
		{
			var question = await _unitOfWork.ResearchQuestions.GetByIdWithDetailsAsync(questionId)
				?? throw new InvalidOperationException($"ResearchQuestion {questionId} không tồn tại.");

			var picocDtos = new List<PicocElementDto>();
			foreach (var picoc in question.PicocElements)
				picocDtos.Add(await BuildPicocElementDtoAsync(picoc));

			return new ResearchQuestionDetailResponse
			{
				ResearchQuestionId = question.Id,
				ProjectId = question.ProjectId,
				QuestionType = question.QuestionType.Name,
				QuestionText = question.QuestionText,
				Rationale = question.Rationale,
				PicocElements = picocDtos,
				CreatedAt = question.CreatedAt
			};
		}

		private async Task<PicocElementDto> BuildPicocElementDtoAsync(PicocElement picoc)
		{
			object? specificDetail = picoc.ElementType switch
			{
				"Population" => await _unitOfWork.Populations.GetByPicocIdAsync(picoc.Id) is var p && p != null
					? new { p.Description } : null,
				"Intervention" => await _unitOfWork.Interventions.GetByPicocIdAsync(picoc.Id) is var i && i != null
					? new { i.Description } : null,
				"Comparison" => await _unitOfWork.Comparisons.GetByPicocIdAsync(picoc.Id) is var c && c != null
					? new { c.Description } : null,
				"Outcome" => await _unitOfWork.Outcomes.GetByPicocIdAsync(picoc.Id) is var o && o != null
					? new { o.Metric, o.Description } : null,
				"Context" => await _unitOfWork.Contexts.GetByPicocIdAsync(picoc.Id) is var ctx && ctx != null
					? new { ctx.Environment, ctx.Description } : null,
				_ => null
			};

			return new PicocElementDto
			{
				PicocId = picoc.Id,
				ElementType = picoc.ElementType,
				Description = picoc.Description,
				SpecificDetail = specificDetail
			};
		}
	}
}
