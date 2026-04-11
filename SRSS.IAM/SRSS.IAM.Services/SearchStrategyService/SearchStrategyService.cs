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


		// ==================== Search Source ====================
		public async Task<List<SearchSourceDto>> BulkUpsertSearchSourcesAsync(List<SearchSourceDto> dtos)
		{
			var results = new List<SearchSource>();

			foreach (var dto in dtos)
			{
				if (dto.MasterSourceId.HasValue)
				{
					// Validate master source existence
					var masterSource = await _unitOfWork.MasterSearchSources.FindSingleOrDefaultAsync(m => m.Id == dto.MasterSourceId.Value);
					if (masterSource == null)
					{
						throw new InvalidOperationException($"Master source with ID {dto.MasterSourceId} not found.");
					}

					// Use master source name if not provided
					if (string.IsNullOrWhiteSpace(dto.Name))
					{
						dto.Name = masterSource.SourceName;
					}
				}

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