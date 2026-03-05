using Microsoft.EntityFrameworkCore;
using SRSS.IAM.Repositories.Entities;
using Shared.Repositories;

namespace SRSS.IAM.Repositories.DataExtractionRepo
{
	public class ExtractionTemplateRepository : GenericRepository<ExtractionTemplate, Guid, AppDbContext>, IExtractionTemplateRepository
	{
		private readonly AppDbContext _context;
		public ExtractionTemplateRepository(AppDbContext context) : base(context) 
		{
			_context = context;
		}

		public async Task<List<ExtractionTemplate>> GetByProtocolIdAsync(Guid protocolId)
		{
			return await _context.ExtractionTemplates
				.Where(t => t.ProtocolId == protocolId)
				.Include(t => t.Fields.Where(f => f.ParentFieldId == null)) // Root fields only
					.ThenInclude(f => f.Options)
				.OrderBy(t => t.CreatedAt)
				.ToListAsync();
		}

		public async Task<ExtractionTemplate?> GetByIdWithFieldsAsync(Guid templateId)
		{
			return await _context.ExtractionTemplates
				.Where(t => t.Id == templateId)
				.Include(t => t.Fields.Where(f => f.ParentFieldId == null)) // Root fields
					.ThenInclude(f => f.Options)
				.Include(t => t.Fields.Where(f => f.ParentFieldId == null))
					.ThenInclude(f => f.SubFields) // Level 1 sub-fields
						.ThenInclude(sf => sf.Options)
				.Include(t => t.Fields.Where(f => f.ParentFieldId == null))
					.ThenInclude(f => f.SubFields)
						.ThenInclude(sf => sf.SubFields) // Level 2 sub-fields
							.ThenInclude(ssf => ssf.Options)
				.FirstOrDefaultAsync();
		}
	}
}