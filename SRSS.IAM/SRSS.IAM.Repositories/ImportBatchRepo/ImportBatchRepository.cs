using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.ImportBatchRepo
{
    public class ImportBatchRepository : GenericRepository<ImportBatch, Guid, AppDbContext>, IImportBatchRepository
    {
        public ImportBatchRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ImportBatch>> GetByProjectIdsAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            return await _context.ImportBatches
                .Where(ib => ib.ProjectId == projectId)
                .ToListAsync(cancellationToken);
        }
    }
}
