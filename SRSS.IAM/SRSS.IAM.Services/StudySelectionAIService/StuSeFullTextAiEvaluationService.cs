using System.Text.Json;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using System.Text;
using Microsoft.Extensions.Logging;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.PaperFullText;
using SRSS.IAM.Services.DTOs.StudySelection;
using SRSS.IAM.Services.Mappers;
using SRSS.IAM.Services.OpenRouter;
using SRSS.IAM.Services.StudySelectionAIService.Retrieval;

namespace SRSS.IAM.Services.StudySelectionAIService
{
    public class StuSeFullTextAiEvaluationService : IStuSeFullTextAiEvaluationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStuSeProtocolChunkRetrievalService _retrievalService;
        private readonly IStudySelectionAIResultService _aiResultService;
        private readonly ILogger<StuSeFullTextAiEvaluationService> _logger;
        private readonly IOpenRouterService _openRouterService;
        private readonly IStuSeFullTextAiEvaluationQueue _queue;

        private const int TopKPerQuery = 3; // Controls retrieval breadth per semantic query
        private const int MaxChunksInPrompt = 20; // Controls final prompt evidence count
        private const int MaxChunkTextLength = 2500; // Character limit per chunk in prompt (simple character-based strategy for v1)

        private readonly IStuSeProtocolRetrievalQueryBuilder _queryBuilder;

        public StuSeFullTextAiEvaluationService(
            IUnitOfWork unitOfWork,
            IStuSeProtocolChunkRetrievalService retrievalService,
            IStuSeProtocolRetrievalQueryBuilder queryBuilder,
            IStudySelectionAIResultService aiResultService,
            ILogger<StuSeFullTextAiEvaluationService> logger,
            IOpenRouterService openRouterService,
            IStuSeFullTextAiEvaluationQueue queue)
        {
            _unitOfWork = unitOfWork;
            _retrievalService = retrievalService;
            _queryBuilder = queryBuilder;
            _aiResultService = aiResultService;
            _logger = logger;
            _openRouterService = openRouterService;
            _queue = queue;
        }

