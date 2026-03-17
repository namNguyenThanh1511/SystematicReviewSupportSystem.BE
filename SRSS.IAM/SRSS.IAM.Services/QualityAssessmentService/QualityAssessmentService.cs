using SRSS.IAM.Repositories;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.QualityAssessment;
using SRSS.IAM.Services.Mappers;
using SRSS.IAM.Repositories.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using SRSS.IAM.Services.NotificationService;
using SRSS.IAM.Services.DTOs.Notification;

namespace SRSS.IAM.Services.QualityAssessmentService
{
	public class QualityAssessmentService : IQualityAssessmentService
	{
		private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public QualityAssessmentService(IUnitOfWork unitOfWork, INotificationService notificationService)
		{
			_unitOfWork = unitOfWork;
            _notificationService = notificationService;
		}

		// ==================== Quality Assessment Strategies ====================
		public async Task<QualityAssessmentStrategyDto> UpsertStrategyAsync(QualityAssessmentStrategyDto dto)
		{
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
			var entities = await _unitOfWork.QualityStrategies.GetByProtocolIdAsync(protocolId);
			return entities.ToDtoList();  
		}

		public async Task DeleteStrategyAsync(Guid strategyId)
		{
			var entity = await _unitOfWork.QualityStrategies.FindSingleAsync(s => s.Id == strategyId);
			if (entity != null)
			{
				await _unitOfWork.QualityStrategies.RemoveAsync(entity);
				await _unitOfWork.SaveChangesAsync();
			}
		}

