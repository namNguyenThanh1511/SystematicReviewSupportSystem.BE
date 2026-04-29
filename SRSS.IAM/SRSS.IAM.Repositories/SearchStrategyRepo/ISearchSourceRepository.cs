using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.SearchStrategyRepo
{
	public interface ISearchSourceRepository : IGenericRepository<SearchSource, Guid, AppDbContext>
	{
		Task<IEnumerable<SearchSource>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
		Task<SearchSource?> GetByIdWithStrategiesAsync(Guid id, CancellationToken cancellationToken = default);
	}
}
