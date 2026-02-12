using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.ProtocolRepo
{
	public interface IReviewProtocolRepository : IGenericRepository<ReviewProtocol, Guid, AppDbContext>
	{
		Task<ReviewProtocol?> GetByIdWithVersionsAsync(Guid protocolId, CancellationToken cancellationToken = default);
		Task<IEnumerable<ReviewProtocol>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
	}

	public interface IProtocolVersionRepository : IGenericRepository<ProtocolVersion, Guid, AppDbContext>
	{
		Task<IEnumerable<ProtocolVersion>> GetByProtocolIdAsync(Guid protocolId, CancellationToken cancellationToken = default);
	}

	public interface IProtocolEvaluationRepository : IGenericRepository<ProtocolEvaluation, Guid, AppDbContext>
	{
		Task<IEnumerable<ProtocolEvaluation>> GetByProtocolIdAsync(Guid protocolId, CancellationToken cancellationToken = default);
	}
}