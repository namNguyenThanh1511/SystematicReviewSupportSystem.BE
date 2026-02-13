using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.ResearchQuestion;

namespace SRSS.IAM.Services.ResearchQuestionService
{
	public class ResearchQuestionService : IResearchQuestionService
	{
		private readonly IUnitOfWork _unitOfWork;

		public ResearchQuestionService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<ResearchQuestionDetailResponse> CreateResearchQuestionAsync(CreateResearchQuestionRequest request)
		{
			await _unitOfWork.BeginTransactionAsync();

			try
			{
				var researchQuestion = new ResearchQuestion
				{
					ProjectId = request.ProjectId,
					QuestionTypeId = request.QuestionTypeId,
					QuestionText = request.QuestionText,
					Rationale = request.Rationale
				};

				await _unitOfWork.ResearchQuestions.AddAsync(researchQuestion);
				await _unitOfWork.SaveChangesAsync();

				// Process PICOC elements
				foreach (var picocRequest in request.PicocElements)
				{
					var picocElement = new PicocElement
					{
						ResearchQuestionId = researchQuestion.Id,
						ElementType = picocRequest.ElementType,
						Description = picocRequest.Description
					};

					await _unitOfWork.PicocElements.AddAsync(picocElement);
					await _unitOfWork.SaveChangesAsync();

					// Create specific element based on type
					await CreateSpecificPicocElementAsync(picocElement.Id, picocRequest);
				}

				await _unitOfWork.SaveChangesAsync();
				await _unitOfWork.CommitTransactionAsync();

				return await GetResearchQuestionDetailAsync(researchQuestion.Id);
			}
			catch
			{
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		public async Task<List<ResearchQuestionDetailResponse>> GetResearchQuestionsByProjectIdAsync(Guid projectId)
		{
			var questions = await _unitOfWork.ResearchQuestions.GetByProjectIdWithDetailsAsync(projectId);
			var responses = new List<ResearchQuestionDetailResponse>();

			foreach (var question in questions)
			{
				responses.Add(await GetResearchQuestionDetailAsync(question.Id));
			}

			return responses;
		}

		private async Task CreateSpecificPicocElementAsync(Guid picocId, CreatePicocElementRequest request)
		{
			switch (request.ElementType)
			{
				case "Population":
					if (request.PopulationDetail != null)
					{
						await _unitOfWork.Populations.AddAsync(new Population
						{
							PicocId = picocId,
							Description = request.PopulationDetail.Description
						});
					}
					break;

				case "Intervention":
					if (request.InterventionDetail != null)
					{
						await _unitOfWork.Interventions.AddAsync(new Intervention
						{
							PicocId = picocId,
							Description = request.InterventionDetail.Description
						});
					}
					break;

				case "Comparison":
					if (request.ComparisonDetail != null)
					{
						await _unitOfWork.Comparisons.AddAsync(new Comparison
						{
							PicocId = picocId,
							Description = request.ComparisonDetail.Description
						});
					}
					break;

				case "Outcome":
					if (request.OutcomeDetail != null)
					{
						await _unitOfWork.Outcomes.AddAsync(new Outcome
						{
							PicocId = picocId,
							Metric = request.OutcomeDetail.Metric,
							Description = request.OutcomeDetail.Description
						});
					}
					break;

				case "Context":
					if (request.ContextDetail != null)
					{
						await _unitOfWork.Contexts.AddAsync(new Context
						{
							PicocId = picocId,
							Environment = request.ContextDetail.Environment,
							Description = request.ContextDetail.Description
						});
					}
					break;
			}
		}

		private async Task<ResearchQuestionDetailResponse> GetResearchQuestionDetailAsync(Guid questionId)
		{
			var question = await _unitOfWork.ResearchQuestions.GetByIdWithDetailsAsync(questionId)
				?? throw new KeyNotFoundException($"Research question {questionId} không tồn tại");

			var picocElements = new List<PicocElementDto>();

			foreach (var picoc in question.PicocElements)
			{
				var dto = new PicocElementDto
				{
					PicocId = picoc.Id,
					ElementType = picoc.ElementType,
					Description = picoc.Description
				};

				// Load specific details based on type
				dto.SpecificDetail = await GetPicocSpecificDetailAsync(picoc);
				picocElements.Add(dto);
			}

			return new ResearchQuestionDetailResponse
			{
				ResearchQuestionId = question.Id,
				ProjectId = question.ProjectId,
				QuestionType = question.QuestionType.Name,
				QuestionText = question.QuestionText,
				Rationale = question.Rationale,
				PicocElements = picocElements,
				CreatedAt = question.CreatedAt
			};
		}

		private async Task<object?> GetPicocSpecificDetailAsync(PicocElement picoc)
		{
			return picoc.ElementType switch
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
		}
	}
}