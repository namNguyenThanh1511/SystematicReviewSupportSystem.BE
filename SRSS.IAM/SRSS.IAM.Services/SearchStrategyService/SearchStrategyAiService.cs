using System;
using System.Linq;
using System.Threading.Tasks;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.AI;
using SRSS.IAM.Services.OpenRouter;

namespace SRSS.IAM.Services.SearchStrategyService
{
    public class SearchStrategyAiService : ISearchStrategyAiService
    {
        private readonly IOpenRouterService _openRouterService;
        private readonly IUnitOfWork _unitOfWork;

        public SearchStrategyAiService(IOpenRouterService openRouterService, IUnitOfWork unitOfWork)
        {
            _openRouterService = openRouterService;
            _unitOfWork = unitOfWork;
        }

        public async Task<PicocAnalysisResponse> AnalyzePicocAsync(PicocAnalysisRequest request)
        {
            string sourceName = "General Database";
            if (request.SearchSourceId.HasValue && request.SearchSourceId.Value != Guid.Empty)
            {
                var source = await _unitOfWork.SearchSources.FindSingleOrDefaultAsync(s => s.Id == request.SearchSourceId.Value);
                if (source != null)
                {
                    sourceName = source.Name;
                }
            }

            var prompt = $@"
Analyze the following PICOC elements for a Systematic Literature Review and break them down into a structured list of key terms (synonyms, related terms, and keywords).
Additionally, generate a comprehensive Boolean search query specifically tailored for the database: {sourceName}.

Input PICOC:
- Population: {request.Population}
- Intervention: {request.Intervention}
- Comparator: {request.Comparator}
- Outcome: {request.Outcome}
- Context: {request.Context}

Target Database: {sourceName}

Requirements:
1. Identify 5-10 highly relevant keywords or synonyms for each PICOC element.
2. Generate a valid Boolean search query (using AND, OR, and appropriate nesting with parentheses) adapted to the specific syntax and search fields of {sourceName}.
3. If {sourceName} is a well-known database like PubMed, Scopus, IEEE, Web of Science, or ACM, use its specific field tags (e.g., [MeSH], [Title/Abstract], TITLE-ABS-KEY, etc.) if applicable.
4. Return ONLY a valid JSON object.

Expected JSON Format:
{{
  ""population"": [""term1"", ""term2"", ...],
  ""intervention"": [""term1"", ""term2"", ...],
  ""comparison"": [""term1"", ""term2"", ...],
  ""outcome"": [""term1"", ""term2"", ...],
  ""context"": [""term1"", ""term2"", ...],
  ""generatedQuery"": ""(term1 OR term2) AND (term3 OR term4) ...""
}}";

            return await _openRouterService.GenerateStructuredContentAsync<PicocAnalysisResponse>(prompt);
        }
    }
}
