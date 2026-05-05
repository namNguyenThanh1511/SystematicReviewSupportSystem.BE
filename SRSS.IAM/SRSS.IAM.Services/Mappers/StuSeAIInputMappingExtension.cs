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

            // 2. CRITERIA GROUPS
            if (process.StudySelectionCriterias != null && process.StudySelectionCriterias.Any())
            {
                result.CriteriaGroups = process.StudySelectionCriterias.Select(cg => new StuSeCriteriaGroupInput
                {
                    Description = cg.Description?.Trim(),
                    Inclusion = MapCriteria(cg.InclusionCriteria?.Select(c => c.Rule)),
                    Exclusion = MapCriteria(cg.ExclusionCriteria?.Select(c => c.Rule))
                }).ToList();
            }
            else
            {
                result.CriteriaGroups = new List<StuSeCriteriaGroupInput>();
            }

            return result;
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
