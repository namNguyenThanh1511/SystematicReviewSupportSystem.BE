using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.StudySelectionCriteriaRepo
{
	public class StudySelectionProcedureRepository : GenericRepository<StudySelectionProcedure, Guid, AppDbContext>, IStudySelectionProcedureRepository
	{
		public StudySelectionProcedureRepository(AppDbContext context) : base(context) { }

		public async Task<IEnumerable<StudySelectionProcedure>> GetByProtocolIdAsync(Guid protocolId, CancellationToken cancellationToken = default)
		{
			return await FindAllAsync(p => p.ProtocolId == protocolId, isTracking: false, cancellationToken);
		}
	}
}