using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.SearchStrategy;
using SRSS.IAM.Services.Mappers;
using SRSS.IAM.Services.UserService;
using Shared.Exceptions;

namespace SRSS.IAM.Services.SearchStrategyService
{
	public class SearchStrategyService : ISearchStrategyService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ICurrentUserService _currentUserService;

		public SearchStrategyService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
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


		// ==================== Search Source ====================
		public async Task<List<SearchSourceDto>> BulkUpsertSearchSourcesAsync(List<SearchSourceDto> dtos)
		{
			if (dtos == null || !dtos.Any())
			{
				return new List<SearchSourceDto>();
			}

			await EnsureLeaderAsync(dtos.First().ProjectId);

			var results = new List<SearchSource>();

			foreach (var dto in dtos)
			{
				var entity = await ProcessUpsertSourceAsync(dto);
				results.Add(entity);
			}

			await _unitOfWork.SaveChangesAsync();
			return results.ToDtoList();
		}

		public async Task<SearchSourceDto> AddSearchSourceAsync(SearchSourceDto dto)
		{
			await EnsureLeaderAsync(dto.ProjectId);
			var entity = await ProcessUpsertSourceAsync(dto);
			await _unitOfWork.SaveChangesAsync();
			return entity.ToDto();
		}

		public async Task<SearchSourceDto> UpdateSearchStrategiesAsync(Guid sourceId, List<SearchStrategyDto> strategies)
		{
			var entity = await _unitOfWork.SearchSources.GetByIdWithStrategiesAsync(sourceId);
			if (entity == null)
			{
				throw new InvalidOperationException($"Search source with ID {sourceId} not found.");
			}

			await EnsureLeaderAsync(entity.ProjectId);

			// 1. Remove strategies not in the incoming list
			var incomingIds = strategies
				.Where(s => s.Id.HasValue && s.Id != Guid.Empty)
				.Select(s => s.Id!.Value)
				.ToList();

			var toRemove = entity.Strategies
				.Where(s => !incomingIds.Contains(s.Id))
				.ToList();

			foreach (var s in toRemove)
			{
				entity.Strategies.Remove(s);
			}

			// 2. Add or Update
			foreach (var sDto in strategies)
			{
				if (sDto.Id.HasValue && sDto.Id != Guid.Empty)
				{
					var sEntity = entity.Strategies.FirstOrDefault(x => x.Id == sDto.Id);
					if (sEntity != null)
					{
						sDto.UpdateEntity(sEntity);
					}
				}
				else
				{
					var newStrategy = sDto.ToEntity();
					entity.Strategies.Add(newStrategy);
				}
			}

			await _unitOfWork.SearchSources.UpdateAsync(entity);
			await _unitOfWork.SaveChangesAsync();

			// Reload to get master source info if needed for response mapping
			if (entity.MasterSourceId.HasValue && entity.MasterSource == null)
			{
				entity.MasterSource = await _unitOfWork.MasterSearchSources.FindSingleOrDefaultAsync(m => m.Id == entity.MasterSourceId.Value);
			}

			return entity.ToDto();
		}

		private async Task<SearchSource> ProcessUpsertSourceAsync(SearchSourceDto dto)
		{
			MasterSearchSources? masterSource = null;
			if (dto.MasterSourceId.HasValue)
			{
				masterSource = await _unitOfWork.MasterSearchSources.FindSingleOrDefaultAsync(m => m.Id == dto.MasterSourceId.Value);
				if (masterSource == null)
				{
					throw new InvalidOperationException($"Master source with ID {dto.MasterSourceId} not found.");
				}

				if (string.IsNullOrWhiteSpace(dto.Name))
				{
					dto.Name = masterSource.SourceName;
				}
			}

			SearchSource? entity = null;

			if (dto.SourceId.HasValue && dto.SourceId.Value != Guid.Empty)
			{
				entity = await _unitOfWork.SearchSources.GetByIdWithStrategiesAsync(dto.SourceId.Value);

				if (entity != null)
				{
					dto.UpdateEntity(entity);

					// Sync strategies
					if (dto.Strategies != null)
					{
						foreach (var sDto in dto.Strategies)
						{
							if (sDto.Id.HasValue && sDto.Id != Guid.Empty)
							{
								var sEntity = entity.Strategies.FirstOrDefault(x => x.Id == sDto.Id);
								if (sEntity != null)
								{
									sDto.UpdateEntity(sEntity);
								}
							}
							else
							{
								var newStrategy = sDto.ToEntity();
								entity.Strategies.Add(newStrategy);
							}
						}
					}
					await _unitOfWork.SearchSources.UpdateAsync(entity);
				}
			}

			if (entity == null)
			{
				entity = dto.ToEntity();
				await _unitOfWork.SearchSources.AddAsync(entity);
			}

			// Ensure master source is attached for URL mapping in response
			if (masterSource != null)
			{
				entity.MasterSource = masterSource;
			}

			return entity;
		}

		public async Task<List<SearchSourceDto>> GetSearchSourcesByProjectIdAsync(Guid projectId)
		{
			var entities = await _unitOfWork.SearchSources.GetByProjectIdAsync(projectId);
			return entities.ToDtoList();
		}
	}
}