using Microsoft.EntityFrameworkCore;
using SRSS.IAM.Repositories.Entities;
using Shared.Repositories;

namespace SRSS.IAM.Repositories.DataExtractionRepo
{
	public class ExtractionFieldRepository : GenericRepository<ExtractionField, Guid, AppDbContext>, IExtractionFieldRepository
	{
		private readonly AppDbContext _context;
		public ExtractionFieldRepository(AppDbContext context) : base(context)
		{
			_context = context;
		}

		public async Task<List<ExtractionField>> GetByTemplateIdAsync(Guid templateId)
		{
			return await _context.ExtractionFields
				.Where(f => f.SectionId == templateId)
				.Include(f => f.Options)
				.Include(f => f.SubFields)
				.OrderBy(f => f.OrderIndex)
				.ToListAsync();
		}

		public async Task<List<ExtractionField>> GetRootFieldsBySectioinIdAsync(Guid sectionId)
		{
			return await _context.ExtractionFields
				.Where(f => f.SectionId == sectionId && f.ParentFieldId == null)
				.Include(f => f.Options)
				.OrderBy(f => f.OrderIndex)
				.ToListAsync();
		}

		public Task<List<ExtractionField>> GetRootFieldsByTemplateIdAsync(Guid templateId)
		{
			throw new NotImplementedException();
		}
	}
}