        public async Task<StuSeAIOutput> EvaluateFullTextAsync(
            Guid studySelectionId,
            Guid paperId,
            Guid reviewerId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting Full-Text AI evaluation for StudySelection {Id}, Paper {PaperId}", studySelectionId, paperId);

            // 0. Check background job and existing results
            if (_queue.IsProcessing(studySelectionId, paperId))
            {
                throw new InvalidOperationException("AI analysis is running in background job. Please wait for the job to complete.");
            }

            var existingResult = await _aiResultService.GetByKeysAsync(studySelectionId, paperId, reviewerId, ScreeningPhase.FullText, cancellationToken);
            if (existingResult != null)
            {
                throw new InvalidOperationException("AI result for this paper already exists. Please reload the page to view it.");
            }

            // 1. Load Process and Paper (Reusing logic philosophy from Title/Abstract)
            var studySelectionProcess = await _unitOfWork.StudySelectionProcesses.GetForAiEvaluationAsync(studySelectionId);
            if (studySelectionProcess == null)
            {
                throw new InvalidOperationException($"StudySelectionProcess with ID {studySelectionId} not found.");
            }

            var paper = await _unitOfWork.Papers.GetForAiEvaluationAsync(paperId);
            if (paper == null)
            {
                throw new InvalidOperationException($"Paper with ID {paperId} not found.");
            }

            // 2. Validation
            if (paper.ProjectId != studySelectionProcess.ReviewProcess.ProjectId)
            {
                throw new ArgumentException("Paper does not belong to the same project as the study selection process.");
            }

            // Check if user is assigned OR is a Project Leader
            var isLeader = studySelectionProcess.ReviewProcess.Project.ProjectMembers
                .Any(m => m.UserId == reviewerId && m.Role == ProjectRole.Leader);

            if (!isLeader && !paper.PaperAssignments.Any(a =>
                a.StudySelectionProcessId == studySelectionId &&
                a.ProjectMember.UserId == reviewerId &&
                a.Phase == ScreeningPhase.FullText))
            {
                throw new InvalidOperationException("Paper is not assigned to the current reviewer for Full-Text screening.");
            }

            // 3. Build AI Input
            var aiInput = studySelectionProcess.BuildStuSeAIInput(paper);

            // 4. Retrieve Relevant Chunks
            var pdf = paper.PaperPdfs.FirstOrDefault(p => p.FullTextProcessed);
            if (pdf == null)
            {
                throw new InvalidOperationException("Paper full-text not found. Extraction and parsing must be completed before AI evaluation.");
            }

            var paperPdfId = pdf.Id;

            var fullText = await _unitOfWork.PaperFullTexts.FindSingleAsync(ft => ft.PaperPdfId == paperPdfId, cancellationToken: cancellationToken);
            if (fullText == null)
            {
                throw new InvalidOperationException("Paper full-text data not found. Extraction and parsing must be completed before AI evaluation.");
            }

            var retrievedChunks = await _retrievalService.RetrieveRelevantChunksAsync(
                paperPdfId,
                aiInput,
                topKPerQuery: TopKPerQuery,
                cancellationToken: cancellationToken);

            if (retrievedChunks == null || !retrievedChunks.Any())
            {
                throw new InvalidOperationException("No relevant chunks retrieved for full-text analysis. Evidence-based scoring cannot proceed.");
            }

            // Step: Prepare chunks for prompt
            // 1. Sort by similarity (most relevant first)
            // 2. Limit total number of chunks to avoid prompt overflow
            // 3. Truncate long chunks to control token size
            var promptChunks = retrievedChunks
                .OrderByDescending(c => c.SimilarityScore)
                .Take(MaxChunksInPrompt)
                .Select(c =>
                {
                    var truncated = c.Text.Length > MaxChunkTextLength
                        ? c.Text.Substring(0, MaxChunkTextLength) + "... [truncated for brevity]"
                        : c.Text;
                    return new ChunkSearchResultDto
                    {
                        ChunkId = c.ChunkId,
                        Order = c.Order,
                        SectionTitle = c.SectionTitle,
                        SectionType = c.SectionType,
                        Text = truncated,
                        SimilarityScore = c.SimilarityScore
                    };
                })
                .ToList();

            var truncatedCount = retrievedChunks.Count(c => c.Text.Length > MaxChunkTextLength);

            _logger.LogInformation(
                "Prompt evidence shaped: RawChunks={RawCount}, FinalChunks={FinalCount}, TruncatedChunks={TruncatedCount}, MaxChunkLength={Limit}",
                retrievedChunks.Count,
                promptChunks.Count,
                truncatedCount,
                MaxChunkTextLength
            );

            var queryCount = _queryBuilder.BuildQueries(aiInput).Count;
            _logger.LogInformation("Orchestration info: ProtocolQueryCount={QueryCount}", queryCount);

            var prompt = BuildFullTextPrompt(aiInput, promptChunks);

            // 6. Call AI Model via OpenRouter
            var aiOutput = await _openRouterService.GenerateStructuredContentAsync<StuSeAIOutput>(prompt, ct: cancellationToken);

            if (aiOutput == null)
            {
                throw new InvalidOperationException("AI Evaluation failed: Could not deserialize model response.");
            }

            // 7. Validate and Harden Output
            ValidateAiOutput(aiOutput, aiInput);

            // 8. Save AI Result
            await _aiResultService.SaveAIResultAsync(studySelectionId, paperId, reviewerId, ScreeningPhase.FullText, aiOutput, cancellationToken);

            _logger.LogInformation("Full-Text AI evaluation completed. Recommendation: {Rec}, Score: {Score}",
                aiOutput.Recommendation, aiOutput.RelevanceScore);

            return aiOutput;
        }

        private string BuildFullTextPrompt(StuSeAIInput input, List<ChunkSearchResultDto> promptChunks)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Act as a senior Systematic Literature Review (SLR) reviewer. Your task is to evaluate a paper for Full-Text screening using STRICT deterministic scoring based ON EXTRACTED EVIDENCE CHUNKS.");
            sb.AppendLine();

