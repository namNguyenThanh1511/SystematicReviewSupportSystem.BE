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

            // 1. Research Questions and PICOC
            foreach (var rq in input.ResearchQuestions)
            {
                if (!string.IsNullOrWhiteSpace(rq.QuestionText))
                {
                    queries.Add(new StuSeRetrievalQueryDto
                    {
                        QueryType = "ResearchQuestion",
                        QueryText = $"research question evidence: {rq.QuestionText.Trim()}",
                        SourceLabel = rq.QuestionText.Trim()
                    });
                }

                if (rq.PICOC != null)
                {
                    AddPicocQueries(queries, rq.PICOC);
                }
            }

            // 2. Criteria Groups, Inclusions and Exclusions
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

                foreach (var rule in group.InclusionRules)
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

                foreach (var rule in group.ExclusionRules)
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

            // Deduplicate queries by QueryText to avoid redundant embeddings
            return queries
                .GroupBy(q => q.QueryText)
                .Select(g => g.First())
                .ToList();
        }

        private void AddPicocQueries(List<StuSeRetrievalQueryDto> queries, StuSePicocInput picoc)
        {
            if (!string.IsNullOrWhiteSpace(picoc.Population))
            {
                queries.Add(new StuSeRetrievalQueryDto 
                { 
                    QueryType = "Population", 
                    QueryText = $"population evidence: {picoc.Population.Trim()}",
                    SourceLabel = picoc.Population.Trim()
                });
            }

            if (!string.IsNullOrWhiteSpace(picoc.Intervention))
            {
                queries.Add(new StuSeRetrievalQueryDto 
                { 
                    QueryType = "Intervention", 
                    QueryText = $"intervention evidence: {picoc.Intervention.Trim()}",
                    SourceLabel = picoc.Intervention.Trim()
                });
            }

            if (!string.IsNullOrWhiteSpace(picoc.Comparison))
            {
                queries.Add(new StuSeRetrievalQueryDto 
                { 
                    QueryType = "Comparison", 
                    QueryText = $"comparison evidence: {picoc.Comparison.Trim()}",
                    SourceLabel = picoc.Comparison.Trim()
                });
            }

            if (!string.IsNullOrWhiteSpace(picoc.Outcome))
            {
                queries.Add(new StuSeRetrievalQueryDto 
                { 
                    QueryType = "Outcome", 
                    QueryText = $"outcome evidence: {picoc.Outcome.Trim()}",
                    SourceLabel = picoc.Outcome.Trim()
                });
            }

            if (!string.IsNullOrWhiteSpace(picoc.Context))
            {
                queries.Add(new StuSeRetrievalQueryDto 
                { 
                    QueryType = "Context", 
                    QueryText = $"context evidence: {picoc.Context.Trim()}",
                    SourceLabel = picoc.Context.Trim()
                });
            }
        }
    }
}
