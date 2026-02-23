using SRSS.IAM.Services.DTOs.CoreGovern;
using SRSS.IAM.Services.DTOs.ResearchQuestion;

namespace SRSS.IAM.Services.CoreGovernService
{
	public interface ICoreGovernService
	{
		// ReviewNeed
		Task<ReviewNeedResponse> CreateReviewNeedAsync(CreateReviewNeedRequest request);
		Task<ReviewNeedResponse> UpdateReviewNeedAsync(UpdateReviewNeedRequest request);
		Task DeleteReviewNeedAsync(Guid id);
		Task<ReviewNeedResponse> GetReviewNeedByIdAsync(Guid id);
		Task<IEnumerable<ReviewNeedResponse>> GetReviewNeedsByProjectIdAsync(Guid projectId);

		// CommissioningDocument
		Task<CommissioningDocumentResponse> CreateCommissioningDocumentAsync(CreateCommissioningDocumentRequest request);
		Task<CommissioningDocumentResponse> UpdateCommissioningDocumentAsync(UpdateCommissioningDocumentRequest request);
		Task DeleteCommissioningDocumentAsync(Guid id);
		Task<CommissioningDocumentResponse> GetCommissioningDocumentByIdAsync(Guid id);
		Task<CommissioningDocumentResponse?> GetCommissioningDocumentByProjectIdAsync(Guid projectId);

		// ReviewObjective
		Task<ReviewObjectiveResponse> CreateReviewObjectiveAsync(CreateReviewObjectiveRequest request);
		Task<ReviewObjectiveResponse> UpdateReviewObjectiveAsync(UpdateReviewObjectiveRequest request);
		Task DeleteReviewObjectiveAsync(Guid id);
		Task<ReviewObjectiveResponse> GetReviewObjectiveByIdAsync(Guid id);
		Task<IEnumerable<ReviewObjectiveResponse>> GetReviewObjectivesByProjectIdAsync(Guid projectId);

		// QuestionType
		Task<QuestionTypeResponse> CreateQuestionTypeAsync(CreateQuestionTypeRequest request);
		Task<QuestionTypeResponse> UpdateQuestionTypeAsync(UpdateQuestionTypeRequest request);
		Task DeleteQuestionTypeAsync(Guid id);
		Task<QuestionTypeResponse> GetQuestionTypeByIdAsync(Guid id);
		Task<IEnumerable<QuestionTypeResponse>> GetAllQuestionTypesAsync();

		// ResearchQuestion (get / update / delete, create is handled by ResearchQuestionService)
		Task<ResearchQuestionDetailResponse> GetResearchQuestionByIdAsync(Guid id);
		Task<IEnumerable<ResearchQuestionDetailResponse>> GetResearchQuestionsByProjectIdAsync(Guid projectId);
		Task<ResearchQuestionDetailResponse> UpdateResearchQuestionAsync(UpdateResearchQuestionRequest request);
		Task DeleteResearchQuestionAsync(Guid id);

		// PICOC (add / update / delete / get individual elements)
		Task<PicocElementDto> AddPicocElementAsync(AddPicocElementRequest request);
		Task<PicocElementDto> UpdatePicocElementAsync(UpdatePicocElementRequest request);
		Task DeletePicocElementAsync(Guid picocElementId);
		Task<PicocElementDto> GetPicocElementByIdAsync(Guid picocElementId);
		Task<IEnumerable<PicocElementDto>> GetPicocElementsByResearchQuestionIdAsync(Guid researchQuestionId);
	}
}
