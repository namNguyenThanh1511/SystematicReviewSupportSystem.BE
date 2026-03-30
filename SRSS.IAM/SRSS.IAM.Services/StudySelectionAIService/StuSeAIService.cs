using System.Text;
using System.Text.Json;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.StudySelection;
using SRSS.IAM.Services.GeminiService;
using SRSS.IAM.Services.Mappers;

namespace SRSS.IAM.Services.StudySelectionAIService
{
    public interface IStuSeAIService
    {
        Task<StuSeAIOutput> EvaluateTitleAbstractAsync(StuSeAIInput input);

        Task<StuSeAIOutput> EvaluateTitleAbstractAsync(
            Guid studySelectionId,
            Guid paperId,
            Guid reviewerId,
            CancellationToken cancellationToken = default
        );
    }

    public class StuSeAIService : IStuSeAIService
    {
        private readonly IGeminiService _geminiService;
        private readonly IUnitOfWork _unitOfWork;

        public StuSeAIService(IGeminiService geminiService, IUnitOfWork unitOfWork)
        {
            _geminiService = geminiService;
            _unitOfWork = unitOfWork;
        }

        public async Task<StuSeAIOutput> EvaluateTitleAbstractAsync(
            Guid studySelectionId,
            Guid paperId,
            Guid reviewerId,
            CancellationToken cancellationToken = default)
        {
            // 1. Load Study Selection Process with necessary relations
            var studySelectionProcess = await _unitOfWork.StudySelectionProcesses.GetForAiEvaluationAsync(studySelectionId);

            if (studySelectionProcess == null)
            {
                throw new InvalidOperationException($"StudySelectionProcess with ID {studySelectionId} not found.");
            }

            // 2. Load Paper with necessary relations
            var paper = await _unitOfWork.Papers.GetForAiEvaluationAsync(paperId);

            if (paper == null)
            {
                throw new InvalidOperationException($"Paper with ID {paperId} not found.");
            }

            // 3. Validation Rules
            if (paper.ProjectId != studySelectionProcess.ReviewProcess.ProjectId)
            {
                throw new ArgumentException("Paper does not belong to the same project as the study selection process.");
            }


            if (studySelectionProcess.CurrentPhase != ScreeningPhase.TitleAbstract)
            {
                throw new InvalidOperationException($"AI evaluation is only available during Title/Abstract phase. Current phase: {studySelectionProcess.CurrentPhase}.");
            }

            if (!paper.PaperAssignments.Any(a =>
                a.StudySelectionProcessId == studySelectionId &&
                a.ProjectMember.UserId == reviewerId &&
                a.Phase == ScreeningPhase.TitleAbstract))
            {
                throw new InvalidOperationException("Paper is not assigned to the current reviewer for Title/Abstract screening.");
            }

            // 4. Load Protocol
            var protocol = studySelectionProcess.ReviewProcess.Protocol;
            if (protocol == null)
            {
                throw new InvalidOperationException("Approved review protocol not found for this process.");
            }

            // if (protocol.Status != ProtocolStatus.Approved)
            // {
            //     throw new InvalidOperationException("Protocol must be approved before AI evaluation.");
            // }

            // 5. Build AI Input and Evaluate
            var aiInput = protocol.BuildStuSeAIInput(paper);
            var aiOutput = await EvaluateTitleAbstractAsync(aiInput);

            // 6. Save AI Result
            await SaveAIResultAsync(studySelectionId, paperId, reviewerId, ScreeningPhase.TitleAbstract, aiOutput, cancellationToken);

            return aiOutput;
        }

        public async Task<StuSeAIOutput> EvaluateTitleAbstractAsync(StuSeAIInput input)
        {
            var prompt = BuildPrompt(input);
            return await _geminiService.GenerateStructuredContentAsync<StuSeAIOutput>(prompt);
        }

        private string BuildPrompt(StuSeAIInput input)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Act as a senior Systematic Literature Review (SLR) reviewer. Your task is to evaluate a paper for Title/Abstract screening using STRICT deterministic scoring.");
            sb.AppendLine();

            sb.AppendLine("### CRITICAL RULES");
            sb.AppendLine("1. ONLY use information explicitly present in the Title/Abstract.");
            sb.AppendLine("2. For EVERY matching field: \"Value\" MUST be copied EXACTLY from the protocol input provided below. DO NOT paraphrase.");
            sb.AppendLine("3. DO NOT infer or assume missing data. If there is NOT enough evidence or protocol value is missing → Match = \"Unknown\".");
            sb.AppendLine("4. Matching values MUST be EXACTLY one of: \"Match\", \"NotMatch\", \"Unknown\". No variations.");
            sb.AppendLine("5. RETURN ONLY valid JSON. No explanation outside JSON.");
            sb.AppendLine();

