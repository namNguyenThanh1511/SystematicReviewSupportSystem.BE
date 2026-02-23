using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.CoreGovernRepo
{
	public interface IReviewNeedRepository : IGenericRepository<ReviewNeed, Guid, AppDbContext>
	{
		Task<IEnumerable<ReviewNeed>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
	}

	public interface ICommissioningDocumentRepository : IGenericRepository<CommissioningDocument, Guid, AppDbContext>
	{
		Task<CommissioningDocument?> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
	}

	public interface IReviewObjectiveRepository : IGenericRepository<ReviewObjective, Guid, AppDbContext>
	{
		Task<IEnumerable<ReviewObjective>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
	}

	public interface IQuestionTypeRepository : IGenericRepository<QuestionType, Guid, AppDbContext>
	{
		Task<QuestionType?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
		Task<IEnumerable<QuestionType>> GetAllWithQuestionsAsync(CancellationToken cancellationToken = default);
	}

	public class ReviewNeedRepository : GenericRepository<ReviewNeed, Guid, AppDbContext>, IReviewNeedRepository
	{
		public ReviewNeedRepository(AppDbContext context) : base(context) { }

		public async Task<IEnumerable<ReviewNeed>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
		{
			return await FindAllAsync(r => r.ProjectId == projectId, isTracking: false, cancellationToken);
		}
	}

	public class CommissioningDocumentRepository : GenericRepository<CommissioningDocument, Guid, AppDbContext>, ICommissioningDocumentRepository
	{
		public CommissioningDocumentRepository(AppDbContext context) : base(context) { }

		public async Task<CommissioningDocument?> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
		{
			return await FindSingleAsync(c => c.ProjectId == projectId, isTracking: false, cancellationToken);
		}
	}

	public class ReviewObjectiveRepository : GenericRepository<ReviewObjective, Guid, AppDbContext>, IReviewObjectiveRepository
	{
		public ReviewObjectiveRepository(AppDbContext context) : base(context) { }

		public async Task<IEnumerable<ReviewObjective>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
		{
			return await FindAllAsync(r => r.ProjectId == projectId, isTracking: false, cancellationToken);
		}
	}

	public class QuestionTypeRepository : GenericRepository<QuestionType, Guid, AppDbContext>, IQuestionTypeRepository
	{
		public QuestionTypeRepository(AppDbContext context) : base(context) { }

		public async Task<QuestionType?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
		{
			return await FindSingleAsync(q => q.Name == name, isTracking: false, cancellationToken);
		}

		public async Task<IEnumerable<QuestionType>> GetAllWithQuestionsAsync(CancellationToken cancellationToken = default)
		{
			return await _context.QuestionTypes
				.Include(q => q.ResearchQuestions)
				.AsNoTracking()
				.ToListAsync(cancellationToken);
		}
	}
}
