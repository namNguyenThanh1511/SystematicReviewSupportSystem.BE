using SRSS.IAM.Services.DTOs.AiSetup;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SRSS.IAM.Services.AiSetupService
{
    public interface IAiSetupService
    {
        Task<AnalyzeTopicResponse> AnalyzeTopicAsync(AnalyzeTopicRequest request);
        Task<GeneratePicocResponse> GeneratePicocAsync(GeneratePicocRequest request);
        Task<GenerateRqsResponse> GenerateRqsAsync(GenerateRqsRequest request);
        Task<ProjectSetupDetailsResponse> GetSetupDetailsAsync(Guid projectId);
        Task UpdateSetupAsync(Guid projectId, UpdateSetupRequest request);
    }
}
