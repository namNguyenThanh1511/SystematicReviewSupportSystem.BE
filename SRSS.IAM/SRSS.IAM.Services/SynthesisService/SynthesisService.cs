using AutoMapper;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.Synthesis;

namespace SRSS.IAM.Services.SynthesisService
{
	public class SynthesisService : ISynthesisService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public SynthesisService(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		// ==================== Data Synthesis Strategies ====================
		public async Task<DataSynthesisStrategyDto> UpsertSynthesisStrategyAsync(DataSynthesisStrategyDto dto)
		{
			DataSynthesisStrategy entity;

			if (dto.SynthesisStrategyId.HasValue && dto.SynthesisStrategyId.Value != Guid.Empty)
			{
				entity = await _unitOfWork.SynthesisStrategies.FindSingleAsync(s => s.Id == dto.SynthesisStrategyId.Value)
					?? throw new KeyNotFoundException($"Strategy {dto.SynthesisStrategyId.Value} không tồn tại");

				_mapper.Map(dto, entity);
				await _unitOfWork.SynthesisStrategies.UpdateAsync(entity);
			}
			else
			{
				entity = _mapper.Map<DataSynthesisStrategy>(dto);
				await _unitOfWork.SynthesisStrategies.AddAsync(entity);
			}

			await _unitOfWork.SaveChangesAsync();
			return _mapper.Map<DataSynthesisStrategyDto>(entity);
		}

		public async Task<List<DataSynthesisStrategyDto>> GetSynthesisStrategiesByProtocolIdAsync(Guid protocolId)
		{
			var entities = await _unitOfWork.SynthesisStrategies.GetByProtocolIdAsync(protocolId);
			return _mapper.Map<List<DataSynthesisStrategyDto>>(entities);
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

				_mapper.Map(dto, entity);
				await _unitOfWork.DisseminationStrategies.UpdateAsync(entity);
			}
			else
			{
				entity = _mapper.Map<DisseminationStrategy>(dto);
				await _unitOfWork.DisseminationStrategies.AddAsync(entity);
			}

			await _unitOfWork.SaveChangesAsync();
			return _mapper.Map<DisseminationStrategyDto>(entity);
		}

		public async Task<List<DisseminationStrategyDto>> GetDisseminationStrategiesByProtocolIdAsync(Guid protocolId)
		{
			var entities = await _unitOfWork.DisseminationStrategies.GetByProtocolIdAsync(protocolId);
			return _mapper.Map<List<DisseminationStrategyDto>>(entities);
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
						_mapper.Map(dto, entity);
						await _unitOfWork.Timetables.UpdateAsync(entity);
					}
					else
					{
						entity = _mapper.Map<ProjectTimetable>(dto);
						await _unitOfWork.Timetables.AddAsync(entity);
					}
				}
				else
				{
					entity = _mapper.Map<ProjectTimetable>(dto);
					await _unitOfWork.Timetables.AddAsync(entity);
				}

				results.Add(entity);
			}

			await _unitOfWork.SaveChangesAsync();
			return _mapper.Map<List<ProjectTimetableDto>>(results);
		}

		public async Task<List<ProjectTimetableDto>> GetTimetableByProtocolIdAsync(Guid protocolId)
		{
			var entities = await _unitOfWork.Timetables.GetByProtocolIdAsync(protocolId);
			return _mapper.Map<List<ProjectTimetableDto>>(entities);
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