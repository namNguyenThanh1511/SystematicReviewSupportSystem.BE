using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.StudySelectionProcessPaperRepo
{
    public interface IStudySelectionProcessPaperRepository : IGenericRepository<StudySelectionProcessPaper, Guid, AppDbContext>
    {
        Task DeleteByProcessAsync(Guid processId, CancellationToken cancellationToken = default);
        Task<(List<StudySelectionProcessPaper> Items, int TotalCount)> GetWithPaperByProcessAsync(
            Guid processId, 
            string? search = null, 
            int pageNumber = 1, 
            int pageSize = 10, 
            CancellationToken cancellationToken = default);
    }
}
