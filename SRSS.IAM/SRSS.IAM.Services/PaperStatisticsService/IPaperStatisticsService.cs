using SRSS.IAM.Services.DTOs.Paper;

namespace SRSS.IAM.Services.PaperStatisticsService
{
    public interface IPaperStatisticsService
    {
        Task<PaperOverviewDto> GetOverviewAsync(Guid projectId, PaperStatisticsFilter filter, CancellationToken cancellationToken = default);
        Task<List<YearCountDto>> GetPapersByYearAsync(Guid projectId, PaperStatisticsFilter filter, CancellationToken cancellationToken = default);
        Task<List<CountItemDto>> GetPublicationTypesAsync(Guid projectId, PaperStatisticsFilter filter, CancellationToken cancellationToken = default);
        Task<List<CountItemDto>> GetTopJournalsAsync(Guid projectId, int top, PaperStatisticsFilter filter, CancellationToken cancellationToken = default);
        Task<List<CountItemDto>> GetTopConferencesAsync(Guid projectId, int top, PaperStatisticsFilter filter, CancellationToken cancellationToken = default);
        Task<List<CountItemDto>> GetTopPublishersAsync(Guid projectId, int top, PaperStatisticsFilter filter, CancellationToken cancellationToken = default);
        Task<List<CountItemDto>> GetLanguagesAsync(Guid projectId, PaperStatisticsFilter filter, CancellationToken cancellationToken = default);
        Task<List<StatusCountItemDto>> GetFulltextStatusAsync(Guid projectId, PaperStatisticsFilter filter, CancellationToken cancellationToken = default);
        Task<List<CountItemDto>> GetTopKeywordsAsync(Guid projectId, int top, PaperStatisticsFilter filter, CancellationToken cancellationToken = default);
        Task<DataQualityDto> GetDataQualityAsync(Guid projectId, PaperStatisticsFilter filter, CancellationToken cancellationToken = default);
    }
}