            sb.AppendLine("### EVIDENCE-BASED DECISION RULES (CRITICAL)");
            sb.AppendLine("1. ONLY use the provided 'Retrieved Evidence Chunks' as source text.");
            sb.AppendLine("2. CHUNK PRIORITY: Evidence chunks are sorted by RELEVANCE. Prioritize information from higher-ranked chunks.");
            sb.AppendLine("3. RESOLVE CONFLICTS: If multiple chunks provide conflicting evidence, explicitly explain the conflict in the 'Reasoning' section.");
            sb.AppendLine("4. DO NOT hallucinate evidence. If no chunk supports a criterion, mark it as 'Unknown'.");
            sb.AppendLine("5. DO NOT force a 'Match' just to increase the score. Accuracy and traceability are paramount.");
            sb.AppendLine("6. CITE EVIDENCE: In the 'Reasoning' section, ALWAYS cite the Section Title(s) and Rank(s) of the evidence that support your judgment.");
            sb.AppendLine();

            sb.AppendLine("### OUTPUT STRUCTURE ENFORCEMENT");
            sb.AppendLine("1. CRITERIA GROUP COUNT LOCK: The number of objects inside \"CriteriaGroupResults\" MUST be EXACTLY the same as the number of CRITERIA GROUPS provided in the protocol input.");
            sb.AppendLine("2. CRITERIA GROUP ORDER LOCK: CriteriaGroupResults MUST appear in the EXACT same order as the criteria groups listed in the protocol input.");
            sb.AppendLine("3. EXCLUSION HIGHLIGHTS FORMAT: \"ExclusionHighlights\" MUST always be an array of strings.");
            sb.AppendLine("4. STRICT JSON OUTPUT: The output MUST be a single valid JSON object. No markdown wrapper, no extra text.");
            sb.AppendLine();

            sb.AppendLine("### DATA INTEGRITY RULES");
            sb.AppendLine("7. NO REASONING INSIDE STRUCTURED FIELDS: Rule, Highlight MUST contain ONLY structured values. Explanations MUST appear ONLY in the \"Reasoning\" section.");
            sb.AppendLine("8. MATCH VALUES: Must be EXACTLY one of: \"Match\", \"NotMatch\", \"Unknown\". No variations.");
            sb.AppendLine("9. UNKNOWN & NOTMATCH BEHAVIOR: 'Unknown' and 'NotMatch' do NOT count as Match. They contribute 0 to scoring. Only 'Match' increases the score.");
            sb.AppendLine();

            sb.AppendLine("10. REASONING COMPLETENESS RULE:");
            sb.AppendLine("The Reasoning section MUST include ALL of the following sections in EXACT order:");
            sb.AppendLine("## Criteria Groups Score");
            sb.AppendLine("## Final Score Calculation");
            sb.AppendLine("## Final Recommendation");
            sb.AppendLine("If ANY section is missing, the output is INVALID.");
            sb.AppendLine();

            sb.AppendLine("### REQUIRED JSON FORMAT");
            sb.AppendLine("{");
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
            sb.AppendLine("  \"Reasoning\": \"Markdown formatted hierarchical scoring calculation citing specific chunks\"");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine("---");
            sb.AppendLine("### PAPER INFORMATION");
            sb.AppendLine($"Title: {input.Paper.Title}");
            sb.AppendLine();

            sb.AppendLine("### RETRIEVED EVIDENCE CHUNKS (THE ONLY SOURCE)");
            foreach (var chunk in promptChunks)
            {
                var sectionTitle = chunk.SectionTitle ?? "Unknown Section";
                sb.AppendLine($"[Source: {sectionTitle} (Rank: {promptChunks.IndexOf(chunk) + 1})]");
                sb.AppendLine(chunk.Text);
                sb.AppendLine();
            }

            sb.AppendLine("### REVIEW PROTOCOL");
            if (input.CriteriaGroups != null && input.CriteriaGroups.Any())
            {
                sb.AppendLine("#### CRITERIA GROUPS");
                foreach (var group in input.CriteriaGroups)
                {
                    sb.AppendLine($"- Group: {group.Description ?? "No description"}");
                    if (group.Inclusion != null && group.Inclusion.Any())
                    {
                        sb.AppendLine("  - Inclusion Rules:");
                        foreach (var ir in group.Inclusion) sb.AppendLine($"    * {ir}");
                    }
                    if (group.Exclusion != null && group.Exclusion.Any())
                    {
                        sb.AppendLine("  - Exclusion Rules:");
                        foreach (var er in group.Exclusion) sb.AppendLine($"    * {er}");
                    }
                }
                sb.AppendLine();
            }

