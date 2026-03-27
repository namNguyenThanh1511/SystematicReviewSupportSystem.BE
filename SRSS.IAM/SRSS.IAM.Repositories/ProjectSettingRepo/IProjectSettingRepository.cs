using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;
using System;

namespace SRSS.IAM.Repositories.ProjectSettingRepo
{
    public interface IProjectSettingRepository : IGenericRepository<ProjectSetting, Guid, AppDbContext>
    {
    }
}