            sb.AppendLine("### FIELD USAGE RULES");
            sb.AppendLine("- All matching fields (Language, Domain, StudyType, TimeRange, Population, etc.) MUST appear in the output.");
            if (string.IsNullOrEmpty(input.Paper.Abstract))
            {
                sb.AppendLine("- CRITICAL HARD RULE: Abstract is null or missing. Evaluation MUST rely on Title ONLY. Most fields should be \"Unknown\" unless explicitly stated in the Title.");
            }
            sb.AppendLine("- If protocol value for a field is null or empty → its matched \"Value\" in JSON must be null and \"Match\" must be \"Unknown\".");
            sb.AppendLine("- Only PROVIDED fields (where protocol input is not null/empty) contribute to the group score calculation.");
            sb.AppendLine();

            sb.AppendLine("### LIST OUTPUT INTEGRITY");
            sb.AppendLine("- ResearchQuestionMatching MUST have EXACTLY the same length and order as the input RESEARCH QUESTIONS list.");
            sb.AppendLine("- InclusionCriteriaResults MUST have EXACTLY the same length and order as the input Inclusion Criteria list.");
            sb.AppendLine("- ExclusionCriteriaResults MUST have EXACTLY the same length and order as the input Exclusion Criteria list.");
            sb.AppendLine("- DO NOT add, remove, or reorder any items in these lists.");
            sb.AppendLine();

            sb.AppendLine("### REQUIRED JSON FORMAT");
            sb.AppendLine("{");
            sb.AppendLine("  \"CriteriaMatching\": {");
            sb.AppendLine("    \"Language\": { \"Value\": \"...\", \"Match\": \"Match|NotMatch|Unknown\" },");
            sb.AppendLine("    \"Domain\": { \"Value\": \"...\", \"Match\": \"Match|NotMatch|Unknown\" },");
            sb.AppendLine("    \"StudyType\": { \"Value\": \"...\", \"Match\": \"Match|NotMatch|Unknown\" },");
            sb.AppendLine("    \"TimeRange\": { \"Value\": \"...\", \"Match\": \"Match|NotMatch|Unknown\" }");
            sb.AppendLine("  },");
            sb.AppendLine("  \"PicocMatching\": {");
            sb.AppendLine("    \"Population\": { \"Value\": \"...\", \"Match\": \"Match|NotMatch|Unknown\" },");
            sb.AppendLine("    \"Intervention\": { \"Value\": \"...\", \"Match\": \"Match|NotMatch|Unknown\" },");
            sb.AppendLine("    \"Comparison\": { \"Value\": \"...\", \"Match\": \"Match|NotMatch|Unknown\" },");
            sb.AppendLine("    \"Outcome\": { \"Value\": \"...\", \"Match\": \"Match|NotMatch|Unknown\" },");
            sb.AppendLine("    \"Context\": { \"Value\": \"...\", \"Match\": \"Match|NotMatch|Unknown\" }");
            sb.AppendLine("  },");
            sb.AppendLine("  \"ResearchQuestionMatching\": [ { \"Question\": \"...\", \"Match\": \"Match|NotMatch|Unknown\" } ],");
            sb.AppendLine("  \"InclusionCriteriaResults\": [ { \"Rule\": \"...\", \"Match\": \"Match|NotMatch|Unknown\" } ],");
            sb.AppendLine("  \"ExclusionCriteriaResults\": [ { \"Rule\": \"...\", \"Match\": \"Match|NotMatch|Unknown\", \"Highlight\": \"...\" } ],");
            sb.AppendLine("  \"InclusionMatches\": 0,");
            sb.AppendLine("  \"ExclusionMatches\": 0,");
            sb.AppendLine("  \"ExclusionHighlights\": [],");
            sb.AppendLine("  \"RelevanceScore\": 0.0,");
            sb.AppendLine("  \"Recommendation\": \"Include|Exclude|Uncertain\",");
            sb.AppendLine("  \"Reasoning\": \"step-by-step scoring calculation\"");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine("---");
            sb.AppendLine("### PAPER INFORMATION");
            sb.AppendLine($"Title: {input.Paper.Title}");
            sb.AppendLine($"Abstract: {(string.IsNullOrEmpty(input.Paper.Abstract) ? "[MISSING - Use Title only]" : input.Paper.Abstract)}");
            sb.AppendLine($"Keywords: {input.Paper.Keywords ?? "Not provided"}");
            sb.AppendLine($"Year: {input.Paper.PublicationYear?.ToString() ?? "Not provided"}");
            sb.AppendLine($"Language: {input.Paper.Language ?? "Not provided"}");
            sb.AppendLine();

