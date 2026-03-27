using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;
using System;

namespace SRSS.IAM.Repositories.ProjectSettingRepo
{
    public class ProjectSettingRepository : GenericRepository<ProjectSetting, Guid, AppDbContext>, IProjectSettingRepository
    {
        public ProjectSettingRepository(AppDbContext context) : base(context)
        {
        }
    }
}
