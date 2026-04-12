using Shared.Repositories;
using SRSS.IAM.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.SynthesisExecutionRepo
{
    public class SynthesisProcessRepository : GenericRepository<SynthesisProcess, Guid, AppDbContext>, ISynthesisProcessRepository
    {
        public SynthesisProcessRepository(AppDbContext context) : base(context) { }
    }

    public class SynthesisThemeRepository : GenericRepository<SynthesisTheme, Guid, AppDbContext>, ISynthesisThemeRepository
    {
        public SynthesisThemeRepository(AppDbContext context) : base(context) { }
    }

    public class ThemeEvidenceRepository : GenericRepository<ThemeEvidence, Guid, AppDbContext>, IThemeEvidenceRepository
    {
        public ThemeEvidenceRepository(AppDbContext context) : base(context) { }
    }

    public class ResearchQuestionFindingRepository : GenericRepository<ResearchQuestionFinding, Guid, AppDbContext>, IResearchQuestionFindingRepository
    {
        public ResearchQuestionFindingRepository(AppDbContext context) : base(context) { }
    }
}
