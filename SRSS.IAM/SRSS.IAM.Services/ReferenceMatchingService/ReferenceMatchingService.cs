using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.ReferenceMatchingService.DTOs;

namespace SRSS.IAM.Services.ReferenceMatchingService
{
    public class ReferenceMatchingService : IReferenceMatchingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReferenceMatchingService> _logger;

        public ReferenceMatchingService(IUnitOfWork unitOfWork, ILogger<ReferenceMatchingService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<MatchResult> MatchAsync(ExtractedReference reference, CancellationToken cancellationToken = default)
        {
            if (reference == null)
            {
                return new MatchResult { MatchedPaper = null, ConfidenceScore = 0, Strategy = MatchStrategy.None };
            }

            // Step 1: DOI Matching (highest confidence)
            if (!string.IsNullOrWhiteSpace(reference.DOI))
            {
                var normalizedDoi = reference.DOI.Trim().ToLowerInvariant();
                var exactDoiMatch = await _unitOfWork.Papers.FindAllAsync(
                    p => p.DOI != null && p.DOI.ToLower() == normalizedDoi,
                    isTracking: false,
                    cancellationToken);

                var doiMatchedPaper = exactDoiMatch.FirstOrDefault();
                if (doiMatchedPaper != null)
                {
                    return new MatchResult
                    {
                        MatchedPaper = doiMatchedPaper,
                        ConfidenceScore = 1.0m,
                        Strategy = MatchStrategy.DOI
                    };
                }
            }

            if (string.IsNullOrWhiteSpace(reference.Title) || reference.Title.Length < 5)
            {
                return new MatchResult { MatchedPaper = null, ConfidenceScore = 0, Strategy = MatchStrategy.None };
            }

            var normalizedRefTitle = NormalizeText(reference.Title);
            var refTokens = Tokenize(normalizedRefTitle);

            if (!refTokens.Any())
            {
                return new MatchResult { MatchedPaper = null, ConfidenceScore = 0, Strategy = MatchStrategy.None };
            }

            // Step 2: Candidate Filtering
            int? refYear = null;
            if (int.TryParse(reference.PublishedYear, out var y))
            {
                refYear = y;
            }

            var keywords = refTokens.OrderByDescending(t => t.Length).Take(3).ToList();
            var kw0 = keywords.Count > 0 ? keywords[0].ToLowerInvariant() : null;
            var kw1 = keywords.Count > 1 ? keywords[1].ToLowerInvariant() : null;
            var kw2 = keywords.Count > 2 ? keywords[2].ToLowerInvariant() : null;

            IEnumerable<Paper> candidates;

            if (refYear.HasValue)
            {
                var minYear = refYear.Value - 1;
                var maxYear = refYear.Value + 1;
                candidates = await _unitOfWork.Papers.FindAllAsync(p =>
                    (p.PublicationYearInt >= minYear && p.PublicationYearInt <= maxYear) ||
                    (kw0 != null && p.Title.ToLower().Contains(kw0)) ||
                    (kw1 != null && p.Title.ToLower().Contains(kw1)) ||
                    (kw2 != null && p.Title.ToLower().Contains(kw2)),
                    isTracking: false, cancellationToken);
            }
            else
            {
                candidates = await _unitOfWork.Papers.FindAllAsync(p =>
                    (kw0 != null && p.Title.ToLower().Contains(kw0)) ||
                    (kw1 != null && p.Title.ToLower().Contains(kw1)) ||
                    (kw2 != null && p.Title.ToLower().Contains(kw2)),
                    isTracking: false, cancellationToken);
            }

            // Limit candidate set manually to prevent huge processing
            candidates = candidates.Take(100);

            Paper? bestMatch = null;
            decimal highestScore = 0;
            MatchStrategy bestStrategy = MatchStrategy.None;

            var refAuthors = ExtractAuthorLastNames(reference.Authors);

            foreach (var candidate in candidates)
            {
                var candidateNormalizedTitle = NormalizeText(candidate.Title);
                
                // Step 3: Title Similarity (Fuzzy Matching)
                decimal titleScore = 0;
                MatchStrategy currentStrategy = MatchStrategy.None;

                if (candidateNormalizedTitle == normalizedRefTitle)
                {
                    titleScore = 1.0m;
                    currentStrategy = MatchStrategy.TitleExact;
                }
                else
                {
                    var candidateTokens = Tokenize(candidateNormalizedTitle);
                    titleScore = ComputeJaccard(refTokens, candidateTokens);
                    currentStrategy = MatchStrategy.TitleFuzzy;
                }

                // Step 4: Author Matching
                var candidateAuthors = ExtractAuthorLastNames(candidate.Authors);
                decimal authorScore = ComputeAuthorScore(refAuthors, candidateAuthors);

                // Step 5: Year Matching
                decimal yearScore = 0;
                if (refYear.HasValue && candidate.PublicationYearInt.HasValue)
                {
                    if (refYear.Value == candidate.PublicationYearInt.Value)
                    {
                        yearScore = 1.0m;
                    }
                    else if (Math.Abs(refYear.Value - candidate.PublicationYearInt.Value) == 1)
                    {
                        yearScore = 0.5m;
                    }
                }

                // Final Score Formula => clamp result to [0, 1]
                decimal finalScore = (titleScore * 0.7m) + (authorScore * 0.2m) + (yearScore * 0.1m);
                if (finalScore > 1.0m) finalScore = 1.0m;
                if (finalScore < 0m) finalScore = 0m;

                // 🚫 False Positive Prevention (CRITICAL)
                if (titleScore < 0.6m || (titleScore < 0.7m && authorScore < 0.3m))
                {
                    continue; // Rejected
                }

                if (finalScore > highestScore)
                {
                    highestScore = finalScore;
                    bestMatch = candidate;
                    bestStrategy = currentStrategy;
                    if (titleScore == 1.0m)
                    {
                        bestStrategy = MatchStrategy.TitleExact;
                    }
                }
            }

            // Match Selection
            if (bestMatch != null && highestScore >= 0.6m) 
            {
                if (highestScore < 0.75m)
                {
                    _logger.LogInformation("Weak match found for Reference '{RawReference}' with Paper ID '{PaperId}'. Confidence: {Score}", reference.RawReference, bestMatch.Id, highestScore);
                }

                return new MatchResult
                {
                    MatchedPaper = bestMatch,
                    ConfidenceScore = highestScore,
                    Strategy = bestStrategy
                };
            }

            return new MatchResult { MatchedPaper = null, ConfidenceScore = 0, Strategy = MatchStrategy.None };
        }

        private string NormalizeText(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            
            var text = input.ToLowerInvariant();
            text = Regex.Replace(text, @"[^\w\s]", ""); // Remove punctuation
            text = Regex.Replace(text, @"\s+", " ");    // Collapse whitespace
            return text.Trim();
        }

        private List<string> Tokenize(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return new List<string>();
            return input.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        private decimal ComputeJaccard(List<string> a, List<string> b)
        {
            if (!a.Any() || !b.Any()) return 0m;
            
            var intersection = a.Intersect(b, StringComparer.OrdinalIgnoreCase).Count();
            var union = a.Union(b, StringComparer.OrdinalIgnoreCase).Count();
            
            if (union == 0) return 0m;
            
            return (decimal)intersection / union;
        }

        private List<string> ExtractAuthorLastNames(string? authors)
        {
            if (string.IsNullOrWhiteSpace(authors)) return new List<string>();

            var authorGroups = authors.Split(new string[] { ",", ";", " and " }, StringSplitOptions.RemoveEmptyEntries);
            var lastNames = new List<string>();

            foreach (var group in authorGroups)
            {
                var nameParts = group.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (nameParts.Length > 0)
                {
                    lastNames.Add(NormalizeText(nameParts.Last()));
                }
            }

            return lastNames;
        }

        private decimal ComputeAuthorScore(List<string> refAuthors, List<string> candidateAuthors)
        {
            if (!refAuthors.Any() || !candidateAuthors.Any()) return 0m;

            var overlap = refAuthors.Intersect(candidateAuthors, StringComparer.OrdinalIgnoreCase).Count();
            return (decimal)overlap / refAuthors.Count;
        }
    }
}
