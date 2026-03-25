using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.DataExtractionRepo
{
	public class ExtractionSectionRepository : GenericRepository<ExtractionSection, Guid, AppDbContext>, IExtractionSectionRepository
	{
		public ExtractionSectionRepository(AppDbContext context) : base(context) { }
	}
}
