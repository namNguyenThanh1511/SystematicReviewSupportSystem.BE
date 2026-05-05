using Microsoft.EntityFrameworkCore;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.Paper;

namespace SRSS.IAM.Services.PaperStatisticsService
{
    public class PaperStatisticsService : IPaperStatisticsService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PaperStatisticsService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private IQueryable<Paper> GetFilteredPapers(Guid projectId, PaperStatisticsFilter filter)
        {
            var query = _unitOfWork.Papers.GetQueryable(p => p.ProjectId == projectId && !p.IsDeleted && !p.IsDuplicated, false);

            if (filter.YearFrom.HasValue)
            {
                query = query.Where(p => p.PublicationYearInt >= filter.YearFrom.Value);
            }

            if (filter.YearTo.HasValue)
            {
                query = query.Where(p => p.PublicationYearInt <= filter.YearTo.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Source))
            {
                query = query.Where(p => p.Source == filter.Source);
            }

            return query;
        }

        public async Task<PaperOverviewDto> GetOverviewAsync(Guid projectId, PaperStatisticsFilter filter, CancellationToken cancellationToken = default)
        {
            var query = GetFilteredPapers(projectId, filter);

            var totalPapers = await query.CountAsync(cancellationToken);
            var totalWithFulltext = await query.CountAsync(p => p.FullTextRetrievalStatus == FullTextRetrievalStatus.Retrieved, cancellationToken);
            var totalMissingDoi = await query.CountAsync(p => string.IsNullOrEmpty(p.DOI), cancellationToken);
            var totalMissingAbstract = await query.CountAsync(p => string.IsNullOrEmpty(p.Abstract), cancellationToken);

            return new PaperOverviewDto
            {
                TotalPapers = totalPapers,
                TotalPapersWithFulltext = totalWithFulltext,
                FulltextAvailablePercentage = totalPapers > 0 ? (double)totalWithFulltext / totalPapers * 100 : 0,
                TotalMissingDoi = totalMissingDoi,
                TotalMissingAbstract = totalMissingAbstract
            };
        }

        public async Task<List<YearCountDto>> GetPapersByYearAsync(Guid projectId, PaperStatisticsFilter filter, CancellationToken cancellationToken = default)
        {
            var query = GetFilteredPapers(projectId, filter);

            return await query
                .Where(p => p.PublicationYearInt.HasValue)
                .GroupBy(p => p.PublicationYearInt!.Value)
                .Select(g => new YearCountDto
                {
                    Year = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Year)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<CountItemDto>> GetPublicationTypesAsync(Guid projectId, PaperStatisticsFilter filter, CancellationToken cancellationToken = default)
        {
            var query = GetFilteredPapers(projectId, filter);

            return await query
                .Where(p => !string.IsNullOrEmpty(p.PublicationType))
                .GroupBy(p => p.PublicationType)
                .Select(g => new CountItemDto
                {
                    Label = g.Key!,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<CountItemDto>> GetTopJournalsAsync(Guid projectId, int top, PaperStatisticsFilter filter, CancellationToken cancellationToken = default)
        {
            var query = GetFilteredPapers(projectId, filter);

            return await query
                .Where(p => !string.IsNullOrEmpty(p.Journal))
                .GroupBy(p => p.Journal)
                .Select(g => new CountItemDto
                {
                    Label = g.Key!,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(top)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<CountItemDto>> GetTopConferencesAsync(Guid projectId, int top, PaperStatisticsFilter filter, CancellationToken cancellationToken = default)
        {
            var query = GetFilteredPapers(projectId, filter);

            return await query
                .Where(p => !string.IsNullOrEmpty(p.ConferenceName))
                .GroupBy(p => p.ConferenceName)
                .Select(g => new CountItemDto
                {
                    Label = g.Key!,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(top)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<CountItemDto>> GetTopPublishersAsync(Guid projectId, int top, PaperStatisticsFilter filter, CancellationToken cancellationToken = default)
        {
            var query = GetFilteredPapers(projectId, filter);

            return await query
                .Where(p => !string.IsNullOrEmpty(p.Publisher))
                .GroupBy(p => p.Publisher)
                .Select(g => new CountItemDto
                {
                    Label = g.Key!,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(top)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<CountItemDto>> GetLanguagesAsync(Guid projectId, PaperStatisticsFilter filter, CancellationToken cancellationToken = default)
        {
            var query = GetFilteredPapers(projectId, filter);

            return await query
                .Where(p => !string.IsNullOrEmpty(p.Language))
                .GroupBy(p => p.Language)
                .Select(g => new CountItemDto
                {
                    Label = g.Key!,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<StatusCountItemDto>> GetFulltextStatusAsync(Guid projectId, PaperStatisticsFilter filter, CancellationToken cancellationToken = default)
        {
            var query = GetFilteredPapers(projectId, filter);

            return await query
                .GroupBy(p => p.FullTextRetrievalStatus)
                .Select(g => new StatusCountItemDto
                {
                    Status = (int)g.Key,
                    Label = g.Key.ToString(),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<CountItemDto>> GetTopKeywordsAsync(Guid projectId, int top, PaperStatisticsFilter filter, CancellationToken cancellationToken = default)
        {
            var query = GetFilteredPapers(projectId, filter);

            // Fetching keywords for client-side processing as EF doesn't support complex string split/flatten in LINQ
            var keywordsList = await query
                .Where(p => !string.IsNullOrEmpty(p.Keywords))
                .Select(p => p.Keywords)
                .ToListAsync(cancellationToken);

            var keywordCounts = keywordsList
                .SelectMany(k => k!.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
                .Select(k => k.Trim().ToLower())
                .GroupBy(k => k)
                .Select(g => new CountItemDto
                {
                    Label = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(top)
                .ToList();

            return keywordCounts;
        }

        public async Task<DataQualityDto> GetDataQualityAsync(Guid projectId, PaperStatisticsFilter filter, CancellationToken cancellationToken = default)
        {
            var query = GetFilteredPapers(projectId, filter);

            var missingDoi = await query.CountAsync(p => string.IsNullOrEmpty(p.DOI), cancellationToken);
            var missingAbstract = await query.CountAsync(p => string.IsNullOrEmpty(p.Abstract), cancellationToken);
            var missingAuthors = await query.CountAsync(p => string.IsNullOrEmpty(p.Authors), cancellationToken);
            var missingYear = await query.CountAsync(p => !p.PublicationYearInt.HasValue, cancellationToken);

            return new DataQualityDto
            {
                MissingDoiCount = missingDoi,
                MissingAbstractCount = missingAbstract,
                MissingAuthorsCount = missingAuthors,
                MissingYearCount = missingYear
            };
        }
    }
}
