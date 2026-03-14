using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.QualityAssessment;
using SRSS.IAM.Services.Mappers;

namespace SRSS.IAM.Services.QualityAssessmentService
{
	public class QualityAssessmentService : IQualityAssessmentService
	{
		private readonly IUnitOfWork _unitOfWork;

		public QualityAssessmentService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
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
	}
}