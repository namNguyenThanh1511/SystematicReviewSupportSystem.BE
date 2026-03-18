using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.PaperCitationRepo
{
    public interface IPaperCitationRepository : IGenericRepository<PaperCitation, Guid, AppDbContext>
    {
        Task<int> CountByTargetAsync(Guid targetPaperId, CancellationToken cancellationToken = default);
        Task<int> CountBySourceAsync(Guid sourcePaperId, CancellationToken cancellationToken = default);
        Task<List<PaperCitation>> GetCitationsWithSourcePaperAsync(Guid targetPaperId, CancellationToken cancellationToken = default);
        Task<List<PaperCitation>> GetReferencesWithTargetPaperAsync(Guid sourcePaperId, CancellationToken cancellationToken = default);
        Task<List<PaperCitation>> GetEdgesBySourcesAsync(IEnumerable<Guid> sourcePaperIds, decimal minConfidence, CancellationToken cancellationToken = default);
    }
}
