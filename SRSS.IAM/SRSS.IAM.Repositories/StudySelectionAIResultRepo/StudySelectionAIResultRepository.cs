using Shared.Repositories;
using Microsoft.EntityFrameworkCore;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.Repositories.StudySelectionAIResultRepo
{
    public class StudySelectionAIResultRepository : GenericRepository<StudySelectionAIResult, Guid, AppDbContext>, IStudySelectionAIResultRepository
    {
        public StudySelectionAIResultRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<StudySelectionAIResult?> GetByKeysAsync(
            Guid studySelectionId,
            Guid paperId,
            Guid reviewerId,
            ScreeningPhase phase,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<StudySelectionAIResult>()
                .FirstOrDefaultAsync(x => 
                    x.StudySelectionProcessId == studySelectionId && 
                    x.PaperId == paperId && 
                    x.ReviewerId == reviewerId &&
                    x.Phase == phase, 
                    cancellationToken);
        }
    }
}
