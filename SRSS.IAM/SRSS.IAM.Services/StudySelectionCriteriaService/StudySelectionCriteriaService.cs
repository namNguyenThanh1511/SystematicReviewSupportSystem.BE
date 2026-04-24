using Microsoft.EntityFrameworkCore;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.SelectionCriteria;
using SRSS.IAM.Services.OpenRouter;
using System.Text;
using System.Text.Json;

namespace SRSS.IAM.Services.StudySelectionCriteriaService
{
    public class StudySelectionCriteriaService : IStudySelectionCriteriaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOpenRouterService _openRouterService;

        public StudySelectionCriteriaService(IUnitOfWork unitOfWork, IOpenRouterService openRouterService)
        {
            _unitOfWork = unitOfWork;
            _openRouterService = openRouterService;
        }

        public async Task<AICriteriaResponse> GenerateCriteriaWithAiAsync(Guid studySelectionProcessId, CancellationToken ct = default)
        {
            // 1. Build the normalized AI input
            var aiInput = await BuildCriteriaAIInput(studySelectionProcessId);

            // 2. Build the strict JSON prompt from that input
            var prompt = BuildCriteriaPrompt(aiInput);

            // 3. Send the prompt to OpenRouter through the existing structured response method
            var result = await _openRouterService.GenerateStructuredContentAsync<AICriteriaResponse>(prompt, ct: ct);

            return result;
        }

        public async Task SaveAICriteriaAsync(SaveAICriteriaRequestV2 request, CancellationToken ct = default)
        {
            // 1. Validate process exists
            var process = await _unitOfWork.StudySelectionProcesses
                .GetQueryable(p => p.Id == request.StudySelectionProcessId, isTracking: true)
                .FirstOrDefaultAsync(ct)
                ?? throw new InvalidOperationException($"Study selection process with ID {request.StudySelectionProcessId} not found.");

            // 2. Check if criteria or AI response already exists
            var hasExistingCriteria = await _unitOfWork.SelectionCriterias
                .GetQueryable(c => c.StudySelectionProcessId == request.StudySelectionProcessId)
                .AnyAsync(ct);

            var hasExistingAiResponse = await _unitOfWork.StudySelectionCriteriaAIResponses
                .GetQueryable(r => r.StudySelectionProcessId == request.StudySelectionProcessId)
                .AnyAsync(ct);

            var hasExistingAiPaperResults = await _unitOfWork.StudySelectionAIResults
                .GetQueryable(r => r.StudySelectionProcessId == request.StudySelectionProcessId)
                .AnyAsync(ct);

            if (hasExistingCriteria || hasExistingAiResponse || hasExistingAiPaperResults)
            {
                throw new InvalidOperationException("Study selection criteria, AI criteria response, or AI screening results already exist.");
            }

            // 3. Save the raw AI response for traceability
            var aiResponse = new StudySelectionCriteriaAIResponse
            {
                Id = Guid.NewGuid(),
                StudySelectionProcessId = request.StudySelectionProcessId,
                RawJson = request.RawJson,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };
            await _unitOfWork.StudySelectionCriteriaAIResponses.AddAsync(aiResponse);

            // 4. Save the actual criteria groups
            if (request.CriteriaGroups != null && request.CriteriaGroups.Any())
            {
                foreach (var group in request.CriteriaGroups)
                {
                    var criteria = new StudySelectionCriteria
                    {
                        Id = Guid.NewGuid(),
                        StudySelectionProcessId = request.StudySelectionProcessId,
                        Description = group.Description,
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = DateTimeOffset.UtcNow
                    };

                    // Add inclusion criteria
                    if (group.InclusionCriteria != null)
                    {
                        foreach (var rule in group.InclusionCriteria)
                        {
                            criteria.InclusionCriteria.Add(new InclusionCriterion
                            {
                                Id = Guid.NewGuid(),
                                CriteriaId = criteria.Id,
                                Rule = rule,
                                CreatedAt = DateTimeOffset.UtcNow,
                                ModifiedAt = DateTimeOffset.UtcNow
                            });
                        }
                    }

                    // Add exclusion criteria
                    if (group.ExclusionCriteria != null)
                    {
                        foreach (var rule in group.ExclusionCriteria)
                        {
                            criteria.ExclusionCriteria.Add(new ExclusionCriterion
                            {
                                Id = Guid.NewGuid(),
                                CriteriaId = criteria.Id,
                                Rule = rule,
                                CreatedAt = DateTimeOffset.UtcNow,
                                ModifiedAt = DateTimeOffset.UtcNow
                            });
                        }
                    }

                    await _unitOfWork.SelectionCriterias.AddAsync(criteria);
                }
            }

            await _unitOfWork.SaveChangesAsync(ct);
        }

        private async Task<AICriteriaGenerationInput> BuildCriteriaAIInput(Guid studySelectionProcessId)
        {
            var process = await _unitOfWork.StudySelectionProcesses
                .GetQueryable(p => p.Id == studySelectionProcessId, isTracking: false)
                .Include(p => p.ReviewProcess)
                    .ThenInclude(rp => rp.Project)
                        .ThenInclude(proj => proj.ProjectPicocs)
                .Include(p => p.ReviewProcess)
                    .ThenInclude(rp => rp.Project)
                        .ThenInclude(proj => proj.ResearchQuestions)
                .FirstOrDefaultAsync()
                ?? throw new InvalidOperationException($"Study selection process with ID {studySelectionProcessId} not found.");

            var project = process.ReviewProcess.Project;
            var picoc = project.ProjectPicocs.FirstOrDefault();
            var researchQuestions = project.ResearchQuestions.Select(rq => rq.QuestionText).Take(1).ToList();

            return new AICriteriaGenerationInput
            {
                Population = picoc?.Population,
                Intervention = picoc?.Intervention,
                Comparator = picoc?.Comparator,
                Outcome = picoc?.Outcome,
                Context = picoc?.Context,
                ResearchQuestions = researchQuestions
            };
        }

