using Microsoft.EntityFrameworkCore;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.StudySelectionChecklist;
using SRSS.IAM.Services.Mappers;
using Shared.Exceptions;

namespace SRSS.IAM.Services.StudySelectionChecklists
{
    public class StudySelectionChecklistService : IStudySelectionChecklistService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StudySelectionChecklistService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<StudySelectionChecklistTemplateDto> CreateTemplateAsync(Guid projectId, CreateStudySelectionChecklistTemplateRequest request, CancellationToken cancellationToken = default)
        {
            var project = await _unitOfWork.SystematicReviewProjects.FindSingleAsync(p => p.Id == projectId, cancellationToken: cancellationToken);
            if (project == null)
            {
                throw new InvalidOperationException($"Project with ID {projectId} not found.");
            }

            var sections = request.Sections.Select(s => (
                s.Title,
                s.Description,
                s.Order,
                Items: s.Items.Select(i => (i.Text, i.Order))
            ));

            var template = await SaveNewTemplateVersionAsync(projectId, request.Name, request.Description, sections, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var createdTemplate = await _unitOfWork.StudySelectionChecklistTemplates.GetActiveWithDetailsAsync(projectId, cancellationToken);
            return createdTemplate!.MapToDto();
        }



        private async Task<StudySelectionChecklistTemplate> SaveNewTemplateVersionAsync(
            Guid projectId,
            string name,
            string? description,
            IEnumerable<(string Title, string? Description, int Order, IEnumerable<(string Text, int Order)> Items)> sections,
            CancellationToken cancellationToken)
        {
            var latest = await _unitOfWork.StudySelectionChecklistTemplates.GetByProjectIdAsync(projectId, cancellationToken);
            int nextVersion = (latest?.Version ?? 0) + 1;

            var template = new StudySelectionChecklistTemplate
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                Name = name,
                Description = description,
                Version = nextVersion,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.StudySelectionChecklistTemplates.AddAsync(template, cancellationToken);

            foreach (var s in sections)
            {
                var section = new StudySelectionChecklistTemplateSection
                {
                    Id = Guid.NewGuid(),
                    TemplateId = template.Id,
                    Title = s.Title,
                    Description = s.Description,
                    Order = s.Order,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                };
                await _unitOfWork.StudySelectionChecklistTemplateSections.AddAsync(section, cancellationToken);

                foreach (var i in s.Items)
                {
                    var item = new StudySelectionChecklistTemplateItem
                    {
                        Id = Guid.NewGuid(),
                        SectionId = section.Id,
                        Text = i.Text,
                        Order = i.Order,
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = DateTimeOffset.UtcNow
                    };
                    await _unitOfWork.StudySelectionChecklistTemplateItems.AddAsync(item, cancellationToken);
                }
            }

            return template;
        }

        public async Task<IEnumerable<StudySelectionChecklistTemplateSummaryDto>> GetTemplatesByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            var templates = await _unitOfWork.StudySelectionChecklistTemplates.GetAllByProjectIdAsync(projectId, cancellationToken);
            return templates.Select(t => t.MapToSummaryDto());
        }

        public async Task<StudySelectionChecklistTemplateDto> GetTemplateDetailAsync(Guid projectId, Guid templateId, CancellationToken cancellationToken = default)
        {
            var template = await _unitOfWork.StudySelectionChecklistTemplates.GetByIdWithDetailsAsync(templateId, projectId, cancellationToken);
            if (template == null)
            {
                throw new InvalidOperationException($"Template with ID {templateId} not found for project {projectId}.");
            }
            return template.MapToDto();
        }

