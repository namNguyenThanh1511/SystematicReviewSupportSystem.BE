using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.DataExtractionRepo
{
	public interface IExtractionSectionRepository : IGenericRepository<ExtractionSection, Guid, AppDbContext>
	{
	}
}
