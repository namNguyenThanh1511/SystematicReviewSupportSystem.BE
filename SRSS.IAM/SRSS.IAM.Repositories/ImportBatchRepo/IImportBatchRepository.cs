using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.ImportBatchRepo
{
    public interface IImportBatchRepository : IGenericRepository<ImportBatch,Guid,AppDbContext>
    {
    }
}
