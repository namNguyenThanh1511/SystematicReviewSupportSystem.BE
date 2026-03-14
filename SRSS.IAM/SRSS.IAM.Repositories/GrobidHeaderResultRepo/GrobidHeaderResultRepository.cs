using System;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.GrobidHeaderResultRepo
{
    public class GrobidHeaderResultRepository : GenericRepository<GrobidHeaderResult, Guid, AppDbContext>, IGrobidHeaderResultRepository
    {
        public GrobidHeaderResultRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
