using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.DataExtractionRepo
{
	public class ExtractionMatrixColumnRepository : GenericRepository<ExtractionMatrixColumn, Guid, AppDbContext>, IExtractionMatrixColumnRepository
	{
		public ExtractionMatrixColumnRepository(AppDbContext context) : base(context)
		{
		}
	}
}
