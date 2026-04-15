using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.ChecklistRepo
{
    public interface IChecklistTemplateRepository : IGenericRepository<ChecklistTemplate, Guid, AppDbContext>
    {
        Task<List<ChecklistTemplate>> GetAllWithItemsAsync(bool? isSystem = null, CancellationToken cancellationToken = default);
        Task<ChecklistTemplate?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<ChecklistTemplate>> GetSystemTemplatesAsync(CancellationToken cancellationToken = default);
    }

    public interface IChecklistItemTemplateRepository : IGenericRepository<ChecklistItemTemplate, Guid, AppDbContext>
    {
        Task<List<ChecklistItemTemplate>> GetByTemplateIdAsync(Guid templateId, CancellationToken cancellationToken = default);
    }

    public interface IReviewChecklistRepository : IGenericRepository<ReviewChecklist, Guid, AppDbContext>
    {
        Task<List<ReviewChecklist>> GetByReviewIdWithDetailsAsync(Guid reviewId, CancellationToken cancellationToken = default);
        Task<ReviewChecklist?> GetByIdWithDetailsAsync(Guid reviewChecklistId, CancellationToken cancellationToken = default);
    }

    public interface IChecklistItemResponseRepository : IGenericRepository<ChecklistItemResponse, Guid, AppDbContext>
    {
        Task<ChecklistItemResponse?> GetByReviewChecklistAndItemAsync(Guid reviewChecklistId, Guid itemTemplateId, CancellationToken cancellationToken = default);
    }

    public class ChecklistTemplateRepository : GenericRepository<ChecklistTemplate, Guid, AppDbContext>, IChecklistTemplateRepository
    {
        public ChecklistTemplateRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<ChecklistTemplate>> GetAllWithItemsAsync(bool? isSystem = null, CancellationToken cancellationToken = default)
        {
            var query = _context.ChecklistTemplates
                .Include(x => x.ItemTemplates)
                .AsNoTracking()
                .AsQueryable();

            if (isSystem.HasValue)
            {
                query = query.Where(x => x.IsSystem == isSystem.Value);
            }

            return await query
                .OrderByDescending(x => x.IsSystem)
                .ThenBy(x => x.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<ChecklistTemplate?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.ChecklistTemplates
                .Include(x => x.ItemTemplates)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<List<ChecklistTemplate>> GetSystemTemplatesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.ChecklistTemplates
                .Where(x => x.IsSystem)
                .OrderBy(x => x.Name)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }

    public class ChecklistItemTemplateRepository : GenericRepository<ChecklistItemTemplate, Guid, AppDbContext>, IChecklistItemTemplateRepository
    {
        public ChecklistItemTemplateRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<ChecklistItemTemplate>> GetByTemplateIdAsync(Guid templateId, CancellationToken cancellationToken = default)
        {
            return await _context.ChecklistItemTemplates
                .Where(x => x.TemplateId == templateId)
                .OrderBy(x => x.Order)
                .ThenBy(x => x.ItemNumber)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }

    public class ReviewChecklistRepository : GenericRepository<ReviewChecklist, Guid, AppDbContext>, IReviewChecklistRepository
    {
        public ReviewChecklistRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<ReviewChecklist>> GetByReviewIdWithDetailsAsync(Guid reviewId, CancellationToken cancellationToken = default)
        {
            return await _context.ReviewChecklists
                .Include(x => x.Project)
                .Include(x => x.Template)
                    .ThenInclude(t => t.ItemTemplates)
                .AsNoTracking()
                .Where(x => x.ProjectId == reviewId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<ReviewChecklist?> GetByIdWithDetailsAsync(Guid reviewChecklistId, CancellationToken cancellationToken = default)
        {
            return await _context.ReviewChecklists
                .Include(x => x.Project)
                .Include(x => x.Template)
                    .ThenInclude(t => t.ItemTemplates)
                .Include(x => x.ItemResponses)
                .FirstOrDefaultAsync(x => x.Id == reviewChecklistId, cancellationToken);
        }
    }

    public class ChecklistItemResponseRepository : GenericRepository<ChecklistItemResponse, Guid, AppDbContext>, IChecklistItemResponseRepository
    {
        public ChecklistItemResponseRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<ChecklistItemResponse?> GetByReviewChecklistAndItemAsync(Guid reviewChecklistId, Guid itemTemplateId, CancellationToken cancellationToken = default)
        {
            return await _context.ChecklistItemResponses
                .FirstOrDefaultAsync(
                    x => x.ReviewChecklistId == reviewChecklistId && x.ItemTemplateId == itemTemplateId,
                    cancellationToken);
        }
    }
}
