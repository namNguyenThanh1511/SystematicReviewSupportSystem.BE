using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.ProtocolRepo
{
	public class ReviewProtocolRepository : GenericRepository<ReviewProtocol, Guid, AppDbContext>, IReviewProtocolRepository
	{
		public ReviewProtocolRepository(AppDbContext context) : base(context) { }

		public async Task<ReviewProtocol?> GetByIdWithVersionsAsync(Guid protocolId, CancellationToken cancellationToken = default)
		{
			return await _context.ReviewProtocols
				.Include(p => p.Versions)
				.AsNoTracking()
				.FirstOrDefaultAsync(p => p.Id == protocolId, cancellationToken);
		}

		public async Task<IEnumerable<ReviewProtocol>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
		{
			return await FindAllAsync(p => p.ProjectId == projectId, isTracking: false, cancellationToken);
		}
	}

	public class ProtocolVersionRepository : GenericRepository<ProtocolVersion, Guid, AppDbContext>, IProtocolVersionRepository
	{
		public ProtocolVersionRepository(AppDbContext context) : base(context) { }

		public async Task<IEnumerable<ProtocolVersion>> GetByProtocolIdAsync(Guid protocolId, CancellationToken cancellationToken = default)
		{
			return await FindAllAsync(v => v.ProtocolId == protocolId, isTracking: false, cancellationToken);
		}
	}

	public class ProtocolEvaluationRepository : GenericRepository<ProtocolEvaluation, Guid, AppDbContext>, IProtocolEvaluationRepository
	{
		public ProtocolEvaluationRepository(AppDbContext context) : base(context) { }

		public async Task<IEnumerable<ProtocolEvaluation>> GetByProtocolIdAsync(Guid protocolId, CancellationToken cancellationToken = default)
		{
			return await FindAllAsync(e => e.ProtocolId == protocolId, isTracking: false, cancellationToken);
		}
	}
}