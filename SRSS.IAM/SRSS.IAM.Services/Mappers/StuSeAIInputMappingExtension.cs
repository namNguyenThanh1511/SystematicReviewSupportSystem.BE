using System;
using System.Collections.Generic;
using System.Linq;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.StudySelection;

namespace SRSS.IAM.Services.Mappers
{
    public static class StuSeAIInputMappingExtension
    {
        public static StuSeAIInput BuildStuSeAIInput(this StudySelectionProcess process, Paper paper)
        {
            var project = process.ReviewProcess.Project;
            var result = new StuSeAIInput
            {
                Paper = new StuSePaperInput
                {
                    Title = paper.Title?.Trim() ?? string.Empty
                }
            };

            // 1. PAPER
            AssignIfNotEmpty(v => result.Paper.Abstract = v, paper.Abstract);
            AssignIfNotEmpty(v => result.Paper.Keywords = v, paper.Keywords);
            AssignIfNotEmpty(v => result.Paper.Language = v, paper.Language);
            if (paper.PublicationYearInt.HasValue)
                result.Paper.PublicationYear = paper.PublicationYearInt.Value;


            // 4. RESEARCH QUESTIONS (Hierarchical)
            result.ResearchQuestions = project.ResearchQuestions?
                .Select(rq => new StuSeRQInput
                {
                    QuestionText = rq.QuestionText?.Trim() ?? string.Empty,
                    // PICOC = MapPicocFromElements(rq.PicocElements)
                })
                .ToList() ?? new List<StuSeRQInput>();

            // 5. CRITERIA GROUPS
            if (process.StudySelectionCriterias != null && process.StudySelectionCriterias.Any())
            {
                result.CriteriaGroups = process.StudySelectionCriterias.Select(cg => new StuSeCriteriaGroupInput
                {
                    Description = cg.Description?.Trim(),
                    InclusionRules = MapCriteria(cg.InclusionCriteria?.Select(c => c.Rule)),
                    ExclusionRules = MapCriteria(cg.ExclusionCriteria?.Select(c => c.Rule))
                }).ToList();
            }
            else
            {
                result.CriteriaGroups = new List<StuSeCriteriaGroupInput>();
            }


            return result;
        }

        private static StuSePicocInput? MapPicocFromElements(IEnumerable<PicocElement> elements)
        {
            if (elements == null || !elements.Any()) return null;

            var picoc = new StuSePicocInput();
            var hasValue = false;

            foreach (var element in elements)
            {
                var type = element.ElementType?.Trim().ToLower();
                var desc = element.Description?.Trim();
                if (string.IsNullOrWhiteSpace(desc)) continue;

                switch (type)
                {
                    case "population": picoc.Population = desc; hasValue = true; break;
                    case "intervention": picoc.Intervention = desc; hasValue = true; break;
                    case "comparison": picoc.Comparison = desc; hasValue = true; break;
                    case "outcome": picoc.Outcome = desc; hasValue = true; break;
                    case "context": picoc.Context = desc; hasValue = true; break;
                }
            }

            return hasValue ? picoc : null;
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
    }
}
