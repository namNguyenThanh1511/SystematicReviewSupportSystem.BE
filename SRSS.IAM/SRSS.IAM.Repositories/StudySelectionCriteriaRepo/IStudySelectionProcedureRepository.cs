using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.StudySelectionCriteriaRepo
{
	public interface IStudySelectionProcedureRepository : IGenericRepository<StudySelectionProcedure, Guid, AppDbContext>
	{
		Task<IEnumerable<StudySelectionProcedure>> GetByProtocolIdAsync(Guid protocolId, CancellationToken cancellationToken = default);
	}
}