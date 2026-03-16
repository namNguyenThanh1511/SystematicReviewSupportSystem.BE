using Microsoft.EntityFrameworkCore;
using Shared.Models;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.DataExtraction;
using SRSS.IAM.Services.UserService;

namespace SRSS.IAM.Services.DataExtractionService
{
	public class DataExtractionConductingService : IDataExtractionConductingService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ICurrentUserService _currentUserService;

		public DataExtractionConductingService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
		{
			_unitOfWork = unitOfWork;
			_currentUserService = currentUserService;
		}

		private async Task SyncEligiblePapersAsync(Guid extractionProcessId)
        {
            // 1. Lấy thông tin ReviewProcess để biết ID của StudySelectionProcess
			var extractionProcess = await _unitOfWork.DataExtractionProcesses.GetQueryable()
				.Include(dp => dp.ReviewProcess)
					.ThenInclude(rp => rp.StudySelectionProcess)
				.FirstOrDefaultAsync(dp => dp.Id == extractionProcessId);
			
            if (extractionProcess?.ReviewProcess?.StudySelectionProcess == null) return;

            var selectionProcessId = extractionProcess.ReviewProcess.StudySelectionProcess.Id;

            // 2. Lấy danh sách PaperId đã PASS vòng Full-Text Screening
            // (FinalDecision == Include & Phase == FullText)
            var eligiblePapers = await _unitOfWork.ScreeningResolutions.FindAllAsync(sr => sr.StudySelectionProcessId == selectionProcessId
                          && sr.Phase == ScreeningPhase.FullText
                          && sr.FinalDecision == ScreeningDecisionType.Include);

			var eligiblePaperIds = eligiblePapers.Select(sr => sr.PaperId).ToList();

            // 3. Lấy danh sách Task đã tồn tại trong Data Extraction để tránh tạo trùng
            var existingTaskPaperIds = await _unitOfWork.ExtractionPaperTasks.FindAllAsync(t => t.DataExtractionProcessId == extractionProcessId);

            // 4. Tìm ra những Paper mới được pass Screening nhưng chưa có Task ở Extraction
            var newPaperIds = eligiblePaperIds.Where(paperId => !existingTaskPaperIds.Select(t => t.PaperId).Contains(paperId)).ToList();

            if (newPaperIds.Any())
            {
                var newTasks = newPaperIds.Select(paperId => new ExtractionPaperTask
                {
                    DataExtractionProcessId = extractionProcessId,
                    PaperId = paperId,
                    Status = PaperExtractionStatus.NotStarted,
                    Reviewer1Status = ReviewerTaskStatus.NotStarted,
                    Reviewer2Status = ReviewerTaskStatus.NotStarted,
                    CreatedAt = DateTimeOffset.UtcNow
                }).ToList();

                // Lưu hàng loạt xuống DB
                // Lưu ý: Nếu Repo của bạn không có AddRangeAsync, có thể dùng foreach
                await _unitOfWork.ExtractionPaperTasks.AddRangeAsync(newTasks);
                await _unitOfWork.SaveChangesAsync();
            }
        }

		public async Task<ExtractionDashboardResponseDto> GetDashboardAsync(Guid extractionProcessId, ExtractionDashboardFilterDto filter)
		{
			await SyncEligiblePapersAsync(extractionProcessId);

			var query = _unitOfWork.ExtractionPaperTasks.GetTasksByProcessQueryable(extractionProcessId);

			// Calculate Summary on the entire dataset
			var totalIncluded = await query.CountAsync();
			var inProgressCount = await query.CountAsync(t => t.Status == PaperExtractionStatus.InProgress);
			var awaitingConsensusCount = await query.CountAsync(t => t.Status == PaperExtractionStatus.AwaitingConsensus);
			var completedCount = await query.CountAsync(t => t.Status == PaperExtractionStatus.Completed);

			// Apply SearchFilter
			if (!string.IsNullOrWhiteSpace(filter.SearchQuery))
			{
				var searchTerm = filter.SearchQuery.ToLower();
				query = query.Where(t => 
					t.Paper.Title.ToLower().Contains(searchTerm) || 
					(t.Paper.Authors != null && t.Paper.Authors.ToLower().Contains(searchTerm)));
			}

			// Apply StatusFilter
			if (!string.IsNullOrWhiteSpace(filter.StatusFilter) && filter.StatusFilter.ToLower() != "all")
			{
				var statusStr = filter.StatusFilter.ToLower();
				var statusEnum = statusStr switch
				{
					"todo" => PaperExtractionStatus.NotStarted,
					"in-progress" => PaperExtractionStatus.InProgress,
					"awaiting-consensus" => PaperExtractionStatus.AwaitingConsensus,
					"completed" => PaperExtractionStatus.Completed,
					_ => (PaperExtractionStatus?)null
				};

				if (statusEnum.HasValue)
				{
					query = query.Where(t => t.Status == statusEnum.Value);
				}
			}

			// Pagination
			var totalCount = await query.CountAsync();
			var items = await query
				.OrderBy(t => t.Paper.Title)
				.Skip((filter.PageNumber - 1) * filter.PageSize)
				.Take(filter.PageSize)
				.Select(t => new ExtractionDashboardTaskDto
				{
					TaskId = t.Id,
					PaperId = t.PaperId,
					Title = t.Paper.Title,
					Authors = t.Paper.Authors,
					PublicationYear = t.Paper.PublicationYearInt,
					Reviewer1Id = t.Reviewer1Id,
					Reviewer2Id = t.Reviewer2Id,
					Status = t.Status.ToString()
				})
				.ToListAsync();

			return new ExtractionDashboardResponseDto
			{
				Summary = new ExtractionDashboardSummaryDto
				{
					TotalIncluded = totalIncluded,
					InProgress = inProgressCount,
					AwaitingConsensus = awaitingConsensusCount,
					Completed = completedCount
				},
				Tasks = new PaginatedList<ExtractionDashboardTaskDto>(items, totalCount, filter.PageNumber, filter.PageSize)
			};
		}

