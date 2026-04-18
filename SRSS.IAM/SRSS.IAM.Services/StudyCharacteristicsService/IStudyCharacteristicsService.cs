using SRSS.IAM.Services.DTOs.SelectionCriteria;

namespace SRSS.IAM.Services.StudyCharacteristicsService
{
	public interface IStudyCharacteristicsService
	{
		Task<StudyCharacteristicsDto> UpsertCharacteristicsAsync(Guid protocolId, StudyCharacteristicsDto dto);
		Task<StudyCharacteristicsDto?> GetByProtocolIdAsync(Guid protocolId);
		Task DeleteCharacteristicsAsync(Guid protocolId);
	}
}
