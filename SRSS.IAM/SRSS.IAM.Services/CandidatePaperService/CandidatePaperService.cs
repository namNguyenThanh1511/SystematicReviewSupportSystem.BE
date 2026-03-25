using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.CandidatePaperService.DTOs;
using SRSS.IAM.Services.GrobidClient;
using SRSS.IAM.Services.PaperService;
using SRSS.IAM.Services.GrobidClient.DTOs;
using SRSS.IAM.Services.DTOs.Common;
using SRSS.IAM.Services.ReferenceMatchingService;
using SRSS.IAM.Services.ReferenceMatchingService.DTOs;
using SRSS.IAM.Services.PaperEnrichmentService;

namespace SRSS.IAM.Services.CandidatePaperService
{
    public class CandidatePaperService : ICandidatePaperService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGrobidService _grobidService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CandidatePaperService> _logger;
        private readonly IReferenceMatchingService _referenceMatchingService;
        private readonly IPaperEnrichmentService _paperEnrichmentService;

        public CandidatePaperService(
            IUnitOfWork unitOfWork,
            IGrobidService grobidService,
            IHttpClientFactory httpClientFactory,
            ILogger<CandidatePaperService> logger,
            IReferenceMatchingService referenceMatchingService,
            IPaperEnrichmentService paperEnrichmentService)
        {
            _unitOfWork = unitOfWork;
            _grobidService = grobidService;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _referenceMatchingService = referenceMatchingService;
            _paperEnrichmentService = paperEnrichmentService;
        }

