using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories;

namespace SRSS.IAM.Repositories.StudyCharacteristicsRepo
{
	public interface IStudyCharacteristicsRepository : IGenericRepository<StudyCharacteristics, Guid, AppDbContext>
	{
		Task<StudyCharacteristics?> GetByProtocolIdAsync(Guid protocolId, CancellationToken cancellationToken = default);
	}
}
