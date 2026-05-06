using Microsoft.EntityFrameworkCore;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.AiSetupService;
using SRSS.IAM.Services.DTOs.AiSetup;
using SRSS.IAM.Services.GeminiService;
using SRSS.IAM.Services.OpenRouter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SRSS.IAM.Services.AiSetupService
{
    public class AiSetupService : IAiSetupService
    {
        private readonly IGeminiService _geminiService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOpenRouterService _openRouterService;

        private const string GlobalAiInstructions = @"
### OUTPUT STRUCTURE ENFORCEMENT
1. STRICT JSON OUTPUT: The output MUST be a single valid JSON object. 
2. NO MARKDOWN WRAPPER: Do NOT wrap the JSON inside markdown (no ```json). 
3. NO EXPLANATIONS: Do NOT include explanations outside JSON. Do NOT include text before or after the JSON.
4. FIELD COMPLETENESS: The output JSON MUST contain ALL required fields defined in the schema.
";

        public AiSetupService(IGeminiService geminiService, IUnitOfWork unitOfWork, IOpenRouterService openRouterService)
        {
            _geminiService = geminiService;
            _unitOfWork = unitOfWork;
            _openRouterService = openRouterService;
        }

        public async Task<AnalyzeTopicResponse> AnalyzeTopicAsync(AnalyzeTopicRequest request)
        {
            var prompt = $@"
            Act as a senior Research Consultant specializing in Systematic Literature Reviews (SLR) and PRISMA guidelines.
            Your task is to analyze a raw research idea/topic and extract structured metadata.

            ### INPUT
            Research Topic: ""{request.Topic}""

            ### TASKS
            1. Identify the primary Scientific Domain (e.g., Software Engineering, Health Informatics).
            2. Synthesize a concise Research Objective statement (max 2 sentences).

            ### LANGUAGE REQUIREMENT
            - The output MUST be written entirely in: {request.Language}
            - Do NOT mix languages.
            ### OUTPUT SCHEMA (MANDATORY)
                {{
                ""domain"": ""..."",
                ""objectives"": ""..."",
                }}


            {GlobalAiInstructions}
            ";

            try
            {
                return await _openRouterService.GenerateStructuredContentAsync<AnalyzeTopicResponse>(prompt);
            }
            catch (Exception ex)
            {
                // Log the actual error for debugging (hypothetical logger usage)
                Console.WriteLine($"AI Analysis failed: {ex.Message}");

                // Graceful Degradation: Return fallback data
                return new AnalyzeTopicResponse
                {
                    Objectives = "Failed to analyze topic. Please provide manual input.",
                    Domain = "Unknown"
                };
            }
        }

        public async Task<GeneratePicocResponse> GeneratePicocAsync(GeneratePicocRequest request)
        {
            var prompt = $@"
                Act as a senior academic researcher in systematic literature reviews.

                Your task is to generate precise PICO-C elements.

                ### CONTEXT
                - Topic: {request.Topic}
                - Objectives: {request.Objectives}
                - Domain: {request.Domain}

                ### DEFINITIONS
                - Population: Specific target users or subjects (clearly defined group)
                - Intervention: Clearly defined method, framework, or strategy (NOT a long list)
                - Comparator: A realistic baseline or alternative approach
                - Outcome: 2–4 measurable research outcomes ONLY
                - Context: Specific application environment

                ### STRICT REQUIREMENTS
                1. Each field MUST be concise (1–2 sentences max).
                2. Avoid vague terms like ""improvement"" without specifying what improves.
                3. Intervention MUST NOT list more than 3 techniques.
                4. Outcome MUST include ONLY the most critical evaluation metrics (max 4).
                5. Keep all elements aligned strictly with the given Domain.
                6. Do NOT generalize beyond the Topic.

                ### LANGUAGE REQUIREMENT
                - The output MUST be written entirely in: {request.Language}
                - Do NOT mix languages.
                - All fields must strictly follow this language.

                ### OUTPUT SCHEMA (MANDATORY)
                {{
                ""Population"": ""..."",
                ""Intervention"": ""..."",
                ""Comparator"": ""..."",
                ""Outcome"": ""..."",
                ""Context"": ""...""
                }}

                {GlobalAiInstructions}
                ";

            try
            {
                return await _openRouterService.GenerateStructuredContentAsync<GeneratePicocResponse>(prompt);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AI Analysis failed: {ex.Message}");
                // Graceful Degradation: Return empty/standard payload
                return new GeneratePicocResponse
                {
                    Population = string.Empty,
                    Intervention = string.Empty,
                    Comparator = string.Empty,
                    Outcome = string.Empty,
                    Context = string.Empty
                };
            }
        }

        public async Task<GenerateRqsResponse> GenerateRqsAsync(GenerateRqsRequest request)
        {
            var prompt = $@"
            Act as a senior Academic Peer Reviewer. Based on the fully defined scope below, suggest 3 to 5 high-quality, researchable, and non-trivial Research Questions (RQs) for a Systematic Literature Review.

            ### SCOPE
            - Topic: {request.Topic}
            - Objectives: {request.Objectives}
            - Domain: {request.Domain}
            - PICO-C:
            * Population: {request.Picoc.Population}
            * Intervention: {request.Picoc.Intervention}
            * Comparator: {request.Picoc.Comparator}
            * Outcome: {request.Picoc.Outcome}
            * Context: {request.Picoc.Context}

            ### RQ DESIGN RULES
            1. The questions must be answerable through literature synthesis.
            2. Mix broad overview questions (e.g., ""What are the state-of-the-art...?"") with specific analytical questions (e.g., ""What are the primary challenges in...?"").
            3. Ensure RQs map directly to the defined PICO-C elements.

            ### LANGUAGE REQUIREMENT
            - The output MUST be written entirely in: {request.Language}
            - Do NOT mix languages.
            
            ### ANNOTATION REQUIREMENT
            1. Each Research Question MUST explicitly include PICO-C annotations.
            2. Use inline tags:
            - (P: ...) for Population
            - (I: ...) for Intervention
            - (C: ...) for Comparator
            - (O: ...) for Outcome
            - (Ctx: ...) for Context
            3. The tags MUST appear directly after the relevant phrase in the question.
            4. Each question MUST contain at least (P) and (I). Other elements are optional but encouraged.

            ### EXAMPLE
            ""What are the effects of (I: deep learning techniques) on (O: diagnostic accuracy) for (P: lung cancer patients) compared to (C: traditional methods) in (Ctx: clinical settings)?""
            
            ### OUTPUT FORMAT REQUIREMENT
            1. The output MUST be a valid JSON object.
            2. The JSON MUST follow this exact structure:
            {{
            ""SuggestedQuestions"": [
                ""Research Question 1?"",
                ""Research Question 2?"",
                ""Research Question 3?""    
            ]
            }}

            {GlobalAiInstructions}
            ";

            try
            {
                return await _openRouterService.GenerateStructuredContentAsync<GenerateRqsResponse>(prompt);
            }
            catch (Exception)
            {
                // Graceful Degradation
                return new GenerateRqsResponse { SuggestedQuestions = new List<string>() };
            }
        }

        public async Task<ProjectSetupDetailsResponse> GetSetupDetailsAsync(Guid projectId)
        {
            var project = await _unitOfWork.SystematicReviewProjects.GetQueryable()
                .Where(x => x.Id == projectId)
                .Select(p => new ProjectSetupDetailsResponse
                {
                    ResearchTopic = p.ResearchTopic ?? string.Empty,
                    ResearchObjective = p.ResearchObjective ?? string.Empty,
                    Domain = p.Domain ?? string.Empty,
                    Picoc = p.ProjectPicocs.OrderByDescending(pic => pic.CreatedAt).Select(pic => new GeneratePicocResponse
                    {
                        Population = pic.Population ?? string.Empty,
                        Intervention = pic.Intervention ?? string.Empty,
                        Comparator = pic.Comparator ?? string.Empty,
                        Outcome = pic.Outcome ?? string.Empty,
                        Context = pic.Context ?? string.Empty
                    }).FirstOrDefault() ?? new GeneratePicocResponse(),
                    ResearchQuestions = p.ResearchQuestions.Select(rq => new ResearchQuestionDetailDto
                    {
                        Id = rq.Id,
                        QuestionText = rq.QuestionText
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (project == null)
            {
                throw new KeyNotFoundException($"Project with ID {projectId} not found.");
            }

            return project;
        }

        public async Task UpdateSetupAsync(Guid projectId, UpdateSetupRequest request)
        {
            var project = await _unitOfWork.SystematicReviewProjects.FindFirstOrDefaultAsync(x => x.Id == projectId);
            if (project == null)
            {
                throw new InvalidOperationException($"Project with ID {projectId} not found.");
            }

            // 1. Update Project Details
            project.ResearchTopic = request.ResearchTopic;
            project.ResearchObjective = request.ResearchObjective;
            project.Domain = request.Domain;
            project.ModifiedAt = DateTimeOffset.UtcNow;

            // 2. ProjectPicoc (Upsert Strategy)
            var existingPicoc = await _unitOfWork.ProjectPicocs.FindFirstOrDefaultAsync(pic => pic.ProjectId == projectId);
            if (existingPicoc != null)
            {
                existingPicoc.Population = request.Picoc.Population;
                existingPicoc.Intervention = request.Picoc.Intervention;
                existingPicoc.Comparator = request.Picoc.Comparator;
                existingPicoc.Outcome = request.Picoc.Outcome;
                existingPicoc.Context = request.Picoc.Context;
                existingPicoc.ModifiedAt = DateTimeOffset.UtcNow;
            }
            else
            {
                var newPicoc = new ProjectPicoc
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    Population = request.Picoc.Population,
                    Intervention = request.Picoc.Intervention,
                    Comparator = request.Picoc.Comparator,
                    Outcome = request.Picoc.Outcome,
                    Context = request.Picoc.Context,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                };
                await _unitOfWork.ProjectPicocs.AddAsync(newPicoc);
            }

            // 3. ResearchQuestions (Merge Strategy)
            var existingRqs = await _unitOfWork.ResearchQuestions.FindAllAsync(rq => rq.ProjectId == projectId);
            var existingRqDict = existingRqs.ToDictionary(rq => rq.Id);
            var incomingRqIds = request.FinalResearchQuestions
                .Where(q => q.Id.HasValue)
                .Select(q => q.Id!.Value)
                .ToHashSet();

            // Deletions: Remove RQs not in incoming list
            foreach (var dbRq in existingRqs)
            {
                if (!incomingRqIds.Contains(dbRq.Id))
                {
                    // Note: We might need check for dependent entities in future
                    await _unitOfWork.ResearchQuestions.RemoveAsync(dbRq);
                }
            }

            // Additions & Updates
            foreach (var incomingRq in request.FinalResearchQuestions)
            {
                if (incomingRq.Id.HasValue && existingRqDict.TryGetValue(incomingRq.Id.Value, out var dbRq))
                {
                    // Update
                    dbRq.QuestionText = incomingRq.QuestionText;
                    dbRq.ModifiedAt = DateTimeOffset.UtcNow;
                }
                else
                {
                    // Add
                    var newRq = new ResearchQuestion
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = projectId,
                        QuestionText = incomingRq.QuestionText,
                        QuestionTypeId = null,
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = DateTimeOffset.UtcNow
                    };
                    await _unitOfWork.ResearchQuestions.AddAsync(newRq);
                }
            }

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
