using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.StuSeExclusionCodeRepo
{
    public class StuSeExclusionCodeRepository : GenericRepository<StudySelectionExclusionReason, Guid, AppDbContext>, IStuSeExclusionCodeRepository
    {
        public StuSeExclusionCodeRepository(AppDbContext context) : base(context)
        {
        }
    }
}
