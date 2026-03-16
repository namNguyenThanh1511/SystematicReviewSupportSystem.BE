using SRSS.IAM.Repositories.Entities;
using Shared.Repositories;

namespace SRSS.IAM.Repositories.DataExtractionRepo
{
	public interface IExtractionPaperTaskRepository : IGenericRepository<ExtractionPaperTask, Guid, AppDbContext>
	{
		IQueryable<ExtractionPaperTask> GetTasksByProcessQueryable(Guid processId);
		IQueryable<ExtractionPaperTask> GetQueryable();
	}
}
