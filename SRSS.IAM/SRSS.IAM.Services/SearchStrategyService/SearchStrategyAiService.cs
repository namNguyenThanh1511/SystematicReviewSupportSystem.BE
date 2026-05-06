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
            Analyze the following PICOC elements for a Systematic Literature Review and generate a concise, executable Boolean search query.

            ### INPUT PICOC
            - Population: {request.Population}
            - Intervention: {request.Intervention}
            - Comparator: {request.Comparator}
            - Outcome: {request.Outcome}
            - Context: {request.Context}

            Target Database: {sourceName}

            ---

            ### REQUIREMENTS

            ## 1. KEYWORD SELECTION (STRICT)
            - Select ONLY 3-5 most important keywords for each PICOC element.
            - DO NOT generate long synonym lists.
            - Prefer widely used academic terms.
            - Use truncation (*) to shorten terms (e.g., ""diagnos*"" instead of ""diagnosis OR diagnosing"").
            - Use quotation marks for exact phrases when necessary.

            ## 2. QUERY GENERATION (CRITICAL)
            - Generate ONLY ONE Boolean query.
            - The query MUST be SHORT, SIMPLE, and EXECUTABLE.

            - Use structure:
            (Population) AND (Intervention) AND (Outcome)

            - Comparator:
                - Include ONLY as keywords list
                - DO NOT include in main query unless essential

            - Context:
                - Include ONLY as keywords list
                - DO NOT include in main query unless essential

            ## 3. OPTIMIZATION RULES (VERY IMPORTANT)
            - Limit each group to MAX 2-4 terms.
            - Limit total terms in query to ~12 keywords.
            - Avoid long OR chains.
            - Avoid deep nesting.
            - If query becomes too long → REMOVE least important terms automatically.
            - Ensure parentheses are correct and minimal.

            ## 4. DATABASE ADAPTATION
            Adapt syntax based on database:

            - IEEE Xplore:
            (""All Metadata"": (...))

            - Scopus:
            TITLE-ABS-KEY(...)

            - Web of Science:
            TS=(...)

            - Google Scholar:
            - Use simple Boolean only
            - NO complex nesting
            - NO field tags

            - Others:
            Use standard Boolean without field tags

            ## 5. OUTPUT FORMAT (STRICT)
            Return ONLY a valid JSON object:

            {{
            ""population"": [""term1"", ""term2""],
            ""intervention"": [""term1"", ""term2""],
            ""comparison"": [""term1"", ""term2""],
            ""outcome"": [""term1"", ""term2""],
            ""context"": [""term1"", ""term2""],
            ""generatedQuery"": ""short optimized Boolean query for {sourceName}""
            }}

            DO NOT include explanation outside JSON.
            ";

            return await _openRouterService.GenerateStructuredContentAsync<PicocAnalysisResponse>(prompt);
        }
    }
}
