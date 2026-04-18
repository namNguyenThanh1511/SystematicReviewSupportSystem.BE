using Microsoft.EntityFrameworkCore;
using SRSS.IAM.Repositories.Entities;
using Shared.Repositories;

namespace SRSS.IAM.Repositories.DataExtractionRepo
{
	public class ExtractedDataValueRepository : GenericRepository<ExtractedDataValue, Guid, AppDbContext>, IExtractedDataValueRepository
	{
		private readonly AppDbContext _context;
		public ExtractedDataValueRepository(AppDbContext context) : base(context)
		{
			_context = context;
		}

		public async Task<List<ExtractedDataValue>> GetByPaperIdAsync(Guid paperId)
		{
			return await _context.ExtractedDataValues
				.Where(v => v.PaperId == paperId)
				.Include(v => v.Field)
				.Include(v => v.Option)
				.Include(v => v.Reviewer)
				.ToListAsync();
		}

		public async Task<List<ExtractedDataValue>> GetByFieldIdAsync(Guid fieldId)
		{
			return await _context.ExtractedDataValues
				.Where(v => v.FieldId == fieldId)
				.Include(v => v.Paper)
				.Include(v => v.Option)
				.Include(v => v.Reviewer)
				.ToListAsync();
		}
	}
}