using Microsoft.EntityFrameworkCore;
using SRSS.IAM.Repositories.Entities;
using Shared.Repositories;

namespace SRSS.IAM.Repositories.DataExtractionRepo
{
	public class FieldOptionRepository : GenericRepository<FieldOption, Guid, AppDbContext>, IFieldOptionRepository
	{
		private readonly AppDbContext _context;
		public FieldOptionRepository(AppDbContext context) : base(context) 
		{
			_context = context;
		}

		public async Task<List<FieldOption>> GetByFieldIdAsync(Guid fieldId)
		{
			return await _context.FieldOptions
				.Where(o => o.FieldId == fieldId)
				.OrderBy(o => o.DisplayOrder)
				.ToListAsync();
		}
	}
}