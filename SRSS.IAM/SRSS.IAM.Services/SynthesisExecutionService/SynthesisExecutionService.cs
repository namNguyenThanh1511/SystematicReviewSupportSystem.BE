using Microsoft.EntityFrameworkCore;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.SynthesisExecution;
using SRSS.IAM.Services.UserService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SRSS.IAM.Services.SynthesisExecutionService
{
    public class SynthesisExecutionService : ISynthesisExecutionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public SynthesisExecutionService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<SynthesisWorkspaceDto> GetSynthesisWorkspaceAsync(Guid reviewProcessId)
        {
            var process = await _unitOfWork.SynthesisProcesses.FindSingleAsync(p => p.ReviewProcessId == reviewProcessId);

            if (process == null)
            {
                throw new InvalidOperationException($"SynthesisProcess for ReviewProcess {reviewProcessId} not found.");
            }

            var themes = await _unitOfWork.SynthesisThemes.FindAllAsync(t => t.SynthesisProcessId == process.Id);
            var themesList = themes.ToList();
            var themeIds = themesList.Select(t => t.Id).ToList();
            
            var evidences = await _unitOfWork.ThemeEvidences.FindAllAsync(e => themeIds.Contains(e.ThemeId));
            var evidencesList = evidences.ToList();

            var extractedDataValueIds = evidencesList.Select(e => e.ExtractedDataValueId).Distinct().ToList();
            var extractedValues = await _unitOfWork.ExtractedDataValues.FindAllAsync(v => extractedDataValueIds.Contains(v.Id));
            var extractedValuesList = extractedValues.ToList();
            
            var paperIds = extractedValuesList.Select(v => v.PaperId).Distinct().ToList();
            var papers = await _unitOfWork.Papers.FindAllAsync(p => paperIds.Contains(p.Id));
            var papersDict = papers.ToDictionary(p => p.Id, p => p.Title);
            
            var optionIds = extractedValuesList.Where(v => v.OptionId.HasValue).Select(v => v.OptionId!.Value).Distinct().ToList();
            var fieldOptions = await _unitOfWork.FieldOptions.FindAllAsync(o => optionIds.Contains(o.Id));
            var optionsDict = fieldOptions.ToDictionary(o => o.Id, o => o.Value);

            string GetDisplayValue(ExtractedDataValue v)
            {
                if (v.OptionId.HasValue && optionsDict.ContainsKey(v.OptionId.Value))
                    return optionsDict[v.OptionId.Value];
                if (!string.IsNullOrEmpty(v.StringValue))
                    return v.StringValue;
                if (v.NumericValue.HasValue)
                    return v.NumericValue.Value.ToString();
                if (v.BooleanValue.HasValue)
                    return v.BooleanValue.Value ? "Yes" : "No";
                return string.Empty;
            }

            var extractedValuesDict = extractedValuesList.ToDictionary(
                v => v.Id, 
                v => new { Raw = v, PaperTitle = papersDict.ContainsKey(v.PaperId) ? papersDict[v.PaperId] : "Unknown Paper", DisplayValue = GetDisplayValue(v) });

            var findings = await _unitOfWork.ResearchQuestionFindings.FindAllAsync(f => f.SynthesisProcessId == process.Id);
            var findingsList = findings.ToList();

            var rqIds = findingsList.Select(f => f.ResearchQuestionId).Distinct().ToList();
            var rqs = await _unitOfWork.ResearchQuestions.FindAllAsync(rq => rqIds.Contains(rq.Id));
            var rqsDict = rqs.ToDictionary(rq => rq.Id, rq => rq.QuestionText);

            var processDto = new SynthesisProcessDto
            {
                Id = process.Id,
                ReviewProcessId = process.ReviewProcessId,
                Status = process.Status,
                StartedAt = process.StartedAt,
                CompletedAt = process.CompletedAt
            };

            var themeDtos = themesList.Select(t => new SynthesisThemeDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                ColorCode = t.ColorCode,
                CreatedById = t.CreatedById,
                CreatedAt = t.CreatedAt,
                ModifiedAt = t.ModifiedAt,
                Evidences = evidencesList.Where(e => e.ThemeId == t.Id).Select(e => new ThemeEvidenceDto
                {
                    Id = e.Id,
                    ThemeId = e.ThemeId,
                    ExtractedDataValueId = e.ExtractedDataValueId,
                    PaperTitle = extractedValuesDict.ContainsKey(e.ExtractedDataValueId) ? extractedValuesDict[e.ExtractedDataValueId].PaperTitle : "Unknown Paper",
                    StringValue = extractedValuesDict.ContainsKey(e.ExtractedDataValueId) ? extractedValuesDict[e.ExtractedDataValueId].Raw.StringValue : null,
                    NumericValue = extractedValuesDict.ContainsKey(e.ExtractedDataValueId) ? extractedValuesDict[e.ExtractedDataValueId].Raw.NumericValue : null,
                    BooleanValue = extractedValuesDict.ContainsKey(e.ExtractedDataValueId) ? extractedValuesDict[e.ExtractedDataValueId].Raw.BooleanValue : null,
                    OptionId = extractedValuesDict.ContainsKey(e.ExtractedDataValueId) ? extractedValuesDict[e.ExtractedDataValueId].Raw.OptionId : null,
                    DisplayValue = extractedValuesDict.ContainsKey(e.ExtractedDataValueId) ? extractedValuesDict[e.ExtractedDataValueId].DisplayValue : string.Empty,
                    Notes = e.Notes,
                    CreatedById = e.CreatedById,
                    CreatedAt = e.CreatedAt,
                    ModifiedAt = e.ModifiedAt
                }).ToList()
            }).ToList();

            var findingDtos = findingsList.Select(f => new ResearchQuestionFindingDto
            {
                Id = f.Id,
                ResearchQuestionId = f.ResearchQuestionId,
                QuestionText = rqsDict.ContainsKey(f.ResearchQuestionId) ? rqsDict[f.ResearchQuestionId] : "Unknown Question",
                AnswerText = f.AnswerText,
                Status = f.Status,
                AuthorId = f.AuthorId,
                CreatedAt = f.CreatedAt,
                ModifiedAt = f.ModifiedAt
            }).ToList();

            return new SynthesisWorkspaceDto
            {
                Process = processDto,
                Themes = themeDtos,
                Findings = findingDtos
            };
        }

        public async Task<SynthesisProcessDto> StartSynthesisProcessAsync(Guid reviewProcessId)
        {
            var reviewProcess = await _unitOfWork.ReviewProcesses.FindSingleAsync(r => r.Id == reviewProcessId);

            if (reviewProcess == null)
            {
                throw new InvalidOperationException($"ReviewProcess with ID {reviewProcessId} not found.");
            }

            if (reviewProcess.ProtocolId == null)
            {
                throw new InvalidOperationException($"ReviewProcess {reviewProcessId} has no assigned Protocol.");
            }

            var synthesisProcess = await _unitOfWork.SynthesisProcesses.FindSingleAsync(p => p.ReviewProcessId == reviewProcessId);

            if (synthesisProcess == null)
            {
                throw new InvalidOperationException("Synthesis process has not been initialized for this review process.");
            }

            if (synthesisProcess.Status != SynthesisProcessStatus.NotStarted)
            {
                throw new InvalidOperationException($"Synthesis process is already in {synthesisProcess.Status} status.");
            }

            var currentUserId = Guid.Parse(_currentUserService.GetUserId());
            var timestamp = DateTimeOffset.UtcNow;

            synthesisProcess.Status = SynthesisProcessStatus.InProgress;
            synthesisProcess.StartedAt = timestamp;
            synthesisProcess.ModifiedAt = timestamp;

            await _unitOfWork.SynthesisProcesses.UpdateAsync(synthesisProcess);

            var researchQuestions = await _unitOfWork.ResearchQuestions.FindAllAsync(rq => rq.ProjectId == reviewProcess.ProjectId);

            foreach (var rq in researchQuestions)
            {
                var finding = new ResearchQuestionFinding
                {
                    Id = Guid.NewGuid(),
                    SynthesisProcessId = synthesisProcess.Id,
                    ResearchQuestionId = rq.Id,
                    AnswerText = string.Empty,
                    Status = FindingStatus.Draft,
                    AuthorId = currentUserId,
                    CreatedAt = timestamp,
                    ModifiedAt = timestamp
                };
                await _unitOfWork.ResearchQuestionFindings.AddAsync(finding);
            }

            await _unitOfWork.SaveChangesAsync();

            return new SynthesisProcessDto
            {
                Id = synthesisProcess.Id,
                ReviewProcessId = synthesisProcess.ReviewProcessId,
                Status = synthesisProcess.Status,
                StartedAt = synthesisProcess.StartedAt,
                CompletedAt = synthesisProcess.CompletedAt
            };
        }

        public async Task CompleteSynthesisProcessAsync(Guid reviewProcessId)
        {
            var process = await _unitOfWork.SynthesisProcesses.FindSingleAsync(p => p.ReviewProcessId == reviewProcessId);
            if (process == null) throw new InvalidOperationException($"SynthesisProcess for ReviewProcess {reviewProcessId} not found.");

            if (process.Status != SynthesisProcessStatus.InProgress)
            {
                throw new InvalidOperationException("Process must be InProgress to complete.");
            }

            var findings = await _unitOfWork.ResearchQuestionFindings.FindAllAsync(f => f.SynthesisProcessId == process.Id);
            if (findings.Any(f => f.Status == FindingStatus.Draft))
            {
                throw new InvalidOperationException("Cannot complete Synthesis Phase while some Research Question findings are still drafts.");
            }

            process.Status = SynthesisProcessStatus.Completed;
            process.CompletedAt = DateTimeOffset.UtcNow;
            process.ModifiedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SynthesisProcesses.UpdateAsync(process);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<SourceDataGroupDto>> GetExtractedDataForSynthesisAsync(Guid reviewProcessId)
        {
            var extractionProcess = await _unitOfWork.DataExtractionProcesses
                .FindSingleAsync(p => p.ReviewProcessId == reviewProcessId);

            if (extractionProcess == null) return new List<SourceDataGroupDto>();

            var paperTasks = await _unitOfWork.ExtractionPaperTasks
                .FindAllAsync(pt => pt.DataExtractionProcessId == extractionProcess.Id);
                
            var paperIds = paperTasks.Select(pt => pt.PaperId).Distinct().ToList();
            if (!paperIds.Any()) return new List<SourceDataGroupDto>();

            var rawValues = await _unitOfWork.ExtractedDataValues
                .FindAllAsync(v => paperIds.Contains(v.PaperId) && v.IsConsensusFinal);
            
            var consensusList = rawValues.ToList();
            if (!consensusList.Any()) return new List<SourceDataGroupDto>();

            var optionIds = consensusList.Where(v => v.OptionId.HasValue).Select(v => v.OptionId!.Value).Distinct().ToList();
            var fieldOptions = await _unitOfWork.FieldOptions.FindAllAsync(o => optionIds.Contains(o.Id));
            var optionsDict = fieldOptions.ToDictionary(o => o.Id, o => o.Value);

            string GetDataDisplayValue(ExtractedDataValue v)
            {
                if (v.OptionId.HasValue && optionsDict.ContainsKey(v.OptionId.Value))
                    return optionsDict[v.OptionId.Value];
                if (!string.IsNullOrEmpty(v.StringValue))
                    return v.StringValue;
                if (v.NumericValue.HasValue)
                    return v.NumericValue.Value.ToString();
                if (v.BooleanValue.HasValue)
                    return v.BooleanValue.Value ? "Yes" : "No";
                return string.Empty;
            }

            var fieldIds = consensusList.Select(v => v.FieldId).Distinct().ToList();
            var fields = await _unitOfWork.ExtractionFields.FindAllAsync(f => fieldIds.Contains(f.Id));
            var fieldDict = fields.ToDictionary(f => f.Id, f => f.Name);

            var reqPaperIds = consensusList.Select(v => v.PaperId).Distinct().ToList();
            var papers = await _unitOfWork.Papers.FindAllAsync(p => reqPaperIds.Contains(p.Id));
            var paperDict = papers.ToDictionary(p => p.Id, p => p.Title);

            var groups = consensusList
                .Where(v => fieldDict.ContainsKey(v.FieldId) && paperDict.ContainsKey(v.PaperId))
                .GroupBy(v => v.FieldId)
                .Select(g => new SourceDataGroupDto
                {
                    FieldId = g.Key,
                    FieldName = fieldDict[g.Key],
                    Values = g.Select(v => new SourceDataValueDto
                    {
                        ExtractedDataValueId = v.Id,
                        PaperId = v.PaperId,
                        PaperTitle = paperDict[v.PaperId],
                        StringValue = v.StringValue,
                        NumericValue = v.NumericValue,
                        BooleanValue = v.BooleanValue,
                        OptionId = v.OptionId,
                        DisplayValue = GetDataDisplayValue(v)
                    }).ToList()
                }).ToList();

            return groups;
        }

        public async Task<SynthesisThemeDto> CreateThemeAsync(Guid processId, CreateThemeRequest request)
        {
            var process = await _unitOfWork.SynthesisProcesses.FindSingleAsync(p => p.Id == processId);
            if (process == null) throw new InvalidOperationException("Synthesis process not found.");

            var timestamp = DateTimeOffset.UtcNow;
            var theme = new SynthesisTheme
            {
                Id = Guid.NewGuid(),
                SynthesisProcessId = processId,
                Name = request.Name,
                Description = request.Description,
                ColorCode = request.ColorCode,
                CreatedById = Guid.Parse(_currentUserService.GetUserId()),
                CreatedAt = timestamp,
                ModifiedAt = timestamp
            };

            await _unitOfWork.SynthesisThemes.AddAsync(theme);
            await _unitOfWork.SaveChangesAsync();

            return new SynthesisThemeDto
            {
                Id = theme.Id,
                Name = theme.Name,
                Description = theme.Description,
                ColorCode = theme.ColorCode,
                CreatedById = theme.CreatedById,
                CreatedAt = theme.CreatedAt,
                ModifiedAt = theme.ModifiedAt,
                Evidences = new List<ThemeEvidenceDto>()
            };
        }

        public async Task UpdateThemeAsync(Guid themeId, UpdateThemeRequest request)
        {
            var theme = await _unitOfWork.SynthesisThemes.FindSingleAsync(t => t.Id == themeId);
            if (theme == null) throw new KeyNotFoundException("Theme not found.");

            theme.Name = request.Name;
            theme.Description = request.Description;
            theme.ColorCode = request.ColorCode;
            theme.ModifiedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SynthesisThemes.UpdateAsync(theme);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteThemeAsync(Guid themeId)
        {
            var theme = await _unitOfWork.SynthesisThemes.FindSingleAsync(t => t.Id == themeId);
            if (theme == null) throw new KeyNotFoundException("Theme not found.");

            await _unitOfWork.SynthesisThemes.RemoveAsync(theme);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<ThemeEvidenceDto> AddEvidenceToThemeAsync(Guid themeId, AddEvidenceRequest request)
        {
            var theme = await _unitOfWork.SynthesisThemes.FindSingleAsync(t => t.Id == themeId);
            if (theme == null) throw new KeyNotFoundException("Theme not found.");

            var extractedValue = await _unitOfWork.ExtractedDataValues.FindSingleAsync(ev => ev.Id == request.ExtractedDataValueId);

            if (extractedValue == null)
            {
                throw new InvalidOperationException("Extracted data value not found.");
            }
            if (!extractedValue.IsConsensusFinal)
            {
                throw new InvalidOperationException("Extracted data value is not a final consensus value.");
            }

            var timestamp = DateTimeOffset.UtcNow;
            var evidence = new ThemeEvidence
            {
                Id = Guid.NewGuid(),
                ThemeId = themeId,
                ExtractedDataValueId = request.ExtractedDataValueId,
                Notes = request.Notes,
                CreatedById = Guid.Parse(_currentUserService.GetUserId()),
                CreatedAt = timestamp,
                ModifiedAt = timestamp
            };

            await _unitOfWork.ThemeEvidences.AddAsync(evidence);
            await _unitOfWork.SaveChangesAsync();

            return new ThemeEvidenceDto
            {
                Id = evidence.Id,
                ThemeId = evidence.ThemeId,
                ExtractedDataValueId = evidence.ExtractedDataValueId,
                Notes = evidence.Notes,
                CreatedById = evidence.CreatedById,
                CreatedAt = evidence.CreatedAt,
                ModifiedAt = evidence.ModifiedAt
            };
        }

        public async Task RemoveEvidenceAsync(Guid evidenceId)
        {
            var evidence = await _unitOfWork.ThemeEvidences.FindSingleAsync(e => e.Id == evidenceId);
            if (evidence == null) throw new KeyNotFoundException("Theme evidence not found.");

            await _unitOfWork.ThemeEvidences.RemoveAsync(evidence);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task SaveFindingAsync(Guid findingId, SaveFindingRequest request)
        {
            var finding = await _unitOfWork.ResearchQuestionFindings.FindSingleAsync(f => f.Id == findingId);
            if (finding == null) throw new KeyNotFoundException("Finding not found.");

            finding.AnswerText = request.AnswerText;
            finding.Status = request.Status;
            finding.AuthorId = Guid.Parse(_currentUserService.GetUserId());
            finding.ModifiedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.ResearchQuestionFindings.UpdateAsync(finding);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
