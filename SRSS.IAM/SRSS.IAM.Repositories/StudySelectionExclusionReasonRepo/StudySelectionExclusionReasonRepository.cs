using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.StudySelectionExclusionReasonRepo
{
    public class StudySelectionExclusionReasonRepository : GenericRepository<StudySelectionExclusionReason, Guid, AppDbContext>, IStudySelectionExclusionReasonRepository
    {
        public StudySelectionExclusionReasonRepository(AppDbContext context) : base(context)
        {
        }
    }
}
