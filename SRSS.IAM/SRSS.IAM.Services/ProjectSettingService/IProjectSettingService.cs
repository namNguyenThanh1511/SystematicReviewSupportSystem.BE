using System;
using System.Threading.Tasks;
using SRSS.IAM.Services.DTOs.ProjectSetting;

namespace SRSS.IAM.Services.ProjectSettingService
{
    public interface IProjectSettingService
    {
        Task<ProjectSettingDto> GetProjectSettingAsync(Guid projectId);
        Task<ProjectSettingDto> UpdateProjectSettingAsync(Guid projectId, UpdateProjectSettingRequest request);
    }
}
