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

        public async Task<IEnumerable<ImportBatch>> GetBySearchExecutionIdsWithSourceAsync(IEnumerable<Guid> executionIds, CancellationToken cancellationToken = default)
        {
            return await _context.ImportBatches
                .Include(ib => ib.SearchExecution)
                    .ThenInclude(se => se.SearchSource)
                .Where(ib => ib.SearchExecutionId != null && executionIds.Contains(ib.SearchExecutionId.Value))
                .ToListAsync(cancellationToken);
        }
    }
}
