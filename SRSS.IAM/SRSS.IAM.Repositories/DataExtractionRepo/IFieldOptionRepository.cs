using SRSS.IAM.Repositories.Entities;
using Shared.Repositories;

namespace SRSS.IAM.Repositories.DataExtractionRepo
{
	public interface IFieldOptionRepository : IGenericRepository<FieldOption, Guid, AppDbContext>
	{
		Task<List<FieldOption>> GetByFieldIdAsync(Guid fieldId);
	}
}