            sb.AppendLine("---");
            sb.AppendLine("### SCORING MODEL (Deterministic & Hierarchical)");
            sb.AppendLine("RelevanceScore ∈ [0, 1] (Final RelevanceScore MUST be clamped to [0, 1] and rounded to 2-4 decimal places).");
            sb.AppendLine();
            sb.AppendLine("1. Valid Scoring Groups (G) - 2-Phase Evaluation:");
            sb.AppendLine("   - Step 1: Identify candidate groups (groups that have at least one inclusion rule).");
            sb.AppendLine("   - Step 2: AFTER semantic clustering of rules against paper evidence, only groups with at least one valid inclusion cluster supported or addressed in evidence are considered ValidScoringGroups.");
            sb.AppendLine("   - G = number of ValidScoringGroups.");
            sb.AppendLine();
            sb.AppendLine("   - Handling of non-scoring groups:");
            sb.AppendLine("     * Groups with ONLY exclusion rules MUST be evaluated and included in CriteriaGroupResults, but they MUST NOT contribute to scoring (no GroupScore, no weight, no penalty applied to final score).");
            sb.AppendLine("     * Groups with 0 inclusion clusters MUST be excluded from G and RelevanceScore.");
            sb.AppendLine();
            sb.AppendLine("2. Per-Group Scoring (GroupScore) for ValidScoringGroups:");
            sb.AppendLine("   - All exclusion rules are treated as SOFT penalties. There is no 'Hard' exclusion override.");
            sb.AppendLine("   - AI MUST explicitly determine for each group:");
            sb.AppendLine("     * Total number of UNIQUE inclusion clusters.");
            sb.AppendLine("     * Number of MATCHED inclusion clusters (supported by evidence).");
            sb.AppendLine("     * Total number of UNIQUE exclusion clusters.");
            sb.AppendLine("     * Number of VIOLATED exclusion clusters (evidence found for exclusion).");
            sb.AppendLine("   - InclusionScore = (matched inclusion clusters) / (total unique inclusion clusters).");
            sb.AppendLine("   - Penalty Calculation:");
            sb.AppendLine("     * If total unique exclusion clusters == 0: Penalty = 0.");
            sb.AppendLine("     * Else: Penalty = (violated exclusion clusters) / (total unique exclusion clusters).");
            sb.AppendLine("   - GroupScore = InclusionScore × (1 - Penalty).");
            sb.AppendLine();
            sb.AppendLine("3. Final Aggregation:");
            sb.AppendLine("   - RelevanceScore = average(GroupScore of all ValidScoringGroups).");
            sb.AppendLine("   - If G == 0, RelevanceScore = 0 and Recommendation = 'Uncertain'.");
            sb.AppendLine("   - Round final RelevanceScore to 2-4 decimal places.");
            sb.AppendLine();

            sb.AppendLine("### FINAL RECOMMENDATION (Score-Only)");
            sb.AppendLine("- IF RelevanceScore ≥ 0.7 -> Recommendation = 'Include'.");
            sb.AppendLine("- IF RelevanceScore < 0.4 -> Recommendation = 'Exclude'.");
            sb.AppendLine("- ELSE -> Recommendation = 'Uncertain'.");
            sb.AppendLine();

