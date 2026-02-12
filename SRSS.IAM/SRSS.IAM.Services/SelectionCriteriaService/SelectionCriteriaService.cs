using AutoMapper;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.SelectionCriteria;

namespace SRSS.IAM.Services.SelectionCriteriaService
{
	public class SelectionCriteriaService : ISelectionCriteriaService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public SelectionCriteriaService(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		// ==================== Study Selection Criteria ====================
		public async Task<StudySelectionCriteriaDto> UpsertCriteriaAsync(StudySelectionCriteriaDto dto)
		{
			StudySelectionCriteria entity;

			if (dto.CriteriaId.HasValue && dto.CriteriaId.Value != Guid.Empty)
			{
				entity = await _unitOfWork.SelectionCriterias.FindSingleAsync(c => c.Id == dto.CriteriaId.Value)
					?? throw new KeyNotFoundException($"Criteria {dto.CriteriaId.Value} không tồn tại");

				_mapper.Map(dto, entity);
				await _unitOfWork.SelectionCriterias.UpdateAsync(entity);
			}
			else
			{
				entity = _mapper.Map<StudySelectionCriteria>(dto);
				await _unitOfWork.SelectionCriterias.AddAsync(entity);
			}

			await _unitOfWork.SaveChangesAsync();
			return _mapper.Map<StudySelectionCriteriaDto>(entity);
		}

		public async Task<List<StudySelectionCriteriaDto>> GetAllByProtocolIdAsync(Guid protocolId)
		{
			var entities = await _unitOfWork.SelectionCriterias.GetByProtocolIdAsync(protocolId);
			return _mapper.Map<List<StudySelectionCriteriaDto>>(entities);
		}

		public async Task DeleteCriteriaAsync(Guid criteriaId)
		{
			var entity = await _unitOfWork.SelectionCriterias.FindSingleAsync(c => c.Id == criteriaId);
			if (entity != null)
			{
				await _unitOfWork.SelectionCriterias.RemoveAsync(entity);
				await _unitOfWork.SaveChangesAsync();
			}
		}

		// ==================== Inclusion Criteria ====================
		public async Task<List<InclusionCriterionDto>> BulkUpsertInclusionCriteriaAsync(List<InclusionCriterionDto> dtos)
		{
			var results = new List<InclusionCriterion>();

			foreach (var dto in dtos)
			{
				InclusionCriterion entity;

				if (dto.InclusionId.HasValue && dto.InclusionId.Value != Guid.Empty)
				{
					entity = await _unitOfWork.InclusionCriteria.FindSingleAsync(c => c.Id == dto.InclusionId.Value);

					if (entity != null)
					{
						_mapper.Map(dto, entity);
						await _unitOfWork.InclusionCriteria.UpdateAsync(entity);
					}
					else
					{
						entity = _mapper.Map<InclusionCriterion>(dto);
						await _unitOfWork.InclusionCriteria.AddAsync(entity);
					}
				}
				else
				{
					entity = _mapper.Map<InclusionCriterion>(dto);
					await _unitOfWork.InclusionCriteria.AddAsync(entity);
				}

				results.Add(entity);
			}

			await _unitOfWork.SaveChangesAsync();
			return _mapper.Map<List<InclusionCriterionDto>>(results);
		}

		public async Task<List<InclusionCriterionDto>> GetInclusionByCriteriaIdAsync(Guid criteriaId)
		{
			var entities = await _unitOfWork.InclusionCriteria.GetByCriteriaIdAsync(criteriaId);
			return _mapper.Map<List<InclusionCriterionDto>>(entities);
		}

		// ==================== Exclusion Criteria ====================
		public async Task<List<ExclusionCriterionDto>> BulkUpsertExclusionCriteriaAsync(List<ExclusionCriterionDto> dtos)
		{
			var results = new List<ExclusionCriterion>();

			foreach (var dto in dtos)
			{
				ExclusionCriterion entity;

				if (dto.ExclusionId.HasValue && dto.ExclusionId.Value != Guid.Empty)
				{
					entity = await _unitOfWork.ExclusionCriteria.FindSingleAsync(c => c.Id == dto.ExclusionId.Value);

					if (entity != null)
					{
						_mapper.Map(dto, entity);
						await _unitOfWork.ExclusionCriteria.UpdateAsync(entity);
					}
					else
					{
						entity = _mapper.Map<ExclusionCriterion>(dto);
						await _unitOfWork.ExclusionCriteria.AddAsync(entity);
					}
				}
				else
				{
					entity = _mapper.Map<ExclusionCriterion>(dto);
					await _unitOfWork.ExclusionCriteria.AddAsync(entity);
				}

				results.Add(entity);
			}

			await _unitOfWork.SaveChangesAsync();
			return _mapper.Map<List<ExclusionCriterionDto>>(results);
		}

		public async Task<List<ExclusionCriterionDto>> GetExclusionByCriteriaIdAsync(Guid criteriaId)
		{
			var entities = await _unitOfWork.ExclusionCriteria.GetByCriteriaIdAsync(criteriaId);
			return _mapper.Map<List<ExclusionCriterionDto>>(entities);
		}

		// ==================== Selection Procedures ====================
		public async Task<StudySelectionProcedureDto> UpsertProcedureAsync(StudySelectionProcedureDto dto)
		{
			StudySelectionProcedure entity;

			if (dto.ProcedureId.HasValue && dto.ProcedureId.Value != Guid.Empty)
			{
				entity = await _unitOfWork.SelectionProcedures.FindSingleAsync(p => p.Id == dto.ProcedureId.Value)
					?? throw new KeyNotFoundException($"Procedure {dto.ProcedureId.Value} không tồn tại");

				_mapper.Map(dto, entity);
				await _unitOfWork.SelectionProcedures.UpdateAsync(entity);
			}
			else
			{
				entity = _mapper.Map<StudySelectionProcedure>(dto);
				await _unitOfWork.SelectionProcedures.AddAsync(entity);
			}

			await _unitOfWork.SaveChangesAsync();
			return _mapper.Map<StudySelectionProcedureDto>(entity);
		}

		public async Task<List<StudySelectionProcedureDto>> GetProceduresByProtocolIdAsync(Guid protocolId)
		{
			var entities = await _unitOfWork.SelectionProcedures.GetByProtocolIdAsync(protocolId);
			return _mapper.Map<List<StudySelectionProcedureDto>>(entities);
		}

		public async Task DeleteProcedureAsync(Guid procedureId)
		{
			var entity = await _unitOfWork.SelectionProcedures.FindSingleAsync(p => p.Id == procedureId);
			if (entity != null)
			{
				await _unitOfWork.SelectionProcedures.RemoveAsync(entity);
				await _unitOfWork.SaveChangesAsync();
			}
		}
	}
}