            if (input.ResearchQuestions != null && input.ResearchQuestions.Any())
            {
                sb.AppendLine("### RESEARCH QUESTIONS (Context only, does NOT affect RelevanceScore)");
                foreach (var rq in input.ResearchQuestions) sb.AppendLine($"- {rq}");
                sb.AppendLine();
            }

            sb.AppendLine("### REVIEW PROTOCOL");
            sb.AppendLine($"#### Domain: {input.Criteria?.Domain ?? "Not provided"}");
            sb.AppendLine();
            sb.AppendLine("#### PICOC");
            sb.AppendLine($"Population: {input.PICOC?.Population ?? "Not provided"}");
            sb.AppendLine($"Intervention: {input.PICOC?.Intervention ?? "Not provided"}");
            sb.AppendLine($"Comparison: {input.PICOC?.Comparison ?? "Not provided"}");
            sb.AppendLine($"Outcome: {input.PICOC?.Outcome ?? "Not provided"}");
            sb.AppendLine($"Context: {input.PICOC?.Context ?? "Not provided"}");
            sb.AppendLine();

            if (input.InclusionCriteria != null && input.InclusionCriteria.Any())
            {
                sb.AppendLine("#### Inclusion Criteria");
                foreach (var ic in input.InclusionCriteria) sb.AppendLine($"- {ic}");
                sb.AppendLine();
            }

            if (input.ExclusionCriteria != null && input.ExclusionCriteria.Any())
            {
                sb.AppendLine("#### Exclusion Criteria");
                foreach (var ec in input.ExclusionCriteria) sb.AppendLine($"- {ec}");
                sb.AppendLine();
            }

            sb.AppendLine("---");
            sb.AppendLine("### MATCHING LOGIC");
            sb.AppendLine("For EACH field (except Exclusion Criteria):");
            sb.AppendLine("- Match → clearly aligned with paper evidence");
            sb.AppendLine("- NotMatch → clearly contradicted");
            sb.AppendLine("- Unknown → insufficient info or protocol value missing");
            sb.AppendLine();
            sb.AppendLine("For Exclusion Criteria (Special Rule):");
            sb.AppendLine("- Match → the paper VIOLATES this exclusion criterion (VIOLATION!). \"Highlight\" field MUST contain the exact violating text from the paper.");
            sb.AppendLine("- NotMatch → the paper DOES NOT violate this criterion. \"Highlight\" field MUST be empty string \"\".");
            sb.AppendLine("- Unknown → insufficient info to determine violation. \"Highlight\" field MUST be empty string \"\".");
            sb.AppendLine();
            sb.AppendLine("IMPORTANT: DO NOT mark Match without explicit evidence. DO NOT guess.");
            sb.AppendLine();

            sb.AppendLine("### SCORING MODEL (STRICT DETERMINISTIC)");
            sb.AppendLine("Total score = 1.0 (4 groups × 0.25)");
            sb.AppendLine();

            // Group 1: Criteria Matching
            var criteriaWeights = new List<string>();
            if (input.Criteria != null && !string.IsNullOrEmpty(input.Criteria.Domain)) criteriaWeights.Add("Domain");

            sb.AppendLine("GROUP 1: CRITERIA MATCHING (0.25)");
            if (criteriaWeights.Count > 0)
            {
                double w = 0.25 / criteriaWeights.Count;
                sb.AppendLine($"- Weight per PROVIDED field ({string.Join(", ", criteriaWeights)}) = {w:F4}");
                sb.AppendLine("- All other fields (Language, StudyType, TimeRange) have Match: \"Unknown\" and weight: 0.");
                sb.AppendLine("- Scoring: Match = \"Match\" → full weight, otherwise → 0.");
            }
            else sb.AppendLine("- No criteria provided. Group Score = 0.");
            sb.AppendLine();

            // Group 2: PICOC Matching
            var picocWeights = new List<string>();
            if (input.PICOC != null)
            {
                if (!string.IsNullOrEmpty(input.PICOC.Population)) picocWeights.Add("Population");
                if (!string.IsNullOrEmpty(input.PICOC.Intervention)) picocWeights.Add("Intervention");
                if (!string.IsNullOrEmpty(input.PICOC.Comparison)) picocWeights.Add("Comparison");
                if (!string.IsNullOrEmpty(input.PICOC.Outcome)) picocWeights.Add("Outcome");
                if (!string.IsNullOrEmpty(input.PICOC.Context)) picocWeights.Add("Context");
            }