            sb.AppendLine("### REASONING FORMAT & EVIDENCE MAPPING (STRICT ENFORCEMENT)");
            sb.AppendLine("The 'Reasoning' field MUST follow this structure for EACH Criteria Group:");
            sb.AppendLine();
            sb.AppendLine("## Group: <Group Description>");
            sb.AppendLine("- Inclusion Cluster Evaluation:");
            sb.AppendLine("  - Cluster 1: <description>");
            sb.AppendLine("    - Rule: <rule text>");
            sb.AppendLine("    - Match: Match | NotMatch | Unknown");
            sb.AppendLine("    - Evidence: [Section: <Section Title>, Rank: <#>] \"<short snippet>\"");
            sb.AppendLine("    - Match Explanation: How the evidence supports/contradicts this rule.");
            sb.AppendLine("- Exclusion Cluster Evaluation:");
            sb.AppendLine("  - Cluster 1: <description>");
            sb.AppendLine("    - Rule: <rule text>");
            sb.AppendLine("    - Match: Match | NotMatch | Unknown");
            sb.AppendLine("    - Evidence: [Section: <Section Title>, Rank: <#>] \"<short snippet>\"");
            sb.AppendLine("    - Violation Explanation: Why this exclusion rule is matched (violated) or not.");
            sb.AppendLine();
            sb.AppendLine("- Scoring Metrics:");
            sb.AppendLine("  - Matched Clusters: <#> / <Total>");
            sb.AppendLine("  - Violated Exclusions: <#> / <Total>");
            sb.AppendLine("  - InclusionScore: <#>");
            sb.AppendLine("  - Penalty: <#>");
            sb.AppendLine("  - GroupScore: <#>");
            sb.AppendLine();
            sb.AppendLine("### CRITICAL EVIDENCE RULES:");
            sb.AppendLine("1. EXPLICIT MAPPING: EVERY inclusion and exclusion rule MUST be explicitly listed. Do NOT skip any rule.");
            sb.AppendLine("2. SOURCE TRACEABILITY: Evidence MUST use the exact format: [Section: ..., Rank: ...]. Do NOT just say 'based on text'.");
            sb.AppendLine("3. NO HALLUCINATION: If no evidence is found for a rule:");
            sb.AppendLine("   - Match MUST be 'Unknown'");
            sb.AppendLine("   - Evidence MUST state: 'No relevant evidence found in retrieved chunks'");
            sb.AppendLine("4. CONFLICT RESOLUTION: If multiple chunks contradict:");
            sb.AppendLine("   - MUST list both [Section: ..., Rank: ...]");
            sb.AppendLine("   - MUST explain resolution: which is trusted and why.");
            sb.AppendLine("5. EXCLUSION EVIDENCE: Violated exclusions MUST have strong evidence snippets. Non-matched exclusions MUST explain why the evidence does NOT apply or is absent.");
            sb.AppendLine("6. EVIDENCE DENSITY:");
            sb.AppendLine("   - Prefer multiple evidence chunks when available.");
            sb.AppendLine("   - Prioritize higher-ranked chunks (Rank 1-5). Lower-ranked chunks can support but not override stronger evidence.");
            sb.AppendLine("7. SEMANTIC CLUSTERING: Explicitly state: 'Scoring is based on UNIQUE semantic clusters identified in evidence, not raw rule counts'.");
            sb.AppendLine();
            sb.AppendLine("IMPORTANT: RETURN ONLY VALID JSON. NO MARKDOWN WRAPPER. NO TEXT BEFORE/AFTER.");

            return sb.ToString();
        }

        private void ValidateAiOutput(StuSeAIOutput? output, StuSeAIInput input)
        {
            if (output == null)
            {
                throw new InvalidOperationException("AI Evaluation failed: AI returned null output.");
            }

            if (string.IsNullOrWhiteSpace(output.Reasoning))
            {
                throw new InvalidOperationException("AI Evaluation failed: Structured output reasoning is empty.");
            }

            if (output.CriteriaGroupResults == null || output.CriteriaGroupResults.Count != input.CriteriaGroups.Count)
            {
                throw new InvalidOperationException($"AI Evaluation failed: Criteria Group result count ({output.CriteriaGroupResults?.Count ?? 0}) does not match input count ({input.CriteriaGroups.Count}).");
            }

            if (output.RelevanceScore < 0 || output.RelevanceScore > 1)
            {
                throw new InvalidOperationException($"AI Evaluation failed: Invalid relevance score range: {output.RelevanceScore}");
            }

            if (string.IsNullOrEmpty(output.Recommendation))
            {
                throw new InvalidOperationException("AI Evaluation failed: Missing recommendation field.");
            }

            var validRecommendations = new[] { "Include", "Exclude", "Uncertain" };
            if (!validRecommendations.Contains(output.Recommendation))
            {
                throw new InvalidOperationException($"AI Evaluation failed: Invalid recommendation value: {output.Recommendation}. Allowed: Include, Exclude, Uncertain.");
            }
        }
    }
}
