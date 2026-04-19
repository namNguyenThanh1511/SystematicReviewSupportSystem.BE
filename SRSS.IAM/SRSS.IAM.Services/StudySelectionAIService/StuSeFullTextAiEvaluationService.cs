using System.Text.Json;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using System.Text;
using Microsoft.Extensions.Logging;
using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.PaperFullText;
using SRSS.IAM.Services.DTOs.StudySelection;
using SRSS.IAM.Services.Mappers;
using SRSS.IAM.Services.StudySelectionAIService.Retrieval;

namespace SRSS.IAM.Services.StudySelectionAIService
{
    public class StuSeFullTextAiEvaluationService : IStuSeFullTextAiEvaluationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStuSeProtocolChunkRetrievalService _retrievalService;
        private readonly IStudySelectionAIResultService _aiResultService;
        private readonly ILogger<StuSeFullTextAiEvaluationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

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
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _unitOfWork = unitOfWork;
            _retrievalService = retrievalService;
            _queryBuilder = queryBuilder;
            _aiResultService = aiResultService;
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<StuSeAIOutput> EvaluateFullTextAsync(
            Guid studySelectionId,
            Guid paperId,
            Guid reviewerId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting Full-Text AI evaluation for StudySelection {Id}, Paper {PaperId}", studySelectionId, paperId);

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

            if (!paper.PaperAssignments.Any(a =>
                a.StudySelectionProcessId == studySelectionId &&
                a.ProjectMember.UserId == reviewerId &&
                a.Phase == ScreeningPhase.FullText))
            {
                throw new InvalidOperationException("Paper is not assigned to the current reviewer for Full-Text screening.");
            }

            var protocol = studySelectionProcess.ReviewProcess.Protocol;
            if (protocol == null)
            {
                throw new InvalidOperationException("Approved review protocol not found for this process.");
            }

            // 3. Build AI Input
            var aiInput = protocol.BuildStuSeAIInput(paper);

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

            // 6. Call Custom Model
            var baseUrl = _configuration["FullText:Model"];
            var apiKey = _configuration["FullText:ModelKey"];
            var modelName = _configuration["FullText:ModelId"] ?? "gpt-5.4";

            if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("Full-Text AI Model configuration is missing (FullText__Model or FullText__ModelKey).");
            }

            using var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            var requestBody = new
            {
                model = modelName,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = 0.1
            };

            var httpResponse = await client.PostAsJsonAsync($"{baseUrl}/chat/completions", requestBody, cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                var errorContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("AI Model error: {StatusCode} - {Error}", httpResponse.StatusCode, errorContent);
                throw new InvalidOperationException($"AI Evaluation failed with status {httpResponse.StatusCode}: {errorContent}");
            }

            var jsonResponse = await httpResponse.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
            var choice = jsonResponse.GetProperty("choices")[0];
            var message = choice.GetProperty("message");
            var content = message.GetProperty("content").GetString();

            if (string.IsNullOrEmpty(content))
            {
                throw new InvalidOperationException("AI Evaluation failed: Received empty response content from model.");
            }

            // Cleanup potential markdown wrappers
            var cleanJson = content.Trim();
            if (cleanJson.StartsWith("```json")) cleanJson = cleanJson.Substring(7);
            if (cleanJson.EndsWith("```")) cleanJson = cleanJson.Substring(0, cleanJson.Length - 3);
            cleanJson = cleanJson.Trim();

