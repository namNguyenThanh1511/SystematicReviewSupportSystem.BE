using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.PaperEnrichmentService;
using SRSS.IAM.Services.ReferenceClassificationService;
using SRSS.IAM.Services.ReferenceMatchingService;
using SRSS.IAM.Services.ReferenceMatchingService.DTOs;

namespace SRSS.IAM.Services.ReferenceProcessingService
{
    public class ReferenceProcessingService : IReferenceProcessingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IReferenceMatchingService _referenceMatchingService;
        private readonly IPaperEnrichmentService _paperEnrichmentService;
        private readonly IReferenceClassificationService _referenceClassificationService;
        private readonly ILogger<ReferenceProcessingService> _logger;

        public ReferenceProcessingService(
            IUnitOfWork unitOfWork,
            IReferenceMatchingService referenceMatchingService,
            IPaperEnrichmentService paperEnrichmentService,
            IReferenceClassificationService referenceClassificationService,
            ILogger<ReferenceProcessingService> logger)
        {
            _unitOfWork = unitOfWork;
            _referenceMatchingService = referenceMatchingService;
            _paperEnrichmentService = paperEnrichmentService;
            _referenceClassificationService = referenceClassificationService;
            _logger = logger;
        }

        public async Task ProcessCandidatesAsync(Guid processId, Guid paperId, CancellationToken cancellationToken = default)
        {
            var reviewProcess = await _unitOfWork.ReviewProcesses.FindSingleAsync(
                rp => rp.Id == processId, isTracking: false, cancellationToken);

            if (reviewProcess == null)
            {
                throw new InvalidOperationException($"Review process {processId} not found.");
            }

            var detectedCandidates = await _unitOfWork.CandidatePapers.FindAllAsync(
                c => c.OriginPaperId == paperId
                     && c.ReviewProcessId == processId
                     && c.Status == CandidateStatus.Detected,
                isTracking: true,
                cancellationToken);

            if (!detectedCandidates.Any())
            {
                _logger.LogInformation(
                    "No detected candidates found for Paper {PaperId} in Process {ProcessId}. Skipping processing.",
                    paperId, processId);
                return;
            }

            _logger.LogInformation(
                "Starting reference processing for {Count} candidates from Paper {PaperId}.",
                detectedCandidates.Count(), paperId);

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                foreach (var candidate in detectedCandidates)
                {
                    await ProcessSingleCandidateAsync(candidate, reviewProcess, cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation(
                    "Successfully processed {Count} candidates from Paper {PaperId}.",
                    detectedCandidates.Count(), paperId);
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        private async Task ProcessSingleCandidateAsync(
            CandidatePaper candidate,
            ReviewProcess reviewProcess,
            CancellationToken cancellationToken)
        {
            // 1. Fetch Identification Process Context for snapshot check
            var idProcess = await _unitOfWork.IdentificationProcesses.FindSingleAsync(
                x => x.ReviewProcessId == reviewProcess.Id, isTracking: false, cancellationToken);

            // 2. Initial Setup
            var extractionResult = CalculateExtractionQuality(candidate);
            var extractionScore = extractionResult.Score;
            var reference = MapToExtractedReference(candidate);
            var referenceType = candidate.ReferenceType != ReferenceType.Unknown 
                ? candidate.ReferenceType 
                : _referenceClassificationService.Classify(reference);

            Guid? targetPaperId = null;
            Guid? referenceEntityId = null;
            decimal extractionQualityScore = extractionResult.Score;
            decimal matchConfidenceScore = 0m;
            var status = candidate.Status;
            bool isSelectedInScreening = false;
            DateTimeOffset? selectedAt = null;
            var notes = new List<string>();

            // 3. Identification/Matching Logic (Snapshot-Only Strategy)
            if (referenceType == ReferenceType.AcademicPaper && idProcess != null)
            {
                // Step 1: Match against Snapshot only (Current Identification Process scope)
                var match = await _referenceMatchingService.MatchAgainstSnapshotAsync(reference, idProcess.Id, cancellationToken);

                if (match.MatchedPaper != null && match.ConfidenceScore >= 0.6m)
                {
                    // Match Found: Link to existing paper in the snapshot
                    targetPaperId = match.MatchedPaper.Id;
                    matchConfidenceScore = match.ConfidenceScore;
                    status = CandidateStatus.Matched;
                    isSelectedInScreening = true;

                    // Identity Warning: Snapshot Duplicate
                    if (match.IsSnapshotDuplicate)
                    {
                        notes.Add("Already in screening dataset");

                        var existingSnapshot = await _unitOfWork.IdentificationProcessPapers.FindSingleAsync(
                            ipp => ipp.IdentificationProcessId == idProcess.Id && ipp.PaperId == targetPaperId.Value,
                            isTracking: false, cancellationToken);
                        
                        if (existingSnapshot != null)
                        {
                            selectedAt = existingSnapshot.CreatedAt;
                        }
                    }

                    // Identity Warning: Uncertain Match
                    if (matchConfidenceScore >= 0.6m && matchConfidenceScore < 0.85m)
                    {
                        notes.Add("Uncertain match with existing paper - please verify identity");
                    }

                    _logger.LogInformation(
                        "Snapshot match found (Score: {MatchScore}) for Candidate {CandidateId}.",
                        matchConfidenceScore, candidate.Id);
                }
                else
                {
                    // Step 2: No Match Found in snapshot — Create New Paper for this project
                    var newPaper = new Paper
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = reviewProcess.ProjectId,
                        Title = candidate.Title ?? string.Empty,
                        Authors = candidate.Authors ?? string.Empty,
                        DOI = candidate.DOI,
                        PublicationYear = candidate.PublicationYear,
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
                    
                    // Note: TryEnrichPaperAsync usually runs asynchronously in the background in production,
                    // but here we keep it commented out as per recent USER changes or requirements.
                    // await TryEnrichPaperAsync(newPaper, candidate.Id, cancellationToken);

                    targetPaperId = newPaper.Id;
                    matchConfidenceScore = 0m;
                    status = CandidateStatus.Created;
                    isSelectedInScreening = false;
                }
            }
            else if (referenceType != ReferenceType.AcademicPaper)
            {
                // Non-paper reference — resolve as reference entity
                var refEntity = new ReferenceEntity
                {
                    Id = Guid.NewGuid(),
                    Title = candidate.Title,
                    Authors = candidate.Authors,
                    DOI = candidate.DOI,
                    Url = ExtractUrl(candidate.RawReference),
                    Type = referenceType,
                    RawReference = candidate.RawReference,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                };

                await _unitOfWork.ReferenceEntities.AddAsync(refEntity, cancellationToken);
                referenceEntityId = refEntity.Id;
                matchConfidenceScore = 1.0m;
                status = CandidateStatus.Resolved;
            }

            // 4. Validation Note Generation (Prioritized)
            // Quality Warning: Extraction Integrity
            if (extractionQualityScore < 0.7m)
            {
                notes.Add($"Low extraction quality: Missing {string.Join(", ", extractionResult.MissingFields)}");
            }

            // 5. Citation Creation (Always link extraction outcome to source)
            if (candidate.OriginPaperId.HasValue && (targetPaperId.HasValue || referenceEntityId.HasValue))
            {
                var citation = await CreateCitationIfNotExistsAsync(
                    sourcePaperId: candidate.OriginPaperId.Value,
                    targetPaperId: targetPaperId,
                    referenceEntityId: referenceEntityId,
                    referenceType: referenceType,
                    rawReference: candidate.RawReference,
                    extractionQuality: extractionQualityScore,
                    matchConfidence: matchConfidenceScore,
                    cancellationToken: cancellationToken);

                if (citation != null)
                {
                    candidate.CitationId = citation.Id;
                }
            }

            // 6. Final State Persistence
            candidate.TargetPaperId = targetPaperId;
            candidate.ReferenceEntityId = referenceEntityId;
            candidate.ExtractionQualityScore = extractionQualityScore;
            candidate.MatchConfidenceScore = matchConfidenceScore;
            candidate.ConfidenceScore = extractionQualityScore; // Legacy fallback
            
            candidate.IsSelectedInScreening = isSelectedInScreening;
            candidate.SelectedAt = selectedAt;
            candidate.ValidationNote = notes.Any() ? string.Join("; ", notes) : null;
            candidate.Status = (status == CandidateStatus.Matched || status == CandidateStatus.Created) 
                ? status 
                : CandidateStatus.Resolved; 
            
            candidate.ModifiedAt = DateTimeOffset.UtcNow;
            await _unitOfWork.CandidatePapers.UpdateAsync(candidate, cancellationToken);
        }

        private string? ExtractUrl(string? rawReference)
        {
            if (string.IsNullOrWhiteSpace(rawReference)) return null;

            // Simple URL extraction logic
            var startIndex = rawReference.IndexOf("http", StringComparison.OrdinalIgnoreCase);
            if (startIndex == -1) return null;

            var urlPart = rawReference.Substring(startIndex);
            var spaceIndex = urlPart.IndexOf(' ');
            
            return spaceIndex == -1 ? urlPart : urlPart.Substring(0, spaceIndex);
        }

        private async Task TryEnrichPaperAsync(Paper paper, Guid candidateId, CancellationToken cancellationToken)
        {
            try
            {
                await _paperEnrichmentService.EnrichFromOpenAlexAsync(paper, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to enrich paper from OpenAlex for Candidate {CandidateId}. " +
                    "Paper {PaperId} was created without external metadata.",
                    candidateId, paper.Id);
                // Do NOT rethrow — enrichment failure should not block citation creation
            }
        }

        private async Task<PaperCitation?> CreateCitationIfNotExistsAsync(
            Guid sourcePaperId,
            Guid? targetPaperId,
            Guid? referenceEntityId,
            ReferenceType referenceType,
            string? rawReference,
            decimal extractionQuality, // Điểm chất lượng trích xuất
            decimal matchConfidence,   // Điểm tin cậy đối soát
            CancellationToken cancellationToken)
        {
            // 1. Kiểm tra tồn tại (Giữ nguyên logic cũ nhưng tối ưu query)
            var existing = await _unitOfWork.PaperCitations.FindSingleAsync(
                x => x.SourcePaperId == sourcePaperId 
                    && (targetPaperId != null ? x.TargetPaperId == targetPaperId : x.ReferenceEntityId == referenceEntityId),
                isTracking: false,
                cancellationToken);

            if (existing != null)
            {
                return existing;
            }

            // 2. Tính toán Confidence Score tổng hợp cho Citation
            // Condition A (New Paper): If matchConfidence == 0, set finalConfidence = extractionQuality * 0.9m
            // Condition B (Matched Paper): If matchConfidence > 0, set weighted finalConfidence
            decimal finalConfidence = matchConfidence == 0 
                ? extractionQuality * 0.9m 
                : (matchConfidence * 0.7m) + (extractionQuality * 0.3m);

            finalConfidence = Math.Clamp(finalConfidence, 0m, 1m);
            
            // Một citation bị coi là Low Confidence nếu:
            // - Điểm tổng hợp < 0.75
            // - HOẶC Match Confidence quá thấp (< 0.6) (Chỉ áp dụng khi có match)
            // - HOẶC Extraction Quality cực thấp (< 0.4)
            bool isLowConfidence = finalConfidence < 0.75m 
                                   || (matchConfidence > 0 && matchConfidence < 0.6m) 
                                   || extractionQuality < 0.4m;

            // 3. Khởi tạo Entity
            var citation = new PaperCitation
            {
                Id = Guid.NewGuid(),
                SourcePaperId = sourcePaperId,
                TargetPaperId = targetPaperId,
                ReferenceEntityId = referenceEntityId,
                ReferenceType = referenceType,
                RawReference = rawReference,
                
                // Lưu điểm tổng hợp
                ConfidenceScore = Math.Clamp(finalConfidence, 0m, 1m),
                
                // Lưu vết thêm các mốc điểm riêng lẻ 
                ExtractionQuality = extractionQuality, 
                MatchConfidence = matchConfidence,

                Source = CitationSource.Grobid,
                IsLowConfidence = isLowConfidence,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.PaperCitations.AddAsync(citation, cancellationToken);

            // 4. Logging chuyên sâu
            if (isLowConfidence)
            {
                _logger.LogWarning(
                    "Low-confidence citation created. Match: {MScore}, Extraction: {EScore}, Final: {FScore}. Path: {Source} -> {Target}",
                    matchConfidence, extractionQuality, finalConfidence, sourcePaperId, 
                    targetPaperId?.ToString() ?? referenceEntityId?.ToString());
            }

            return citation;
        }

        private static ExtractionQuality CalculateExtractionQuality(CandidatePaper candidate)
        {
            decimal score = 0.0m;
            var missingFields = new List<string>();

            // DOI (50%) - Must contain a slash to be considered valid for scoring
            if (!string.IsNullOrWhiteSpace(candidate.DOI) && candidate.DOI.Contains('/'))
            {
                score += 0.5m;
            }
            else
            {
                missingFields.Add("DOI");
            }

            // Title (30%)
            if (!string.IsNullOrWhiteSpace(candidate.Title))
            {
                if (candidate.Title.Length > 30)
                {
                    score += 0.3m;
                }
                else if (candidate.Title.Length >= 10)
                {
                    score += 0.15m;
                    missingFields.Add("Full Title");
                }
                else
                {
                    missingFields.Add("Title");
                }
            }
            else
            {
                missingFields.Add("Title");
            }

            // Authors (10%)
            if (!string.IsNullOrWhiteSpace(candidate.Authors))
            {
                score += 0.1m;
            }
            else
            {
                missingFields.Add("Authors");
            }

            // Year (10%) - Must be a valid 4-digit string
            if (!string.IsNullOrWhiteSpace(candidate.PublicationYear) && 
                System.Text.RegularExpressions.Regex.IsMatch(candidate.PublicationYear, @"^\d{4}$"))
            {
                score += 0.1m;
            }
            else
            {
                missingFields.Add("Year");
            }

            return new ExtractionQuality
            {
                Score = Math.Clamp(score, 0.0m, 1.0m),
                MissingFields = missingFields
            };
        }

        private class ExtractionQuality
        {
            public decimal Score { get; set; }
            public List<string> MissingFields { get; set; } = new();
        }

        private static ExtractedReference MapToExtractedReference(CandidatePaper candidate)
        {
            return new ExtractedReference
            {
                DOI = candidate.DOI,
                Title = candidate.Title,
                Authors = candidate.Authors,
                PublishedYear = candidate.PublicationYear,
                // Đảm bảo không bỏ sót Journal/Source
                // Journal = candidate.Journal ?? candidate.Source, 
                RawReference = candidate.RawReference
            };
        }
    }
}
