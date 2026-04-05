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
using SRSS.IAM.Services.DTOs.Common;
using SRSS.IAM.Services.ReferenceProcessingService;
using SRSS.IAM.Services.Utils;

namespace SRSS.IAM.Services.CandidatePaperService
{
    public class CandidatePaperService : ICandidatePaperService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGrobidService _grobidService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CandidatePaperService> _logger;
        private readonly System.Threading.Channels.Channel<ReferenceProcessingJob> _jobChannel;

        public CandidatePaperService(
            IUnitOfWork unitOfWork,
            IGrobidService grobidService,
            IHttpClientFactory httpClientFactory,
            ILogger<CandidatePaperService> logger,
            System.Threading.Channels.Channel<ReferenceProcessingJob> jobChannel)
        {
            _unitOfWork = unitOfWork;
            _grobidService = grobidService;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _jobChannel = jobChannel;
        }

        public async Task ExtractReferencesFromPaperAsync(Guid processId, Guid paperId, CancellationToken cancellationToken = default)
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
                        // Giữ lại những cái Status = Rejected hoặc IsSelectedInScreening = true
                        var removableCandidates = await _unitOfWork.CandidatePapers.FindAllAsync(
                            c => c.OriginPaperId == paperId 
                                && c.Status != CandidateStatus.Rejected 
                                && c.IsSelectedInScreening == false, 
                            isTracking: true, cancellationToken);
                        if (removableCandidates.Any())
                        {
                            await _unitOfWork.CandidatePapers.RemoveMultipleAsync(removableCandidates, cancellationToken);
                        }

                        var clonedCandidates = existingCandidates.Select(c => new CandidatePaper
                        {
                            Id = Guid.NewGuid(),
                            ReviewProcessId = processId,
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
                            ProcessId = processId,
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
                            ReviewProcessId = processId,
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
                            ProcessId = processId,
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
                _logger.LogError(ex, "Failed to extract references for paper {PaperId} in process {ProcessId}.", paperId, processId);
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

        public async Task RejectCandidatePapersAsync(Guid processId, RejectCandidatePaperRequest request, CancellationToken cancellationToken = default)
        {
            foreach (var candidateId in request.CandidateIds)
            {
                var candidate = await _unitOfWork.CandidatePapers.FindSingleAsync(c => c.Id == candidateId && c.ReviewProcessId == processId, isTracking: true, cancellationToken);
                if (candidate != null && (candidate.Status == CandidateStatus.Detected || candidate.Status == CandidateStatus.Matched || candidate.Status == CandidateStatus.Resolved))
                {
                    candidate.Status = CandidateStatus.Rejected;
                    candidate.ModifiedAt = DateTimeOffset.UtcNow;
                    await _unitOfWork.CandidatePapers.UpdateAsync(candidate, cancellationToken);
                }
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Selects candidates for inclusion in the review process.
        /// Citations and papers are already created during processing — this method
        /// only handles adding papers to the IdentificationProcess snapshot.
        /// </summary>
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
                var c = await _unitOfWork.CandidatePapers.FindSingleAsync(
                    x => x.Id == cId && x.ReviewProcessId == processId,
                    isTracking: true,
                    cancellationToken);

                // Only allow selecting candidates that have not been selected before to prevent duplicates in the identification snapshot.
                if (c != null && !c.IsSelectedInScreening)
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
                if (candidate.TargetPaperId.HasValue)
                {
                    var targetPaperId = candidate.TargetPaperId.Value;
                
                    // Check if paper is already in identification snapshot
                    var alreadyInSnapshot = await _unitOfWork.IdentificationProcessPapers.FindSingleAsync(
                        ipp => ipp.IdentificationProcessId == idProcess.Id && ipp.PaperId == targetPaperId,
                        isTracking: false,
                        cancellationToken);

                    if (alreadyInSnapshot == null)
                    {
                        var snapshotPaper = new IdentificationProcessPaper
                        {
                            Id = Guid.NewGuid(),
                            IdentificationProcessId = idProcess.Id,
                            PaperId = targetPaperId,
                            IncludedAfterDedup = true,
                            SourceType = PaperSourceType.Snowballing,
                            CreatedAt = DateTimeOffset.UtcNow,
                            ModifiedAt = DateTimeOffset.UtcNow
                        };

                        await _unitOfWork.IdentificationProcessPapers.AddAsync(snapshotPaper, cancellationToken);
                    }

                    candidate.IsSelectedInScreening = true;
                    candidate.SelectedAt = DateTimeOffset.UtcNow;
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

        public async Task<PaginatedResponse<PaperWithCandidateDto>> GetPapersWithCandidatesAsync(
            Guid processId,
            GetPapersRequest request,
            CancellationToken cancellationToken = default)
        {
            // Only papers that survived Identification (frozen snapshot) are eligible for Snowballing
            var identificationProcess = await _unitOfWork.IdentificationProcesses.FindSingleAsync(
                ip => ip.ReviewProcessId == processId,
                cancellationToken: cancellationToken);

            if (identificationProcess == null)
            {
                return new PaginatedResponse<PaperWithCandidateDto>
                {
                    Items = new List<PaperWithCandidateDto>(),
                    TotalCount = 0,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
            }

            var eligiblePaperIds = await _unitOfWork.IdentificationProcessPapers.GetIncludedPaperIdsByProcessAsync(
                identificationProcess.Id,
                cancellationToken);

            if (!eligiblePaperIds.Any())
            {
                return new PaginatedResponse<PaperWithCandidateDto>
                {
                    Items = new List<PaperWithCandidateDto>(),
                    TotalCount = 0,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
            }

            var query = _unitOfWork.Papers.GetPapersQueryable(eligiblePaperIds);

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
            var candidateStats = _unitOfWork.CandidatePapers.GetCandidatesQueryable(processId)
           .GroupBy(c => c.OriginPaperId)
           .Select(g => new
               {
                   PaperId = g.Key,
                   Count = (int?)g.Count(),
                   HighConfCount = (int?)g.Count(c => c.ConfidenceScore >= 0.8m),
                   DupCount = (int?)0 // Duplicate derived via logic if needed, but removed from Status
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
                        HighConfidenceCount = x.stats.HighConfCount ?? 0,
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
            Guid processId,
            Guid paperId,
            GetCandidatePapersRequest request,
            CancellationToken cancellationToken = default)
        {
            var query = _unitOfWork.CandidatePapers.GetCandidatesQueryable(processId)
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
                    StatusText = c.Status.ToString(),
                    ConfidenceScore = c.ConfidenceScore,
                    ExtractionQualityScore = c.ExtractionQualityScore,
                    MatchConfidenceScore = c.MatchConfidenceScore,
                    IsSelectedInScreening = c.IsSelectedInScreening,
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
    }
}