            var aiOutput = JsonSerializer.Deserialize<StuSeAIOutput>(cleanJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

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
            sb.AppendLine("### EVIDENCE-BASED DECISION RULE (CRITICAL)");
            sb.AppendLine("1. ONLY use the provided 'Retrieved Evidence Chunks' as source text.");
            sb.AppendLine("2. CHUNK PRIORITY: Evidence chunks are sorted by RELEVANCE (most relevant first). Prioritize information from higher-ranked chunks.");
            sb.AppendLine("3. RESOLVE CONFLICTS: If multiple chunks provide conflicting evidence, explicitly explain the conflict in the 'Reasoning' section.");
            sb.AppendLine("4. DO NOT hallucinate evidence. If no chunk supports a PICOC element or criterion, mark it as 'Unknown'.");
            sb.AppendLine("5. DO NOT force a 'Match' just to increase the score. Accuracy and traceability are paramount.");
            sb.AppendLine("6. CITE EVIDENCE: In the 'Reasoning' section, ALWAYS cite the Section Title(s) of the evidence (e.g., Abstract, Methods, Results) that support your judgment.");
            sb.AppendLine("### OUTPUT STRUCTURE ENFORCEMENT");
            sb.AppendLine("1. RESEARCH QUESTION COUNT LOCK: EXACTLY match the number of RQs provided.");
            sb.AppendLine("2. CRITERIA GROUP COUNT LOCK: EXACTLY match the number of Criteria Groups provided.");
            sb.AppendLine("3. STRICT JSON: No markdown wrapper, no extra text.");
            sb.AppendLine("### REQUIRED JSON FORMAT");

            sb.AppendLine("{");
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
            sb.AppendLine("  \"Reasoning\": \"Markdown formatted hierarchical scoring calculation with CHUNK CITATIONS\"");
            sb.AppendLine("}");
            sb.AppendLine("---");

            sb.AppendLine("### PAPER INFORMATION");
            sb.AppendLine($"Title: {input.Paper.Title}");

            sb.AppendLine("### RETRIEVED EVIDENCE CHUNKS");

            foreach (var chunk in promptChunks)
            {
                var sectionTitle = chunk.SectionTitle ?? "Unknown Section";
                var sectionType = chunk.SectionType ?? "Unknown";
                sb.AppendLine($"[Section: {sectionTitle} | Type: {sectionType}]");
                sb.AppendLine(chunk.Text);
                sb.AppendLine();
            }

            sb.AppendLine("---");
            sb.AppendLine("### REVIEW PROTOCOL");
            if (input.ResearchQuestions.Any())
            {
                sb.AppendLine("#### RESEARCH QUESTIONS");
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
                }
                sb.AppendLine();
            }

            if (input.CriteriaGroups.Any())
            {
                sb.AppendLine("#### CRITERIA GROUPS");
                foreach (var group in input.CriteriaGroups)
                {
                    sb.AppendLine($"- Group: {group.Description ?? "No description"}");
                    foreach (var ir in group.InclusionRules) sb.AppendLine($"  - Inclusion: {ir}");
                    foreach (var er in group.ExclusionRules) sb.AppendLine($"  - Exclusion: {er}");
                }
                sb.AppendLine();
            }

            sb.AppendLine("---");
            sb.AppendLine("### SCORING MODEL (Deterministic & Hierarchical)");
            sb.AppendLine("RelevanceScore = (0.50 × PICOCMatchingScore) + (0.50 × CriteriaGroupsScore)");

            sb.AppendLine("1. PICOC Matching Score (0.50): Mean of RQ scores. RQScore = Mean of PICOC Match (Match=1, etc=0).");
            sb.AppendLine("2. Criteria Groups Score (0.50): Mean of unique semantic cluster scores per group.");

            sb.AppendLine("---");
            sb.AppendLine("### FINAL RECOMMENDATION");
            sb.AppendLine("- Include: RelevanceScore ≥ 0.7 AND NO exclusion violation.");
            sb.AppendLine("- Exclude: RelevanceScore < 0.4 OR AT LEAST ONE exclusion violation.");
            sb.AppendLine("- Uncertain: otherwise.");

            sb.AppendLine("### REASONING FORMAT (Markdown)");
            sb.AppendLine("The Reasoning section MUST include ALL of the following sections in EXACT order:");

            return sb.ToString();
        }

        private void ValidateAiOutput(StuSeAIOutput? output, StuSeAIInput input)
        {
            if (output == null)
            {
                throw new InvalidOperationException("AI Evaluation failed: Gemini returned null output.");
            }

            if (string.IsNullOrWhiteSpace(output.Reasoning))
            {
                throw new InvalidOperationException("AI Evaluation failed: Structured output reasoning is empty.");
            }

            if (output.ResearchQuestionResults == null || output.ResearchQuestionResults.Count != input.ResearchQuestions.Count)
            {
                throw new InvalidOperationException($"AI Evaluation failed: Research Question result count ({output.ResearchQuestionResults?.Count ?? 0}) does not match input count ({input.ResearchQuestions.Count}).");
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
