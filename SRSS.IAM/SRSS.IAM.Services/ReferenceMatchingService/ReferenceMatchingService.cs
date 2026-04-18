using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.ReferenceMatchingService.DTOs;
using SRSS.IAM.Services.Utils;

namespace SRSS.IAM.Services.ReferenceMatchingService
{
    public class ReferenceMatchingService : IReferenceMatchingService
    {
        private readonly IUnitOfWork _unitOfWork;
        
        private static readonly HashSet<string> Stopwords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "the", "for", "and", "of", "to", "in", "using", "a", "an", "on", "with", "as", "by", "at", "it", "from"
        };
        private readonly ILogger<ReferenceMatchingService> _logger;

        public ReferenceMatchingService(IUnitOfWork unitOfWork, ILogger<ReferenceMatchingService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<MatchResult?> FindSemanticMatchAsync(
            float[] embedding,
            Guid currentPaperId,
            CancellationToken cancellationToken = default)
        {
            if (embedding == null || embedding.Length == 0)
            {
                return null;
            }

            var nearestEmbedding = await _unitOfWork.PaperEmbeddings.FindClosestByCosineDistanceAsync(
                embedding,
                currentPaperId,
                cancellationToken,
                take: 1);

            if (nearestEmbedding?.Paper == null)
            {
                return null;
            }

            var queryVector = new Vector(embedding);
            var candidateVector = nearestEmbedding.Embedding.ToArray();
            var similarity = CosineSimilarity(queryVector.ToArray(), candidateVector);

            if (similarity < 0.85f)
            {
                return null;
            }

            return new MatchResult
            {
                MatchedPaper = nearestEmbedding.Paper,
                MatchedPaperId = nearestEmbedding.PaperId,
                ConfidenceScore = (decimal)similarity,
                Strategy = MatchStrategy.Semantic
            };
        }

        public async Task<MatchResult> MatchAsync(ExtractedReference reference, Guid projectId, CancellationToken cancellationToken = default)
        {
            if (reference == null)
            {
                return new MatchResult { MatchedPaper = null, ConfidenceScore = 0, Strategy = MatchStrategy.None };
            }

            // Step 1: DOI Matching (highest confidence)
            var normalizedDoi = DoiHelper.Normalize(reference.DOI);
            if (!string.IsNullOrWhiteSpace(normalizedDoi))
            {
                var exactDoiMatch = await _unitOfWork.Papers.FindAllAsync(
                    p => p.ProjectId == projectId && p.DOI != null && p.DOI.ToLower() == normalizedDoi,
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

            int minYear = refYear.HasValue ? refYear.Value - 1 : 0;
            int maxYear = refYear.HasValue ? refYear.Value + 1 : 0;

            var candidates = await _unitOfWork.Papers.FindAllWithLimitAsync(p =>
                p.ProjectId == projectId && (
                (refYear.HasValue && p.PublicationYearInt >= minYear && p.PublicationYearInt <= maxYear) ||
                (kw0 != null && !string.IsNullOrWhiteSpace(kw0) && p.Title != null && p.Title.ToLower().Contains(kw0)) ||
                (kw1 != null && !string.IsNullOrWhiteSpace(kw1) && p.Title != null && p.Title.ToLower().Contains(kw1)) ||
                (kw2 != null && !string.IsNullOrWhiteSpace(kw2) && p.Title != null && p.Title.ToLower().Contains(kw2))),
                100, isTracking: false, cancellationToken);

            Paper? bestMatch = null;
            decimal highestScore = 0;
            MatchStrategy bestStrategy = MatchStrategy.None;

            var refAuthors = ExtractAuthorTokens(reference.Authors);

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
                    titleScore = ComputeTitleScore(normalizedRefTitle, candidateNormalizedTitle);
                    currentStrategy = MatchStrategy.TitleFuzzy;
                }

                // Step 4: Author Matching
                var candidateAuthors = ExtractAuthorTokens(candidate.Authors);
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

        public async Task<IEnumerable<MatchResult>> MatchBatchAsync(
            IEnumerable<ExtractedReference> references,
            Guid identificationProcessId,
            CancellationToken cancellationToken = default)
        {
            var referenceList = references?.ToList() ?? new List<ExtractedReference>();
            if (!referenceList.Any())
            {
                return new List<MatchResult>();
            }

            var cancelledDuplicates = await _unitOfWork.DeduplicationResults.FindAllAsync(
                dr => dr.IdentificationProcessId == identificationProcessId
                    && dr.ReviewStatus == DeduplicationReviewStatus.Confirmed
                    && dr.ResolvedDecision == DuplicateResolutionDecision.CANCEL,
                cancellationToken: cancellationToken);

            var cancelledDuplicateIds = cancelledDuplicates.Select(dr => dr.PaperId).ToHashSet();

            var scopedPapers = await _unitOfWork.Papers.FindAllAsync(
                p => p.ImportBatch != null
                    && p.ImportBatch.SearchExecution != null
                    && p.ImportBatch.SearchExecution.IdentificationProcessId == identificationProcessId,
                isTracking: false,
                cancellationToken);

            var existingPaperList = scopedPapers
                .Where(p => !cancelledDuplicateIds.Contains(p.Id))
                .ToList();

            var doiLookup = existingPaperList
                .Where(p => !string.IsNullOrWhiteSpace(p.DOI))
                .GroupBy(p => DoiHelper.Normalize(p.DOI) ?? p.DOI!.Trim().ToLowerInvariant())
                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.CreatedAt).First());

            var results = new List<MatchResult>(referenceList.Count);

            foreach (var reference in referenceList)
            {
                MatchResult? doiMatch = null;
                var normalizedRefDoi = DoiHelper.Normalize(reference.DOI);
                if (!string.IsNullOrWhiteSpace(normalizedRefDoi)
                    && doiLookup.TryGetValue(normalizedRefDoi, out var doiPaper))
                {
                    doiMatch = new MatchResult
                    {
                        MatchedPaper = doiPaper,
                        MatchedPaperId = doiPaper.Id,
                        ConfidenceScore = 1.0m,
                        Strategy = MatchStrategy.DOI
                    };
                }

                var filteredCandidates = FilterCandidates(reference, existingPaperList);
                var fuzzyMatch = MatchAgainstAsync(reference, filteredCandidates);

                MatchResult? semanticMatch = null;
                if (reference.TitleEmbedding is { Length: > 0 }
                    && !(fuzzyMatch.Strategy == MatchStrategy.TitleExact && fuzzyMatch.ConfidenceScore >= 1.0m)
                    && doiMatch == null)
                {
                    semanticMatch = await FindSemanticMatchInProcessAsync(
                        reference.TitleEmbedding,
                        reference,
                        identificationProcessId,
                        cancellationToken);
                }

                results.Add(SelectBestHybridMatch(doiMatch, fuzzyMatch, semanticMatch));
            }

            return results;
        }

        public MatchResult MatchAgainstProcessed(ExtractedReference reference, IEnumerable<ProcessedReference> processedReferences)
        {
            if (reference == null) return new MatchResult { ConfidenceScore = 0 };

            // Step 1: DOI Matching
            var normalizedRefDoi = DoiHelper.Normalize(reference.DOI);
            if (!string.IsNullOrEmpty(normalizedRefDoi))
            {
                var doiMatch = processedReferences.FirstOrDefault(p =>
                    !string.IsNullOrEmpty(p.Reference.DOI) && DoiHelper.Normalize(p.Reference.DOI) == normalizedRefDoi);

                if (doiMatch != null)
                {
                    return new MatchResult
                    {
                        MatchedPaperId = doiMatch.PaperId,
                        ConfidenceScore = 1.0m,
                        Strategy = MatchStrategy.DOI
                    };
                }
            }

            // Step 2: Fuzzy Matching
            if (string.IsNullOrWhiteSpace(reference.Title) || reference.Title.Length < 10)
            {
                return new MatchResult { ConfidenceScore = 0 };
            }

            var refData = PrepareMatchData(reference);
            MatchResult bestMatch = new MatchResult { ConfidenceScore = 0 };

            foreach (var candidate in processedReferences)
            {
                var candidateData = PrepareMatchData(candidate.Reference);
                var score = CalculateScore(refData, candidateData, out var strategy);

                if (score > bestMatch.ConfidenceScore)
                {
                    bestMatch = new MatchResult
                    {
                        MatchedPaperId = candidate.PaperId,
                        ConfidenceScore = score,
                        Strategy = strategy
                    };
                }
            }

            return bestMatch.ConfidenceScore >= 0.7m ? bestMatch : new MatchResult { ConfidenceScore = 0 };
        }


        private MatchResult MatchAgainstAsync(
            ExtractedReference reference,
            List<Paper> candidates)
        {
            if (reference == null) return new MatchResult { ConfidenceScore = 0 };

            if (string.IsNullOrWhiteSpace(reference.Title) || reference.Title.Length < 10)
            {
                return new MatchResult { ConfidenceScore = 0 };
            }

            var refData = PrepareMatchData(reference);
            MatchResult bestMatch = new MatchResult { ConfidenceScore = 0 };

            foreach (var candidate in candidates)
            {
                var candidateData = PrepareMatchData(candidate);
                var score = CalculateScore(refData, candidateData, out var strategy);

                if (score > bestMatch.ConfidenceScore)
                {
                    bestMatch = new MatchResult
                    {
                        MatchedPaper = candidate,
                        MatchedPaperId = candidate.Id,
                        ConfidenceScore = score,
                        Strategy = strategy
                    };
                }
            }

            return bestMatch.ConfidenceScore >= 0.7m ? bestMatch : new MatchResult { ConfidenceScore = 0 };
        }

        private async Task<MatchResult?> FindSemanticMatchInProcessAsync(
            float[] embedding,
            ExtractedReference reference,
            Guid identificationProcessId,
            CancellationToken cancellationToken)
        {
            // Retrieve top-5 nearest candidates for metadata veto filtering
            var nearestEmbeddings = await _unitOfWork.PaperEmbeddings.FindClosestByCosineDistanceInIdentificationProcessAsync(
                embedding,
                identificationProcessId,
                cancellationToken,
                take: 5);

            if (!nearestEmbeddings.Any())
            {
                return null;
            }

            // Extract reference metadata for cross-check
            int? refYear = int.TryParse(reference.PublishedYear, out var ry) ? ry : null;
            var refAuthorTokens = ExtractAuthorTokens(reference.Authors);

            foreach (var candidateEmbedding in nearestEmbeddings)
            {
                if (candidateEmbedding.Paper == null) continue;

                var similarity = CosineSimilarity(embedding, candidateEmbedding.Embedding.ToArray());

                // Hard threshold: reject anything below 0.85
                if (similarity < 0.85f)
                {
                    continue;
                }

                // Near-exact semantic match (>= 0.95): accept unconditionally
                if (similarity < 0.95f)
                {
                    // Metadata cross-check veto for sub-0.95 matches
                    var candidatePaper = candidateEmbedding.Paper;

                    // Year veto: reject if year difference > 2
                    if (refYear.HasValue && candidatePaper.PublicationYearInt.HasValue)
                    {
                        if (Math.Abs(refYear.Value - candidatePaper.PublicationYearInt.Value) > 2)
                        {
                            _logger.LogDebug(
                                "Semantic match vetoed by year difference: ref={RefYear}, candidate={CandYear}, similarity={Similarity}",
                                refYear.Value, candidatePaper.PublicationYearInt.Value, similarity);
                            continue;
                        }
                    }

                    // Author veto: reject if author overlap < 0.3
                    var candidateAuthorTokens = ExtractAuthorTokens(candidatePaper.Authors);
                    if (refAuthorTokens.Any() && candidateAuthorTokens.Any())
                    {
                        var authorScore = ComputeAuthorScore(refAuthorTokens, candidateAuthorTokens);
                        if (authorScore < 0.3m)
                        {
                            _logger.LogDebug(
                                "Semantic match vetoed by low author score: {AuthorScore}, similarity={Similarity}",
                                authorScore, similarity);
                            continue;
                        }
                    }
                }

                // Candidate passed all veto checks
                return new MatchResult
                {
                    MatchedPaper = candidateEmbedding.Paper,
                    MatchedPaperId = candidateEmbedding.PaperId,
                    ConfidenceScore = (decimal)similarity,
                    Strategy = MatchStrategy.Semantic
                };
            }

            return null;
        }

        private static MatchResult SelectBestHybridMatch(params MatchResult?[] matches)
        {
            var validMatches = matches
                .Where(m => m != null && m.ConfidenceScore > 0)
                .Select(m => m!)
                .ToList();

            if (!validMatches.Any())
            {
                return new MatchResult { ConfidenceScore = 0, Strategy = MatchStrategy.None };
            }

            var doiMatch = validMatches.FirstOrDefault(m => m.Strategy == MatchStrategy.DOI && m.ConfidenceScore >= 1.0m);
            if (doiMatch != null)
            {
                return doiMatch;
            }

            var titleExactMatch = validMatches.FirstOrDefault(m => m.Strategy == MatchStrategy.TitleExact && m.ConfidenceScore >= 1.0m);
            if (titleExactMatch != null)
            {
                return titleExactMatch;
            }

            return validMatches
                .Where(m => m.Strategy == MatchStrategy.TitleFuzzy || m.Strategy == MatchStrategy.Semantic)
                .OrderByDescending(m => m.ConfidenceScore)
                .FirstOrDefault() ?? new MatchResult { ConfidenceScore = 0, Strategy = MatchStrategy.None };
        }

        private MatchData PrepareMatchData(ExtractedReference reference)
        {
            return new MatchData
            {
                NormalizedTitle = NormalizeText(reference.Title),
                Tokens = Tokenize(NormalizeText(reference.Title)),
                Authors = ExtractAuthorTokens(reference.Authors),
                Year = int.TryParse(reference.PublishedYear, out var y) ? y : null,
                Journal = reference.Journal,
                NormalizedDoi = DoiHelper.Normalize(reference.DOI),
                TitleEmbedding = reference.TitleEmbedding
            };
        }

        private MatchData PrepareMatchData(Paper paper)
        {
            return new MatchData
            {
                NormalizedTitle = NormalizeText(paper.Title),
                Tokens = Tokenize(NormalizeText(paper.Title)),
                Authors = ExtractAuthorTokens(paper.Authors),
                Year = paper.PublicationYearInt,
                Journal = paper.Journal,
                NormalizedDoi = DoiHelper.Normalize(paper.DOI),
                TitleEmbedding = paper.TitleEmbedding?.Embedding?.ToArray()
            };
        }

        private struct MatchData
        {
            public string NormalizedTitle;
            public List<string> Tokens;
            public List<string> Authors;
            public int? Year;
            public string? Journal;
            public string? NormalizedDoi;
            public float[]? TitleEmbedding;
        }

        private decimal CalculateScore(MatchData refData, MatchData candidateData, out MatchStrategy strategy)
        {
            strategy = MatchStrategy.None;

            if (candidateData.NormalizedTitle == refData.NormalizedTitle)
            {
                strategy = MatchStrategy.TitleExact;
                return 1.0m;
            }

            decimal titleScore = ComputeTitleScore(refData.NormalizedTitle, candidateData.NormalizedTitle);
            decimal authorScore = ComputeAuthorScore(refData.Authors, candidateData.Authors);
            decimal yearScore = 0;

            if (refData.Year.HasValue && candidateData.Year.HasValue)
            {
                if (refData.Year.Value == candidateData.Year.Value) yearScore = 1.0m;
                else if (Math.Abs(refData.Year.Value - candidateData.Year.Value) == 1) yearScore = 0.5m;
            }

            // Semantic strategy: when both have embeddings, use vector similarity with metadata penalty
            if (refData.TitleEmbedding is { Length: > 0 } && candidateData.TitleEmbedding is { Length: > 0 })
            {
                var vectorSimilarity = (decimal)CosineSimilarity(refData.TitleEmbedding, candidateData.TitleEmbedding);

                if (vectorSimilarity >= 0.85m)
                {
                    strategy = MatchStrategy.Semantic;

                    // Weighted semantic score
                    decimal semanticScore = (vectorSimilarity * 0.6m) + (titleScore * 0.2m) + (authorScore * 0.15m) + (yearScore * 0.05m);

                    // Metadata penalty: mismatched DOI
                    if (!string.IsNullOrWhiteSpace(refData.NormalizedDoi)
                        && !string.IsNullOrWhiteSpace(candidateData.NormalizedDoi)
                        && refData.NormalizedDoi != candidateData.NormalizedDoi)
                    {
                        semanticScore -= 0.3m;
                    }

                    // Metadata penalty: very low author overlap
                    if (refData.Authors.Any() && candidateData.Authors.Any() && authorScore < 0.2m)
                    {
                        semanticScore -= 0.15m;
                    }

                    semanticScore = Math.Clamp(semanticScore, 0m, 1.0m);

                    // Use semantic score if it outperforms the fuzzy rule score
                    decimal ruleScore = (titleScore * 0.7m) + (authorScore * 0.2m) + (yearScore * 0.1m);
                    ruleScore = Math.Clamp(ruleScore, 0m, 1.0m);

                    if (semanticScore >= ruleScore)
                    {
                        return semanticScore;
                    }
                    else
                    {
                        strategy = MatchStrategy.TitleFuzzy;
                        return ruleScore;
                    }
                }
            }

            // Fallback: fuzzy rule-based scoring
            if (titleScore < 0.3m)
            {
                return 0;
            }

            strategy = MatchStrategy.TitleFuzzy;

            decimal finalRuleScore = (titleScore * 0.7m) + (authorScore * 0.2m) + (yearScore * 0.1m);

            return finalRuleScore > 1.0m ? 1.0m : finalRuleScore;
        }

        private float CosineSimilarity(float[] v1, float[] v2)
        {
            if (v1 == null || v2 == null || v1.Length != v2.Length || v1.Length == 0)
                return 0;

            float dotProduct = 0;
            float mag1 = 0;
            float mag2 = 0;

            for (int i = 0; i < v1.Length; i++)
            {
                dotProduct += v1[i] * v2[i];
                mag1 += v1[i] * v1[i];
                mag2 += v2[i] * v2[i];
            }

            if (mag1 == 0 || mag2 == 0) return 0;

            return dotProduct / (float)(Math.Sqrt(mag1) * Math.Sqrt(mag2));
        }

        private decimal ComputeTitleScore(string a, string b)
        {
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return 0;
            
            var tokensA = Tokenize(a);
            var tokensB = Tokenize(b);
            
            var jaccard = ComputeJaccard(tokensA, tokensB);
            var levenshtein = ComputeLevenshteinSimilarity(a, b);
            
            return (jaccard * 0.6m) + (levenshtein * 0.4m);
        }

        /// <summary>
        /// Normalizes text for fuzzy matching (Jaccard/Levenshtein) only.
        /// Strips punctuation, lowercases, and removes stopwords.
        /// NOTE: Do NOT use this for embedding input — BERT-based models benefit from
        /// natural language with punctuation and structure intact.
        /// </summary>
        private string NormalizeText(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            
            var text = input.ToLowerInvariant();
            text = Regex.Replace(text, @"[^\w\s]", " "); // Replace punctuation with space
            text = Regex.Replace(text, @"\s+", " ");    // Collapse whitespace
            
            var tokens = text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                             .Where(t => !Stopwords.Contains(t));
                             
            return string.Join(" ", tokens).Trim();
        }

        private List<string> Tokenize(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return new List<string>();
            return input.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        private decimal ComputeJaccard(List<string> a, List<string> b)
        {
            if (!a.Any() || !b.Any()) return 0m;
            
            int intersection = 0;
            var usedB = new bool[b.Count];

            foreach (var tokenA in a)
            {
                for (int i = 0; i < b.Count; i++)
                {
                    if (!usedB[i] && IsSimilarToken(tokenA, b[i]))
                    {
                        intersection++;
                        usedB[i] = true;
                        break;
                    }
                }
            }

            int union = a.Count + b.Count - intersection;
            return union == 0 ? 0m : (decimal)intersection / union;
        }

        private bool IsSimilarToken(string a, string b)
        {
            if (a == b) return true;
            if (a.Length > 3 && b.Length > 3)
            {
                return a.StartsWith(b, StringComparison.OrdinalIgnoreCase) || 
                       b.StartsWith(a, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        private decimal ComputeLevenshteinSimilarity(string s, string t)
        {
            if (string.IsNullOrEmpty(s)) return string.IsNullOrEmpty(t) ? 1 : 0;
            if (string.IsNullOrEmpty(t)) return 0;

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 0; j <= m; d[0, j] = j++) ;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }

            return 1.0m - ((decimal)d[n, m] / Math.Max(n, m));
        }

        private List<string> ExtractAuthorTokens(string? authors)
        {
            if (string.IsNullOrWhiteSpace(authors)) return new List<string>();

            // Handle multiple separators, tokenize everything
            var parts = authors.Split(new[] { ",", ";", " and ", " & ", "." }, StringSplitOptions.RemoveEmptyEntries);
            var tokens = new List<string>();

            foreach (var part in parts)
            {
                var nameTokens = part.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (var nt in nameTokens)
                {
                    var normalized = NormalizeText(nt);
                    if (!string.IsNullOrEmpty(normalized) && normalized.Length > 1)
                    {
                        tokens.Add(normalized);
                    }
                }
            }

            return tokens.Distinct().ToList();
        }

        private decimal ComputeAuthorScore(List<string> refAuthorTokens, List<string> candidateAuthorTokens)
        {
            if (!refAuthorTokens.Any() || !candidateAuthorTokens.Any()) return 0m;

            int matches = 0;
            var usedCandidate = new bool[candidateAuthorTokens.Count];

            foreach (var rToken in refAuthorTokens)
            {
                for (int i = 0; i < candidateAuthorTokens.Count; i++)
                {
                    if (!usedCandidate[i] && IsSimilarToken(rToken, candidateAuthorTokens[i]))
                    {
                        matches++;
                        usedCandidate[i] = true;
                        break;
                    }
                }
            }

            return (decimal)matches / Math.Max(refAuthorTokens.Count, candidateAuthorTokens.Count);
        }

        private List<Paper> FilterCandidates(ExtractedReference reference, List<Paper> candidates)
        {
            if (reference == null || !candidates.Any()) return new List<Paper>();

            int? refYear = int.TryParse(reference.PublishedYear, out var y) ? y : null;
            var normalizedTitle = NormalizeText(reference.Title);
            var tokens = Tokenize(normalizedTitle);
            
            // Extract top 3 keywords (longest meaningful tokens)
            var topKeywords = tokens
                .Where(t => t.Length > 3)
                .OrderByDescending(t => t.Length)
                .Take(3)
                .Select(t => t.ToLowerInvariant())
                .ToList();

            return candidates.Where(p =>
            {
                // Rule 1: Year match (+/- 1 year)
                if (refYear.HasValue && p.PublicationYearInt.HasValue && Math.Abs(refYear.Value - p.PublicationYearInt.Value) <= 1)
                {
                    return true;
                }

                // Rule 2: Journal match (if available)
                if (!string.IsNullOrWhiteSpace(reference.Journal) && 
                    !string.IsNullOrWhiteSpace(p.Journal) && 
                    reference.Journal.Equals(p.Journal, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                // Rule 3: Keyword overlap in title
                if (topKeywords.Any())
                {
                    var paperTitleLower = p.Title?.ToLowerInvariant() ?? string.Empty;
                    foreach (var kw in topKeywords)
                    {
                        if (paperTitleLower.Contains(kw)) return true;
                    }
                }

                return false;
            }).ToList();
        }

        public async Task<MatchResult> MatchAgainstSnapshotAsync(
            ExtractedReference reference,
            Guid identificationProcessId,
            CancellationToken cancellationToken = default)
        {
            if (reference == null)
            {
                return new MatchResult { MatchedPaper = null, ConfidenceScore = 0, Strategy = MatchStrategy.None };
            }

            var snapshotPaperIds = await _unitOfWork.IdentificationProcessPapers.GetIncludedPaperIdsByProcessAsync(
                identificationProcessId,
                cancellationToken);

            if (!snapshotPaperIds.Any())
            {
                return new MatchResult { MatchedPaper = null, ConfidenceScore = 0, Strategy = MatchStrategy.None };
            }

            var snapshotPaperIdSet = snapshotPaperIds.ToHashSet();

            // Tier 1: DOI matching within snapshot only
            var normalizedDoi = DoiHelper.Normalize(reference.DOI);
            if (!string.IsNullOrWhiteSpace(normalizedDoi))
            {
                var exactDoiMatches = await _unitOfWork.Papers.FindAllAsync(
                    p => snapshotPaperIds.Contains(p.Id)
                        && p.DOI != null
                        && p.DOI.ToLower() == normalizedDoi,
                    isTracking: false,
                    cancellationToken);

                var doiMatchedPaper = exactDoiMatches.FirstOrDefault();
                if (doiMatchedPaper != null && snapshotPaperIdSet.Contains(doiMatchedPaper.Id))
                {
                    return new MatchResult
                    {
                        MatchedPaper = doiMatchedPaper,
                        ConfidenceScore = 1.0m,
                        Strategy = MatchStrategy.DOI,
                        IsSnapshotDuplicate = true
                    };
                }
            }

            if (string.IsNullOrWhiteSpace(reference.Title) || reference.Title.Length < 10)
            {
                return new MatchResult { MatchedPaper = null, ConfidenceScore = 0, Strategy = MatchStrategy.None };
            }

            var candidates = await _unitOfWork.Papers.FindAllAsync(
                p => snapshotPaperIds.Contains(p.Id),
                isTracking: false,
                cancellationToken);

            var refData = PrepareMatchData(reference);
            Paper? bestMatch = null;
            decimal highestScore = 0;
            MatchStrategy bestStrategy = MatchStrategy.None;

            foreach (var candidate in candidates)
            {
                var candidateData = PrepareMatchData(candidate);
                var score = CalculateScore(refData, candidateData, out var strategy);

                if (score > highestScore)
                {
                    highestScore = score;
                    bestMatch = candidate;
                    bestStrategy = strategy;
                }
            }

            if (bestMatch != null && highestScore >= 0.7m && snapshotPaperIdSet.Contains(bestMatch.Id))
            {
                if (highestScore < 0.75m)
                {
                    _logger.LogInformation(
                        "Weak snapshot match found for Reference '{RawReference}' with Paper ID '{PaperId}'. Confidence: {Score}",
                        reference.RawReference,
                        bestMatch.Id,
                        highestScore);
                }

                return new MatchResult
                {
                    MatchedPaper = bestMatch,
                    ConfidenceScore = highestScore,
                    Strategy = bestStrategy,
                    IsSnapshotDuplicate = true
                };
            }

            return new MatchResult { MatchedPaper = null, ConfidenceScore = 0, Strategy = MatchStrategy.None };
        }
    }
}