		// ==================== Quality Checklists ====================
		public async Task<List<QualityChecklistDto>> BulkUpsertChecklistsAsync(List<QualityChecklistDto> dtos)
		{
			var results = new List<QualityChecklist>();

			foreach (var dto in dtos)
			{
				QualityChecklist entity;

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

		public async Task<List<QualityChecklistDto>> GetChecklistsByStrategyIdAsync(Guid strategyId)
		{
			var entities = await _unitOfWork.QualityChecklists.GetByStrategyIdAsync(strategyId);
			return entities.ToDtoList();  
		}

		// ==================== Quality Criteria ====================
		public async Task<List<QualityCriterionDto>> BulkUpsertCriteriaAsync(List<QualityCriterionDto> dtos)
		{
			var results = new List<QualityCriterion>();

			foreach (var dto in dtos)
			{
				QualityCriterion entity;

				if (dto.QualityCriterionId.HasValue && dto.QualityCriterionId.Value != Guid.Empty)
				{
					entity = await _unitOfWork.QualityCriteria.FindSingleAsync(c => c.Id == dto.QualityCriterionId.Value);

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

		public async Task<List<QualityCriterionDto>> GetCriteriaByChecklistIdAsync(Guid checklistId)
		{
			var entities = await _unitOfWork.QualityCriteria.GetByChecklistIdAsync(checklistId);
			return entities.ToDtoList();  
		}

        // ==================== Quality Assessment Process ====================
        public async Task<QualityAssessmentProcessResponse> GetProcessByReviewProcessIdAsync(Guid reviewProcessId)
        {
            var process = await _unitOfWork.QualityAssessmentProcesses.FindSingleAsync(
                p => p.ReviewProcessId == reviewProcessId);
                
            if (process == null) return null;
            
            return process.ToResponse();
        }        
		
		public async Task<QualityAssessmentProcessResponse> CreateProcessAsync(CreateQualityAssessmentProcessDto dto)
        {
            // Check existence
            var existing = await _unitOfWork.QualityAssessmentProcesses.FindSingleAsync(p => p.ReviewProcessId == dto.ReviewProcessId);
            if (existing != null)
                throw new InvalidOperationException($"Quality Assessment Process for Review Process {dto.ReviewProcessId} already exists.");

            var entity = new QualityAssessmentProcess
            {
                ReviewProcessId = dto.ReviewProcessId,
                Notes = dto.Notes,
                Status = QualityAssessmentProcessStatus.NotStarted
            };
            
            await _unitOfWork.QualityAssessmentProcesses.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            
            return entity.ToResponse();
        }

        public async Task<QualityAssessmentProcessResponse> StartProcessAsync(Guid id)
        {
            var entity = await _unitOfWork.QualityAssessmentProcesses.FindSingleAsync(p => p.Id == id)
                ?? throw new KeyNotFoundException($"Không tìm thấy QA Process {id}");

            entity.Start();

            await _unitOfWork.QualityAssessmentProcesses.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return entity.ToResponse();
        }

        public async Task<QualityAssessmentProcessResponse> CompleteProcessAsync(Guid id)
        {
            var entity = await _unitOfWork.QualityAssessmentProcesses.FindSingleAsync(p => p.Id == id)
                ?? throw new KeyNotFoundException($"Không tìm thấy QA Process {id}");

            entity.Complete();

            await _unitOfWork.QualityAssessmentProcesses.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return entity.ToResponse();
        }       

        // ==================== Assignments ====================
        public async Task AssignPapersToReviewersAsync(CreateQualityAssessmentAssignmentDto dto)
        {
            var process = await _unitOfWork.QualityAssessmentProcesses.FindSingleAsync(p => p.Id == dto.QualityAssessmentProcessId)
                ?? throw new KeyNotFoundException("Quality Assessment Process not found");

            // Validate Users and Papers exist (optional but recommended)

            foreach (var userId in dto.UserIds)
            {
                // Find existing assignment or create new
                var assignment = await _unitOfWork.QualityAssessmentAssignments
                    .FindSingleAsync(a => a.QualityAssessmentProcessId == dto.QualityAssessmentProcessId && a.UserId == userId);

                if (assignment == null)
                {
                    // isNewAssignment = true;
                    assignment = new QualityAssessmentAssignment
                    {
                        QualityAssessmentProcessId = dto.QualityAssessmentProcessId,
                        UserId = userId,
                        AssignedAt = DateTimeOffset.UtcNow
                    };
                    await _unitOfWork.QualityAssessmentAssignments.AddAsync(assignment);
                    await _unitOfWork.SaveChangesAsync(); // Save to get ID
                }

                // Check resolution for each paper before assigning
                var papersToAdd = new List<Paper>();
                foreach(var paperId in dto.PaperIds)
                {
                    // If paper has resolution, skip adding
                     var hasResolution = await _unitOfWork.QualityAssessmentResolutions
                        .AnyAsync(r => r.PaperId == paperId && r.QualityAssessmentProcessId == dto.QualityAssessmentProcessId);
                    
                    if (hasResolution) continue;

                    var paper = await _unitOfWork.Papers.FindSingleAsync(p => p.Id == paperId);
                    if (paper != null)
                    {
                        papersToAdd.Add(paper);
                    }
                }

                if (assignment.Paper == null) assignment.Paper = new List<Paper>();
                
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
                        if (!assignmentWithPapers.Paper.Any(p => p.Id == paper.Id))
                        {
                            assignmentWithPapers.Paper.Add(paper);
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

        public async Task<List<MyAssignedPaperDto>> GetMyAssignedPapersAsync(Guid userId, Guid reviewProcessId)
        {
            var process = await _unitOfWork.QualityAssessmentProcesses.FindSingleAsync(p => p.ReviewProcessId == reviewProcessId);
            if (process == null) return new List<MyAssignedPaperDto>();

            var assignment = await _unitOfWork.QualityAssessmentAssignments.GetWithPapersByProcessAndUserAsync(process.Id, userId);

            if (assignment == null || assignment.Paper == null) return new List<MyAssignedPaperDto>();

            // Need to calculate completion percentage.
            // 1. Get Protocol -> QA Strategy -> Checklists -> Criteria count
            var protocolId = (await _unitOfWork.ReviewProcesses.FindSingleAsync(rp => rp.Id == reviewProcessId))?.ProtocolId;
            if (protocolId == null) return new List<MyAssignedPaperDto>(); // Should not happen if configured correctly

            var strategies = await _unitOfWork.QualityStrategies.GetByProtocolIdAsync(protocolId.Value);
            var criteriaCount = 0;
            foreach(var strat in strategies)
            {
                var checklists = await _unitOfWork.QualityChecklists.GetByStrategyIdAsync(strat.Id);
                foreach(var cl in checklists)
                {
                     var criteria = await _unitOfWork.QualityCriteria.GetByChecklistIdAsync(cl.Id);
                     criteriaCount += criteria.Count();
                }
            }

            var result = new List<MyAssignedPaperDto>();
            foreach (var paper in assignment.Paper)
            {
                // Check for resolution
                 var resolution = await _unitOfWork.QualityAssessmentResolutions.FindSingleAsync(
                    r => r.PaperId == paper.Id && r.QualityAssessmentProcessId == process.Id);

                // Calculate decisions count for this paper by this user
                var paperDecisions = await _unitOfWork.QualityAssessmentDecisions.FindAllAsync(
                    d => d.ReviewerId == userId && d.PaperId == paper.Id);
                var userDecisionsCount = paperDecisions.Count();
                
                double percentage = criteriaCount > 0 ? (double)userDecisionsCount / criteriaCount * 100 : 0;
                if (percentage > 100) percentage = 100; // Cap at 100 if updates happen

                var dto = new MyAssignedPaperDto
                {
                    Id = paper.Id,
                    Title = paper.Title,
                    Authors = paper.Authors,
                    PublicationYear = paper.PublicationYear,
                     // Map other paper properties as needed...
                    CompletionPercentage = Math.Round(percentage, 2),
                    ResolutionDecision = resolution?.FinalDecision != null ? resolution.FinalDecision.ToString() : null
                };
                result.Add(dto);
            }
            return result;
        }

        // ==================== Decisions ====================
        public async Task CreateDecisionAsync(Guid userId, CreateQualityAssessmentDecisionDto dto)
        {
             // Validate and find assignment
             var assignment = await _unitOfWork.QualityAssessmentAssignments.GetByUserAndPaperAsync(userId, dto.PaperId);
             if (assignment == null) throw new KeyNotFoundException("Assignment not found for this user and paper");

             // Check if resolution exists
             var resolution = await _unitOfWork.QualityAssessmentResolutions.FindSingleAsync(
                 r => r.QualityAssessmentProcessId == assignment.QualityAssessmentProcessId && r.PaperId == dto.PaperId);
            
            if (resolution != null)
            {
                throw new InvalidOperationException("Cannot add decision because a final resolution has already been made for this paper.");
            }

            // Check existing decision
            var existing = await _unitOfWork.QualityAssessmentDecisions.FindSingleAsync(
                d => d.ReviewerId == userId && d.PaperId == dto.PaperId && d.QualityCriterionId == dto.QualityCriterionId);
            
            if (existing != null)
                throw new InvalidOperationException("Decision for this criterion already exists. Use update instead.");

            var decision = dto.ToEntity(userId);
            
            await _unitOfWork.QualityAssessmentDecisions.AddAsync(decision);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task CreateDecisionsForPaperAsync(Guid userId, Guid paperId, List<CreateQualityAssessmentDecisionItemDto> dtos)
        {
             // Validate and find assignment
             var assignment = await _unitOfWork.QualityAssessmentAssignments.GetByUserAndPaperAsync(userId, paperId);
             if (assignment == null) throw new KeyNotFoundException("Assignment not found for this user and paper");

             // Check if resolution exists
             var resolution = await _unitOfWork.QualityAssessmentResolutions.FindSingleAsync(
                 r => r.QualityAssessmentProcessId == assignment.QualityAssessmentProcessId && r.PaperId == paperId);
            
            if (resolution != null)
            {
                throw new InvalidOperationException("Cannot add decisions because a final resolution has already been made for this paper.");
            }

            foreach (var dto in dtos)
            {
                // Check existing decision
                var existing = await _unitOfWork.QualityAssessmentDecisions.FindSingleAsync(
                    d => d.ReviewerId == userId && d.PaperId == paperId && d.QualityCriterionId == dto.QualityCriterionId);
                
                if (existing != null)
                {
                    // For batch create, we could either throw or just skip/update.
                    // Given the strict separation, we'll throw to inform the caller they are creating over an existing record.
                    // Alternatively, we can just skip it, but throwing is safer for integrity.
                    throw new InvalidOperationException($"Decision for criterion {dto.QualityCriterionId} already exists. Use update instead.");
                }

                var decision = dto.ToEntity(userId, paperId);
                
                await _unitOfWork.QualityAssessmentDecisions.AddAsync(decision);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateDecisionAsync(Guid userId, Guid paperId, Guid criterionId, UpdateQualityAssessmentDecisionDto dto)
        {
             // Get assignment (to get process ID for resolution check)
             var assignment = await _unitOfWork.QualityAssessmentAssignments.GetByUserAndPaperAsync(userId, paperId);
             if (assignment == null) throw new KeyNotFoundException("Assignment not found");

             var resolution = await _unitOfWork.QualityAssessmentResolutions.FindSingleAsync(
                 r => r.QualityAssessmentProcessId == assignment.QualityAssessmentProcessId && r.PaperId == paperId);
            
            if (resolution != null)
            {
                throw new InvalidOperationException("Cannot update decision because a resolution exists.");
            }

            var decision = await _unitOfWork.QualityAssessmentDecisions.FindSingleAsync(
                d => d.ReviewerId == userId && d.PaperId == paperId && d.QualityCriterionId == criterionId);
            
            if (decision == null)
                throw new KeyNotFoundException("Decision not found.");

            dto.UpdateEntity(decision);

            await _unitOfWork.QualityAssessmentDecisions.UpdateAsync(decision);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateDecisionsForPaperAsync(Guid userId, Guid paperId, List<UpdateQualityAssessmentDecisionItemDto> dtos)
        {
            var assignment = await _unitOfWork.QualityAssessmentAssignments.GetByUserAndPaperAsync(userId, paperId);
            if (assignment == null) throw new KeyNotFoundException("Assignment not found");

            var resolution = await _unitOfWork.QualityAssessmentResolutions.FindSingleAsync(
                r => r.QualityAssessmentProcessId == assignment.QualityAssessmentProcessId && r.PaperId == paperId);
            
            if (resolution != null)
            {
                throw new InvalidOperationException("Cannot update decisions because a resolution exists.");
            }

            foreach (var dto in dtos)
            {
                var decision = await _unitOfWork.QualityAssessmentDecisions.FindSingleAsync(
                    d => d.ReviewerId == userId && d.PaperId == paperId && d.QualityCriterionId == dto.QualityCriterionId);

                if (decision != null)
                {
                    dto.UpdateEntity(decision);
                    await _unitOfWork.QualityAssessmentDecisions.UpdateAsync(decision);
                }
                else
                {
                    // Create if it doesn't exist to allow upsert-like behavior in batch
                    var newDecision = dto.ToEntity(userId, paperId);
                    await _unitOfWork.QualityAssessmentDecisions.AddAsync(newDecision);
                }
            }
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<QualityAssessmentDecisionDto>> GetDecisionsByPaperIdAsync(Guid paperId)
        {
            var decisions = await _unitOfWork.QualityAssessmentDecisions.GetByPaperIdWithDetailsAsync(paperId);

            return decisions.Select(d => d.ToDto()).ToList();
        }

        // ==================== Resolutions ====================
        public async Task<QualityAssessmentResolutionResponse> CreateResolutionAsync(CreateQualityAssessmentResolutionDto dto)
        {
             // Verify existing
             var existing = await _unitOfWork.QualityAssessmentResolutions.FindSingleAsync(
                 r => r.QualityAssessmentProcessId == dto.QualityAssessmentProcessId && r.PaperId == dto.PaperId);
            
            if (existing != null) throw new InvalidOperationException("Resolution already exists for this paper");

            var entity = dto.ToEntity();

            await _unitOfWork.QualityAssessmentResolutions.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            
            return entity.ToResponse();
        }

        public async Task<QualityAssessmentResolutionResponse> UpdateResolutionAsync(Guid id, UpdateQualityAssessmentResolutionDto dto)
        {
            var entity = await _unitOfWork.QualityAssessmentResolutions.FindSingleAsync(r => r.Id == id)
                ?? throw new KeyNotFoundException("Resolution not found");

            dto.UpdateEntity(entity);
            // ResolvedBy / At usually stay with creator or update? Let's assume update logic if needed
            // But Resolution is often final.
            
            await _unitOfWork.QualityAssessmentResolutions.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return entity.ToResponse();
        }
        
        public async Task<QualityAssessmentResolutionResponse> GetResolutionByPaperIdAsync(Guid paperId)
        {
            var res = await _unitOfWork.QualityAssessmentResolutions.FindSingleAsync(r => r.PaperId == paperId);
            if (res == null) return null!;
            
            return res.ToResponse();
        }
    }
}