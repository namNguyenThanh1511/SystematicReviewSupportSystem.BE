using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Writer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.CandidatePaperService.DTOs;
using SRSS.IAM.Services.GrobidClient;
using SRSS.IAM.Services.GrobidClient.DTOs;
using SRSS.IAM.Services.ReferenceMatchingService;
using SRSS.IAM.Services.ReferenceMatchingService.DTOs;
using SRSS.IAM.Services.ReferenceProcessingService;
using SRSS.IAM.Services.Utils;
using SRSS.IAM.Services.DTOs.Common;

namespace SRSS.IAM.Services.CandidatePaperService
{
    public class CandidatePaperService : ICandidatePaperService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGrobidService _grobidService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CandidatePaperService> _logger;
        private readonly IReferenceMatchingService _referenceMatchingService;
        private readonly System.Threading.Channels.Channel<ReferenceProcessingJob> _jobChannel;

        public CandidatePaperService(
            IUnitOfWork unitOfWork,
            IGrobidService grobidService,
            IHttpClientFactory httpClientFactory,
            ILogger<CandidatePaperService> logger,
            IReferenceMatchingService referenceMatchingService,
            System.Threading.Channels.Channel<ReferenceProcessingJob> jobChannel)
        {
            _unitOfWork = unitOfWork;
            _grobidService = grobidService;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _referenceMatchingService = referenceMatchingService;
            _jobChannel = jobChannel;
        }

