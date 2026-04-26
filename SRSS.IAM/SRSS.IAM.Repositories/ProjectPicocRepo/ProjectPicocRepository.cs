using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;
using System;

namespace SRSS.IAM.Repositories.ProjectPicocRepo
{
    public class ProjectPicocRepository : GenericRepository<ProjectPicoc, Guid, AppDbContext>, IProjectPicocRepository
    {
        public ProjectPicocRepository(AppDbContext context) : base(context)
        {
        }
    }
}
