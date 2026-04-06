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
            if (idProcess != null)
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
                    // Note: TryEnrichPaperAsync usually runs asynchronously in the background in production,
                    // but here we keep it commented out as per recent USER changes or requirements.
                    // await TryEnrichPaperAsync(newPaper, candidate.Id, cancellationToken);
                    // No Match Found in snapshot — Do NOT create Paper here.
                    // Creation is delayed until the leader selects this candidate in screening.
                    targetPaperId = null;
                    matchConfidenceScore = 0m;
                    status = CandidateStatus.Detected;
                    isSelectedInScreening = false;
                }
            }


            // 4. Validation Note Generation (Prioritized)
            // Quality Warning: Extraction Integrity
            if (extractionQualityScore < 0.7m)
            {
                notes.Add($"Low extraction quality: Missing {string.Join(", ", extractionResult.MissingFields)}");
            }

            // 5. Citation Creation (DELAYED until selection)
            // Paper and Citation creation are moved to SelectCandidatePapersAsync in CandidatePaperService
            // to prevent creating unused resources for candidates that are never selected.

            // 6. Final State Persistence
            candidate.TargetPaperId = targetPaperId;
            candidate.ExtractionQualityScore = extractionQualityScore;
            candidate.MatchConfidenceScore = matchConfidenceScore;
            candidate.ConfidenceScore = extractionQualityScore; // Legacy fallback
            
            candidate.IsSelectedInScreening = isSelectedInScreening;
            candidate.SelectedAt = selectedAt;
            candidate.ValidationNote = notes.Any() ? string.Join("; ", notes) : null;
            candidate.Status = status; // Matched or Detected (extraction done)
            
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
