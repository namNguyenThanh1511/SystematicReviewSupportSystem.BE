using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.SynthesisExecutionRepo
{
    public interface ISynthesisProcessRepository : IGenericRepository<SynthesisProcess, Guid, AppDbContext> { }
    public interface ISynthesisThemeRepository : IGenericRepository<SynthesisTheme, Guid, AppDbContext> { }
    public interface IThemeEvidenceRepository : IGenericRepository<ThemeEvidence, Guid, AppDbContext> { }
    public interface IResearchQuestionFindingRepository : IGenericRepository<ResearchQuestionFinding, Guid, AppDbContext> { }
}