        public async Task ExtractReferencesFromPaperAsync(Guid paperId, CancellationToken cancellationToken = default)
        {
            var paper = await _unitOfWork.Papers.FindSingleAsync(p => p.Id == paperId, isTracking: true, cancellationToken);
            if (paper == null)
            {
                throw new InvalidOperationException($"Paper with ID {paperId} not found.");
            }

            var pdfUrl = paper.PdfUrl;
            if (string.IsNullOrWhiteSpace(pdfUrl))
            {
                throw new InvalidOperationException("No PDF URL available to extract references from.");
            }

            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                using var response = await httpClient.GetAsync(pdfUrl, cancellationToken);
                response.EnsureSuccessStatusCode();

                using var pdfMemoryStream = new MemoryStream();
                await response.Content.CopyToAsync(pdfMemoryStream, cancellationToken);
                pdfMemoryStream.Position = 0;

                // 1. Compute Hash for caching
                var hash = HashHelper.ComputeSha256Hash(pdfMemoryStream);
                pdfMemoryStream.Position = 0;

                var paperPdf = await _unitOfWork.PaperPdfs.GetLatestPaperPdfAsync(paperId, cancellationToken);

                // Update paper and pdf hash if not set (or different)
                bool needsSave = false;
                if (paper.CurrentFileHash != hash)
                {
                    paper.CurrentFileHash = hash;
                    needsSave = true;
                }
                if (paperPdf != null && paperPdf.FileHash != hash)
                {
                    paperPdf.FileHash = hash;
                    needsSave = true;
                }

                if (needsSave)
                {
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                // 2. Cache Hit Logic via PaperPdf: Check for any other PDF with the same hash already extracted inside a project
                var existingPaperPdfWithRefs = await _unitOfWork.PaperPdfs.FindFirstOrDefaultAsync(
                    p => p.FileHash == hash && p.RefsExtracted == true && p.ProjectId == paper.ProjectId,
                    isTracking: false,
                    cancellationToken);

                if (existingPaperPdfWithRefs != null)
                {
                    var existingCandidates = await _unitOfWork.CandidatePapers.FindAllAsync(
                        c => c.OriginPaperId == existingPaperPdfWithRefs.PaperId,
                        isTracking: false,
                        cancellationToken);

                    if (existingCandidates.Any())
                    {
                        _logger.LogInformation("Cache hit for paper {PaperId} with hash {Hash}. Cloning {Count} candidates from paper {SourcePaperId}.",
                            paperId, hash, existingCandidates.Count(), existingPaperPdfWithRefs.PaperId);

                        // Chỉ xóa những Candidate chưa được Select và chưa bị Reject
                        // Giữ lại những cái Status = Rejected hoặc IsSelectedInProjectRepository = true
                        var removableCandidates = await _unitOfWork.CandidatePapers.FindAllAsync(
                            c => c.OriginPaperId == paperId
                                && c.Status != CandidateStatus.Rejected
                                && c.IsSelectedInProjectRepository == false,
                            isTracking: true, cancellationToken);
                        if (removableCandidates.Any())
                        {
                            await _unitOfWork.CandidatePapers.RemoveMultipleAsync(removableCandidates, cancellationToken);
                        }
                        var needCloneCandidates = existingCandidates.Where(c => c.Status != CandidateStatus.Rejected && c.IsSelectedInProjectRepository == false).ToList();

                        var clonedCandidates = needCloneCandidates.Select(c => new CandidatePaper
                        {
                            Id = Guid.NewGuid(),
                            OriginPaperId = paperId,
                            Title = c.Title,
                            Authors = c.Authors,
                            PublicationYear = c.PublicationYear,
                            DOI = c.DOI,
                            RawReference = c.RawReference,
                            ReferenceType = c.ReferenceType,
                            NormalizedReference = c.NormalizedReference,
                            Status = CandidateStatus.Detected, // Reset to Trigger processing
                            CreatedAt = DateTimeOffset.UtcNow,
                            ModifiedAt = DateTimeOffset.UtcNow
                        }).ToList();

                        await _unitOfWork.CandidatePapers.AddRangeAsync(clonedCandidates, cancellationToken);

                        // Update current paperPdf.RefsExtracted = true
                        if (paperPdf != null)
                        {
                            paperPdf.RefsExtracted = true;
                        }

                        await _unitOfWork.SaveChangesAsync(cancellationToken);

                        await _jobChannel.Writer.WriteAsync(new ReferenceProcessingJob
                        {
                            ProjectId = paper.ProjectId,
                            PaperId = paperId
                        }, cancellationToken);

                        return;
                    }
                }

                // 3. PDF Splitting & GROBID API (Cache Miss)
                _logger.LogInformation("Cache miss for paper {PaperId}. Extracting references via GROBID with PDF splitting.", paperId);

                Stream streamToSend = pdfMemoryStream;
                var fileName = paperPdf?.FileName ?? paper.PdfFileName ?? "paper.pdf";

                try
                {
                    // Attempt to extract only the last 8 pages to save GROBID processing time
                    streamToSend = ExtractLastPagesAsync(pdfMemoryStream, 8);
                    streamToSend.Position = 0;

                    var extractedRefs = await _grobidService.ExtractReferencesAsync(streamToSend, fileName, cancellationToken);

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
                            OriginPaperId = paperId,
                            Title = r.Title,
                            Authors = r.Authors,
                            PublicationYear = r.PublishedYear,
                            DOI = r.DOI,
                            RawReference = r.RawReference,
                            ReferenceType = r.ReferenceType,
                            NormalizedReference = r.Title?.ToLowerInvariant() ?? r.RawReference?.ToLowerInvariant() ?? string.Empty,
                            Status = CandidateStatus.Detected,
                            CreatedAt = DateTimeOffset.UtcNow,
                            ModifiedAt = DateTimeOffset.UtcNow
                        }).ToList();

                        await _unitOfWork.CandidatePapers.AddRangeAsync(newCandidates, cancellationToken);

                        // Finally update current paperPdf.RefsExtracted = true
                        if (paperPdf != null)
                        {
                            paperPdf.RefsExtracted = true;
                        }

                        await _unitOfWork.SaveChangesAsync(cancellationToken);

                        // Immediately process candidates: match, create papers, create citations
                        // Converted to background job for performance
                        await _jobChannel.Writer.WriteAsync(new ReferenceProcessingJob
                        {
                            ProjectId = paper.ProjectId,
                            PaperId = paperId
                        }, cancellationToken);
                    }
                }
                finally
                {
                    // Only dispose if it was a new stream created by ExtractLastPagesAsync
                    if (streamToSend != pdfMemoryStream)
                    {
                        streamToSend.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract references for paper {PaperId} in project {ProjectId}.", paperId, paper.ProjectId);
                throw new InvalidOperationException("Failed to extract references due to PDF download or GROBID error.", ex);
            }
        }


        private Stream ExtractLastPagesAsync(Stream sourceStream, int pageCount)
        {
            try
            {
                // Note: PdfDocument.Open might throw if PDF is encrypted or corrupted
                using var document = PdfDocument.Open(sourceStream);
                var totalPages = document.NumberOfPages;

                // If the document is small, just use the original stream
                if (totalPages <= pageCount)
                {
                    sourceStream.Position = 0;
                    return sourceStream;
                }

                var startPage = Math.Max(1, totalPages - pageCount + 1);
                var builder = new PdfDocumentBuilder();

                for (int i = startPage; i <= totalPages; i++)
                {
                    builder.AddPage(document, i);
                }

                var pdfBytes = builder.Build();
                var outputStream = new MemoryStream(pdfBytes);
                outputStream.Position = 0;

                _logger.LogInformation("Successfully split PDF to last {Count} pages (from page {StartPage} to {TotalPages}).",
                    pageCount, startPage, totalPages);

                return outputStream;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to split PDF using PdfPig. Falling back to original stream.");
                sourceStream.Position = 0;
                return sourceStream;
            }
        }

        public async Task<PaginatedResponse<CandidatePaperDto>> GetCandidatePapersAsync(Guid paperId, GetCandidatePapersRequest request, CancellationToken cancellationToken = default)
        {
            var query = _unitOfWork.CandidatePapers.GetCandidatesQueryable();

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
                    StatusText = c.Status.ToString(),
                    ConfidenceScore = c.ConfidenceScore
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

        public async Task RejectCandidatePapersAsync(RejectCandidatePaperRequest request, CancellationToken cancellationToken = default)
        {
            foreach (var candidateId in request.CandidateIds)
            {
                var candidate = await _unitOfWork.CandidatePapers.FindSingleAsync(c => c.Id == candidateId, isTracking: true, cancellationToken);
                if (candidate != null && (candidate.Status == CandidateStatus.Detected || candidate.Status == CandidateStatus.Matched))
                {
                    candidate.Status = CandidateStatus.Rejected;
                    candidate.ModifiedAt = DateTimeOffset.UtcNow;
                    await _unitOfWork.CandidatePapers.UpdateAsync(candidate, cancellationToken);
                }
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Selects candidates for inclusion in the project.
        /// Citations and papers are already created during processing — this method
        /// only handles adding papers to the Paper .
        /// </summary>
        public async Task SelectCandidatePapersAsync(SelectCandidatePaperRequest request, Guid userId, CancellationToken cancellationToken = default)
        {

            var candidates = new List<CandidatePaper>();
            foreach (var cId in request.CandidateIds)
            {
                var c = await _unitOfWork.CandidatePapers.FindSingleAsync(
                    x => x.Id == cId,
                    isTracking: true,
                    cancellationToken);

                // Only allow selecting candidates that have not been selected before to prevent duplicates in the identification snapshot.
                if (c != null && !c.IsSelectedInProjectRepository)
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
                    // Step 1: Resolve existing Paper in project or create a new one
                    var paper = await ResolveOrCreatePaperAsync(candidate, request.ProjectId, cancellationToken);
                    var targetPaperId = paper.Id;

                    // Step 2: Handle Delayed Citation Creation
                    if (candidate.OriginPaperId.HasValue)
                    {
                        var existingCitation = await _unitOfWork.PaperCitations.FindSingleAsync(
                            pc => pc.SourcePaperId == candidate.OriginPaperId.Value
                               && pc.TargetPaperId == targetPaperId,
                            isTracking: false,
                            cancellationToken);

                        if (existingCitation == null)
                        {
                            // Calculate Citation Confidence
                            decimal extractionQuality = candidate.ExtractionQualityScore;
                            decimal matchConfidence = candidate.MatchConfidenceScore;

                            decimal finalConfidence = matchConfidence == 0
                                ? extractionQuality * 0.9m
                                : (matchConfidence * 0.7m) + (extractionQuality * 0.3m);

                            finalConfidence = Math.Clamp(finalConfidence, 0m, 1m);

                            bool isLowConfidence = finalConfidence < 0.75m
                                                   || (matchConfidence > 0 && matchConfidence < 0.6m)
                                                   || extractionQuality < 0.4m;

                            var citation = new PaperCitation
                            {
                                Id = Guid.NewGuid(),
                                SourcePaperId = candidate.OriginPaperId.Value,
                                TargetPaperId = targetPaperId,
                                ReferenceType = candidate.ReferenceType,
                                RawReference = candidate.RawReference,
                                ConfidenceScore = finalConfidence,
                                ExtractionQuality = extractionQuality,
                                MatchConfidence = matchConfidence,
                                Source = CitationSource.Grobid,
                                IsLowConfidence = isLowConfidence,
                                CreatedAt = DateTimeOffset.UtcNow,
                                ModifiedAt = DateTimeOffset.UtcNow
                            };

                            await _unitOfWork.PaperCitations.AddAsync(citation, cancellationToken);

                            // We will save changes at the end of the loop or here if needed.
                            // Let's call SaveChanges to get CitationId if we were to assign it, 
                            // though Guid.NewGuid() is assigned.
                            await _unitOfWork.SaveChangesAsync(cancellationToken);
                            candidate.CitationId = citation.Id;
                        }
                        else
                        {
                            candidate.CitationId = existingCitation.Id;
                        }
                    }

                    // Step 4: Finalize Candidate
                    candidate.IsSelectedInProjectRepository = true;
                    candidate.SelectedAt = DateTimeOffset.UtcNow;
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

        public async Task<PaginatedResponse<PaperWithCandidateDto>> GetPapersWithCandidatesAsync(
            Guid projectId,
            GetPapersRequest request,
            CancellationToken cancellationToken = default)
        {
            var query = _unitOfWork.Papers.GetQueryable(p => p.ProjectId == projectId);

            // Filtering
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var search = request.SearchTerm.ToLower();
                query = query.Where(p =>
                    p.Title.ToLower().Contains(search) ||
                    (p.Authors != null && p.Authors.ToLower().Contains(search)) ||
                    (p.DOI != null && p.DOI.ToLower().Contains(search))
                );
            }

            if (!string.IsNullOrWhiteSpace(request.Year))
            {
                query = query.Where(p => p.PublicationYear == request.Year);
            }

            // Optimize count using GroupBy on CandidatePapers
            var candidateStats = _unitOfWork.CandidatePapers.GetCandidatesQueryable()
           .GroupBy(c => c.OriginPaperId)
           .Select(g => new
           {
               PaperId = g.Key,
               Count = (int?)g.Count(),
               SuggestedCount = (int?)g.Count(c => c.Status == CandidateStatus.Suggested),
               DupCount = (int?)g.Count(c => c.Status == CandidateStatus.Matched && c.IsSelectedInProjectRepository)
           });



            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                    .GroupJoin(
                        candidateStats,
                        p => p.Id,
                        c => c.PaperId,
                        (p, candidates) => new { p, stats = candidates.FirstOrDefault() }
                    )
                    .OrderByDescending(x => x.p.CreatedAt)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(x => new PaperWithCandidateDto
                    {
                        Id = x.p.Id,
                        Title = x.p.Title,
                        Authors = x.p.Authors ?? string.Empty,
                        Abstract = x.p.Abstract ?? string.Empty,
                        PublicationYear = x.p.PublicationYear ?? string.Empty,
                        DOI = x.p.DOI ?? string.Empty,
                        SourceType = x.p.SourceType.ToString(),
                        Source = x.p.Source ?? string.Empty,
                        PdfUrl = x.p.PdfUrl,
                        ImportedAt = x.p.ImportedAt ?? DateTimeOffset.MinValue,
                        CreatedAt = x.p.CreatedAt,
                        ModifiedAt = x.p.ModifiedAt,
                        // Dùng ?? cho các kiểu int? đã ép ở trên
                        CandidateCount = x.stats.Count ?? 0,
                        SuggestedCount = x.stats.SuggestedCount ?? 0,
                        DuplicateCount = x.stats.DupCount ?? 0
                    }).ToListAsync(cancellationToken);

            return new PaginatedResponse<PaperWithCandidateDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<PaginatedResponse<CandidatePaperDto>> GetCandidatesByPaperIdAsync(
            Guid paperId,
            GetCandidatePapersRequest request,
            CancellationToken cancellationToken = default)
        {
            var query = _unitOfWork.CandidatePapers.GetCandidatesQueryable()
                .Where(c => c.OriginPaperId == paperId);

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

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .ThenByDescending(c => c.Id)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(c => new CandidatePaperDto
                {
                    CandidateId = c.Id,
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
                    StatusText = c.Status.ToString(),
                    ConfidenceScore = c.ConfidenceScore,
                    ExtractionQualityScore = c.ExtractionQualityScore,
                    MatchConfidenceScore = c.MatchConfidenceScore,
                    IsSelectedInProjectRepository = c.IsSelectedInProjectRepository,
                    ValidationNote = c.ValidationNote
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

        private async Task<Paper> ResolveOrCreatePaperAsync(CandidatePaper candidate, Guid projectId, CancellationToken cancellationToken)
        {
            // 1. If TargetPaperId is already set, verify it exists
            if (candidate.TargetPaperId.HasValue)
            {
                var existingPaper = await _unitOfWork.Papers.FindSingleAsync(
                    p => p.Id == candidate.TargetPaperId.Value && p.ProjectId == projectId,
                    isTracking: true,
                    cancellationToken);

                if (existingPaper != null)
                {
                    return existingPaper;
                }
            }

            // 2. Last-minute check for project-level match (handles late arrivals)
            var reference = new ExtractedReference
            {
                DOI = candidate.DOI,
                Title = candidate.Title,
                Authors = candidate.Authors,
                PublishedYear = candidate.PublicationYear,
                RawReference = candidate.RawReference
            };

            var match = await _referenceMatchingService.MatchAsync(reference, projectId, cancellationToken);
            if (match.MatchedPaper != null && match.ConfidenceScore >= 0.7m) // Strict threshold for auto-reuse
            {
                _logger.LogInformation("Reusing existing paper {PaperId} for candidate {CandidateId} in project {ProjectId}.",
                    match.MatchedPaper.Id, candidate.Id, projectId);

                candidate.TargetPaperId = match.MatchedPaper.Id;
                return match.MatchedPaper;
            }

            // 3. Create New Paper
            var newPaper = new Paper
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                Title = candidate.Title ?? "Untitled Paper",
                Authors = candidate.Authors ?? string.Empty,
                DOI = candidate.DOI,
                PublicationYear = candidate.PublicationYear,
                SearchSourceId = null,
                SourceType = PaperSourceType.Snowballing,
                Source = "Snowballing (GROBID)",
                ImportedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };

            if (int.TryParse(candidate.PublicationYear, out int year))
            {
                newPaper.PublicationYearInt = year;
            }

            await _unitOfWork.Papers.AddAsync(newPaper, cancellationToken);

            // We save changes here to ensure the paper exists before citation link
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            candidate.TargetPaperId = newPaper.Id;
            return newPaper;
        }
    }
}
