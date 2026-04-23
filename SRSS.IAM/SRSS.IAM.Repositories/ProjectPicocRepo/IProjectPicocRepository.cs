using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;
using System;

namespace SRSS.IAM.Repositories.ProjectPicocRepo
{
    public interface IProjectPicocRepository : IGenericRepository<ProjectPicoc, Guid, AppDbContext>
    {
    }
}
