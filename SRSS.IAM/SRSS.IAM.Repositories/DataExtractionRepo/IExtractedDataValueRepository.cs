using SRSS.IAM.Repositories.Entities;
using Shared.Repositories;

namespace SRSS.IAM.Repositories.DataExtractionRepo
{
	public interface IExtractedDataValueRepository : IGenericRepository<ExtractedDataValue, Guid, AppDbContext>
	{
		Task<List<ExtractedDataValue>> GetByPaperIdAsync(Guid paperId);
		Task<List<ExtractedDataValue>> GetByFieldIdAsync(Guid fieldId);
	}
}