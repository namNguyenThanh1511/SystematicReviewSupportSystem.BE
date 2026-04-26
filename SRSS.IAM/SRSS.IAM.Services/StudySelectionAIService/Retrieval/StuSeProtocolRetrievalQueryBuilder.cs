using System;
using System.Collections.Generic;
using System.Linq;
using SRSS.IAM.Services.DTOs.StudySelection;

namespace SRSS.IAM.Services.StudySelectionAIService.Retrieval
{
    public interface IStuSeProtocolRetrievalQueryBuilder
    {
        List<StuSeRetrievalQueryDto> BuildQueries(StuSeAIInput input);
    }

    public class StuSeProtocolRetrievalQueryBuilder : IStuSeProtocolRetrievalQueryBuilder
    {
        public List<StuSeRetrievalQueryDto> BuildQueries(StuSeAIInput input)
        {
            var queries = new List<StuSeRetrievalQueryDto>();
            if (input == null) return queries;

            // 1. Criteria Groups, Inclusions and Exclusions
            if (input.CriteriaGroups != null)
            {
                foreach (var group in input.CriteriaGroups)
                {
                    if (!string.IsNullOrWhiteSpace(group.Description))
                    {
                        queries.Add(new StuSeRetrievalQueryDto
                        {
                            QueryType = "CriteriaGroup",
                            QueryText = $"criteria group evidence: {group.Description.Trim()}",
                            SourceLabel = group.Description.Trim()
                        });
                    }

                    if (group.Inclusion != null)
                    {
                        foreach (var rule in group.Inclusion)
                        {
                            if (!string.IsNullOrWhiteSpace(rule))
                            {
                                queries.Add(new StuSeRetrievalQueryDto
                                {
                                    QueryType = "InclusionRule",
                                    QueryText = $"inclusion criteria evidence: {rule.Trim()}",
                                    SourceLabel = rule.Trim()
                                });
                            }
                        }
                    }

                    if (group.Exclusion != null)
                    {
                        foreach (var rule in group.Exclusion)
                        {
                            if (!string.IsNullOrWhiteSpace(rule))
                            {
                                queries.Add(new StuSeRetrievalQueryDto
                                {
                                    QueryType = "ExclusionRule",
                                    QueryText = $"exclusion criteria evidence: {rule.Trim()}",
                                    SourceLabel = rule.Trim()
                                });
                            }
                        }
                    }
                }
            }

            // Deduplicate queries by QueryText to avoid redundant embeddings
            return queries
                .GroupBy(q => q.QueryText)
                .Select(g => g.First())
                .ToList();
        }
    }
}
