using Microsoft.EntityFrameworkCore;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.StudySelectionChecklist;
using SRSS.IAM.Services.Mappers;

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

            var createdTemplate = await _unitOfWork.StudySelectionChecklistTemplates.GetWithDetailsAsync(projectId, cancellationToken);
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
            var template = await _unitOfWork.StudySelectionChecklistTemplates.GetWithDetailsAsync(rProcess!.ProjectId, cancellationToken);

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
    }
}
