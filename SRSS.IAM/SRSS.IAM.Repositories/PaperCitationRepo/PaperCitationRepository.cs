using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.PaperCitationRepo
{
    public class PaperCitationRepository : GenericRepository<PaperCitation, Guid, AppDbContext>, IPaperCitationRepository
    {
        public PaperCitationRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<int> CountByTargetAsync(Guid targetPaperId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<PaperCitation>()
                .CountAsync(c => c.TargetPaperId == targetPaperId, cancellationToken);
        }

        public async Task<int> CountBySourceAsync(Guid sourcePaperId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<PaperCitation>()
                .CountAsync(c => c.SourcePaperId == sourcePaperId, cancellationToken);
        }

        public async Task<Dictionary<Guid, int>> CountByTargetsAsync(IEnumerable<Guid> targetPaperIds, CancellationToken cancellationToken = default)
        {
            var results = await _context.Set<PaperCitation>()
                .Where(c => targetPaperIds.Contains(c.TargetPaperId))
                .GroupBy(c => c.TargetPaperId)
                .Select(g => new { PaperId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.PaperId, x => x.Count, cancellationToken);
                
            return results;
        }

        public async Task<Dictionary<Guid, int>> CountBySourcesAsync(IEnumerable<Guid> sourcePaperIds, CancellationToken cancellationToken = default)
        {
            var results = await _context.Set<PaperCitation>()
                .Where(c => sourcePaperIds.Contains(c.SourcePaperId))
                .GroupBy(c => c.SourcePaperId)
                .Select(g => new { PaperId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.PaperId, x => x.Count, cancellationToken);
                
            return results;
        }

        public async Task<List<PaperCitation>> GetCitationsWithSourcePaperAsync(Guid targetPaperId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<PaperCitation>()
                .Include(c => c.SourcePaper)
                .ThenInclude(p => p.IncomingCitations)
                .Where(c => c.TargetPaperId == targetPaperId && c.SourcePaperId != c.TargetPaperId)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

    public async Task<List<PaperCitation>> GetReferencesWithTargetPaperAsync(Guid sourcePaperId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<PaperCitation>()
            .Where(c => c.SourcePaperId == sourcePaperId && c.SourcePaperId != c.TargetPaperId)
            .Select(c => new PaperCitation 
            {
                SourcePaperId = c.SourcePaperId,
                TargetPaperId = c.TargetPaperId,
                TargetPaper = new Paper 
                {
                    Id = c.TargetPaper.Id,
                    Title = c.TargetPaper.Title,
                    PublicationDate = c.TargetPaper.PublicationDate,
                    PublicationYearInt = c.TargetPaper.PublicationYearInt,
                    Authors = c.TargetPaper.Authors,
                    DOI = c.TargetPaper.DOI,
                    // Chỉ lấy những thông tin cần thiết của IncomingCitations
                    IncomingCitations = c.TargetPaper.IncomingCitations.ToList() 
                }
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

        public async Task<List<PaperCitation>> GetEdgesBySourcesAsync(IEnumerable<Guid> sourcePaperIds, decimal minConfidence, CancellationToken cancellationToken = default)
        {
            return await _context.Set<PaperCitation>()
                .Where(c => sourcePaperIds.Contains(c.SourcePaperId) 
                    && c.ConfidenceScore >= minConfidence
                    && c.SourcePaperId != c.TargetPaperId)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}
