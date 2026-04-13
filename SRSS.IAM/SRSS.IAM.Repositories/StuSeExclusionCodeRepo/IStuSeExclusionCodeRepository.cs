using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.StuSeExclusionCodeRepo
{
    public interface IStuSeExclusionCodeRepository : IGenericRepository<StudySelectionExclusionReason, Guid, AppDbContext>
    {
    }
}
