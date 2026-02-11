using Microsoft.EntityFrameworkCore;
using SRSS.IAM.Repositories;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.ResearchQuestion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Services.ResearchQuestionService
{
	public class ResearchQuestionService : IResearchQuestionService
	{
		private readonly AppDbContext _context;
		public ResearchQuestionService(AppDbContext context)
		{
			_context = context;
		}
		public async Task<ResearchQuestionDetailResponse> CreateResearchQuestionAsync(CreateResearchQuestionRequest request)
		{
			using var transaction = await _context.Database.BeginTransactionAsync();

			try
			{
				var researchQuestion = new ResearchQuestion
				{
					ProjectId = request.ProjectId,
					QuestionTypeId = request.QuestionTypeId,
					QuestionText = request.QuestionText,
					Rationale = request.Rationale,
					CreatedAt = DateTimeOffset.UtcNow,
					ModifiedAt = DateTimeOffset.UtcNow
				};

				_context.ResearchQuestions.Add(researchQuestion);
				await _context.SaveChangesAsync();

				// Process PICOC elements
				foreach (var picocRequest in request.PicocElements)
				{
					var picocElement = new PicocElement
					{
						ResearchQuestionId = researchQuestion.Id,
						ElementType = picocRequest.ElementType,
						Description = picocRequest.Description
					};

					_context.PicocElements.Add(picocElement);
					await _context.SaveChangesAsync();

					// Create specific element based on type
					switch (picocRequest.ElementType)
					{
						case "Population":
							if (picocRequest.PopulationDetail != null)
							{
								_context.Populations.Add(new Population
								{
									PicocId = picocElement.Id,
									Description = picocRequest.PopulationDetail.Description
								});
							}
							break;

						case "Intervention":
							if (picocRequest.InterventionDetail != null)
							{
								_context.Interventions.Add(new Intervention
								{
									PicocId = picocElement.Id,
									Description = picocRequest.InterventionDetail.Description
								});
							}
							break;

						case "Comparison":
							if (picocRequest.ComparisonDetail != null)
							{
								_context.Comparisons.Add(new Comparison
								{
									PicocId = picocElement.Id,
									Description = picocRequest.ComparisonDetail.Description
								});
							}
							break;
						case "Outcome":
							if (picocRequest.OutcomeDetail != null)
							{
								_context.Outcomes.Add(new Outcome
								{
									PicocId = picocElement.Id,
									Metric = picocRequest.OutcomeDetail.Metric,
									Description = picocRequest.OutcomeDetail.Description
								});
							}
							break;

						case "Context":
							if (picocRequest.ContextDetail != null)
							{
								_context.Contexts.Add(new Context
								{
									PicocId = picocElement.Id,
									Environment = picocRequest.ContextDetail.Environment,
									Description = picocRequest.ContextDetail.Description
								});
							}
							break;
					}
				}

				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

				return await GetResearchQuestionDetailAsync(researchQuestion.Id);
			}
			catch
			{
				await transaction.RollbackAsync();
				throw;
			}
		}

		public async Task<List<ResearchQuestionDetailResponse>> GetResearchQuestionsByProjectIdAsync(Guid projectId)
		{
			var questions = await _context.ResearchQuestions
				.Include(q => q.QuestionType)
				.Include(q => q.PicocElements)
				.Where(q => q.ProjectId == projectId)
				.ToListAsync();

			var responses = new List<ResearchQuestionDetailResponse>();

			foreach (var question in questions)
			{
				responses.Add(await GetResearchQuestionDetailAsync(question.Id));
			}

			return responses;
		}

		private async Task<ResearchQuestionDetailResponse> GetResearchQuestionDetailAsync(Guid questionId)
		{
			var question = await _context.ResearchQuestions
				.Include(q => q.QuestionType)
				.Include(q => q.PicocElements)
				.FirstOrDefaultAsync(q => q.Id == questionId)
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
				switch (picoc.ElementType)
				{
					case "Population":
						var population = await _context.Populations.FirstOrDefaultAsync(p => p.PicocId == picoc.Id);
						if (population != null)
							dto.SpecificDetail = new { population.Description };
						break;

					case "Intervention":
						var intervention = await _context.Interventions.FirstOrDefaultAsync(i => i.PicocId == picoc.Id);
						if (intervention != null)
							dto.SpecificDetail = new { intervention.Description };
						break;

					case "Comparison":
						var comparison = await _context.Comparisons.FirstOrDefaultAsync(c => c.PicocId == picoc.Id);
						if (comparison != null)
							dto.SpecificDetail = new { comparison.Description };
						break;
					case "Outcome":
						var outcome = await _context.Outcomes.FirstOrDefaultAsync(o => o.PicocId == picoc.Id);
						if (outcome != null)
							dto.SpecificDetail = new { outcome.Metric, outcome.Description };
						break;

					case "Context":
						var context = await _context.Contexts.FirstOrDefaultAsync(c => c.PicocId == picoc.Id);
						if (context != null)
							dto.SpecificDetail = new { context.Environment, context.Description };
						break;
				}

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
	}
}