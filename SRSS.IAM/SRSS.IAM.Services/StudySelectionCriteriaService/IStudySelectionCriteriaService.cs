using SRSS.IAM.Services.DTOs.SelectionCriteria;

namespace SRSS.IAM.Services.StudySelectionCriteriaService
{
    public interface IStudySelectionCriteriaService
    {
        Task<AICriteriaResponse> GenerateCriteriaWithAiAsync(Guid studySelectionProcessId, CancellationToken ct = default);
        Task SaveAICriteriaAsync(SaveAICriteriaRequestV2 request, CancellationToken ct = default);
    }
}
