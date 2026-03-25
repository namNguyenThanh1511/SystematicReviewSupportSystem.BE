using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.Paper;
using SRSS.IAM.Services.OpenAlex;

namespace SRSS.IAM.Services.PaperEnrichmentService
{
    public class PaperEnrichmentService : IPaperEnrichmentService
    {
        private readonly IOpenAlexService _openAlexService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PaperEnrichmentService> _logger;

        public PaperEnrichmentService(
            IOpenAlexService openAlexService,
            IUnitOfWork unitOfWork,
            ILogger<PaperEnrichmentService> logger)
        {
            _openAlexService = openAlexService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task EnrichFromOpenAlexAsync(Paper paper, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(paper.DOI))
            {
                _logger.LogDebug("Paper {PaperId} has no DOI — skipping OpenAlex enrichment.", paper.Id);
                return;
            }

            try
            {
                var openAlexRequestDoi = $"doi:{paper.DOI}";
                var work = await _openAlexService.GetWorkAsync(openAlexRequestDoi, ct);

                paper.OpenAlexId = work.Id;
                paper.ExternalCitationCount = work.CitedByCount;
                paper.ExternalReferenceCount = work.ReferencedWorksCount;
                if (work.CitedByPercentileYear != null)
                {
                    paper.ExternalCitedByPercentile = work.CitedByPercentileYear.Min;
                }

                paper.ExternalSource = "OpenAlex";
                paper.ExternalLastFetchedAt = DateTimeOffset.UtcNow;
                paper.ExternalDataFetched = true;

                _logger.LogInformation(
                    "Enriched Paper {PaperId} from OpenAlex: CitedBy={CitedBy}, Refs={Refs}",
                    paper.Id, work.CitedByCount, work.ReferencedWorksCount);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to enrich Paper {PaperId} (DOI: {DOI}) from OpenAlex. Continuing without external data.",
                    paper.Id, paper.DOI);

                // Non-critical — do not re-throw; the paper import should still succeed
            }
        }

        public async Task<EnrichPaperResponseDto> EnrichSingleAsync(Guid paperId, CancellationToken ct)
        {
            var paper = await _unitOfWork.Papers.FindSingleAsync(p => p.Id == paperId, true, ct);
            if (paper == null)
            {
                throw new InvalidOperationException($"Paper with ID {paperId} not found.");
            }

            await EnrichFromOpenAlexAsync(paper, ct);

            // We update the paper anyway even if OpenAlex enrichment fails (to log the attempt or fields)
            // But EnrichFromOpenAlexAsync only sets ExternalDataFetched = true on success.
            
            await _unitOfWork.SaveChangesAsync(ct);

            return new EnrichPaperResponseDto
            {
                PaperId = paper.Id,
                CitationCount = paper.ExternalCitationCount,
                ReferenceCount = paper.ExternalReferenceCount,
                IsSuccess = paper.ExternalDataFetched,
                Message = paper.ExternalDataFetched ? "Paper enriched successfully." : "Enrichment failed or no data found."
            };
        }

        public async Task<BatchEnrichResponseDto> EnrichBatchAsync(List<Guid> paperIds, CancellationToken ct)
        {
            var response = new BatchEnrichResponseDto
            {
                Total = paperIds.Count
            };

            foreach (var paperId in paperIds)
            {
                try
                {
                    var result = await EnrichSingleAsync(paperId, ct);
                    response.Results.Add(result);
                    if (result.IsSuccess)
                    {
                        response.SuccessCount++;
                    }
                    else
                    {
                        response.FailedCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error processing paper {PaperId} in batch enrichment.", paperId);
                    response.FailedCount++;
                    response.Results.Add(new EnrichPaperResponseDto
                    {
                        PaperId = paperId,
                        IsSuccess = false,
                        Message = ex.Message
                    });
                }
            }

            return response;
        }

        public async Task<BatchEnrichResponseDto> EnrichMissingAsync(int pageSize, CancellationToken ct)
        {
            var (papers, totalCount) = await _unitOfWork.Papers.GetPapersMissingExternalDataAsync(1, pageSize, ct);
            
            var paperIds = papers.Select(p => p.Id).ToList();
            
            var result = await EnrichBatchAsync(paperIds, ct);
            
            // Override total recorded in batch with the actual total in DB if needed, 
            // but for a single request, the result's Total = paperIds.Count is fine.
            return result;
        }
    }
}
