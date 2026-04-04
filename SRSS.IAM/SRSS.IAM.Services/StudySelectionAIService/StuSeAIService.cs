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
            sb.AppendLine("- All matching fields MUST appear in the output JSON.");
            if (string.IsNullOrEmpty(input.Paper.Abstract))
            {
                sb.AppendLine("- CRITICAL HARD RULE: Abstract is null or missing. Evaluation MUST rely on Title ONLY. Most fields should be \"Unknown\" unless explicitly stated in the Title.");
            }
            sb.AppendLine("- If protocol value for a field is null or empty → its matched \"Value\" in JSON must be null and \"Match\" must be \"Unknown\".");
            sb.AppendLine();

            sb.AppendLine("### LIST OUTPUT INTEGRITY");
            sb.AppendLine("- ResearchQuestionResults MUST have EXACTLY the same length and order as the input RESEARCH QUESTIONS list.");
            sb.AppendLine("- CriteriaGroupResults MUST have EXACTLY the same length and order as the input CRITERIA GROUPS list.");
            sb.AppendLine("- Within each CriteriaGroupResult, InclusionResults and ExclusionResults MUST match the order and length of the corresponding rules in that group.");
            sb.AppendLine();

            sb.AppendLine("### REQUIRED JSON FORMAT");
            sb.AppendLine("{");
            sb.AppendLine("  \"CriteriaMatching\": {");
            sb.AppendLine("    \"Language\": { \"Value\": \"...\", \"Match\": \"Match|NotMatch|Unknown\" },");
            sb.AppendLine("    \"Domain\": { \"Value\": \"...\", \"Match\": \"Match|NotMatch|Unknown\" },");
            sb.AppendLine("    \"StudyType\": { \"Value\": \"...\", \"Match\": \"Match|NotMatch|Unknown\" }");
            sb.AppendLine("  },");
            sb.AppendLine("  },");
            sb.AppendLine("  \"ResearchQuestionResults\": [");
            sb.AppendLine("    {");
            sb.AppendLine("      \"Question\": \"...\",");
            sb.AppendLine("      \"Match\": \"Match|NotMatch|Unknown\",");
            sb.AppendLine("      \"PicocMatching\": {");
            sb.AppendLine("        \"Population\": { \"Value\": \"...\", \"Match\": \"Match|NotMatch|Unknown\" },");
            sb.AppendLine("        \"Intervention\": { \"Value\": \"...\", \"Match\": \"Match|NotMatch|Unknown\" },");
            sb.AppendLine("        \"Comparison\": { \"Value\": \"...\", \"Match\": \"Match|NotMatch|Unknown\" },");
            sb.AppendLine("        \"Outcome\": { \"Value\": \"...\", \"Match\": \"Match|NotMatch|Unknown\" },");
            sb.AppendLine("        \"Context\": { \"Value\": \"...\", \"Match\": \"Match|NotMatch|Unknown\" }");
            sb.AppendLine("      }");
            sb.AppendLine("    }");
            sb.AppendLine("  ],");
            sb.AppendLine("  \"CriteriaGroupResults\": [");
            sb.AppendLine("    {");
            sb.AppendLine("      \"Description\": \"...\",");
            sb.AppendLine("      \"InclusionResults\": [ { \"Rule\": \"...\", \"Match\": \"Match|NotMatch|Unknown\" } ],");
            sb.AppendLine("      \"ExclusionResults\": [ { \"Rule\": \"...\", \"Match\": \"Match|NotMatch|Unknown\", \"Highlight\": \"...\" } ]");
            sb.AppendLine("    }");
            sb.AppendLine("  ],");
            sb.AppendLine("  \"InclusionMatches\": 0,");
            sb.AppendLine("  \"ExclusionMatches\": 0,");
            sb.AppendLine("  \"ExclusionHighlights\": [],");
            sb.AppendLine("  \"RelevanceScore\": 0.0,");
            sb.AppendLine("  \"Recommendation\": \"Include|Exclude|Uncertain\",");
            sb.AppendLine("  \"Reasoning\": \"Step-by-step hierarchical scoring calculation\"");
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

            sb.AppendLine("### REVIEW PROTOCOL");
            sb.AppendLine($"#### Domain: {input.Criteria?.Domain ?? "Not provided"}");
            sb.AppendLine();

            if (input.ResearchQuestions != null && input.ResearchQuestions.Any())
            {
                sb.AppendLine("#### RESEARCH QUESTIONS (with per-RQ PICOC)");
                foreach (var rq in input.ResearchQuestions)
                {
                    sb.AppendLine($"- RQ: {rq.QuestionText}");
                    if (rq.PICOC != null)
                    {
                        if (!string.IsNullOrEmpty(rq.PICOC.Population)) sb.AppendLine($"  - Population: {rq.PICOC.Population}");
                        if (!string.IsNullOrEmpty(rq.PICOC.Intervention)) sb.AppendLine($"  - Intervention: {rq.PICOC.Intervention}");
                        if (!string.IsNullOrEmpty(rq.PICOC.Comparison)) sb.AppendLine($"  - Comparison: {rq.PICOC.Comparison}");
                        if (!string.IsNullOrEmpty(rq.PICOC.Outcome)) sb.AppendLine($"  - Outcome: {rq.PICOC.Outcome}");
                        if (!string.IsNullOrEmpty(rq.PICOC.Context)) sb.AppendLine($"  - Context: {rq.PICOC.Context}");
                    }
                    else sb.AppendLine("  - PICOC: Undefined (Score RQ text relevance directly)");
                }
                sb.AppendLine();
            }

            if (input.CriteriaGroups != null && input.CriteriaGroups.Any())
            {
                sb.AppendLine("#### CRITERIA GROUPS");
                foreach (var group in input.CriteriaGroups)
                {
                    sb.AppendLine($"- Group: {group.Description ?? "No description"}");
                    if (group.InclusionRules.Any())
                    {
                        sb.AppendLine("  - Inclusion Rules:");
                        foreach (var ir in group.InclusionRules) sb.AppendLine($"    * {ir}");
                    }
                    if (group.ExclusionRules.Any())
                    {
                        sb.AppendLine("  - Exclusion Rules:");
                        foreach (var er in group.ExclusionRules) sb.AppendLine($"    * {er}");
                    }
                }
                sb.AppendLine();
            }

            sb.AppendLine("---");
            sb.AppendLine("### MATCHING LOGIC");
            sb.AppendLine("For EACH field (except Exclusion Criteria):");
            sb.AppendLine("- Match → clearly aligned with paper evidence");
            sb.AppendLine("- NotMatch → clearly contradicted");
            sb.AppendLine("- Unknown → insufficient info or protocol value missing");
            sb.AppendLine();
            sb.AppendLine("For Exclusion Rules (Violation Logic):");
            sb.AppendLine("- Match → the paper VIOLATES this exclusion criterion (VIOLATION!). \"Highlight\" field MUST contain violating text.");
            sb.AppendLine("- NotMatch → no violation found. \"Highlight\" empty.");
            sb.AppendLine("- Unknown → insufficient info to determine violation. \"Highlight\" empty.");
            sb.AppendLine();

            sb.AppendLine("### SCORING MODEL (Deterministic & Hierarchical)");
            sb.AppendLine("Composite Score = (0.25 × CriteriaMatchingScore) + (0.25 × PICOCMatchingScore) + (0.50 × CriteriaGroupsScore)");
            sb.AppendLine();

            sb.AppendLine("1. Criteria Matching Score (0.25):");
            sb.AppendLine("- average(all evaluated general criteria rules: Language, Domain, etc.)");
            sb.AppendLine("- Each rule: Match=1, others=0. If protocol field is missing, ignore it in averaging.");
            sb.AppendLine();

            sb.AppendLine("2. PICOC Matching Score (0.25):");
            sb.AppendLine("- Step 1 (Per RQ): RQScore = average(all PICOC elements defined for THAT RQ). Match=1, others=0.");
            sb.AppendLine("- If an RQ has NO PICOC elements defined → RQScore = Match result for the Research Question text itself.");
            sb.AppendLine("- Step 2 (Aggregate): PICOCMatchingScore = average(all RQScore values).");
            sb.AppendLine();

            sb.AppendLine("3. Criteria Groups Score (0.50):");
            sb.AppendLine("- Step 1 (Per Group):");
            sb.AppendLine("  * If ANY Exclusion rule in the group is \"Match\" (Violation) → GroupScore = 0.");
            sb.AppendLine("  * Else → GroupScore = (count of matched inclusion rules / total inclusion rules in group).");
            sb.AppendLine("  * If group has NO inclusion rules and NO exclusion violation → GroupScore = 1.");
            sb.AppendLine("- Step 2 (Aggregate): CriteriaGroupsScore = average(all GroupScore values).");
            sb.AppendLine();

            sb.AppendLine("### FINAL RECOMMENDATION");
            sb.AppendLine("- Include: RelevanceScore ≥ 0.7 AND ExclusionMatches = 0.");
            sb.AppendLine("- Exclude: RelevanceScore < 0.4 OR ExclusionMatches > 0.");
            sb.AppendLine("- Uncertain: otherwise.");
            sb.AppendLine();

            sb.AppendLine("### REASONING FORMAT REQUIREMENTS (STRICT)");
            sb.AppendLine("- The \"Reasoning\" field MUST be formatted using clean Markdown.");
            sb.AppendLine("- Use headings (##, ###) to separate major sections.");
            sb.AppendLine("- Use bullet points (-) for listing items.");
            sb.AppendLine("- Use indentation (2 spaces) for nested details.");
            sb.AppendLine("- Add blank lines between sections for readability.");
            sb.AppendLine("- DO NOT return everything in a single line.");
            sb.AppendLine("- DO NOT escape normal characters like '+' or '\\''.");
            sb.AppendLine();

            sb.AppendLine("### REQUIRED REASONING STRUCTURE");
            sb.AppendLine("The reasoning MUST follow this structure exactly:");
            sb.AppendLine();

            sb.AppendLine("## 1. Formula");
            sb.AppendLine("RelevanceScore = (0.25 × CriteriaMatchingScore) + (0.25 × PICOCMatchingScore) + (0.50 × CriteriaGroupsScore)");
            sb.AppendLine();

            sb.AppendLine("## 2. Criteria Matching Score");
            sb.AppendLine("**CriteriaMatchingScore = <value>**");
            sb.AppendLine("- <field>: Match | NotMatch | Unknown");
            sb.AppendLine();

            sb.AppendLine("## 3. PICOC Matching Score");
            sb.AppendLine("**PICOCMatchingScore = <value>**");
            sb.AppendLine("- RQ 1: <score>");
            sb.AppendLine("- RQ 2: <score>");
            sb.AppendLine();

            sb.AppendLine("## 4. Criteria Groups Score");
            sb.AppendLine("**CriteriaGroupsScore = <value>**");
            sb.AppendLine("- Group 1: <score>");
            sb.AppendLine("- Group 2: <score>");
            sb.AppendLine();

            sb.AppendLine("## 5. Final Calculation");
            sb.AppendLine("**Final Score = <value>**");
            sb.AppendLine();

            sb.AppendLine("## 6. Exclusion Summary");
            sb.AppendLine("**ExclusionMatches = <number>**");
            sb.AppendLine();

            sb.AppendLine("## 7. Recommendation");
            sb.AppendLine("**Recommendation = Include | Exclude | Uncertain**");
            sb.AppendLine("- Reason: <short explanation>");
            sb.AppendLine();

            sb.AppendLine("IMPORTANT: If the Reasoning is not properly formatted in Markdown with clear sections, the response is INVALID.");

            sb.AppendLine("### HARD CONSTRAINTS");
            sb.AppendLine("- Output MUST be valid JSON. NO markdown, only markdown for reasoning. NO explanation outside JSON. NO missing fields.");

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
