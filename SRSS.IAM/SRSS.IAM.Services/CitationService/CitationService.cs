using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.Citation;

namespace SRSS.IAM.Services.CitationService
{
    public class CitationService : ICitationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CitationService> _logger;

        public CitationService(IUnitOfWork unitOfWork, ILogger<CitationService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<int> GetCitationCountAsync(Guid paperId, CancellationToken cancellationToken = default)
        {
            return await _unitOfWork.PaperCitations.CountByTargetAsync(paperId, cancellationToken);
        }

        public async Task<int> GetReferenceCountAsync(Guid paperId, CancellationToken cancellationToken = default)
        {
            return await _unitOfWork.PaperCitations.CountBySourceAsync(paperId, cancellationToken);
        }

        public async Task<List<PaperNodeDto>> GetCitationsAsync(Guid paperId, CancellationToken cancellationToken = default)
        {
            var citations = await _unitOfWork.PaperCitations.GetCitationsWithSourcePaperAsync(paperId, cancellationToken);
            
            return citations.Select(c => new PaperNodeDto
            {
                Id = c.SourcePaper.Id,
                Title = c.SourcePaper.Title,
                Year = c.SourcePaper.PublicationYearInt,
                Authors = c.SourcePaper.Authors,
                Doi = c.SourcePaper.DOI,
                CitationCount = c.SourcePaper.IncomingCitations.Count
            })
            .DistinctBy(x => x.Id)
            .ToList();
        }

        public async Task<List<PaperNodeDto>> GetReferencesAsync(Guid paperId, CancellationToken cancellationToken = default)
        {
            var references = await _unitOfWork.PaperCitations.GetReferencesWithTargetPaperAsync(paperId, cancellationToken);
            
            return references.Select(c => new PaperNodeDto
            {
                Id = c.TargetPaper.Id,
                Title = c.TargetPaper.Title,
                Year = c.TargetPaper.PublicationYearInt,
                Authors = c.TargetPaper.Authors,
                Doi = c.TargetPaper.DOI,
                CitationCount = c.TargetPaper.IncomingCitations.Count
            })
            .DistinctBy(x => x.Id)
            .ToList();
        }

        public async Task<CitationGraphDto> GetCitationGraphAsync(Guid paperId, int depth, decimal minConfidence, CancellationToken cancellationToken = default)
        {
            if (depth < 0) depth = 0;
            if (depth > 3) depth = 3;

            var graph = new CitationGraphDto();
            var visitedNodes = new HashSet<Guid>();
            var queuedNodes = new HashSet<Guid> { paperId };
            var edges = new HashSet<CitationEdgeDto>(new CitationEdgeComparer());

            var currentLevelIds = new List<Guid> { paperId };
            int maxNodes = 500;

            for (int currentDepth = 0; currentDepth < depth; currentDepth++)
            {
                if (!currentLevelIds.Any() || visitedNodes.Count >= maxNodes) break;

                // Load all outgoing edges for the current level in one batch
                var levelEdges = await _unitOfWork.PaperCitations.GetEdgesBySourcesAsync(currentLevelIds, minConfidence, cancellationToken);

                var nextLevelIds = new HashSet<Guid>();

                foreach (var edge in levelEdges)
                {
                    edges.Add(new CitationEdgeDto 
                    {
                        SourcePaperId = edge.SourcePaperId,
                        TargetPaperId = edge.TargetPaperId,
                        ConfidenceScore = edge.ConfidenceScore
                    });

                    if (!visitedNodes.Contains(edge.TargetPaperId) && !queuedNodes.Contains(edge.TargetPaperId))
                    {
                        queuedNodes.Add(edge.TargetPaperId);
                        nextLevelIds.Add(edge.TargetPaperId);
                    }
                }

                foreach (var id in currentLevelIds)
                {
                    visitedNodes.Add(id);
                }

                currentLevelIds = nextLevelIds.Take(maxNodes - visitedNodes.Count).ToList();
            }

            foreach (var id in currentLevelIds)
            {
                visitedNodes.Add(id);
            }

            // We have all nodes up to maxNodes. Let's fetch their metadata.
            var allNodeIds = visitedNodes.ToList();
            
            // To prevent N+1 for citation count, we load them in bulk
            var papers = await _unitOfWork.Papers.GetPapersWithCitationCountByIdsAsync(allNodeIds, cancellationToken);
            var nodesData = papers.Select(p => new PaperNodeDto
            {
                Id = p.Id,
                Title = p.Title,
                Year = p.PublicationYearInt,
                Authors = p.Authors,
                Doi = p.DOI,
                CitationCount = p.IncomingCitations.Count
            }).ToList();

            _logger.LogInformation("Graph built for paper {PaperId} with {NodeCount} nodes and {EdgeCount} edges.", paperId, nodesData.Count, edges.Count);

            graph.Nodes = nodesData;
            graph.Edges = edges.ToList();

            return graph;
        }

        public async Task<List<PaperNodeDto>> GetTopCitedPapersAsync(int topN, CancellationToken cancellationToken = default)
        {
            if (topN <= 0) topN = 10;
            if (topN > 100) topN = 100;

            var papers = await _unitOfWork.Papers.GetTopCitedPapersAsync(topN, cancellationToken);
            return papers.Select(p => new PaperNodeDto
            {
                Id = p.Id,
                Title = p.Title,
                Year = p.PublicationYearInt,
                Authors = p.Authors,
                Doi = p.DOI,
                CitationCount = p.IncomingCitations.Count
            }).ToList();
        }

        public async Task<List<PaperNodeDto>> GetSuggestedPapersAsync(Guid paperId, int limit = 5, CancellationToken cancellationToken = default)
        {
            // Giới hạn limit
            limit = Math.Clamp(limit, 1, 50);

            // Chạy tuần tự để tránh lỗi "Second operation started..."
            var references = await _unitOfWork.PaperCitations.GetReferencesWithTargetPaperAsync(paperId, cancellationToken);
            var citations = await _unitOfWork.PaperCitations.GetCitationsWithSourcePaperAsync(paperId, cancellationToken);

            var referencedPapers = references.Select(x => x.TargetPaper);
            var citingPapers = citations.Select(x => x.SourcePaper);

            var allAdjacent = referencedPapers.Concat(citingPapers)
                .Where(p => p != null && p.Id != paperId)
                .GroupBy(p => p.Id)
                .Select(g => g.First())
                // Sắp xếp theo số lượng trích dẫn (đã được Include ở tầng Repository)
                .OrderByDescending(p => p.IncomingCitations?.Count ?? 0)
                .Take(limit)
                .ToList();

            return allAdjacent.Select(p => new PaperNodeDto
            {
                Id = p.Id,
                Title = p.Title,
                Year = p.PublicationYearInt,
                Authors = p.Authors,
                Doi = p.DOI,
                CitationCount = p.IncomingCitations?.Count ?? 0
            }).ToList();
        }


        private class CitationEdgeComparer : IEqualityComparer<CitationEdgeDto>
        {
            public bool Equals(CitationEdgeDto? x, CitationEdgeDto? y)
            {
                if (x == null || y == null) return false;
                return x.SourcePaperId == y.SourcePaperId && x.TargetPaperId == y.TargetPaperId;
            }

            public int GetHashCode(CitationEdgeDto obj)
            {
                return HashCode.Combine(obj.SourcePaperId, obj.TargetPaperId);
            }
        }
    }
}