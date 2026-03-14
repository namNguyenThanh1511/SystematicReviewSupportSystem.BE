using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.ResearchQuestionRepo
{
	public interface IResearchQuestionRepository : IGenericRepository<ResearchQuestion, Guid, AppDbContext>
	{
		Task<ResearchQuestion?> GetByIdWithDetailsAsync(Guid questionId, CancellationToken cancellationToken = default);
		Task<IEnumerable<ResearchQuestion>> GetByProjectIdWithDetailsAsync(Guid projectId, CancellationToken cancellationToken = default);
	}

	public interface IPicocElementRepository : IGenericRepository<PicocElement, Guid, AppDbContext>
	{
		Task<PicocElement?> GetByIdWithChildrenAsync(Guid picocId, CancellationToken cancellationToken = default);
		Task<IEnumerable<PicocElement>> GetByResearchQuestionIdAsync(Guid questionId, CancellationToken cancellationToken = default);
	}

	public interface IPopulationRepository : IGenericRepository<Population, Guid, AppDbContext>
	{
		Task<Population?> GetByPicocIdAsync(Guid picocId, CancellationToken cancellationToken = default);
	}

	public interface IInterventionRepository : IGenericRepository<Intervention, Guid, AppDbContext>
	{
		Task<Intervention?> GetByPicocIdAsync(Guid picocId, CancellationToken cancellationToken = default);
	}

	public interface IComparisonRepository : IGenericRepository<Comparison, Guid, AppDbContext>
	{
		Task<Comparison?> GetByPicocIdAsync(Guid picocId, CancellationToken cancellationToken = default);
	}

	public interface IOutcomeRepository : IGenericRepository<Outcome, Guid, AppDbContext>
	{
		Task<Outcome?> GetByPicocIdAsync(Guid picocId, CancellationToken cancellationToken = default);
	}

	public interface IContextRepository : IGenericRepository<Context, Guid, AppDbContext>
	{
		Task<Context?> GetByPicocIdAsync(Guid picocId, CancellationToken cancellationToken = default);
	}
}