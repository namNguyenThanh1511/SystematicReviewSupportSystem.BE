using SRSS.IAM.Repositories;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.QualityAssessment;
using SRSS.IAM.Services.Mappers;
using SRSS.IAM.Repositories.Entities.Enums;
using ClosedXML.Excel;
using System.IO;
using Microsoft.EntityFrameworkCore;
using SRSS.IAM.Services.NotificationService;
using SRSS.IAM.Services.DTOs.Notification;
using SRSS.IAM.Services.UserService;
using SRSS.IAM.Services.GeminiService;
using SRSS.IAM.Services.GrobidClient;
using SRSS.IAM.Services.RagService; // Added
using System.Net.Http;
using SRSS.IAM.Services.AuditLogService;

namespace SRSS.IAM.Services.QualityAssessmentService
{
    public class QualityAssessmentService : IQualityAssessmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IGeminiService _geminiService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IGrobidService _grobidService;
        private readonly IRagRetrievalService _ragRetrievalService; // Added
        private readonly IAuditLogService _auditLogService;

        public QualityAssessmentService(
            IUnitOfWork unitOfWork,
            INotificationService notificationService,
            ICurrentUserService currentUserService,
            IGeminiService geminiService,
            IHttpClientFactory httpClientFactory,
            IGrobidService grobidService,
            IRagRetrievalService ragRetrievalService, // Added
            IAuditLogService auditLogService)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _currentUserService = currentUserService;
            _geminiService = geminiService;
            _httpClientFactory = httpClientFactory;
            _grobidService = grobidService;
            _ragRetrievalService = ragRetrievalService; // Added
            _auditLogService = auditLogService;
        }

        private async Task<(Guid userId, ProjectRole role)> ValidateUserProjectRoleAsync(Guid projectId, ProjectRole minRoleRequired = ProjectRole.Member)
        {
            var (currentUserIdStr, _) = _currentUserService.GetCurrentUser();
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
            {
                throw new UnauthorizedAccessException("Current user ID is invalid.");
            }

            var currentUserProjectMember = await _unitOfWork.SystematicReviewProjects.GetQueryable()
                .Where(p => p.Id == projectId)
                .SelectMany(p => p.ProjectMembers)
                .FirstOrDefaultAsync(pm => pm.UserId == currentUserId);

            if (currentUserProjectMember == null)
            {
                throw new UnauthorizedAccessException($"User is not authorized. Must be a member of project {projectId}.");
            }

            if (minRoleRequired == ProjectRole.Leader && currentUserProjectMember.Role != ProjectRole.Leader)
            {
                throw new UnauthorizedAccessException($"User is not authorized. Must be a Leader for project {projectId}.");
            }

            return (currentUserId, currentUserProjectMember.Role);
        }

        // Helper to find ProjectId from ProtocolId
        private async Task<Guid> GetProjectIdFromProtocolIdAsync(Guid protocolId)
        {
            var rp = await _unitOfWork.ReviewProcesses.FindSingleAsync(r => r.ProtocolId == protocolId);
            if (rp == null) throw new KeyNotFoundException("Review process not found for this protocol.");
            return rp.ProjectId;
        }

        // Helper to find ProjectId from ReviewProcessId
        private async Task<Guid> GetProjectIdFromReviewProcessIdAsync(Guid reviewProcessId)
        {
            var rp = await _unitOfWork.ReviewProcesses.FindSingleAsync(r => r.Id == reviewProcessId);
            if (rp == null) throw new KeyNotFoundException("Review process not found.");
            return rp.ProjectId;
        }

        // Helper to find ProjectId from StrategyId
        private async Task<Guid> GetProjectIdFromStrategyIdAsync(Guid strategyId)
        {
            var strategy = await _unitOfWork.QualityStrategies.FindSingleAsync(s => s.Id == strategyId);
            if (strategy == null) throw new KeyNotFoundException("Strategy not found.");
            return await GetProjectIdFromProtocolIdAsync(strategy.ProtocolId);
        }

        // Helper to find ProjectId from ProcessId
        private async Task<Guid> GetProjectIdFromProcessIdAsync(Guid processId)
        {
            var process = await _unitOfWork.QualityAssessmentProcesses.FindSingleAsync(p => p.Id == processId);
            if (process == null) throw new KeyNotFoundException("Process not found.");
            return await GetProjectIdFromReviewProcessIdAsync(process.ReviewProcessId);
        }

        // Helper to find ProjectId from ChecklistId
        private async Task<Guid> GetProjectIdFromChecklistIdAsync(Guid checklistId)
        {
            var checklist = await _unitOfWork.QualityChecklists.FindSingleAsync(c => c.Id == checklistId);
            if (checklist == null) throw new KeyNotFoundException("Checklist not found.");
            return await GetProjectIdFromStrategyIdAsync(checklist.QaStrategyId);
        }

        // ==================== Quality Assessment Strategies ====================
        public async Task<QualityAssessmentStrategyDto> UpsertStrategyAsync(QualityAssessmentStrategyDto dto)
        {
            var projectId = await GetProjectIdFromProtocolIdAsync(dto.ProtocolId);
            await ValidateUserProjectRoleAsync(projectId, ProjectRole.Leader);

            QualityAssessmentStrategy entity;

            if (dto.QaStrategyId.HasValue && dto.QaStrategyId.Value != Guid.Empty)
            {
                entity = await _unitOfWork.QualityStrategies.FindSingleAsync(s => s.Id == dto.QaStrategyId.Value)
                    ?? throw new KeyNotFoundException($"Strategy {dto.QaStrategyId.Value} không tồn tại");

                dto.UpdateEntity(entity);
                await _unitOfWork.QualityStrategies.UpdateAsync(entity);
            }
            else
            {
                entity = dto.ToEntity();
                await _unitOfWork.QualityStrategies.AddAsync(entity);
            }

            await _unitOfWork.SaveChangesAsync();
            return entity.ToDto();
        }

        public async Task<List<QualityAssessmentStrategyDto>> GetStrategiesByProtocolIdAsync(Guid protocolId)
        {
            var projectId = await GetProjectIdFromProtocolIdAsync(protocolId);
            await ValidateUserProjectRoleAsync(projectId, ProjectRole.Member);

            var entities = await _unitOfWork.QualityStrategies.GetByProtocolIdAsync(protocolId);
            return entities.ToDtoList();
        }

        /// <summary>
        /// Given a QualityAssessmentProcess id, return the full QA strategies for the underlying protocol
        /// including checklists and criteria.
        /// </summary>
        public async Task<List<QualityAssessmentStrategyDto>> GetStrategiesByProcessIdAsync(Guid processId)
        {
            var projectId = await GetProjectIdFromProcessIdAsync(processId);
            await ValidateUserProjectRoleAsync(projectId, ProjectRole.Member);

            var process = await _unitOfWork.QualityAssessmentProcesses.FindSingleAsync(p => p.Id == processId);
            if (process == null) return new List<QualityAssessmentStrategyDto>();

            var reviewProcess = await _unitOfWork.ReviewProcesses.FindSingleAsync(rp => rp.Id == process.ReviewProcessId);
            if (reviewProcess == null || reviewProcess.ProtocolId == null) return new List<QualityAssessmentStrategyDto>();

            var strategy = await _unitOfWork.QualityStrategies.GetFullStrategyByProtocolIdAsync(reviewProcess.ProtocolId.Value);

            return strategy.ToDtoList();
        }

        public async Task DeleteStrategyAsync(Guid strategyId)
        {
            var projectId = await GetProjectIdFromStrategyIdAsync(strategyId);
            await ValidateUserProjectRoleAsync(projectId, ProjectRole.Leader);

            var entity = await _unitOfWork.QualityStrategies.FindSingleAsync(s => s.Id == strategyId);
            if (entity != null)
            {
                await _unitOfWork.QualityStrategies.RemoveAsync(entity);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        // ==================== Quality Checklists ====================
        public async Task<List<QualityAssessmentChecklistDto>> BulkUpsertChecklistsAsync(List<QualityAssessmentChecklistDto> dtos)
        {
            if (dtos.Any())
            {
                var projectId = await GetProjectIdFromStrategyIdAsync(dtos.First().QaStrategyId);
                await ValidateUserProjectRoleAsync(projectId, ProjectRole.Leader);
            }

            var results = new List<QualityChecklist>();

            foreach (var dto in dtos)
            {
                QualityChecklist? entity;

                if (dto.ChecklistId.HasValue && dto.ChecklistId.Value != Guid.Empty)
                {
                    entity = await _unitOfWork.QualityChecklists.FindSingleAsync(c => c.Id == dto.ChecklistId.Value);

                    if (entity != null)
                    {
                        dto.UpdateEntity(entity);
                        await _unitOfWork.QualityChecklists.UpdateAsync(entity);
                    }
                    else
                    {
                        entity = dto.ToEntity();
                        await _unitOfWork.QualityChecklists.AddAsync(entity);
                    }
                }
                else
                {
                    entity = dto.ToEntity();
                    await _unitOfWork.QualityChecklists.AddAsync(entity);
                }

                results.Add(entity);
            }

            await _unitOfWork.SaveChangesAsync();
            return results.ToDtoList();
        }

        public async Task<List<QualityAssessmentChecklistDto>> GetChecklistsByStrategyIdAsync(Guid strategyId)
        {
            var projectId = await GetProjectIdFromStrategyIdAsync(strategyId);
            await ValidateUserProjectRoleAsync(projectId, ProjectRole.Member);

            var entities = await _unitOfWork.QualityChecklists.GetByStrategyIdAsync(strategyId);
            return entities.ToDtoList();
        }

        // ==================== Quality Criteria ====================
        public async Task<List<QualityAssessmentCriterionDto>> BulkUpsertCriteriaAsync(List<QualityAssessmentCriterionDto> dtos)
        {
            if (dtos.Any())
            {
                var projectId = await GetProjectIdFromChecklistIdAsync(dtos.First().ChecklistId);
                await ValidateUserProjectRoleAsync(projectId, ProjectRole.Leader);
            }

            var results = new List<QualityCriterion>();

            foreach (var dto in dtos)
            {
                QualityCriterion? entity;

                if (dto.CriterionId.HasValue && dto.CriterionId.Value != Guid.Empty)
                {
                    entity = await _unitOfWork.QualityCriteria.FindSingleAsync(c => c.Id == dto.CriterionId.Value);

                    if (entity != null)
                    {
                        dto.UpdateEntity(entity);
                        await _unitOfWork.QualityCriteria.UpdateAsync(entity);
                    }
                    else
                    {
                        entity = dto.ToEntity();
                        await _unitOfWork.QualityCriteria.AddAsync(entity);
                    }
                }
                else
                {
                    entity = dto.ToEntity();
                    await _unitOfWork.QualityCriteria.AddAsync(entity);
                }

                results.Add(entity);
            }

            await _unitOfWork.SaveChangesAsync();
            return results.ToDtoList();
        }

        public async Task<List<QualityAssessmentCriterionDto>> GetCriteriaByChecklistIdAsync(Guid checklistId)
        {
            var projectId = await GetProjectIdFromChecklistIdAsync(checklistId);
            await ValidateUserProjectRoleAsync(projectId, ProjectRole.Member);

            var entities = await _unitOfWork.QualityCriteria.GetByChecklistIdAsync(checklistId);
            return entities.ToDtoList();
        }

        // ==================== Quality Assessment Process ====================
        public async Task<QualityAssessmentProcessResponse> GetProcessByReviewProcessIdAsync(Guid reviewProcessId)
        {
            var projectId = await GetProjectIdFromReviewProcessIdAsync(reviewProcessId);
            await ValidateUserProjectRoleAsync(projectId, ProjectRole.Member);

            var process = await _unitOfWork.QualityAssessmentProcesses.FindSingleAsync(
                p => p.ReviewProcessId == reviewProcessId);

            if (process == null) return null!;

            return process.ToResponse();
        }

        public async Task<QualityAssessmentProcessResponse> CreateProcessAsync(CreateQualityAssessmentProcessDto dto)
        {
            var projectId = await GetProjectIdFromReviewProcessIdAsync(dto.ReviewProcessId);
           await ValidateUserProjectRoleAsync(projectId, ProjectRole.Leader);

            // Check existence
            var existing = await _unitOfWork.QualityAssessmentProcesses.FindSingleAsync(p => p.ReviewProcessId == dto.ReviewProcessId);
            if (existing != null)
                throw new InvalidOperationException($"Quality Assessment Process for Review Process {dto.ReviewProcessId} already exists.");

            var entity = dto.ToEntity();

            await _unitOfWork.QualityAssessmentProcesses.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return entity.ToResponse();
        }

        public async Task<QualityAssessmentProcessResponse> StartProcessAsync(Guid id)
        {
            var projectId = await GetProjectIdFromProcessIdAsync(id);
            await ValidateUserProjectRoleAsync(projectId, ProjectRole.Leader);

            var entity = await _unitOfWork.QualityAssessmentProcesses.FindSingleAsync(p => p.Id == id)
                ?? throw new KeyNotFoundException($"Không tìm thấy QA Process {id}");

            var studySelectionProcess = await _unitOfWork.StudySelectionProcesses
                .FindSingleAsync(ssp => ssp.ReviewProcessId == entity.ReviewProcessId);

            if (studySelectionProcess != null && studySelectionProcess.Status != SelectionProcessStatus.Completed)
            {
                throw new InvalidOperationException("Study Selection Process must be completed before starting the Quality Assessment Process.");
            }

            entity.Start();

            // Populate QualityAssessmentPaper from StudySelectionProcessPaper
            if (studySelectionProcess != null)
            {
                // Fetch the papers that passed full text screening (and thus are in StudySelectionProcessPaper with Include decision)
                // Note: The StudySelectionProcessPaper holds the "final snapshot" of included papers after Complete phase.
                var passedPapers = await _unitOfWork.StudySelectionProcessPapers.GetWithPaperByProcessAsync(studySelectionProcess.Id, default);

                // Get currently existing QualityAssessmentPaper to avoid duplication if Start is called multiple times.
                var existingQAPapers = await _unitOfWork.QualityAssessmentPapers.FindAllAsync(x => x.QualityAssessmentProcessId == id);
                var existingPaperIds = existingQAPapers.Select(x => x.PaperId).ToHashSet();

                var newQAPapers = passedPapers
                    .Where(p => !existingPaperIds.Contains(p.PaperId))
                    .Select(p => new QualityAssessmentPaper
                    {
                        QualityAssessmentProcessId = id,
                        PaperId = p.PaperId,
                        CreatedAt = DateTimeOffset.UtcNow
                    }).ToList();

                if (newQAPapers.Any())
                {
                    await _unitOfWork.QualityAssessmentPapers.AddRangeAsync(newQAPapers);
                }
            }

            await _unitOfWork.QualityAssessmentProcesses.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return entity.ToResponse();
        }

        public async Task<QualityAssessmentProcessResponse> CompleteProcessAsync(Guid id)
        {
            var projectId = await GetProjectIdFromProcessIdAsync(id);
            await ValidateUserProjectRoleAsync(projectId, ProjectRole.Leader);

            var process = await _unitOfWork.QualityAssessmentProcesses.FindSingleAsync(p => p.Id == id)
                ?? throw new KeyNotFoundException("Quality Assessment Process not found");

            process.Status = QualityAssessmentProcessStatus.Completed;
            process.CompletedAt = DateTimeOffset.UtcNow;
            await _unitOfWork.QualityAssessmentProcesses.UpdateAsync(process);
            await _unitOfWork.SaveChangesAsync();

            return process.ToResponse();
        }

        public async Task<QALeaderDashboardResponse> GetLeaderDashboardAsync(Guid id)
        {
            var projectId = await GetProjectIdFromProcessIdAsync(id);
            await ValidateUserProjectRoleAsync(projectId, ProjectRole.Leader);

            var result = new QALeaderDashboardResponse();

            var process = await _unitOfWork.QualityAssessmentProcesses.FindSingleAsync(p => p.Id == id);
            if (process == null) return result;

            var eligiblePaper = await _unitOfWork.QualityAssessmentPapers.GetByProcessIdWithDetailsAsync(process.Id);

            // Also include any papers that are already assigned or have resolutions just in case
            var assignments = await _unitOfWork.QualityAssessmentAssignments.GetAllWithPapersByProcessIdAsync(process.Id);
            var processResolutions = await _unitOfWork.QualityAssessmentResolutions.FindAllAsync(r => r.QualityAssessmentProcessId == process.Id);

            // Calculate criteria count to compute percentage
            var protocolId = (await _unitOfWork.ReviewProcesses.FindSingleAsync(rp => rp.Id == process.ReviewProcessId))?.ProtocolId;
            var criteriaCount = 0;
            if (protocolId != null)
            {
                var strategies = await _unitOfWork.QualityStrategies.GetFullStrategyByProtocolIdAsync(protocolId.Value);
                foreach (var strat in strategies)
                {
                    foreach (var cl in strat.Checklists)
                    {
                        criteriaCount += cl.Criteria.Count;
                    }
                }
            }

            // Also prepare Member Progress tracking
            var memberProgressDict = assignments
                .Where(a => a.User != null)
                .GroupBy(a => a.User.Id)
                .ToDictionary(g => g.Key, g => new QAReviewerProgressResponse
                {
                    ReviewerId = g.Key,
                    ReviewerName = g.First().User.FullName ?? g.First().User.Username,
                    TotalExpectedDecisions = g.SelectMany(a => a.QualityAssessmentPapers).Distinct().Count() * criteriaCount,
                    ActualDecisions = 0,
                    CompletedPapers = 0,
                    InProgressPapers = 0,
                    NotStartedPapers = 0
                });

            foreach (var paper in eligiblePaper)
            {
                var paperDecisions = await _unitOfWork.QualityAssessmentDecisions.GetByQaPaperIdWithDetailsAsync(paper.Id);

                var reviewersAssignedToPaper = assignments
                    .Where(a => a.QualityAssessmentPapers.Any(p => p.Id == paper.Id))
                    .Select(a => a.User)
                    .Where(u => u != null)
                    .GroupBy(u => u.Id)
                    .Select(g => g.First())
                    .ToList();

                // Build Member Progress per paper
                foreach (var reviewer in reviewersAssignedToPaper)
                {
                    if (memberProgressDict.TryGetValue(reviewer.Id, out var tracker))
                    {
                        var reviewerDecision = paperDecisions.FirstOrDefault(d => d.ReviewerId == reviewer.Id);
                        var itemsCount = reviewerDecision?.DecisionItems?.Count ?? 0;
                        tracker.ActualDecisions += itemsCount;

                        double reviewerCompletionPercentage = criteriaCount > 0 ? (double)itemsCount / criteriaCount * 100 : 0;

                        if (reviewerCompletionPercentage >= 100)
                        {
                            tracker.CompletedPapers++;
                        }
                        else if (reviewerCompletionPercentage > 0)
                        {
                            tracker.InProgressPapers++;
                        }
                        else
                        {
                            tracker.NotStartedPapers++;
                        }
                    }
                }

                // All criteria == all assigned reviewers * criteria count 
                var expectedDecisions = reviewersAssignedToPaper.Count * criteriaCount;
                var actualDecisions = 0;
                foreach (var decision in paperDecisions)
                {
                    actualDecisions += decision.DecisionItems.Count;
                }

                var percentage = expectedDecisions > 0 ? (double)actualDecisions / expectedDecisions * 100 : 0;
                if (percentage > 100) percentage = 100;

                var resolution = processResolutions.FirstOrDefault(r => r.QualityAssessmentPaperId == paper.Id);

                string? resolvedByName = null;
                if (resolution != null)
                {
                    var resolutionReviewer = await _unitOfWork.Users.FindSingleAsync(u => u.Id == resolution.ResolvedBy);
                    resolvedByName = resolutionReviewer?.FullName ?? resolutionReviewer?.Username;
                }

                var summary = paper.ToLeaderDashboardPaperResponse(
                    percentage,
                    resolution,
                    reviewersAssignedToPaper,
                    paperDecisions,
                    resolvedByName
                );

                if (summary.Status == "resolved") result.CompletedPapers++;
                else if (summary.Status == "in-progress") result.InProgressPapers++;
                else result.NotStartedPapers++;

                result.Papers.Add(summary);
            }

            result.TotalPapers = result.Papers.Count;

            foreach (var kvp in memberProgressDict)
            {
                var info = kvp.Value;
                var completion = info.TotalExpectedDecisions > 0
                    ? (double)info.ActualDecisions / info.TotalExpectedDecisions * 100
                    : 0;

                if (completion > 100) completion = 100;

                info.CompletionPercentage = completion;
                result.ReviewerProgresses.Add(info);
            }

            return result;
        }

        public async Task<QualityAssessmentStatisticsResponse> GetQualityStatisticsAsync(Guid processId)
        {
            var projectId = await GetProjectIdFromProcessIdAsync(processId);
            await ValidateUserProjectRoleAsync(projectId, ProjectRole.Leader);

            var process = await _unitOfWork.QualityAssessmentProcesses.FindSingleAsync(p => p.Id == processId);
            if (process == null) return new QualityAssessmentStatisticsResponse();

            var eligiblePaper = await _unitOfWork.QualityAssessmentPapers.GetByProcessIdWithDetailsAsync(process.Id);
            var processResolutions = await _unitOfWork.QualityAssessmentResolutions.FindAllAsync(r => r.QualityAssessmentProcessId == process.Id);

            var totalPapers = eligiblePaper.Count;
            var highQuality = processResolutions.Count(r => r.FinalDecision == Repositories.Entities.Enums.QualityAssessmentResolutionDecision.HighQuality);
            var lowQuality = processResolutions.Count(r => r.FinalDecision == Repositories.Entities.Enums.QualityAssessmentResolutionDecision.LowQuality);

            var assignments = await _unitOfWork.QualityAssessmentAssignments.GetAllWithPapersByProcessIdAsync(process.Id);

            int notStartedCount = 0;
            int inProgressCount = 0;

            foreach (var paper in eligiblePaper)
            {
                var resolution = processResolutions.FirstOrDefault(r => r.QualityAssessmentPaperId == paper.Id);
                if (resolution != null)
                {
                    continue; // Handled by High/Low
                }

                // It's not resolved, check progress
                var paperDecisions = await _unitOfWork.QualityAssessmentDecisions.GetByQaPaperIdWithDetailsAsync(paper.Id);
                var reviewersAssignedToPaper = assignments
                    .Where(a => a.QualityAssessmentPapers.Any(p => p.Id == paper.Id))
                    .Select(a => a.User)
                    .Where(u => u != null)
                    .GroupBy(u => u.Id)
                    .Select(g => g.First())
                    .ToList();

                var expectedDecisionsCount = reviewersAssignedToPaper.Count; // This is just checking if we expect anything
                var actualDecisionsCount = paperDecisions.Sum(d => d.DecisionItems.Count);

                if (expectedDecisionsCount > 0 && actualDecisionsCount > 0)
                {
                    inProgressCount++;
                }
                else
                {
                    notStartedCount++;
                }
            }

            return new QualityAssessmentStatisticsResponse
            {
                TotalPapers = totalPapers,
                HighQualityPapers = highQuality,
                LowQualityPapers = lowQuality,
                InProgressPapers = inProgressCount,
                NotStartedPapers = notStartedCount
            };
        }

        // ==================== Assignments ====================
        public async Task AssignPapersToReviewersAsync(CreateQualityAssessmentAssignmentRequest dto)
        {
            var projectId = await GetProjectIdFromProcessIdAsync(dto.QualityAssessmentProcessId);
            await ValidateUserProjectRoleAsync(projectId, ProjectRole.Leader);

            var process = await _unitOfWork.QualityAssessmentProcesses.FindSingleAsync(p => p.Id == dto.QualityAssessmentProcessId)
                ?? throw new KeyNotFoundException("Quality Assessment Process not found");

            var processAssignments = await _unitOfWork.QualityAssessmentAssignments.GetAllWithPapersByProcessIdAsync(process.Id);

            // Validate reviewer limit per paper (max 2)
            foreach (var qualityAssessmentPaperId in dto.QualityAssessmentPaperIds)
            {
                var assignedUsers = processAssignments
                    .Where(a => a.QualityAssessmentPapers != null && a.QualityAssessmentPapers.Any(p => p.Id == qualityAssessmentPaperId))
                    .Select(a => a.UserId)
                    .ToList();

                var newUsers = dto.UserIds.Except(assignedUsers).ToList();
                if (assignedUsers.Count + newUsers.Count > 2)
                {
                    throw new InvalidOperationException($"Cannot assign paper with ID {qualityAssessmentPaperId} to more than 2 reviewers.");
                }
            }

            // Validate Users and Papers exist (optional but recommended)

            foreach (var userId in dto.UserIds)
            {
                // Find existing assignment or create new
                var assignment = await _unitOfWork.QualityAssessmentAssignments
                    .FindSingleAsync(a => a.QualityAssessmentProcessId == dto.QualityAssessmentProcessId && a.UserId == userId);

                if (assignment == null)
                {
                    // isNewAssignment = true;
                    assignment = dto.ToEntity(userId);
                    await _unitOfWork.QualityAssessmentAssignments.AddAsync(assignment);
                    await _unitOfWork.SaveChangesAsync(); // Save to get ID
                }

                // Check resolution for each paper before assigning
                var papersToAdd = new List<QualityAssessmentPaper>();
                foreach (var qualityAssessmentPaperId in dto.QualityAssessmentPaperIds)
                {
                    // If paper has resolution, skip adding
                    var qaPaper = await _unitOfWork.QualityAssessmentPapers
                        .FindSingleAsync(p => p.Id == qualityAssessmentPaperId && p.QualityAssessmentProcessId == dto.QualityAssessmentProcessId);

                    if (qaPaper == null) continue;

                    var hasResolution = await _unitOfWork.QualityAssessmentResolutions
                       .AnyAsync(r => r.QualityAssessmentPaperId == qaPaper.Id && r.QualityAssessmentProcessId == dto.QualityAssessmentProcessId);

                    if (hasResolution) continue;

                    papersToAdd.Add(qaPaper);
                }

                if (assignment.QualityAssessmentPapers == null) assignment.QualityAssessmentPapers = new List<QualityAssessmentPaper>();

                // Add papers to assignment (many-to-many) - This requires loading the collection or using a direct insert approach if valid
                // Since EF Core generic repo might not expose direct collection manipulation easily without tracking,
                // we might need to rely on the tracking. Ensure FindSingleAsync tracks or attached.
                // Best to load with Include if possible, but for now let's assume we can add.

                // Actually, for many-to-many updates in disconnected scenarios or without full loading, it can be tricky.
                // Let's assume we need to load the assignment with papers first to avoid duplicates.
                var assignmentWithPapers = await _unitOfWork.QualityAssessmentAssignments.GetWithPapersAsync(assignment.Id);

                if (assignmentWithPapers != null)
                {
                    foreach (var paper in papersToAdd)
                    {
                        if (!assignmentWithPapers.QualityAssessmentPapers.Any(p => p.Id == paper.Id))
                        {
                            assignmentWithPapers.QualityAssessmentPapers.Add(paper);
                        }
                    }
                    await _unitOfWork.QualityAssessmentAssignments.UpdateAsync(assignmentWithPapers);
                }

                // Send Notification
                if (papersToAdd.Any())
                {
                    await _notificationService.SendAsync(userId, "New QA Assignment", $"You have been assigned {papersToAdd.Count} papers for Quality Assessment.", NotificationType.System);
                }
            }
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<QAMemberDashboardResponse> GetMemberDashboardAsync(Guid id)
        {
            var projectId = await GetProjectIdFromProcessIdAsync(id);
            var (userId, _) = await ValidateUserProjectRoleAsync(projectId, ProjectRole.Member);

            var process = await _unitOfWork.QualityAssessmentProcesses.FindSingleAsync(p => p.Id == id);
            if (process == null) return new QAMemberDashboardResponse();

            var assignment = await _unitOfWork.QualityAssessmentAssignments.GetWithPapersByProcessAndUserAsync(process.Id, userId);

            if (assignment == null || assignment.QualityAssessmentPapers == null) return new QAMemberDashboardResponse();

            // Need to calculate completion percentage.
            // 1. Get Protocol -> QA Strategy -> Checklists -> Criteria count
            var protocolId = (await _unitOfWork.ReviewProcesses.FindSingleAsync(rp => rp.Id == process.ReviewProcessId))?.ProtocolId;
            if (protocolId == null) return new QAMemberDashboardResponse(); // Should not happen if configured correctly

            var strategies = await _unitOfWork.QualityStrategies.GetFullStrategyByProtocolIdAsync(protocolId.Value);
            var criteriaCount = 0;
            foreach (var strat in strategies)
            {
                foreach (var cl in strat.Checklists)
                {
                    criteriaCount += cl.Criteria.Count;
                }
            }

            var result = new QAMemberDashboardResponse();
            var totalExpectedDecisions = assignment.QualityAssessmentPapers.Count * criteriaCount;
            var actualDecisions = 0;

            foreach (var qaPaper in assignment.QualityAssessmentPapers)
            {
                // Check for resolution
                var resolution = await _unitOfWork.QualityAssessmentResolutions.FindSingleAsync(
                   r => r.QualityAssessmentPaperId == qaPaper.Id && r.QualityAssessmentProcessId == process.Id);

                string? resolvedByName = null;
                if (resolution != null)
                {
                    var resovledBy = await _unitOfWork.Users.FindSingleAsync(u => u.Id == resolution.ResolvedBy);
                    resolvedByName = resovledBy?.FullName ?? resovledBy?.Username;
                }

                // Calculate decisions count for this paper by this user
                var userDecisions = await _unitOfWork.QualityAssessmentDecisions.GetByQaPaperIdAndUserIdWithDetailsAsync(qaPaper.Id, userId);
                var userDecisionsCount = userDecisions?.DecisionItems?.Count ?? 0;
                actualDecisions += userDecisionsCount;

                double percentage = criteriaCount > 0 ? (double)userDecisionsCount / criteriaCount * 100 : 0;
                if (percentage > 100) percentage = 100; // Cap at 100 if updates happen

                var dto = qaPaper.ToMemberDashboardPaperResponse(percentage, resolution, userDecisions, resolvedByName);
                result.Papers.Add(dto);

                if (dto.Status == "completed" || dto.Status == "resolved") result.CompletedPapers++;
                else if (dto.Status == "in-progress") result.InProgressPapers++;
                else result.NotStartedPapers++;
            }

            result.TotalPapers = assignment.QualityAssessmentPapers.Count;
            result.CompletionPercentage = totalExpectedDecisions > 0 ? (double)actualDecisions / totalExpectedDecisions * 100 : 0;
            if (result.CompletionPercentage > 100) result.CompletionPercentage = 100;

            return result;
        }

        // ==================== Decisions ====================
        public async Task CreateDecisionAsync(CreateQualityAssessmentDecisionRequest dto)
        {
            var projectId = await GetProjectIdFromProcessIdAsync(dto.QualityAssessmentProcessId);
            var (userId, _) = await ValidateUserProjectRoleAsync(projectId, ProjectRole.Member);

            // Validate and find assignment
            var assignment = await _unitOfWork.QualityAssessmentAssignments.GetWithPapersByProcessAndUserAsync(dto.QualityAssessmentProcessId, userId);
            if (assignment == null) throw new KeyNotFoundException("Assignment not found for this user in this process");

            var qaPaper = assignment.QualityAssessmentPapers.FirstOrDefault(x => x.Id == dto.QualityAssessmentPaperId);
            if (qaPaper == null) throw new KeyNotFoundException("Assignment not found for this user and QA paper");

            // Check if resolution exists
            var resolution = await _unitOfWork.QualityAssessmentResolutions.FindSingleAsync(
                r => r.QualityAssessmentProcessId == assignment.QualityAssessmentProcessId && r.QualityAssessmentPaperId == qaPaper.Id);

            if (resolution != null)
            {
                throw new InvalidOperationException("Cannot add decision because a final resolution has already been made for this paper.");
            }

            // Check existing decision
            var existing = await _unitOfWork.QualityAssessmentDecisions.FindSingleAsync(
                d => d.ReviewerId == userId && d.QualityAssessmentPaperId == qaPaper.Id);

            if (existing != null)
                throw new InvalidOperationException("Decision for this paper already exists. Use update instead.");

            // Construct new decision
            var decision = dto.ToEntity(userId, qaPaper.Id);

            if (dto.DecisionItems != null && dto.DecisionItems.Any())
            {
                foreach (var item in dto.DecisionItems)
                {
                    var criterionExists = await _unitOfWork.QualityCriteria.FindSingleAsync(c => c.Id == item.QualityCriterionId) != null;
                    if (!criterionExists)
                    {
                        throw new KeyNotFoundException($"Quality Criterion not found: {item.QualityCriterionId}");
                    }

                    decision.DecisionItems.Add(item.ToEntity());
                }
            }

            await _unitOfWork.QualityAssessmentDecisions.AddAsync(decision);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateDecisionAsync(Guid decisionId, UpdateQualityAssessmentDecisionRequest dto)
        {
            var decision = await _unitOfWork.QualityAssessmentDecisions.GetByIdWithDetailsAsync(decisionId);
            if (decision == null) throw new KeyNotFoundException("Decision not found.");

            var qaPaperId = decision.QualityAssessmentPaperId;

            var qaPaper = await _unitOfWork.QualityAssessmentPapers.FindSingleAsync(p => p.Id == qaPaperId);
            if (qaPaper == null) throw new KeyNotFoundException("Paper not found");

            var projectId = await GetProjectIdFromProcessIdAsync(qaPaper.QualityAssessmentProcessId);
            var (userId, _) = await ValidateUserProjectRoleAsync(projectId, ProjectRole.Member);
                        
            if (decision.ReviewerId != userId)
                throw new UnauthorizedAccessException("You can only update your own decisions.");

            // Get assignment (to get process ID for resolution check)
            var assignment = await _unitOfWork.QualityAssessmentAssignments.GetByUserAndQaPaperAsync(userId, qaPaper.Id);
            if (assignment == null) throw new KeyNotFoundException("Assignment not found");

            var resolution = await _unitOfWork.QualityAssessmentResolutions.FindSingleAsync(
                r => r.QualityAssessmentProcessId == assignment.QualityAssessmentProcessId && r.QualityAssessmentPaperId == qaPaperId);

            if (resolution != null)
            {
                throw new InvalidOperationException("Cannot update decision because a final resolution has already been made for this paper.");
            }

            dto.UpdateEntity(decision);

            // Update items
            foreach (var itemDto in dto.DecisionItems)
            {
                QualityAssessmentDecisionItem? item = null;

                if (itemDto.Id.HasValue && itemDto.Id.Value != Guid.Empty)
                {
                    item = decision.DecisionItems.FirstOrDefault(di => di.Id == itemDto.Id.Value);
                }

                if (item == null && itemDto.QualityCriterionId.HasValue && itemDto.QualityCriterionId.Value != Guid.Empty)
                {
                    item = decision.DecisionItems.FirstOrDefault(di => di.QualityCriterionId == itemDto.QualityCriterionId.Value);
                }

                if (item != null)
                {
                    itemDto.UpdateEntity(item);
                }
                else if (itemDto.QualityCriterionId.HasValue && itemDto.QualityCriterionId.Value != Guid.Empty)
                {
                    var criterionExists = await _unitOfWork.QualityCriteria.FindSingleAsync(c => c.Id == itemDto.QualityCriterionId.Value) != null;
                    if (!criterionExists)
                    {
                        throw new InvalidOperationException($"Quality Criterion not found: {itemDto.QualityCriterionId.Value}");
                    }

                    // If item does not exist, add new
                    decision.DecisionItems.Add(itemDto.ToEntity());
                }
                else
                {
                    throw new InvalidOperationException("Could not identify the decision item to update or create.");
                }
            }

            await _unitOfWork.QualityAssessmentDecisions.UpdateAsync(decision);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<QualityAssessmentDecisionResponse>> GetDecisionsByQaPaperIdAsync(Guid qaPaperId)
        {
            var qaPaper = await _unitOfWork.QualityAssessmentPapers.FindSingleAsync(p => p.Id == qaPaperId);
            if (qaPaper != null)
            {
                var projectId = await GetProjectIdFromProcessIdAsync(qaPaper.QualityAssessmentProcessId);
                await ValidateUserProjectRoleAsync(projectId, ProjectRole.Member);
            }

            var decisions = await _unitOfWork.QualityAssessmentDecisions.GetByQaPaperIdWithDetailsAsync(qaPaperId);

            return decisions.Select(d => d.ToDto()).ToList();
        }

        // ==================== Resolutions ====================
        public async Task<QualityAssessmentResolutionResponse> CreateResolutionAsync(CreateQualityAssessmentResolutionRequest dto)
        {
            var projectId = await GetProjectIdFromProcessIdAsync(dto.QualityAssessmentProcessId);
            var (userId, _) = await ValidateUserProjectRoleAsync(projectId, ProjectRole.Leader);

            var qaPaper = await _unitOfWork.QualityAssessmentPapers.FindSingleAsync(p => p.Id == dto.QualityAssessmentPaperId && p.QualityAssessmentProcessId == dto.QualityAssessmentProcessId);
            if (qaPaper == null) throw new KeyNotFoundException("Paper not found");

            // Verify existing
            var existing = await _unitOfWork.QualityAssessmentResolutions.FindSingleAsync(
                r => r.QualityAssessmentProcessId == dto.QualityAssessmentProcessId && r.QualityAssessmentPaperId == dto.QualityAssessmentPaperId);

            if (existing != null) throw new InvalidOperationException("Resolution already exists for this paper");

            // set the resolved by using current user
            var entity = dto.ToEntity(userId);

            await _unitOfWork.QualityAssessmentResolutions.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return entity.ToResponse();
        }

        public async Task<QualityAssessmentResolutionResponse> UpdateResolutionAsync(Guid id, UpdateQualityAssessmentResolutionRequest dto)
        {
            var entity = await _unitOfWork.QualityAssessmentResolutions.FindSingleAsync(r => r.Id == id)
                ?? throw new KeyNotFoundException("Resolution not found");

            var projectId = await GetProjectIdFromProcessIdAsync(entity.QualityAssessmentProcessId);
            var (userId, _) = await ValidateUserProjectRoleAsync(projectId, ProjectRole.Leader);

            dto.UpdateEntity(entity);
            // Optionally update ResolvedBy or ResolvedAt here if desired:
            entity.ResolvedBy = userId;
            entity.ResolvedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.QualityAssessmentResolutions.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return entity.ToResponse();
        }

        public async Task<QualityAssessmentResolutionResponse> GetResolutionByQaPaperIdAsync(Guid qaPaperId)
        {
            var qaPaper = await _unitOfWork.QualityAssessmentPapers.FindSingleAsync(p => p.Id == qaPaperId);
            if (qaPaper == null) return null!;

            var projectId = await GetProjectIdFromProcessIdAsync(qaPaper.QualityAssessmentProcessId);
            await ValidateUserProjectRoleAsync(projectId, ProjectRole.Member);

            var res = await _unitOfWork.QualityAssessmentResolutions.FindSingleAsync(r => r.QualityAssessmentPaperId == qaPaper.Id);
            if (res == null) return null!;

            return res.ToResponse();
        }

        public async Task<List<QAPaperResponse>> GetHighQualityPaperIdsAsync(Guid processId)
        {
            var projectId = await GetProjectIdFromProcessIdAsync(processId);
            await ValidateUserProjectRoleAsync(projectId, ProjectRole.Member);

            var resolutions = await _unitOfWork.QualityAssessmentResolutions
                .FindAllAsync(r => r.QualityAssessmentProcessId == processId && r.FinalDecision == QualityAssessmentResolutionDecision.HighQuality);

            var qaPaperIds = resolutions.Select(r => r.QualityAssessmentPaperId).ToList();
            var qaPapers = await _unitOfWork.QualityAssessmentPapers.FindAllAsync(p => qaPaperIds.Contains(p.Id));

            var papers = await _unitOfWork.Papers.FindAllAsync(p => qaPapers.Select(qa => qa.PaperId).Contains(p.Id));

            return papers.Select(p => p.ToResponse()).ToList();
        }

        public async Task AutoResolveProcessAsync(AutoResolveQualityAssessmentRequest request)
        {
            var projectId = await GetProjectIdFromProcessIdAsync(request.QualityAssessmentProcessId);
            var (currentUserId, _) = await ValidateUserProjectRoleAsync(projectId, ProjectRole.Leader);

            if (!request.Score.HasValue && !request.Percentage.HasValue)
                throw new ArgumentException("Either Score or Percentage must be provided");

            var process = await _unitOfWork.QualityAssessmentProcesses.FindSingleAsync(p => p.Id == request.QualityAssessmentProcessId);
            if (process == null) throw new KeyNotFoundException("Process not found");

            var eligiblePaper = await _unitOfWork.QualityAssessmentPapers.GetByProcessIdWithDetailsAsync(process.Id);
            var assignments = await _unitOfWork.QualityAssessmentAssignments.GetAllWithPapersByProcessIdAsync(process.Id);
            var processResolutions = await _unitOfWork.QualityAssessmentResolutions.FindAllAsync(r => r.QualityAssessmentProcessId == process.Id);

            var protocolId = (await _unitOfWork.ReviewProcesses.FindSingleAsync(rp => rp.Id == process.ReviewProcessId))?.ProtocolId;
            var criteriaCount = 0;
            if (protocolId != null)
            {
                var strategies = await _unitOfWork.QualityStrategies.GetFullStrategyByProtocolIdAsync(protocolId.Value);
                foreach (var strat in strategies)
                {
                    foreach (var cl in strat.Checklists)
                    {
                        criteriaCount += cl.Criteria.Count;
                    }
                }
            }

            foreach (var paper in eligiblePaper)
            {
                if (processResolutions.Any(r => r.QualityAssessmentPaperId == paper.Id)) continue;
                
                var paperDecisions = await _unitOfWork.QualityAssessmentDecisions.GetByQaPaperIdWithDetailsAsync(paper.Id);

                var reviewersAssignedToPaper = assignments
                    .Where(a => a.QualityAssessmentPapers.Any(p => p.Id == paper.Id))
                    .Select(a => a.User)
                    .Where(u => u != null)
                    .GroupBy(u => u.Id)
                    .Select(g => g.First())
                    .ToList();

                var expectedDecisions = reviewersAssignedToPaper.Count * criteriaCount;
                var actualDecisions = paperDecisions.Sum(d => d.DecisionItems.Count);
                
                var completionPercentage = expectedDecisions > 0 ? (double)actualDecisions / expectedDecisions * 100 : 0;

                if (expectedDecisions == 0 || completionPercentage < 100) continue;

                double totalScore = 0;
                foreach (var decision in paperDecisions)
                {
                    foreach (var item in decision.DecisionItems)
                    {
                        if (item.Value == QualityAssessmentDecisionValue.Yes) totalScore += 1;
                        else if (item.Value == QualityAssessmentDecisionValue.Unclear) totalScore += 0.5;
                    }
                }

                double avgScore = reviewersAssignedToPaper.Count > 0 ? totalScore / reviewersAssignedToPaper.Count : 0;

                bool isPassed = false;
                if (request.Score.HasValue)
                {
                    isPassed = avgScore >= request.Score.Value;
                }
                else if (request.Percentage.HasValue)
                {
                    double maxPossibleAvgScore = criteriaCount;
                    double currPercentage = maxPossibleAvgScore > 0 ? (avgScore / maxPossibleAvgScore) * 100 : 0;
                    isPassed = currPercentage >= request.Percentage.Value;
                }

                var resolution = new QualityAssessmentResolution
                {
                    QualityAssessmentProcessId = process.Id,
                    QualityAssessmentPaperId = paper.Id,
                    FinalDecision = isPassed ? QualityAssessmentResolutionDecision.HighQuality : QualityAssessmentResolutionDecision.LowQuality,
                    FinalScore = (decimal)avgScore,
                    ResolutionNotes = "Auto-resolved",
                    ResolvedBy = currentUserId,
                    ResolvedAt = DateTimeOffset.UtcNow,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                };

                await _unitOfWork.QualityAssessmentResolutions.AddAsync(resolution);
            }
            
            await _unitOfWork.SaveChangesAsync();
        }

        // ==================== Quality Assessment Automate ====================
        public async Task<List<QualityAssessmentDecisionItemAIResponse>> AutomateQualityAssessmentAsync(AutomateQualityAssessmentRequest request)
        {
            var qaPaper = await _unitOfWork.QualityAssessmentPapers.FindSingleAsync(p => p.Id == request.QualityAssessmentPaperId);
            if (qaPaper == null) throw new KeyNotFoundException("Quality Assessment Paper Not Found");

            var projectId = await GetProjectIdFromProcessIdAsync(qaPaper.QualityAssessmentProcessId);
            var (userId, _) = await ValidateUserProjectRoleAsync(projectId, ProjectRole.Member);

            // Validate and find assignment
            var assignment = await _unitOfWork.QualityAssessmentAssignments.GetWithPapersByProcessAndUserAsync(request.QualityAssessmentProcessId, userId);
            if (assignment == null) throw new KeyNotFoundException("Assignment not found for this user in this process");

            // Check if resolution exists
            var resolution = await _unitOfWork.QualityAssessmentResolutions.FindSingleAsync(
                r => r.QualityAssessmentProcessId == assignment.QualityAssessmentProcessId && r.QualityAssessmentPaperId == qaPaper.Id);

            var paper = await _unitOfWork.Papers.FindSingleAsync(p => p.Id == qaPaper.PaperId);
            if (paper == null) throw new KeyNotFoundException("Paper Not Found");

            var process = await _unitOfWork.QualityAssessmentProcesses.FindSingleAsync(p => p.Id == qaPaper.QualityAssessmentProcessId)
                ?? throw new KeyNotFoundException("Process not found");

            var reviewProcess = await _unitOfWork.ReviewProcesses.FindSingleAsync(rp => rp.Id == process.ReviewProcessId)
                ?? throw new KeyNotFoundException("Review process not found");

            if (!reviewProcess.ProtocolId.HasValue)
                throw new InvalidOperationException("Protocol not found for this review process");

            var strategy = await _unitOfWork.QualityStrategies.GetFullStrategyByProtocolIdAsync(reviewProcess.ProtocolId.Value);

            var criteriaQuestions = strategy.SelectMany(s => s.Checklists.SelectMany(c => c.Criteria)).Select(c => new { c.Id, c.Question }).ToList();

            // RAG Integration: Instead of loading full pdf and extracting via Grobid, fetch relevant semantic chunks for each criterion.
            var relevantChunksByCriterion = new Dictionary<string, string>();
            foreach (var c in criteriaQuestions)
            {
                var chunks = await _ragRetrievalService.GetRelevantChunksAsync(qaPaper.PaperId, c.Question, topK: 10);
                var formattedChunks = string.Join("\n\n---\n\n", chunks.Select(chunk =>
                    $"[Source Chunk coordinates: {chunk.CoordinatesJson}]\n{chunk.TextContent}"
                ));
                relevantChunksByCriterion.Add(c.Question, string.IsNullOrWhiteSpace(formattedChunks) ? "No relevant chunk found." : formattedChunks);
            }

            var prompt = $@"
Assume you are an expert reviewer conducting a quality assessment of a scientific paper. I want you to evaluate this paper against the following criteria questions.

Note: 
- For each question, decide if the answer is Yes (0), No (1), or Unclear (2), and provide a brief comment explaining your reasoning.
- Also give pdfHighlightCoordinates based on the [Source Chunk coordinates: ...] provided alongside the evidence for each criterion. Keep precise format `page,x,y,height,width` for example: `'1,72.0,103.0,174.0,20.0'` (semicolon separated for multiple). Ensure the bounding boxes match where you drew your evidence.

-------------------------

Criteria Questions and retrieved relevant excerpts from abstract/full text:

{string.Join("\n\n", criteriaQuestions.Select(c =>
   $"--- CRITERION START ---\n" +
   $"ID: {c.Id}\n" +
   $"Question: {c.Question}\n" +
   $"Retrieved Evidence Chunks:\n{relevantChunksByCriterion[c.Question]}\n" +
   $"--- CRITERION END ---"
))}

--------------------------------

Here are the paper details:
- Title: {qaPaper.Paper.Title}
- Authors: {qaPaper.Paper.Authors}
- Publication Year: {qaPaper.Paper.PublicationYear}
- Journal/Conference: {(!string.IsNullOrWhiteSpace(qaPaper.Paper.Journal) ? qaPaper.Paper.Journal : qaPaper.Paper.ConferenceName)}
- Abstract: {qaPaper.Paper.Abstract}
";

            var result = await _geminiService.GenerateStructuredContentAsync<List<QualityAssessmentDecisionItemAIResponse>>(prompt);

            if (result == null || !result.Any())
            {
                throw new Exception("AI failed to generate assessment.");
            }

            await _auditLogService.AppendCustomAuditLogAsync(
                projectId: reviewProcess.ProjectId,
                action: "AI Automated Quality Assessment executed",
                actionType: "Automate",
                resourceType: "QualityAssessmentPaper",
                resourceId: qaPaper.Id.ToString(),
                newValue: new { GeneratedDecisionsCount = result.Count, PaperTitle = qaPaper.Paper.Title }
            );

            return result;
        }
        // ==================== Export Excel ====================
        public async Task<byte[]> ExportProcessToExcelAsync(Guid processId)
        {
            var projectId = await GetProjectIdFromProcessIdAsync(processId);
            await ValidateUserProjectRoleAsync(projectId, ProjectRole.Leader);

            var process = await _unitOfWork.QualityAssessmentProcesses.FindSingleAsync(p => p.Id == processId);
            if (process == null) throw new KeyNotFoundException("Quality Assessment Process not found");

            // Reuse existing method to get paper summaries
            var papersResult = await GetLeaderDashboardAsync(processId);
            var papers = papersResult.Papers;

            // Fetch full strategy to get criteria list (domains)
            var strategies = await GetStrategiesByProcessIdAsync(processId);
            // Flatten all criteria from all strategies and checklists
            var allCriteria = strategies
                .SelectMany(s => s.Checklists)
                .SelectMany(c => c.Criteria)
                .ToList();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("QualityAssessment");

            // Build headers: Row1 main headers, Row2 subheaders
            // Columns: A=Study Id, B=Reviewer
            ws.Cell("A1").Value = "Study Id";
            ws.Range("A1:A2").Merge().Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell("B1").Value = "Reviewer";
            ws.Range("B1:B2").Merge().Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var startCol = 3; // C
            for (int i = 0; i < allCriteria.Count; i++)
            {
                var criterion = allCriteria[i];
                var col = startCol + i * 3;

                // Merge three cells for criterion question (domain)
                var startAddress = ws.Cell(1, col).Address.ToStringRelative();
                var endAddress = ws.Cell(1, col + 2).Address.ToStringRelative();
                ws.Range(startAddress + ":" + endAddress).Merge();

                ws.Cell(1, col).Value = criterion.Question; // Use criteria name/question as domain name
                ws.Cell(2, col).Value = "Judgement";
                ws.Cell(2, col + 1).Value = "Quotes";
                ws.Cell(2, col + 2).Value = "Comments";

                // center domain title
                var domainRange = ws.Range(startAddress + ":" + endAddress);
                domainRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                domainRange.Style.Font.SetBold();
            }

            // Style first two header cells
            var headerRange = ws.Range("A1:B2");
            headerRange.Style.Font.SetBold();
            headerRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            // Populate data
            var currentRow = 3;
            foreach (var paper in papers)
            {
                // Use decisions list (each decision is per reviewer)
                var decisions = paper.Decisions ?? new List<QualityAssessmentDecisionResponse>();

                // Also include resolution as a consensus row if exists
                // if (paper.Resolution != null)
                // {
                //     // Create a synthetic decision row for consensus
                //     decisions = decisions.Append(new QualityAssessmentDecisionResponse
                //     {
                //         ReviewerId = paper.Resolution.ResolvedBy,
                //         ReviewerName = paper.Resolution.ResolvedByName ?? "Consensus",
                //         QualityAssessmentPaperId = paper.Resolution.QualityAssessmentPaperId,
                //         DecisionItems = new List<QualityAssessmentDecisionItemResponse>()
                //         // Note: Resolution doesn't currently store per-criterion items in DTO easily accessibly 
                //         // unless we load them separately or if they are just the final decision.
                //         // For now, Consensus row might be empty for items unless Resolution has items (it usually doesn't in this simplified model 
                //         // unless we query QualityAssessmentResolution -> which is just FinalDecision/Score in DTO).
                //         // If Resolution had items, we would map them here. Assuming empty for now as per DTO.
                //     }).ToList();
                // }

                foreach (var dec in decisions)
                {
                    ws.Cell(currentRow, 1).Value = paper.Title ?? paper.Id.ToString();
                    ws.Cell(currentRow, 2).Value = dec.ReviewerName ?? string.Empty;

                    // For each criterion column, find the matching decision item
                    for (int i = 0; i < allCriteria.Count; i++)
                    {
                        var criterion = allCriteria[i];
                        var col = startCol + i * 3;

                        // Find item by CriterionId
                        var item = dec.DecisionItems?.FirstOrDefault(di => di.QualityCriterionId == criterion.CriterionId);

                        if (item != null)
                        {
                            ws.Cell(currentRow, col).Value = item.Value?.ToString() ?? string.Empty; // Judgement
                            ws.Cell(currentRow, col + 1).Value = string.Empty; // Quotes
                            ws.Cell(currentRow, col + 2).Value = item.Comment ?? string.Empty; // Comments
                        }
                        else
                        {
                            ws.Cell(currentRow, col).Value = string.Empty;
                            ws.Cell(currentRow, col + 1).Value = string.Empty;
                            ws.Cell(currentRow, col + 2).Value = string.Empty;
                        }
                    }

                    currentRow++;
                }
            }

            // Auto-fit columns
            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }
    }
}