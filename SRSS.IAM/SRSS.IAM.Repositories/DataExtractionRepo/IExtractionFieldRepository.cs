using SRSS.IAM.Repositories.Entities;
using Shared.Repositories;

namespace SRSS.IAM.Repositories.DataExtractionRepo
{
	public interface IExtractionFieldRepository : IGenericRepository<ExtractionField, Guid, AppDbContext>
	{
		Task<List<ExtractionField>> GetByTemplateIdAsync(Guid templateId);
		Task<List<ExtractionField>> GetRootFieldsByTemplateIdAsync(Guid templateId);
	}
}