using System.Threading.Tasks;
using SRSS.IAM.Services.DTOs.AI;

namespace SRSS.IAM.Services.SearchStrategyService
{
    public interface ISearchStrategyAiService
    {
        Task<PicocAnalysisResponse> AnalyzePicocAsync(PicocAnalysisRequest request);
    }
}
