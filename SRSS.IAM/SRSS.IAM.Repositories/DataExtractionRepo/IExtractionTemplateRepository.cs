using SRSS.IAM.Repositories.Entities;
using Shared.Repositories;

namespace SRSS.IAM.Repositories.DataExtractionRepo
{
    public interface IExtractionTemplateRepository : IGenericRepository<ExtractionTemplate, Guid, AppDbContext>
    {
        Task<List<ExtractionTemplate>> GetByProcessIdAsync(Guid processId);
        Task<ExtractionTemplate?> GetByIdWithFieldsAsync(Guid templateId);
    }
}