        private string BuildCriteriaPrompt(AICriteriaGenerationInput input)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You are a senior Systematic Literature Review (SLR) specialist. Your task is to generate study selection criteria based on the provided PICOC elements and Research Questions.");
            sb.AppendLine();
            sb.AppendLine("### INPUT DATA ###");
            sb.AppendLine($"- Population: {input.Population ?? "Not specified"}");
            sb.AppendLine($"- Intervention: {input.Intervention ?? "Not specified"}");
            sb.AppendLine($"- Comparator: {input.Comparator ?? "Not specified"}");
            sb.AppendLine($"- Outcome: {input.Outcome ?? "Not specified"}");
            sb.AppendLine($"- Context: {input.Context ?? "Not specified"}");
            sb.AppendLine("- Research Questions:");
            if (input.ResearchQuestions == null || !input.ResearchQuestions.Any())
            {
                sb.AppendLine("  * Not specified");
            }
            else
            {
                foreach (var rq in input.ResearchQuestions)
                {
                    sb.AppendLine($"  * {rq}");
                }
            }
            sb.AppendLine();
            sb.AppendLine("### EXPLICIT CONSTRAINTS ###");
            sb.AppendLine("1. OUTPUT FORMAT: Return ONLY valid JSON. No markdown code blocks, no preamble, no explanation.");
            sb.AppendLine("2. STRUCTURE: Output MUST be a single object with a 'criteriaGroups' array.");
            sb.AppendLine("3. MANDATORY FIELDS: Every group must have 'description', 'inclusionCriteria', and 'exclusionCriteria'. Every criterion must have 'text' and 'sources'. Every source must have 'sourceType' and 'sourceId'.");
            sb.AppendLine("4. NON-EMPTY: Generate at least 1 criteria group. Each group must contain at least 1 inclusion OR 1 exclusion criterion.");
            sb.AppendLine("5. TRACEABILITY: Every criterion MUST have at least one source.");
            sb.AppendLine("   - For PICOC: 'sourceType' is 'PICOC', 'sourceId' MUST be one of [Population, Intervention, Comparator, Outcome, Context].");
            sb.AppendLine("   - For RQ: 'sourceType' is 'RQ', 'sourceId' MUST be the FULL research question text (e.g., 'What are the benefits of...'), NOT labels like 'RQ1'.");
            sb.AppendLine("6. CRITERIA QUALITY: Criteria must be ATOMIC (one condition), SPECIFIC, and TESTABLE from a paper's content. Avoid vague terms like 'relevant', 'significant', or 'high quality'.");
            sb.AppendLine("7. THEMES: Organize criteria into logical groups (e.g., 'Population Filtering', 'Methodology Constraints', 'Outcome Metrics').");
            sb.AppendLine("8. LANGUAGE CONSISTENCY:");
            sb.AppendLine("   - Detect the language of the provided PICOC and Research Questions.");
            sb.AppendLine("   - ALL generated text MUST strictly follow that language, including:");
            sb.AppendLine("     * criteriaGroups[].description");
            sb.AppendLine("     * inclusionCriteria[].text");
            sb.AppendLine("     * exclusionCriteria[].text");
            sb.AppendLine("   - DO NOT mix languages under any circumstances.");
            sb.AppendLine("   - DO NOT translate or switch language unless explicitly instructed.");
            sb.AppendLine("   - The language of each criteria group description MUST match EXACTLY the detected language.");
            sb.AppendLine("9. LIMIT: Generate a reasonable number of criteria groups (typically between 2 and 5). Avoid excessive grouping.");
            sb.AppendLine("10. NO DUPLICATION: Do not generate duplicate or semantically overlapping criteria across or within groups. Each criterion must be distinct in meaning.");
            sb.AppendLine("11. COVERAGE: Within each criteria group, generate as many distinct inclusion and exclusion criteria as needed to adequately cover the relevant aspects of the theme. Do not limit a group to only one criterion if multiple meaningful criteria can be derived.");
            sb.AppendLine();
            sb.AppendLine("### VALID JSON EXAMPLE ###");
            sb.AppendLine(@"{
  ""criteriaGroups"": [
    {
      ""description"": ""Criteria focused on software development tools and developer roles."",
      ""inclusionCriteria"": [
        {
          ""text"": ""Studies evaluating AI-assisted coding tools used by professional software developers."",
          ""sources"": [
            { ""sourceType"": ""PICOC"", ""sourceId"": ""Intervention"" },
            { ""sourceType"": ""PICOC"", ""sourceId"": ""Population"" }
          ]
        }
      ],
      ""exclusionCriteria"": [
        {
          ""text"": ""Studies focusing on undergraduate students or hobbyists rather than professional developers."",
          ""sources"": [
            { ""sourceType"": ""PICOC"", ""sourceId"": ""Population"" }
          ]
        }
      ]
    },
    {
      ""description"": ""Criteria related to productivity and code quality metrics."",
      ""inclusionCriteria"": [
        {
          ""text"": ""Studies measuring the impact of AI tools on code maintainability and lead time."",
          ""sources"": [
            { ""sourceType"": ""PICOC"", ""sourceId"": ""Outcome"" },
            { ""sourceType"": ""RQ"", ""sourceId"": ""What is the impact of AI coding tools on developer productivity?"" }
          ]
        }
      ],
      ""exclusionCriteria"": []
    }
  ]
}");
            sb.AppendLine();
            sb.AppendLine("Final Reminder: Return ONLY the JSON object.");

            return sb.ToString();
        }
    }
}
