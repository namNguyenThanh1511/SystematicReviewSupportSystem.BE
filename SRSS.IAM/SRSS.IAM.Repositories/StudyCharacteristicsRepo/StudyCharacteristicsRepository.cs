using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.StudyCharacteristicsRepo
{
	public class StudyCharacteristicsRepository : GenericRepository<StudyCharacteristics, Guid, AppDbContext>, IStudyCharacteristicsRepository
	{
		public StudyCharacteristicsRepository(AppDbContext context) : base(context) { }

		public async Task<StudyCharacteristics?> GetByProtocolIdAsync(Guid protocolId, CancellationToken cancellationToken = default)
		{
			return await _context.StudyCharacteristics
				.FirstOrDefaultAsync(sc => sc.ProtocolId == protocolId, cancellationToken);
		}
	}
}
