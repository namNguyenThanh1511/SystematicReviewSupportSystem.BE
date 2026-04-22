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
            var researchQuestion = new ResearchQuestion
            {
                Id = Guid.NewGuid(),
                ProjectId = request.ProjectId,
                QuestionTypeId = request.QuestionTypeId,
                QuestionText = request.QuestionText,
                Rationale = request.Rationale,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.ResearchQuestions.AddAsync(researchQuestion);
            await _unitOfWork.SaveChangesAsync();

            return await GetResearchQuestionDetailAsync(researchQuestion.Id);
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

        private async Task<ResearchQuestionDetailResponse> GetResearchQuestionDetailAsync(Guid questionId)
        {
            var question = await _unitOfWork.ResearchQuestions.GetByIdWithDetailsAsync(questionId)
                ?? throw new KeyNotFoundException($"Research question {questionId} không tồn tại");

            return new ResearchQuestionDetailResponse
            {
                ResearchQuestionId = question.Id,
                ProjectId = question.ProjectId,
                QuestionType = question.QuestionType?.Name,
                QuestionText = question.QuestionText,
                Rationale = question.Rationale,
                CreatedAt = question.CreatedAt
            };
        }
    }
}