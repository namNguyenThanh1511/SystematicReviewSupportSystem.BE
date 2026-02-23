using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.ResearchQuestionRepo
{
	public class ResearchQuestionRepository : GenericRepository<ResearchQuestion, Guid, AppDbContext>, IResearchQuestionRepository
	{
		public ResearchQuestionRepository(AppDbContext context) : base(context) { }

		public async Task<ResearchQuestion?> GetByIdWithDetailsAsync(Guid questionId, CancellationToken cancellationToken = default)
		{
			return await _context.ResearchQuestions
				.Include(q => q.QuestionType)
				.Include(q => q.PicocElements)
					.ThenInclude(p => p.Population)
				.Include(q => q.PicocElements)
					.ThenInclude(p => p.Intervention)
				.Include(q => q.PicocElements)
					.ThenInclude(p => p.Comparison)
				.Include(q => q.PicocElements)
					.ThenInclude(p => p.Outcome)
				.Include(q => q.PicocElements)
					.ThenInclude(p => p.Context)
				.AsNoTracking()
				.FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);
		}

		public async Task<IEnumerable<ResearchQuestion>> GetByProjectIdWithDetailsAsync(Guid projectId, CancellationToken cancellationToken = default)
		{
			return await _context.ResearchQuestions
				.Include(q => q.QuestionType)
				.Include(q => q.PicocElements)
					.ThenInclude(p => p.Population)
				.Include(q => q.PicocElements)
					.ThenInclude(p => p.Intervention)
				.Include(q => q.PicocElements)
					.ThenInclude(p => p.Comparison)
				.Include(q => q.PicocElements)
					.ThenInclude(p => p.Outcome)
				.Include(q => q.PicocElements)
					.ThenInclude(p => p.Context)
				.AsNoTracking()
				.Where(q => q.ProjectId == projectId)
				.ToListAsync(cancellationToken);
		}
	}

	public class PicocElementRepository : GenericRepository<PicocElement, Guid, AppDbContext>, IPicocElementRepository
	{
		public PicocElementRepository(AppDbContext context) : base(context) { }

		public async Task<PicocElement?> GetByIdWithChildrenAsync(Guid picocId, CancellationToken cancellationToken = default)
		{
			return await _context.PicocElements
				.Include(p => p.Population)
				.Include(p => p.Intervention)
				.Include(p => p.Comparison)
				.Include(p => p.Outcome)
				.Include(p => p.Context)
				.AsNoTracking()
				.FirstOrDefaultAsync(p => p.Id == picocId, cancellationToken);
		}

		public async Task<IEnumerable<PicocElement>> GetByResearchQuestionIdAsync(Guid questionId, CancellationToken cancellationToken = default)
		{
			return await _context.PicocElements
				.Include(p => p.Population)
				.Include(p => p.Intervention)
				.Include(p => p.Comparison)
				.Include(p => p.Outcome)
				.Include(p => p.Context)
				.AsNoTracking()
				.Where(p => p.ResearchQuestionId == questionId)
				.ToListAsync(cancellationToken);
		}
	}

	public class PopulationRepository : GenericRepository<Population, Guid, AppDbContext>, IPopulationRepository
	{
		public PopulationRepository(AppDbContext context) : base(context) { }

		public async Task<Population?> GetByPicocIdAsync(Guid picocId, CancellationToken cancellationToken = default)
		{
			return await FindSingleAsync(p => p.PicocId == picocId, isTracking: false, cancellationToken);
		}
	}

	public class InterventionRepository : GenericRepository<Intervention, Guid, AppDbContext>, IInterventionRepository
	{
		public InterventionRepository(AppDbContext context) : base(context) { }

		public async Task<Intervention?> GetByPicocIdAsync(Guid picocId, CancellationToken cancellationToken = default)
		{
			return await FindSingleAsync(i => i.PicocId == picocId, isTracking: false, cancellationToken);
		}
	}

	public class ComparisonRepository : GenericRepository<Comparison, Guid, AppDbContext>, IComparisonRepository
	{
		public ComparisonRepository(AppDbContext context) : base(context) { }

		public async Task<Comparison?> GetByPicocIdAsync(Guid picocId, CancellationToken cancellationToken = default)
		{
			return await FindSingleAsync(c => c.PicocId == picocId, isTracking: false, cancellationToken);
		}
	}

	public class OutcomeRepository : GenericRepository<Outcome, Guid, AppDbContext>, IOutcomeRepository
	{
		public OutcomeRepository(AppDbContext context) : base(context) { }

		public async Task<Outcome?> GetByPicocIdAsync(Guid picocId, CancellationToken cancellationToken = default)
		{
			return await FindSingleAsync(o => o.PicocId == picocId, isTracking: false, cancellationToken);
		}
	}

	public class ContextRepository : GenericRepository<Context, Guid, AppDbContext>, IContextRepository
	{
		public ContextRepository(AppDbContext context) : base(context) { }

		public async Task<Context?> GetByPicocIdAsync(Guid picocId, CancellationToken cancellationToken = default)
		{
			return await FindSingleAsync(c => c.PicocId == picocId, isTracking: false, cancellationToken);
		}
	}
}