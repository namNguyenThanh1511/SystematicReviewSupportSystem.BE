using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.CandidatePaperRepo
{
    public interface ICandidatePaperRepository : IGenericRepository<CandidatePaper, Guid, AppDbContext>
    {
        IQueryable<CandidatePaper> GetCandidatesQueryable();

        Task<List<CandidatePaper>> GetCandidatePapersByPaperId(Guid paperId, CancellationToken ct);
    }
}
