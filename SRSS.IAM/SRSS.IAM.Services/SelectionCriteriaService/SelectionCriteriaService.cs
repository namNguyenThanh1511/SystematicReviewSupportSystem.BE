using Microsoft.EntityFrameworkCore;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.SelectionCriteria;
using SRSS.IAM.Services.Mappers;
using SRSS.IAM.Services.UserService;
using Shared.Exceptions;

namespace SRSS.IAM.Services.SelectionCriteriaService
{
	public class SelectionCriteriaService : ISelectionCriteriaService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ICurrentUserService _currentUserService;

		public SelectionCriteriaService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
		{
			_unitOfWork = unitOfWork;
			_currentUserService = currentUserService;
		}

		private async Task EnsureLeaderAsync(Guid projectId)
		{
			var userIdString = _currentUserService.GetUserId();
			if (string.IsNullOrEmpty(userIdString))
			{
				throw new UnauthorizedException("User is not authenticated.");
			}

			var userId = Guid.Parse(userIdString);
			var isLeader = await _unitOfWork.SystematicReviewProjects.IsProjectLeaderAsync(projectId, userId);
			if (!isLeader)
			{
				throw new ForbiddenException("Only project leader can perform this action.");
			}
		}

		private async Task EnsureLeaderByCriteriaIdAsync(Guid criteriaId)
		{
			var criteria = await _unitOfWork.SelectionCriterias
				.GetQueryable(c => c.Id == criteriaId, isTracking: false)
				.Include(c => c.StudySelectionProcess)
					.ThenInclude(p => p.ReviewProcess)
				.FirstOrDefaultAsync()
				?? throw new KeyNotFoundException($"Criteria {criteriaId} not found");

			await EnsureLeaderAsync(criteria.StudySelectionProcess.ReviewProcess.ProjectId);
		}

		private async Task EnsureLeaderByStudySelectionProcessIdAsync(Guid processId)
		{
			var process = await _unitOfWork.StudySelectionProcesses
				.GetQueryable(p => p.Id == processId, isTracking: false)
				.Include(p => p.ReviewProcess)
				.FirstOrDefaultAsync()
				?? throw new KeyNotFoundException($"Study selection process {processId} not found");

			await EnsureLeaderAsync(process.ReviewProcess.ProjectId);
		}

		// ==================== Study Selection Criteria ====================
		public async Task<StudySelectionCriteriaDto> UpsertCriteriaAsync(StudySelectionCriteriaDto dto)
		{
			await EnsureLeaderByStudySelectionProcessIdAsync(dto.StudySelectionProcessId);

			StudySelectionCriteria entity;

			if (dto.CriteriaId.HasValue && dto.CriteriaId.Value != Guid.Empty)
			{
				entity = await _unitOfWork.SelectionCriterias.FindSingleAsync(c => c.Id == dto.CriteriaId.Value)
					?? throw new KeyNotFoundException($"Criteria {dto.CriteriaId.Value} không tồn tại");

				dto.UpdateEntity(entity);  
				await _unitOfWork.SelectionCriterias.UpdateAsync(entity);
			}
			else
			{
				entity = dto.ToEntity();  
				await _unitOfWork.SelectionCriterias.AddAsync(entity);
			}

			await _unitOfWork.SaveChangesAsync();
			return entity.ToDto();  
		}

		public async Task<List<StudySelectionCriteriaDto>> GetAllByStudySelectionProcessIdAsync(Guid studySelectionProcessId)
		{
			var entities = await _unitOfWork.SelectionCriterias.GetByStudySelectionProcessIdAsync(studySelectionProcessId);
			return entities.ToDtoList();  
		}

		public async Task DeleteCriteriaAsync(Guid criteriaId)
		{
			var entity = await _unitOfWork.SelectionCriterias
				.GetQueryable(c => c.Id == criteriaId, isTracking: true)
				.Include(c => c.StudySelectionProcess)
					.ThenInclude(p => p.ReviewProcess)
				.FirstOrDefaultAsync();
			if (entity != null)
			{
				await EnsureLeaderAsync(entity.StudySelectionProcess.ReviewProcess.ProjectId);
				await _unitOfWork.SelectionCriterias.RemoveAsync(entity);
				await _unitOfWork.SaveChangesAsync();
			}
		}

		// ==================== Inclusion Criteria ====================
		public async Task<List<InclusionCriterionDto>> BulkUpsertInclusionCriteriaAsync(List<InclusionCriterionDto> dtos)
		{
			if (dtos.Any())
			{
				await EnsureLeaderByCriteriaIdAsync(dtos.First().CriteriaId);
			}

			var results = new List<InclusionCriterion>();

			foreach (var dto in dtos)
			{
				InclusionCriterion entity;

				if (dto.InclusionId.HasValue && dto.InclusionId.Value != Guid.Empty)
				{
					entity = await _unitOfWork.InclusionCriteria.FindSingleAsync(c => c.Id == dto.InclusionId.Value);

					if (entity != null)
					{
						dto.UpdateEntity(entity);  
						await _unitOfWork.InclusionCriteria.UpdateAsync(entity);
					}
					else
					{
						entity = dto.ToEntity();  
						await _unitOfWork.InclusionCriteria.AddAsync(entity);
					}
				}
				else
				{
					entity = dto.ToEntity();  
					await _unitOfWork.InclusionCriteria.AddAsync(entity);
				}

				results.Add(entity);
			}

			await _unitOfWork.SaveChangesAsync();
			return results.ToDtoList();  
		}

		public async Task<List<InclusionCriterionDto>> GetInclusionByCriteriaIdAsync(Guid criteriaId)
		{
			var entities = await _unitOfWork.InclusionCriteria.GetByCriteriaIdAsync(criteriaId);
			return entities.ToDtoList();  
		}

		// ==================== Exclusion Criteria ====================
		public async Task<List<ExclusionCriterionDto>> BulkUpsertExclusionCriteriaAsync(List<ExclusionCriterionDto> dtos)
		{
			if (dtos.Any())
			{
				await EnsureLeaderByCriteriaIdAsync(dtos.First().CriteriaId);
			}

			var results = new List<ExclusionCriterion>();

			foreach (var dto in dtos)
			{
				ExclusionCriterion entity;

				if (dto.ExclusionId.HasValue && dto.ExclusionId.Value != Guid.Empty)
				{
					entity = await _unitOfWork.ExclusionCriteria.FindSingleAsync(c => c.Id == dto.ExclusionId.Value);

					if (entity != null)
					{
						dto.UpdateEntity(entity);  
						await _unitOfWork.ExclusionCriteria.UpdateAsync(entity);
					}
					else
					{
						entity = dto.ToEntity();  
						await _unitOfWork.ExclusionCriteria.AddAsync(entity);
					}
				}
				else
				{
					entity = dto.ToEntity();  
					await _unitOfWork.ExclusionCriteria.AddAsync(entity);
				}

				results.Add(entity);
			}

			await _unitOfWork.SaveChangesAsync();
			return results.ToDtoList();  
		}

		public async Task<List<ExclusionCriterionDto>> GetExclusionByCriteriaIdAsync(Guid criteriaId)
		{
			var entities = await _unitOfWork.ExclusionCriteria.GetByCriteriaIdAsync(criteriaId);
			return entities.ToDtoList();  
		}

	}
}