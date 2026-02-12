using AutoMapper;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.DataExtraction;

namespace SRSS.IAM.Services.DataExtractionService
{
	public class DataExtractionService : IDataExtractionService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public DataExtractionService(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		// ==================== Extraction Strategies ====================
		public async Task<DataExtractionStrategyDto> UpsertStrategyAsync(DataExtractionStrategyDto dto)
		{
			DataExtractionStrategy entity;

			if (dto.ExtractionStrategyId.HasValue && dto.ExtractionStrategyId.Value != Guid.Empty)
			{
				entity = await _unitOfWork.ExtractionStrategies.FindSingleAsync(s => s.Id == dto.ExtractionStrategyId.Value)
					?? throw new KeyNotFoundException($"Strategy {dto.ExtractionStrategyId.Value} không tồn tại");

				_mapper.Map(dto, entity);
				await _unitOfWork.ExtractionStrategies.UpdateAsync(entity);
			}
			else
			{
				entity = _mapper.Map<DataExtractionStrategy>(dto);
				await _unitOfWork.ExtractionStrategies.AddAsync(entity);
			}

			await _unitOfWork.SaveChangesAsync();
			return _mapper.Map<DataExtractionStrategyDto>(entity);
		}

		public async Task<List<DataExtractionStrategyDto>> GetStrategiesByProtocolIdAsync(Guid protocolId)
		{
			var entities = await _unitOfWork.ExtractionStrategies.GetByProtocolIdAsync(protocolId);
			return _mapper.Map<List<DataExtractionStrategyDto>>(entities);
		}

		public async Task DeleteStrategyAsync(Guid strategyId)
		{
			var entity = await _unitOfWork.ExtractionStrategies.FindSingleAsync(s => s.Id == strategyId);
			if (entity != null)
			{
				await _unitOfWork.ExtractionStrategies.RemoveAsync(entity);
				await _unitOfWork.SaveChangesAsync();
			}
		}

		// ==================== Extraction Forms ====================
		public async Task<List<DataExtractionFormDto>> BulkUpsertFormsAsync(List<DataExtractionFormDto> dtos)
		{
			var results = new List<DataExtractionForm>();

			foreach (var dto in dtos)
			{
				DataExtractionForm entity;

				if (dto.FormId.HasValue && dto.FormId.Value != Guid.Empty)
				{
					entity = await _unitOfWork.ExtractionForms.FindSingleAsync(f => f.Id == dto.FormId.Value);

					if (entity != null)
					{
						_mapper.Map(dto, entity);
						await _unitOfWork.ExtractionForms.UpdateAsync(entity);
					}
					else
					{
						entity = _mapper.Map<DataExtractionForm>(dto);
						await _unitOfWork.ExtractionForms.AddAsync(entity);
					}
				}
				else
				{
					entity = _mapper.Map<DataExtractionForm>(dto);
					await _unitOfWork.ExtractionForms.AddAsync(entity);
				}

				results.Add(entity);
			}

			await _unitOfWork.SaveChangesAsync();
			return _mapper.Map<List<DataExtractionFormDto>>(results);
		}

		public async Task<List<DataExtractionFormDto>> GetFormsByStrategyIdAsync(Guid strategyId)
		{
			var entities = await _unitOfWork.ExtractionForms.GetByStrategyIdAsync(strategyId);
			return _mapper.Map<List<DataExtractionFormDto>>(entities);
		}

		// ==================== Data Items ====================
		public async Task<List<DataItemDefinitionDto>> BulkUpsertDataItemsAsync(List<DataItemDefinitionDto> dtos)
		{
			var results = new List<DataItemDefinition>();

			foreach (var dto in dtos)
			{
				DataItemDefinition entity;

				if (dto.DataItemId.HasValue && dto.DataItemId.Value != Guid.Empty)
				{
					entity = await _unitOfWork.DataItems.FindSingleAsync(d => d.Id == dto.DataItemId.Value);

					if (entity != null)
					{
						_mapper.Map(dto, entity);
						await _unitOfWork.DataItems.UpdateAsync(entity);
					}
					else
					{
						entity = _mapper.Map<DataItemDefinition>(dto);
						await _unitOfWork.DataItems.AddAsync(entity);
					}
				}
				else
				{
					entity = _mapper.Map<DataItemDefinition>(dto);
					await _unitOfWork.DataItems.AddAsync(entity);
				}

				results.Add(entity);
			}

			await _unitOfWork.SaveChangesAsync();
			return _mapper.Map<List<DataItemDefinitionDto>>(results);
		}

		public async Task<List<DataItemDefinitionDto>> GetDataItemsByFormIdAsync(Guid formId)
		{
			var entities = await _unitOfWork.DataItems.GetByFormIdAsync(formId);
			return _mapper.Map<List<DataItemDefinitionDto>>(entities);
		}
	}
}