using SRSS.IAM.Repositories.Entities;
using Shared.Repositories;

namespace SRSS.IAM.Repositories.DataExtractionRepo
{
    public interface IExtractionTemplateRepository : IGenericRepository<ExtractionTemplate, Guid, AppDbContext>
    {
        Task<List<ExtractionTemplate>> GetByProtocolIdAsync(Guid protocolId);
        Task<ExtractionTemplate?> GetByIdWithFieldsAsync(Guid templateId);
    }
}