            sb.AppendLine("GROUP 2: PICOC MATCHING (0.25)");
            if (picocWeights.Count > 0)
            {
                double w = 0.25 / picocWeights.Count;
                sb.AppendLine($"- Weight per PROVIDED field ({string.Join(", ", picocWeights)}) = {w:F4}");
                sb.AppendLine("- Scoring: Match = \"Match\" → full weight, otherwise → 0.");
            }
            else sb.AppendLine("- No PICOC provided. Group Score = 0.");
            sb.AppendLine();

            sb.AppendLine("GROUP 3: INCLUSION (0.25)");
            if (input.InclusionCriteria != null && input.InclusionCriteria.Any())
            {
                sb.AppendLine("- InclusionMatches = count of InclusionCriteriaResults with Match = \"Match\".");
                sb.AppendLine($"- Score = (InclusionMatches / {input.InclusionCriteria.Count}) × 0.25");
            }
            else sb.AppendLine("- No inclusion criteria. InclusionMatches = 0. Score = 0. (DO NOT divide)");
            sb.AppendLine();

            sb.AppendLine("GROUP 4: EXCLUSION (0.25)");
            if (input.ExclusionCriteria != null && input.ExclusionCriteria.Any())
            {
                sb.AppendLine("- ExclusionMatches = count of ExclusionCriteriaResults with Match = \"Match\" (violations).");
                sb.AppendLine("- If ExclusionMatches > 0 → RelevanceScore for this group = 0.");
                sb.AppendLine("- If ExclusionMatches = 0 → RelevanceScore for this group = 0.25.");
            }
            else sb.AppendLine("- No exclusion criteria provided. ExclusionMatches = 0. Score = 0.25 (Implicit pass).");
            sb.AppendLine();

            sb.AppendLine("### FINAL SCORE");
            sb.AppendLine("RelevanceScore = sum of all groups. Range: 0.0 to 1.0. DO NOT round intermediate values.");
            sb.AppendLine("- ExclusionHighlights MUST be an aggregation of ALL individual \"Highlight\" strings from violated exclusion criteria.");
            sb.AppendLine();

            sb.AppendLine("### RECOMMENDATION (BR10)");
            sb.AppendLine("- Include: RelevanceScore ≥ 0.7 AND ExclusionMatches = 0.");
            sb.AppendLine("- Exclude: RelevanceScore < 0.4 OR ExclusionMatches > 0.");
            sb.AppendLine("- Uncertain: otherwise.");
            sb.AppendLine();

            sb.AppendLine("### REASONING REQUIREMENT");
            sb.AppendLine("MUST include: Detailed Match results for each field, each group score calculation with weights used, and a final sum demonstration.");
            sb.AppendLine();

            sb.AppendLine("### HARD CONSTRAINTS");
            sb.AppendLine("- Output MUST be valid JSON. NO markdown. NO explanation outside JSON. NO missing fields.");

            return sb.ToString();
        }

        private async Task SaveAIResultAsync(
            Guid studySelectionId,
            Guid paperId,
            Guid reviewerId,
            ScreeningPhase phase,
            StuSeAIOutput aiOutput,
            CancellationToken cancellationToken)
        {
            var existingResult = await _unitOfWork.StudySelectionAIResults.GetByKeysAsync(studySelectionId, paperId, reviewerId, ScreeningPhase.TitleAbstract, cancellationToken);

            if (existingResult != null)
            {
                existingResult.AIOutputJson = JsonSerializer.Serialize(aiOutput);
                existingResult.RelevanceScore = aiOutput.RelevanceScore;
                existingResult.Recommendation = MapRecommendation(aiOutput.Recommendation);
                existingResult.ModifiedAt = DateTimeOffset.UtcNow;
                await _unitOfWork.StudySelectionAIResults.UpdateAsync(existingResult, cancellationToken);
            }
            else
            {
                var newResult = new StudySelectionAIResult
                {
                    Id = Guid.NewGuid(),
                    StudySelectionProcessId = studySelectionId,
                    PaperId = paperId,
                    ReviewerId = reviewerId,
                    Phase = phase,
                    AIOutputJson = JsonSerializer.Serialize(aiOutput),
                    RelevanceScore = aiOutput.RelevanceScore,
                    Recommendation = MapRecommendation(aiOutput.Recommendation),
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                };
                await _unitOfWork.StudySelectionAIResults.AddAsync(newResult, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private StuSeAIRecommendation MapRecommendation(string recommendation)
        {
            return recommendation switch
            {
                "Include" => StuSeAIRecommendation.Include,
                "Exclude" => StuSeAIRecommendation.Exclude,
                _ => StuSeAIRecommendation.Uncertain
            };
        }
    }
}
