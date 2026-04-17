using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.ProtocolRepo
{
	public class ReviewProtocolRepository : GenericRepository<ReviewProtocol, Guid, AppDbContext>, IReviewProtocolRepository
	{
		public ReviewProtocolRepository(AppDbContext context) : base(context) { }

		// Get protocol including soft deleted ones
		public async Task<ReviewProtocol?> GetByIdIncludeDeletedAsync(Guid id, CancellationToken cancellationToken = default)
		{
			return await _context.ReviewProtocols
				.IgnoreQueryFilters()
				.FirstOrDefaultAsync(rp => rp.Id == id, cancellationToken);
		}

		// Get all protocols including soft deleted ones
		public async Task<IEnumerable<ReviewProtocol>> GetAllIncludeDeletedAsync(CancellationToken cancellationToken = default)
		{
			return await _context.ReviewProtocols
				.IgnoreQueryFilters()
				.ToListAsync(cancellationToken);
		}

		// Get only soft deleted protocols
		public async Task<IEnumerable<ReviewProtocol>> GetDeletedProtocolsAsync(CancellationToken cancellationToken = default)
		{
			return await _context.ReviewProtocols
				.IgnoreQueryFilters()
				.Where(rp => rp.IsDeleted)
				.ToListAsync(cancellationToken);
		}

		public async Task<ReviewProtocol?> GetByIdWithVersionsAsync(Guid protocolId, CancellationToken cancellationToken = default)
		{
			return await _context.ReviewProtocols
				.Include(p => p.Versions)
				.Include(p => p.StudyCharacteristics)
				.Include(p => p.SearchSources)
				.Include(p => p.SelectionCriterias)
				.ThenInclude(sc => sc.InclusionCriteria)
				.Include(p => p.SelectionCriterias)
				.ThenInclude(sc => sc.ExclusionCriteria)
				.Include(p => p.SelectionProcedures)
				.Include(p => p.QualityStrategies)
				.ThenInclude(ps => ps.Checklists)
				.ThenInclude(pc => pc.Criteria)
				.Include(p => p.ExtractionStrategies)
				.ThenInclude(es => es.Forms)
				.ThenInclude(ef => ef.DataItems)
				.Include(p => p.ExtractionTemplates)
				.ThenInclude(et => et.Sections)
				.Include(p => p.SynthesisStrategies)
				.Include(p => p.DisseminationStrategies)
				.AsNoTracking()
				.FirstOrDefaultAsync(p => p.Id == protocolId, cancellationToken);
		}
		
		public async Task<ReviewProtocol?> GetProtocolDetailByIdAsync(Guid protocolId, CancellationToken cancellationToken = default)
		{
			return await _context.ReviewProtocols
				.Include(p => p.StudyCharacteristics)
				.Include(p => p.SearchSources)
				.Include(p => p.SelectionCriterias)
				.Include(p => p.SelectionProcedures)
				.Include(p => p.QualityStrategies)
				.Include(p => p.ExtractionStrategies)
				.Include(p => p.ExtractionTemplates)
				.Include(p => p.SynthesisStrategies)
				.Include(p => p.DisseminationStrategies)
				.Include(p => p.Versions)
				.AsNoTracking()
				.FirstOrDefaultAsync(p => p.Id == protocolId, cancellationToken);
		}

		public async Task<IEnumerable<ReviewProtocol>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
		{
			return await FindAllAsync(p => p.ProjectId == projectId, isTracking: false, cancellationToken);
		}
	}

	public class ProtocolVersionRepository : GenericRepository<ProtocolVersion, Guid, AppDbContext>, IProtocolVersionRepository
	{
		public ProtocolVersionRepository(AppDbContext context) : base(context) { }

		public async Task<IEnumerable<ProtocolVersion>> GetByProtocolIdAsync(Guid protocolId, CancellationToken cancellationToken = default)
		{
			return await FindAllAsync(v => v.ProtocolId == protocolId, isTracking: false, cancellationToken);
		}
	}

	public class ProtocolEvaluationRepository : GenericRepository<ProtocolEvaluation, Guid, AppDbContext>, IProtocolEvaluationRepository
	{
		public ProtocolEvaluationRepository(AppDbContext context) : base(context) { }

		public async Task<IEnumerable<ProtocolEvaluation>> GetByProtocolIdAsync(Guid protocolId, CancellationToken cancellationToken = default)
		{
			return await FindAllAsync(e => e.ProtocolId == protocolId, isTracking: false, cancellationToken);
		}
	}
}