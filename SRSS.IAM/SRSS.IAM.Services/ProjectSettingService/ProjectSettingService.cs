using System;
using System.Threading.Tasks;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.ProjectSetting;
using Shared.Exceptions;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.Mappers;

namespace SRSS.IAM.Services.ProjectSettingService
{
    public class ProjectSettingService : IProjectSettingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProjectSettingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ProjectSettingDto> GetProjectSettingAsync(Guid projectId)
        {
            var project = await _unitOfWork.SystematicReviewProjects.FindSingleAsync(x => x.Id == projectId);
            if (project == null)
            {
                throw new NotFoundException("Project not found");
            }

            var setting = await _unitOfWork.ProjectSettings.FindSingleOrDefaultAsync(x => x.ProjectId == projectId);
            
            if (setting == null)
            {
                // Create default setting if none exists
                setting = new ProjectSetting
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                };
                
                await _unitOfWork.ProjectSettings.AddAsync(setting);
                await _unitOfWork.CommitTransactionAsync();
            }

            return setting.ToResponse();
        }

        public async Task<ProjectSettingDto> UpdateProjectSettingAsync(Guid projectId, UpdateProjectSettingRequest request)
        {
            var project = await _unitOfWork.SystematicReviewProjects.FindSingleAsync(x => x.Id == projectId);
            if (project == null)
            {
                throw new NotFoundException("Project not found");
            }

            var setting = await _unitOfWork.ProjectSettings.FindSingleOrDefaultAsync(x => x.ProjectId == projectId);

            if (setting == null)
            {
                setting = new ProjectSetting
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    CreatedAt = DateTimeOffset.UtcNow
                };
                
                request.ApplyTo(setting);
                setting.ModifiedAt = DateTimeOffset.UtcNow;
                
                await _unitOfWork.ProjectSettings.AddAsync(setting);
            }
            else
            {
                request.ApplyTo(setting);
                setting.ModifiedAt = DateTimeOffset.UtcNow;

                await _unitOfWork.ProjectSettings.UpdateAsync(setting);
            }

            await _unitOfWork.CommitTransactionAsync();

            return setting.ToResponse();
        }
    }
}
