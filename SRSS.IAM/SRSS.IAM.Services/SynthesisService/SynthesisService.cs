using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.Synthesis;
using SRSS.IAM.Services.Mappers;

namespace SRSS.IAM.Services.SynthesisService
{
	public class SynthesisService : ISynthesisService
	{
		private readonly IUnitOfWork _unitOfWork;

		public SynthesisService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		// ==================== Data Synthesis Strategies ====================
		public async Task<DataSynthesisStrategyDto> UpsertSynthesisStrategyAsync(DataSynthesisStrategyDto dto)
		{
			DataSynthesisStrategy entity;

			if (dto.SynthesisStrategyId.HasValue && dto.SynthesisStrategyId.Value != Guid.Empty)
			{
				entity = await _unitOfWork.SynthesisStrategies.FindSingleAsync(s => s.Id == dto.SynthesisStrategyId.Value)
					?? throw new KeyNotFoundException($"Strategy {dto.SynthesisStrategyId.Value} không tồn tại");

				dto.UpdateEntity(entity);  
				await _unitOfWork.SynthesisStrategies.UpdateAsync(entity);
			}
			else
			{
				entity = dto.ToEntity();  
				await _unitOfWork.SynthesisStrategies.AddAsync(entity);
			}

			await _unitOfWork.SaveChangesAsync();
			return entity.ToDto();  
		}

		public async Task<List<DataSynthesisStrategyDto>> GetSynthesisStrategiesByProtocolIdAsync(Guid protocolId)
		{
			var entities = await _unitOfWork.SynthesisStrategies.GetByProtocolIdAsync(protocolId);
			return entities.ToDtoList();  
		}

		public async Task DeleteSynthesisStrategyAsync(Guid strategyId)
		{
			var entity = await _unitOfWork.SynthesisStrategies.FindSingleAsync(s => s.Id == strategyId);
			if (entity != null)
			{
				await _unitOfWork.SynthesisStrategies.RemoveAsync(entity);
				await _unitOfWork.SaveChangesAsync();
			}
		}

		// ==================== Dissemination Strategies ====================
		public async Task<DisseminationStrategyDto> UpsertDisseminationStrategyAsync(DisseminationStrategyDto dto)
		{
			DisseminationStrategy entity;

			if (dto.DisseminationId.HasValue && dto.DisseminationId.Value != Guid.Empty)
			{
				entity = await _unitOfWork.DisseminationStrategies.FindSingleAsync(s => s.Id == dto.DisseminationId.Value)
					?? throw new KeyNotFoundException($"Strategy {dto.DisseminationId.Value} không tồn tại");

				dto.UpdateEntity(entity);  
				await _unitOfWork.DisseminationStrategies.UpdateAsync(entity);
			}
			else
			{
				entity = dto.ToEntity();  
				await _unitOfWork.DisseminationStrategies.AddAsync(entity);
			}

			await _unitOfWork.SaveChangesAsync();
			return entity.ToDto();  
		}

		public async Task<List<DisseminationStrategyDto>> GetDisseminationStrategiesByProtocolIdAsync(Guid protocolId)
		{
			var entities = await _unitOfWork.DisseminationStrategies.GetByProtocolIdAsync(protocolId);
			return entities.ToDtoList();  
		}

		public async Task DeleteDisseminationStrategyAsync(Guid strategyId)
		{
			var entity = await _unitOfWork.DisseminationStrategies.FindSingleAsync(s => s.Id == strategyId);
			if (entity != null)
			{
				await _unitOfWork.DisseminationStrategies.RemoveAsync(entity);
				await _unitOfWork.SaveChangesAsync();
			}
		}

		// ==================== Project Timetable ====================
		public async Task<List<ProjectTimetableDto>> BulkUpsertTimetableAsync(List<ProjectTimetableDto> dtos)
		{
			var results = new List<ProjectTimetable>();

			foreach (var dto in dtos)
			{
				ProjectTimetable entity;

				if (dto.TimetableId.HasValue && dto.TimetableId.Value != Guid.Empty)
				{
					entity = await _unitOfWork.Timetables.FindSingleAsync(t => t.Id == dto.TimetableId.Value);

					if (entity != null)
					{
						dto.UpdateEntity(entity);  
						await _unitOfWork.Timetables.UpdateAsync(entity);
					}
					else
					{
						entity = dto.ToEntity();  
						await _unitOfWork.Timetables.AddAsync(entity);
					}
				}
				else
				{
					entity = dto.ToEntity();  
					await _unitOfWork.Timetables.AddAsync(entity);
				}

				results.Add(entity);
			}

			await _unitOfWork.SaveChangesAsync();
			return results.ToDtoList();  
		}

		public async Task<List<ProjectTimetableDto>> GetTimetableByProtocolIdAsync(Guid protocolId)
		{
			var entities = await _unitOfWork.Timetables.GetByProtocolIdAsync(protocolId);
			return entities.ToDtoList();  
		}

		public async Task DeleteTimetableEntryAsync(Guid timetableId)
		{
			var entity = await _unitOfWork.Timetables.FindSingleAsync(t => t.Id == timetableId);
			if (entity != null)
			{
				await _unitOfWork.Timetables.RemoveAsync(entity);
				await _unitOfWork.SaveChangesAsync();
			}
		}
	}
}