		public async Task AssignReviewersAsync(Guid extractionProcessId, Guid paperId, AssignReviewersDto dto)
		{
			if (!dto.Reviewer1Id.HasValue || !dto.Reviewer2Id.HasValue)
			{
				throw new ArgumentException("Both Reviewer1 and Reviewer2 are required.");
			}

			if (dto.Reviewer1Id == dto.Reviewer2Id)
			{
				throw new ArgumentException("Reviewer1 and Reviewer2 cannot be the same user.");
			}

			var task = await _unitOfWork.ExtractionPaperTasks.GetQueryable()
				.FirstOrDefaultAsync(t => t.DataExtractionProcessId == extractionProcessId && t.PaperId == paperId);

			if (task == null)
			{
				throw new InvalidOperationException($"Extraction task for paper {paperId} in process {extractionProcessId} not found.");
			}

			// Validation: Fetch project info
			var extractionProcess = await _unitOfWork.DataExtractionProcesses.GetQueryable()
				.Include(dp => dp.ReviewProcess)
				.FirstOrDefaultAsync(dp => dp.Id == extractionProcessId);
				
			if (extractionProcess?.ReviewProcess == null)
			{
				throw new InvalidOperationException($"DataExtractionProcess {extractionProcessId} or its ReviewProcess not found.");
			}

			var projectId = extractionProcess.ReviewProcess.ProjectId;

			// Leader Authorization Check
			var currentUserIdStr = _currentUserService.GetUserId();
			if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
			{
				throw new UnauthorizedAccessException("Current user ID is invalid.");
			}

			var currentUserProjectMember = await _unitOfWork.SystematicReviewProjects.GetQueryable()
				.Where(p => p.Id == projectId)
				.SelectMany(p => p.ProjectMembers)
				.FirstOrDefaultAsync(pm => pm.UserId == currentUserId);

			if (currentUserProjectMember == null || currentUserProjectMember.Role != ProjectRole.Leader)
			{
				throw new UnauthorizedAccessException($"User is not authorized. Must be a Leader for project {projectId}.");
			}

            await ValidateReviewerInProjectAsync(projectId, dto.Reviewer1Id);
			await ValidateReviewerInProjectAsync(projectId, dto.Reviewer2Id);

			task.Reviewer1Id = dto.Reviewer1Id;
			task.Reviewer2Id = dto.Reviewer2Id;
			task.Status = PaperExtractionStatus.InProgress;
			task.ModifiedAt = DateTimeOffset.UtcNow;

			await _unitOfWork.SaveChangesAsync();
		}

		private async Task ValidateReviewerInProjectAsync(Guid projectId, Guid? reviewerId)
		{
			if (!reviewerId.HasValue) return;

			var project = await _unitOfWork.SystematicReviewProjects.GetQueryable()
				.Include(p => p.ProjectMembers)
				.FirstOrDefaultAsync(p => p.Id == projectId);

			if (project == null || !project.ProjectMembers.Any(pm => pm.UserId == reviewerId.Value))
			{
				throw new ArgumentException($"User {reviewerId} is not a member of project {projectId}.");
			}
		}

		public async Task<DataExtractionProcessResponse> StartAsync(Guid extractionProcessId)
		{
			var entity = await _unitOfWork.DataExtractionProcesses.GetQueryable()
				.Include(dp => dp.ReviewProcess)
				.FirstOrDefaultAsync(dp => dp.Id == extractionProcessId);

			if (entity == null)
			{
				throw new InvalidOperationException($"Data extraction process {extractionProcessId} not found.");
			}

			if (entity.Status == ExtractionProcessStatus.Completed)
			{
				throw new InvalidOperationException("Data extraction process already completed.");
			}

			if (entity.Status == ExtractionProcessStatus.NotStarted)
			{
				entity.Status = ExtractionProcessStatus.InProgress;
				entity.StartedAt ??= DateTimeOffset.UtcNow;
				entity.ModifiedAt = DateTimeOffset.UtcNow;
			}

			await _unitOfWork.SaveChangesAsync();

			return new DataExtractionProcessResponse
			{
				Id = entity.Id,
				ReviewProcessId = entity.ReviewProcessId,
				Status = entity.Status,
				StatusText = entity.Status.ToString(),
				StartedAt = entity.StartedAt,
				CompletedAt = entity.CompletedAt,
				Notes = entity.Notes,
				CreatedAt = entity.CreatedAt,
				ModifiedAt = entity.ModifiedAt
			};
		}
	}
}