        public async Task ExtractReferencesFromPaperAsync(Guid processId, Guid paperId, CancellationToken cancellationToken = default)
        {
            var paper = await _unitOfWork.Papers.FindSingleAsync(p => p.Id == paperId, isTracking: false, cancellationToken);
            if (paper == null)
            {
                throw new InvalidOperationException($"Paper with ID {paperId} not found.");
            }

            if (paper.ReviewProcessId != null && paper.ReviewProcessId != processId)
            {
                throw new ArgumentException("Paper does not belong to the specified review process.");
            }

            var paperPdf = await _unitOfWork.PaperPdfs.GetLatestPaperPdfAsync(paperId,cancellationToken);
            var pdfUrl = paper.PdfUrl; 
            var fileName = paperPdf?.FileName ?? paper.PdfFileName ?? "paper.pdf";

            if (string.IsNullOrWhiteSpace(pdfUrl))
            {
                throw new InvalidOperationException("No PDF URL available to extract references from.");
            }

            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                using var response = await httpClient.GetAsync(pdfUrl, cancellationToken);
                response.EnsureSuccessStatusCode();

                using var pdfStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                
                var extractedRefs = await _grobidService.ExtractReferencesAsync(pdfStream, fileName, cancellationToken);
                
                if (extractedRefs != null && extractedRefs.Any())
                {
                    var existingDetected = await _unitOfWork.CandidatePapers.FindAllAsync(
                        c => c.OriginPaperId == paperId && c.Status == CandidateStatus.Detected, 
                        isTracking: true, 
                        cancellationToken);
                    
                    if (existingDetected.Any())
                    {
                        await _unitOfWork.CandidatePapers.RemoveMultipleAsync(existingDetected, cancellationToken);
                    }

                    var newCandidates = extractedRefs.Select(r => new CandidatePaper
                    {
                        Id = Guid.NewGuid(),
                        ReviewProcessId = processId,
                        OriginPaperId = paperId,
                        Title = r.Title,
                        Authors = r.Authors,
                        PublicationYear = r.PublishedYear,
                        DOI = r.DOI,
                        RawReference = r.RawReference,
                        NormalizedReference = r.Title?.ToLowerInvariant() ?? r.RawReference?.ToLowerInvariant() ?? string.Empty,
                        Status = CandidateStatus.Detected,
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = DateTimeOffset.UtcNow
                    }).ToList();

                    await _unitOfWork.CandidatePapers.AddRangeAsync(newCandidates, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract references for paper {PaperId} in process {ProcessId}.", paperId, processId);
                throw new InvalidOperationException("Failed to extract references due to PDF download or GROBID error.", ex);
            }
        }

        public async Task<PaginatedResponse<CandidatePaperDto>> GetCandidatePapersAsync(Guid processId, GetCandidatePapersRequest request, CancellationToken cancellationToken = default)
        {
            var query = _unitOfWork.CandidatePapers.GetCandidatesQueryable(processId);

            // Filtering
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var search = request.SearchTerm.ToLower();
                query = query.Where(c => 
                    c.Title.ToLower().Contains(search) || 
                    (c.Authors != null && c.Authors.ToLower().Contains(search)) ||
                    (c.DOI != null && c.DOI.ToLower().Contains(search)) ||
                    c.OriginPaper.Title.ToLower().Contains(search)
                );
            }

            if (request.Status.HasValue)
            {
                query = query.Where(c => (int)c.Status == (int)request.Status.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Year))
            {
                query = query.Where(c => c.PublicationYear == request.Year);
            }

            // Pagination
            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(c => new CandidatePaperDto
                {
                    CandidateId = c.Id,
                    ReviewProcessId = c.ReviewProcessId,
                    OriginPaperId = c.OriginPaperId ?? Guid.Empty,
                    OriginPaperTitle = c.OriginPaper.Title,
                    OriginPaperAuthors = c.OriginPaper.Authors,
                    Title = c.Title,
                    Authors = c.Authors,
                    PublicationYear = c.PublicationYear,
                    DOI = c.DOI,
                    RawReference = c.RawReference,
                    NormalizedReference = c.NormalizedReference,
                    Status = c.Status,
                    StatusText = c.Status.ToString()
                })
                .ToListAsync(cancellationToken);

            return new PaginatedResponse<CandidatePaperDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task RejectCandidatePapersAsync(Guid processId, RejectCandidatePaperRequest request, CancellationToken cancellationToken = default)
        {
            foreach (var candidateId in request.CandidateIds)
            {
                var candidate = await _unitOfWork.CandidatePapers.FindSingleAsync(c => c.Id == candidateId && c.ReviewProcessId == processId, isTracking: true, cancellationToken);
                if (candidate != null && candidate.Status == CandidateStatus.Detected)
                {
                    candidate.Status = CandidateStatus.Rejected;
                    candidate.ModifiedAt = DateTimeOffset.UtcNow;
                    await _unitOfWork.CandidatePapers.UpdateAsync(candidate, cancellationToken);
                }
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task SelectCandidatePapersAsync(Guid processId, SelectCandidatePaperRequest request, Guid userId, CancellationToken cancellationToken = default)
        {
            var reviewProcess = await _unitOfWork.ReviewProcesses.FindSingleAsync(rp => rp.Id == processId, isTracking: false, cancellationToken);
            if (reviewProcess == null)
            {
                throw new InvalidOperationException($"Review process {processId} not found.");
            }

            var idProcess = await _unitOfWork.IdentificationProcesses.FindSingleAsync(
                ip => ip.ReviewProcessId == processId,
                isTracking: false,
                cancellationToken);

            if (idProcess == null)
            {
                throw new InvalidOperationException($"No Identification Process found for review process {processId}. Papers must be part of an identification snapshot.");
            }

            var candidates = new List<CandidatePaper>();
            foreach (var cId in request.CandidateIds)
            {
                var c = await _unitOfWork.CandidatePapers.FindSingleAsync(x => x.Id == cId && x.ReviewProcessId == processId, isTracking: true, cancellationToken);
                if (c != null && c.Status == CandidateStatus.Detected)
                {
                    candidates.Add(c);
                }
            }

            if (!candidates.Any()) return;

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                foreach (var candidate in candidates)
                {
                    var reference = new ExtractedReference
                    {
                        Title = candidate.Title,
                        Authors = candidate.Authors,
                        DOI = candidate.DOI,
                        PublishedYear = candidate.PublicationYear,
                        RawReference = candidate.RawReference
                    };

                    var match = await _referenceMatchingService.MatchAsync(reference, cancellationToken);

                    Guid targetPaperId;

                    if (match.MatchedPaper != null && match.ConfidenceScore >= 0.6m)
                    {
                        if (match.ConfidenceScore >= 0.85m)
                        {
                            candidate.Status = CandidateStatus.Duplicate;
                        }
                        else
                        {
                            candidate.Status = CandidateStatus.Duplicate;
                            _logger.LogWarning("Medium confidence match ({Score}) for Candidate {CandidateId} with Paper {PaperId}.", match.ConfidenceScore, candidate.Id, match.MatchedPaper.Id);
                        }

                        targetPaperId = match.MatchedPaper.Id;
                    }
                    else
                    {
                        candidate.Status = CandidateStatus.Imported;

                        var newPaper = new Paper
                        {
                            Id = Guid.NewGuid(),
                            ProjectId = reviewProcess.ProjectId,
                            ReviewProcessId = processId,
                            Title = candidate.Title ?? string.Empty,
                            Authors = candidate.Authors ?? string.Empty,
                            DOI = candidate.DOI,
                            PublicationYear = candidate.PublicationYear,
                            SourceType = PaperSourceType.Snowballing,
                            Source = "Snowballing (GROBID)",
                            ImportedAt = DateTimeOffset.UtcNow,
                            ImportedBy = userId.ToString(),
                            CreatedAt = DateTimeOffset.UtcNow,
                            ModifiedAt = DateTimeOffset.UtcNow
                        };

                        if (int.TryParse(candidate.PublicationYear, out int year))
                        {
                            newPaper.PublicationYearInt = year;
                        }

                        await _unitOfWork.Papers.AddAsync(newPaper, cancellationToken);

                        // Enrich with external metadata from OpenAlex
                        await _paperEnrichmentService.EnrichFromOpenAlexAsync(newPaper, cancellationToken);

                        // Add to Identification Snapshot (incremental)
                        var snapshotPaper = new IdentificationProcessPaper
                        {
                            Id = Guid.NewGuid(),
                            IdentificationProcessId = idProcess.Id,
                            PaperId = newPaper.Id,
                            IncludedAfterDedup = true,
                            SourceType = PaperSourceType.Snowballing,
                            CreatedAt = DateTimeOffset.UtcNow,
                            ModifiedAt = DateTimeOffset.UtcNow
                        };

                        await _unitOfWork.IdentificationProcessPapers.AddAsync(snapshotPaper, cancellationToken);

                        targetPaperId = newPaper.Id;
                    }

                    if (candidate.OriginPaperId.HasValue && candidate.OriginPaperId.Value != targetPaperId)
                    {
                        var sourceId = candidate.OriginPaperId.Value;
                        var exists = await _unitOfWork.PaperCitations.AnyAsync(
                            x => x.SourcePaperId == sourceId && x.TargetPaperId == targetPaperId,
                            isTracking: false,
                            cancellationToken);

                        if (!exists)
                        {
                            var citation = new PaperCitation
                            {
                                Id = Guid.NewGuid(),
                                SourcePaperId = sourceId,
                                TargetPaperId = targetPaperId,
                                RawReference = candidate.RawReference,
                                ConfidenceScore = match.ConfidenceScore > 0 ? match.ConfidenceScore : 0.9m,
                                Source = CitationSource.Grobid,
                                CreatedAt = DateTimeOffset.UtcNow,
                                ModifiedAt = DateTimeOffset.UtcNow
                            };

                            await _unitOfWork.PaperCitations.AddAsync(citation, cancellationToken);
                        }
                    }

                    candidate.ModifiedAt = DateTimeOffset.UtcNow;
                    await _unitOfWork.CandidatePapers.UpdateAsync(candidate, cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
    }
}
