using SRSS.IAM.Services.DTOs.SynthesisExecution;
using System;
using System.Threading.Tasks;

namespace SRSS.IAM.Services.SynthesisExecutionService
{
    public interface ISynthesisExecutionService
    {
        Task<SynthesisWorkspaceDto> GetSynthesisWorkspaceAsync(Guid reviewProcessId);
        Task<SynthesisProcessDto> StartSynthesisProcessAsync(Guid reviewProcessId);
        Task CompleteSynthesisProcessAsync(Guid reviewProcessId);
        Task<List<SourceDataGroupDto>> GetExtractedDataForSynthesisAsync(Guid reviewProcessId);
        
        Task<SynthesisThemeDto> CreateThemeAsync(Guid processId, CreateThemeRequest request);
        Task UpdateThemeAsync(Guid themeId, UpdateThemeRequest request);
        Task DeleteThemeAsync(Guid themeId);
        
        Task<ThemeEvidenceDto> AddEvidenceToThemeAsync(Guid themeId, AddEvidenceRequest request);
        Task RemoveEvidenceAsync(Guid evidenceId);
        
        Task SaveFindingAsync(Guid findingId, SaveFindingRequest request);
    }
}
