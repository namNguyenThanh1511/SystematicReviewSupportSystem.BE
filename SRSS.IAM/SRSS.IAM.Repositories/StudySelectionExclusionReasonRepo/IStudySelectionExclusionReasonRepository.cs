using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.StudySelectionExclusionReasonRepo
{
    public interface IStudySelectionExclusionReasonRepository : IGenericRepository<StudySelectionExclusionReason, Guid, AppDbContext>
    {
    }
}
