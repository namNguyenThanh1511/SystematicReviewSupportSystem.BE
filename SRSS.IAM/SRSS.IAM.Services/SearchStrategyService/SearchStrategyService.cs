using AutoMapper;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.SearchStrategy;

namespace SRSS.IAM.Services.SearchStrategyService
{
	public class SearchStrategyService : ISearchStrategyService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public SearchStrategyService(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		// ==================== Search Strategy ====================
		public async Task<SearchStrategyDto> UpsertAsync(SearchStrategyDto dto)
		{
			SearchStrategy entity;

			if (dto.StrategyId.HasValue && dto.StrategyId.Value != Guid.Empty)
			{
				entity = await _unitOfWork.SearchStrategies.FindSingleAsync(s => s.Id == dto.StrategyId.Value)
					?? throw new KeyNotFoundException($"SearchStrategy {dto.StrategyId.Value} không tồn tại");

				entity.Description = dto.Description;
				await _unitOfWork.SearchStrategies.UpdateAsync(entity);
			}
			else
			{
				entity = new SearchStrategy
				{
					ProtocolId = dto.ProtocolId,
					Description = dto.Description
				};
				await _unitOfWork.SearchStrategies.AddAsync(entity);
			}

			await _unitOfWork.SaveChangesAsync();
			return _mapper.Map<SearchStrategyDto>(entity);
		}

		public async Task<List<SearchStrategyDto>> GetAllByProtocolIdAsync(Guid protocolId)
		{
			var entities = await _unitOfWork.SearchStrategies.GetByProtocolIdAsync(protocolId);
			return _mapper.Map<List<SearchStrategyDto>>(entities);
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
				SearchString entity;

				if (dto.SearchStringId.HasValue && dto.SearchStringId.Value != Guid.Empty)
				{
					entity = await _unitOfWork.SearchStrings.FindSingleAsync(s => s.Id == dto.SearchStringId.Value);

					if (entity != null)
					{
						entity.Expression = dto.Expression;
						await _unitOfWork.SearchStrings.UpdateAsync(entity);
					}
					else
					{
						entity = _mapper.Map<SearchString>(dto);
						await _unitOfWork.SearchStrings.AddAsync(entity);
					}
				}
				else
				{
					entity = _mapper.Map<SearchString>(dto);
					await _unitOfWork.SearchStrings.AddAsync(entity);
				}

				results.Add(entity);
			}

			await _unitOfWork.SaveChangesAsync();
			return _mapper.Map<List<SearchStringDto>>(results);
		}

		public async Task<List<SearchStringDto>> GetSearchStringsByStrategyIdAsync(Guid strategyId)
		{
			var entities = await _unitOfWork.SearchStrings.GetByStrategyIdAsync(strategyId);
			return _mapper.Map<List<SearchStringDto>>(entities);
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
						entity.Keyword = dto.Keyword;
						entity.Source = dto.Source;
						await _unitOfWork.SearchTerms.UpdateAsync(entity);
					}
					else
					{
						entity = _mapper.Map<SearchTerm>(dto);
						await _unitOfWork.SearchTerms.AddAsync(entity);
					}
				}
				else
				{
					entity = _mapper.Map<SearchTerm>(dto);
					await _unitOfWork.SearchTerms.AddAsync(entity);
				}

				results.Add(entity);
			}

			await _unitOfWork.SaveChangesAsync();
			return _mapper.Map<List<SearchTermDto>>(results);
		}

		public async Task<List<SearchTermDto>> GetSearchTermsBySearchStringIdAsync(Guid searchStringId)
		{
			var entities = await _unitOfWork.SearchTerms.GetBySearchStringIdAsync(searchStringId);
			return _mapper.Map<List<SearchTermDto>>(entities);
		}

		// ==================== Search String Term (Junction) ====================
		public async Task BulkUpsertSearchStringTermsAsync(List<SearchStringTermDto> dtos)
		{
			foreach (var dto in dtos)
			{
				var exists = await _unitOfWork.SearchStringTerms.ExistsAsync(dto.SearchStringId, dto.TermId);

				if (!exists)
				{
					var entity = new SearchStringTerm
					{
						SearchStringId = dto.SearchStringId,
						TermId = dto.TermId
					};
					await _unitOfWork.SearchStringTerms.AddAsync(entity);
				}
			}

			await _unitOfWork.SaveChangesAsync();
		}

		public async Task<List<SearchStringTermDto>> GetSearchStringTermsBySearchStringIdAsync(Guid searchStringId)
		{
			var entities = await _unitOfWork.SearchStringTerms.GetBySearchStringIdAsync(searchStringId);
			return _mapper.Map<List<SearchStringTermDto>>(entities);
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
						entity.SourceType = dto.SourceType;
						entity.Name = dto.Name;
						await _unitOfWork.SearchSources.UpdateAsync(entity);
					}
					else
					{
						entity = _mapper.Map<SearchSource>(dto);
						await _unitOfWork.SearchSources.AddAsync(entity);
					}
				}
				else
				{
					entity = _mapper.Map<SearchSource>(dto);
					await _unitOfWork.SearchSources.AddAsync(entity);
				}

				results.Add(entity);
			}

			await _unitOfWork.SaveChangesAsync();
			return _mapper.Map<List<SearchSourceDto>>(results);
		}

		public async Task<List<SearchSourceDto>> GetSearchSourcesByProtocolIdAsync(Guid protocolId)
		{
			var entities = await _unitOfWork.SearchSources.GetByProtocolIdAsync(protocolId);
			return _mapper.Map<List<SearchSourceDto>>(entities);
		}
	}
}