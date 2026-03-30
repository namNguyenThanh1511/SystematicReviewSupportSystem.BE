using System;
using System.Collections.Generic;
using System.Linq;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.StudySelection;

namespace SRSS.IAM.Services.Mappers
{
    public static class StuSeAIInputMappingExtension
    {
        public static StuSeAIInput BuildStuSeAIInput(this ReviewProtocol protocol, Paper paper)
        {
            var project = protocol.Project ?? protocol.ReviewProcess?.Project;
            var criteriaEntity = protocol.SelectionCriterias?.FirstOrDefault();

            var result = new StuSeAIInput
            {
                Paper = new StuSePaperInput
                {
                    Title = paper.Title?.Trim() ?? string.Empty
                },
                Criteria = new StuSeCriteriaInput(),
                PICOC = new StuSePicocInput()
            };

            // 1. PAPER
            AssignIfNotEmpty(v => result.Paper.Abstract = v, paper.Abstract);
            AssignIfNotEmpty(v => result.Paper.Keywords = v, paper.Keywords);
            AssignIfNotEmpty(v => result.Paper.Language = v, paper.Language);
            if (paper.PublicationYearInt.HasValue) 
                result.Paper.PublicationYear = paper.PublicationYearInt.Value;

            // 2. CRITERIA
            AssignIfNotEmpty(v => result.Criteria.Domain = v, project?.Domain);

            // 3. PICOC
            AssignIfNotEmpty(v => result.PICOC.Population = v, GetPicocValue(project, "Population"));
            AssignIfNotEmpty(v => result.PICOC.Intervention = v, GetPicocValue(project, "Intervention"));
            AssignIfNotEmpty(v => result.PICOC.Comparison = v, GetPicocValue(project, "Comparison"));
            AssignIfNotEmpty(v => result.PICOC.Outcome = v, GetPicocValue(project, "Outcome"));
            AssignIfNotEmpty(v => result.PICOC.Context = v, GetPicocValue(project, "Context"));

            // 4. RESEARCH QUESTIONS
            result.ResearchQuestions = project?.ResearchQuestions?
                .Select(rq => rq.QuestionText?.Trim())
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Cast<string>()
                .ToList() ?? new List<string>();

            // 5. INCLUSION / EXCLUSION
            result.InclusionCriteria = MapCriteria(criteriaEntity?.InclusionCriteria?.Select(c => c.Rule));
            result.ExclusionCriteria = MapCriteria(criteriaEntity?.ExclusionCriteria?.Select(c => c.Rule));

            // Validation: Nullify empty objects
            if (IsCriteriaEmpty(result.Criteria)) result.Criteria = null!;
            if (IsPicocEmpty(result.PICOC)) result.PICOC = null!;

            return result;
        }

        private static bool IsPicocEmpty(StuSePicocInput picoc)
        {
            return string.IsNullOrWhiteSpace(picoc.Population) &&
                   string.IsNullOrWhiteSpace(picoc.Intervention) &&
                   string.IsNullOrWhiteSpace(picoc.Comparison) &&
                   string.IsNullOrWhiteSpace(picoc.Outcome) &&
                   string.IsNullOrWhiteSpace(picoc.Context);
        }

        private static bool IsCriteriaEmpty(StuSeCriteriaInput criteria)
        {
            return string.IsNullOrWhiteSpace(criteria.Domain);
        }

        private static List<string> MapCriteria(IEnumerable<string?>? rules)
        {
            return rules?
                .Select(r => r?.Trim())
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Cast<string>()
                .Distinct()
                .ToList() ?? new List<string>();
        }

        private static void AssignIfNotEmpty(Action<string> assigner, string? value)
        {
            var trimmed = value?.Trim();
            if (!string.IsNullOrWhiteSpace(trimmed))
            {
                assigner(trimmed);
            }
        }

        private static string? GetPicocValue(SystematicReviewProject? project, string type)
        {
            if (project == null) return null;

            var values = new List<string>();

            // 1. From Research Questions' PicocElements
            if (project.ResearchQuestions != null)
            {
                var fromQuestions = project.ResearchQuestions
                    .SelectMany(q => q.PicocElements)
                    .Where(e => e.ElementType != null && e.ElementType.Equals(type, StringComparison.OrdinalIgnoreCase))
                    .Select(e => e.Description?.Trim())
                    .Where(d => !string.IsNullOrWhiteSpace(d))
                    .Distinct();

                values.AddRange(fromQuestions!);
            }

            // 2. Fallback to Review Objectives (only if no specific PICOC elements found)
            if (!values.Any() && project.ReviewObjectives != null)
            {
                var fromObjectives = project.ReviewObjectives
                    .Where(o => o.ObjectiveStatement != null && o.ObjectiveStatement.Contains(type, StringComparison.OrdinalIgnoreCase))
                    .Select(o => o.ObjectiveStatement.Trim())
                    .FirstOrDefault();

                if (fromObjectives != null) values.Add(fromObjectives);
            }

            return values.Any() ? string.Join("; ", values.Distinct()) : null;
        }
    }
}
