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

        public async Task ProcessCandidatesAsync(Guid projectId, Guid paperId, CancellationToken cancellationToken = default)
        {


            var detectedCandidates = await _unitOfWork.CandidatePapers.FindAllAsync(
                c => c.OriginPaperId == paperId
                     && c.Status == CandidateStatus.Detected,
                isTracking: true,
                cancellationToken);

            if (!detectedCandidates.Any())
            {
                _logger.LogInformation(
                    "No detected candidates found for Paper {PaperId} in Project {projectId}. Skipping processing.",
                    paperId, projectId);
                return;
            }

            _logger.LogInformation(
                "Starting reference processing for {Count} candidates from Paper {PaperId}.",
                detectedCandidates.Count(), paperId);

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var candidatesList = detectedCandidates.ToList();
                var references = candidatesList.Select(MapToExtractedReference).ToList();

                var matchResults = (await _referenceMatchingService.MatchBatchInProjectAsync(
                    references, projectId, cancellationToken)).ToList();

                for (int i = 0; i < candidatesList.Count; i++)
                {
                    await ProcessSingleCandidateAsync(candidatesList[i], matchResults[i], cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation(
                    "Successfully processed {Count} candidates from Paper {PaperId}.",
                    candidatesList.Count, paperId);
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        private async Task ProcessSingleCandidateAsync(
            CandidatePaper candidate,
            MatchResult match,
            CancellationToken cancellationToken)
        {
            // 1. Initial Setup
            var extractionResult = CalculateExtractionQuality(candidate);

            Guid? targetPaperId = null;
            decimal extractionQualityScore = extractionResult.Score;
            decimal matchConfidenceScore = match.ConfidenceScore;
            var status = CandidateStatus.Detected;
            var notes = new List<string>();

            // 2. Matching Logic (Project-Scoped)
            if (match.MatchedPaper != null && matchConfidenceScore >= 0.6m)
            {
                // Match Found in Project
                targetPaperId = match.MatchedPaper.Id;
                status = CandidateStatus.Matched;

                if (matchConfidenceScore == 1m)
                {
                    notes.Add("Exact match found in project repository");
                }
                else
                {
                    notes.Add("Potential duplicate found in project repository");
                }

                _logger.LogInformation(
                    "Candidate {CandidateId} matched existing paper {TargetId} (Score: {MatchScore}).",
                    candidate.Id, targetPaperId, matchConfidenceScore);
            }
            else
            {
                // No Match Found — Tiered logic for Suggested vs Detected
                if (extractionQualityScore >= 0.75m)
                {
                    status = CandidateStatus.Suggested;
                    notes.Add("Ready for screening (High metadata quality)");

                    _logger.LogInformation(
                        "Candidate {CandidateId} marked as Suggested (Quality: {QualityScore}).",
                        candidate.Id, extractionQualityScore);
                }
                else
                {
                    status = CandidateStatus.Detected;
                    if (extractionResult.MissingFields.Any())
                    {
                        notes.Add($"Missing {string.Join(", ", extractionResult.MissingFields)}");
                    }

                    _logger.LogInformation(
                        "Candidate {CandidateId} marked as Detected (Quality: {QualityScore}).",
                        candidate.Id, extractionQualityScore);
                }
            }

            // 3. Final State Persistence
            candidate.TargetPaperId = targetPaperId;
            candidate.ExtractionQualityScore = extractionQualityScore;
            candidate.MatchConfidenceScore = matchConfidenceScore;
            candidate.ConfidenceScore = extractionQualityScore; // Legacy fallback

            candidate.ValidationNote = notes.Any() ? string.Join("; ", notes) : null;
            candidate.Status = status;

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
