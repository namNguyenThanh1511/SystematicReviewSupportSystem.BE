using Microsoft.EntityFrameworkCore;
using SRSS.IAM.Repositories.Entities;
using Shared.Repositories;

namespace SRSS.IAM.Repositories.DataExtractionRepo
{
	public class ExtractionPaperTaskRepository : GenericRepository<ExtractionPaperTask, Guid, AppDbContext>, IExtractionPaperTaskRepository
	{
		public ExtractionPaperTaskRepository(AppDbContext context) : base(context) 
		{
		}

		public IQueryable<ExtractionPaperTask> GetTasksByProcessQueryable(Guid processId)
		{
			return _context.Set<ExtractionPaperTask>()
				.Include(t => t.Paper)
				.Include(t => t.DataExtractionProcess)
				.Where(t => t.DataExtractionProcessId == processId);
		}

		public IQueryable<ExtractionPaperTask> GetQueryable()
		{
			return _context.Set<ExtractionPaperTask>();
		}
	}
}
