using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.SelectionCriteria;
using SRSS.IAM.Services.Mappers;

namespace SRSS.IAM.Services.StudyCharacteristicsService
{
	public class StudyCharacteristicsService : IStudyCharacteristicsService
	{
		private readonly IUnitOfWork _unitOfWork;

		public StudyCharacteristicsService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<StudyCharacteristicsDto> UpsertCharacteristicsAsync(Guid protocolId, StudyCharacteristicsDto dto)
		{
			var protocol = await _unitOfWork.Protocols.FindSingleAsync(p => p.Id == protocolId)
				?? throw new KeyNotFoundException($"Protocol with ID {protocolId} not found.");

			var characteristics = await _unitOfWork.StudyCharacteristics.GetByProtocolIdAsync(protocolId);

			if (characteristics == null)
			{
				characteristics = dto.ToEntity(protocolId);
				await _unitOfWork.StudyCharacteristics.AddAsync(characteristics);
			}
			else
			{
				dto.UpdateEntity(characteristics);
				await _unitOfWork.StudyCharacteristics.UpdateAsync(characteristics);
			}

			await _unitOfWork.SaveChangesAsync();
			return characteristics.ToDto();
		}

		public async Task<StudyCharacteristicsDto?> GetByProtocolIdAsync(Guid protocolId)
		{
			var characteristics = await _unitOfWork.StudyCharacteristics.GetByProtocolIdAsync(protocolId);
			return characteristics?.ToDto();
		}

		public async Task DeleteCharacteristicsAsync(Guid protocolId)
		{
			var characteristics = await _unitOfWork.StudyCharacteristics.GetByProtocolIdAsync(protocolId)
				?? throw new KeyNotFoundException($"StudyCharacteristics for protocol {protocolId} not found.");

			await _unitOfWork.StudyCharacteristics.RemoveAsync(characteristics);
			await _unitOfWork.SaveChangesAsync();
		}
	}
}