        public async Task<PaperChecklistResponse> GetChecklistForPaperAsync(Guid processId, Guid paperId, ScreeningPhase phase, CancellationToken cancellationToken = default)
        {
            var process = await _unitOfWork.StudySelectionProcesses.FindSingleAsync(p => p.Id == processId, cancellationToken: cancellationToken);
            if (process == null) throw new InvalidOperationException("Selection process not found.");

            var rProcess = await _unitOfWork.ReviewProcesses.FindSingleAsync(p => p.Id == process.ReviewProcessId, cancellationToken: cancellationToken);
            var template = await _unitOfWork.StudySelectionChecklistTemplates.GetActiveWithDetailsAsync(rProcess!.ProjectId, cancellationToken);

            if (template == null)
            {
                return new PaperChecklistResponse
                {
                    Sections = new List<StudySelectionChecklistTemplateSectionDto>()
                };
            }

            var response = new PaperChecklistResponse
            {
                ChecklistTemplateId = template.Id,
                Sections = template.MapToDto().Sections
            };

            return response;
        }
        public async Task<bool> ActivateTemplateAsync(Guid templateId, CancellationToken cancellationToken = default)
        {
            var template = await _unitOfWork.StudySelectionChecklistTemplates.FindSingleAsync(t => t.Id == templateId, cancellationToken: cancellationToken);
            if (template == null)
            {
                throw new InvalidOperationException($"Template with ID {templateId} not found.");
            }

            // Deactivate all templates for this project
            var templates = await _unitOfWork.StudySelectionChecklistTemplates.FindAllAsync(t => t.ProjectId == template.ProjectId, cancellationToken: cancellationToken);
            foreach (var t in templates)
            {
                t.IsActive = false;
                t.ModifiedAt = DateTimeOffset.UtcNow;
            }

            // Activate the selected template
            template.IsActive = true;
            template.ModifiedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<LiveReviewChecklistDto> GetLiveReviewChecklistByProcessAsync(Guid studySelectionProcessId, CancellationToken cancellationToken = default)
        {
            // Load Study Selection Process with all related data needed for mapping
            var process = await _unitOfWork.StudySelectionProcesses.GetQueryable()
                .AsNoTracking()
                .Include(ssp => ssp.ReviewProcess)
                    .ThenInclude(rp => rp.Project)
                        .ThenInclude(p => p.ResearchQuestions)
                            // .ThenInclude(rq => rq.PicocElements)
                .Include(ssp => ssp.ReviewProcess)
                    .ThenInclude(rp => rp.Project)
                        .ThenInclude(p => p.SelectionCriterias)
                            .ThenInclude(sc => sc.InclusionCriteria)
                .Include(ssp => ssp.ReviewProcess)
                    .ThenInclude(rp => rp.Project)
                        .ThenInclude(p => p.SelectionCriterias)
                            .ThenInclude(sc => sc.ExclusionCriteria)
                .FirstOrDefaultAsync(ssp => ssp.Id == studySelectionProcessId, cancellationToken);

            if (process == null)
            {
                throw new NotFoundException($"Study Selection Process with ID {studySelectionProcessId} not found.");
            }

            var project = process.ReviewProcess.Project;

            return MapToLiveReviewChecklist(project);
        }

        private LiveReviewChecklistDto MapToLiveReviewChecklist(SystematicReviewProject project)
        {
            // 2. Map to LiveReviewChecklistDto
            var result = new LiveReviewChecklistDto
            {
                Title = "Study Selection Checklist",
                Paragraphs = new List<LiveReviewParagraphDto>
                {
                    new LiveReviewParagraphDto { Text = "Basic info" }
                },
                Sections = new List<LiveReviewSectionDto>()
            };

            // 3. Map Research Questions
            if (project.ResearchQuestions != null)
            {
                foreach (var rq in project.ResearchQuestions)
                {
                    // var picocItems = rq.PicocElements != null && rq.PicocElements.Any()
                    //     ? MapPicocToItems(rq.PicocElements)
                    //     : null;

                    var section = new LiveReviewSectionDto
                    {
                        Title = rq.QuestionText ?? "Untitled Research Question",
                        // Items = picocItems
                    };

                    result.Sections.Add(section);
                }
            }

            // 4. Map Criteria Groups
            if (project.SelectionCriterias != null)
            {
                foreach (var cg in project.SelectionCriterias)
                {
                    var section = new LiveReviewSectionDto
                    {
                        Title = cg.Description ?? "Eligibility Criteria",
                        Items = new List<LiveReviewItemDto>()
                    };

                    if (cg.InclusionCriteria != null)
                    {
                        foreach (var inc in cg.InclusionCriteria.Where(c => !string.IsNullOrWhiteSpace(c.Rule)))
                        {
                            section.Items.Add(new LiveReviewItemDto { Text = $"Include: {inc.Rule!.Trim()}" });
                        }
                    }

                    if (cg.ExclusionCriteria != null)
                    {
                        foreach (var exc in cg.ExclusionCriteria.Where(c => !string.IsNullOrWhiteSpace(c.Rule)))
                        {
                            section.Items.Add(new LiveReviewItemDto { Text = $"Exclude: {exc.Rule!.Trim()}" });
                        }
                    }

                    if (!section.Items.Any())
                    {
                        section.Items = null;
                    }

                    result.Sections.Add(section);
                }
            }

            return result;
        }

        private List<LiveReviewItemDto>? MapPicocToItems(IEnumerable<PicocElement> elements)
        {
            var items = new List<LiveReviewItemDto>();
            foreach (var element in elements)
            {
                var type = element.ElementType?.Trim().ToLower();
                var desc = element.Description?.Trim();
                if (string.IsNullOrWhiteSpace(desc)) continue;

                var label = type switch
                {
                    "population" => "Population",
                    "intervention" => "Intervention",
                    "comparison" => "Comparison",
                    "outcome" => "Outcome",
                    "context" => "Context",
                    _ => null
                };

                if (label != null)
                {
                    items.Add(new LiveReviewItemDto { Text = $"{label}: {desc}" });
                }
            }
            return items.Any() ? items : null;
        }
    }
}
