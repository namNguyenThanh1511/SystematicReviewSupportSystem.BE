using System;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.GrobidHeaderResultRepo
{
    public interface IGrobidHeaderResultRepository : IGenericRepository<GrobidHeaderResult, Guid, AppDbContext>
    {
    }
}
