using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.SearchStrategy;
using SRSS.IAM.Services.Mappers;

namespace SRSS.IAM.Services.SearchStrategyService
{
	public class SearchStrategyService : ISearchStrategyService
	{
		private readonly IUnitOfWork _unitOfWork;

		public SearchStrategyService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		// ==================== Search Strategy ====================
		public async Task<SearchStrategyDto> UpsertAsync(SearchStrategyDto dto)
		{
			SearchStrategy entity;

			if (dto.StrategyId.HasValue && dto.StrategyId.Value != Guid.Empty)
			{
				entity = await _unitOfWork.SearchStrategies.FindSingleAsync(s => s.Id == dto.StrategyId.Value)
					?? throw new KeyNotFoundException($"SearchStrategy {dto.StrategyId.Value} không tồn tại");

				dto.UpdateEntity(entity);  
				await _unitOfWork.SearchStrategies.UpdateAsync(entity);
			}
			else
			{
				entity = dto.ToEntity();  
				await _unitOfWork.SearchStrategies.AddAsync(entity);
			}

			await _unitOfWork.SaveChangesAsync();
			return entity.ToDto();  
		}

		public async Task<List<SearchStrategyDto>> GetAllByProtocolIdAsync(Guid protocolId)
		{
			var entities = await _unitOfWork.SearchStrategies.GetByProtocolIdAsync(protocolId);
			return entities.ToDtoList();  
		}

		public async Task DeleteAsync(Guid strategyId)
		{
			var entity = await _unitOfWork.SearchStrategies.FindSingleAsync(s => s.Id == strategyId);
			if (entity != null)
			{
				await _unitOfWork.SearchStrategies.RemoveAsync(entity);
				await _unitOfWork.SaveChangesAsync();
			}
		}

		// ==================== Search String ====================
		public async Task<List<SearchStringDto>> BulkUpsertSearchStringsAsync(List<SearchStringDto> dtos)
		{
			var results = new List<SearchString>();

			foreach (var dto in dtos)
			{
				var strategyExists = await _unitOfWork.SearchStrategies.AnyAsync(s => s.Id == dto.StrategyId);
				if (!strategyExists)
				{
					throw new KeyNotFoundException($"Strategy với ID {dto.StrategyId} không tồn tại");
				}

				SearchString entity;

				if (dto.SearchStringId.HasValue && dto.SearchStringId.Value != Guid.Empty)
				{
					entity = await _unitOfWork.SearchStrings.FindSingleAsync(s => s.Id == dto.SearchStringId.Value);

					if (entity != null)
					{
						dto.UpdateEntity(entity);  
						await _unitOfWork.SearchStrings.UpdateAsync(entity);
					}
					else
					{
						entity = dto.ToEntity();  
						await _unitOfWork.SearchStrings.AddAsync(entity);
					}
				}
				else
				{
					entity = dto.ToEntity();  
					await _unitOfWork.SearchStrings.AddAsync(entity);
				}

				results.Add(entity);
			}

			await _unitOfWork.SaveChangesAsync();
			return results.ToDtoList();  
		}

		public async Task<List<SearchStringDto>> GetSearchStringsByStrategyIdAsync(Guid strategyId)
		{
			var entities = await _unitOfWork.SearchStrings.GetByStrategyIdAsync(strategyId);
			return entities.ToDtoList();  
		}

		// ==================== Search Term ====================
		public async Task<List<SearchTermDto>> BulkUpsertSearchTermsAsync(List<SearchTermDto> dtos)
		{
			var results = new List<SearchTerm>();

			foreach (var dto in dtos)
			{
				SearchTerm entity;

				if (dto.TermId.HasValue && dto.TermId.Value != Guid.Empty)
				{
					entity = await _unitOfWork.SearchTerms.FindSingleAsync(t => t.Id == dto.TermId.Value);

					if (entity != null)
					{
						dto.UpdateEntity(entity);  
						await _unitOfWork.SearchTerms.UpdateAsync(entity);
					}
					else
					{
						entity = dto.ToEntity();  
						await _unitOfWork.SearchTerms.AddAsync(entity);
					}
				}
				else
				{
					entity = dto.ToEntity();  
					await _unitOfWork.SearchTerms.AddAsync(entity);
				}

				results.Add(entity);
			}

			await _unitOfWork.SaveChangesAsync();
			return results.ToDtoList();  
		}

		public async Task<List<SearchTermDto>> GetSearchTermsBySearchStringIdAsync(Guid searchStringId)
		{
			var entities = await _unitOfWork.SearchTerms.GetBySearchStringIdAsync(searchStringId);
			return entities.ToDtoList();  
		}

		// ==================== Search String Term (Junction) ====================
		public async Task BulkUpsertSearchStringTermsAsync(List<SearchStringTermDto> dtos)
		{
			foreach (var dto in dtos)
			{
				var exists = await _unitOfWork.SearchStringTerms.ExistsAsync(dto.SearchStringId, dto.TermId);

				if (!exists)
				{
					var entity = dto.ToEntity();  
					await _unitOfWork.SearchStringTerms.AddAsync(entity);
				}
			}

			await _unitOfWork.SaveChangesAsync();
		}

		public async Task<List<SearchStringTermDto>> GetSearchStringTermsBySearchStringIdAsync(Guid searchStringId)
		{
			var entities = await _unitOfWork.SearchStringTerms.GetBySearchStringIdAsync(searchStringId);
			return entities.ToDtoList();  
		}

		// ==================== Search Source ====================
		public async Task<List<SearchSourceDto>> BulkUpsertSearchSourcesAsync(List<SearchSourceDto> dtos)
		{
			var results = new List<SearchSource>();

			foreach (var dto in dtos)
			{
				SearchSource entity;

				if (dto.SourceId.HasValue && dto.SourceId.Value != Guid.Empty)
				{
					entity = await _unitOfWork.SearchSources.FindSingleAsync(s => s.Id == dto.SourceId.Value);

					if (entity != null)
					{
						dto.UpdateEntity(entity);  
						await _unitOfWork.SearchSources.UpdateAsync(entity);
					}
					else
					{
						entity = dto.ToEntity();  
						await _unitOfWork.SearchSources.AddAsync(entity);
					}
				}
				else
				{
					entity = dto.ToEntity();  
					await _unitOfWork.SearchSources.AddAsync(entity);
				}

				results.Add(entity);
			}

			await _unitOfWork.SaveChangesAsync();
			return results.ToDtoList();  
		}

		public async Task<List<SearchSourceDto>> GetSearchSourcesByProtocolIdAsync(Guid protocolId)
		{
			var entities = await _unitOfWork.SearchSources.GetByProtocolIdAsync(protocolId);
			return entities.ToDtoList();  
		}
	}
}