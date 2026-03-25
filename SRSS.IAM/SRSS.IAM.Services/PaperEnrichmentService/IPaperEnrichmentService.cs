using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.Paper;

namespace SRSS.IAM.Services.PaperEnrichmentService
{
    public interface IPaperEnrichmentService
    {
        Task EnrichFromOpenAlexAsync(Paper paper, CancellationToken ct);
        Task<EnrichPaperResponseDto> EnrichSingleAsync(Guid paperId, CancellationToken ct);
        Task<BatchEnrichResponseDto> EnrichBatchAsync(List<Guid> paperIds, CancellationToken ct);
        Task<BatchEnrichResponseDto> EnrichMissingAsync(int pageSize, CancellationToken ct);
    }
}
