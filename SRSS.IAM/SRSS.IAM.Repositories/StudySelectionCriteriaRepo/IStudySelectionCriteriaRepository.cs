using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.StudySelectionCriteriaRepo
{
	public interface IStudySelectionCriteriaRepository : IGenericRepository<StudySelectionCriteria, Guid, AppDbContext>
	{
		Task<IEnumerable<StudySelectionCriteria>> GetByProtocolIdAsync(Guid protocolId, CancellationToken cancellationToken = default);
	}

	public interface IInclusionCriterionRepository : IGenericRepository<InclusionCriterion, Guid, AppDbContext>
	{
		Task<IEnumerable<InclusionCriterion>> GetByCriteriaIdAsync(Guid criteriaId, CancellationToken cancellationToken = default);
	}

	public interface IExclusionCriterionRepository : IGenericRepository<ExclusionCriterion, Guid, AppDbContext>
	{
		Task<IEnumerable<ExclusionCriterion>> GetByCriteriaIdAsync(Guid criteriaId, CancellationToken cancellationToken = default);
	}
}
