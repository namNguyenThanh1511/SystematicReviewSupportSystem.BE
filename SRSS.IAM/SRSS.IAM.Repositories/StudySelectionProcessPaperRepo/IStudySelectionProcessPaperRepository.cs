using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.StudySelectionProcessPaperRepo
{
    public interface IStudySelectionProcessPaperRepository : IGenericRepository<StudySelectionProcessPaper, Guid, AppDbContext>
    {
        Task DeleteByProcessAsync(Guid processId, CancellationToken cancellationToken = default);
    }
}
