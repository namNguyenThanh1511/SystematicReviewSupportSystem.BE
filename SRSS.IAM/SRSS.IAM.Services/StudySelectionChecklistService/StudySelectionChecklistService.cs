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
            var existing = await _unitOfWork.StudySelectionChecklistTemplates.GetByProjectIdAsync(projectId, cancellationToken);
            if (existing != null)
            {
                throw new InvalidOperationException("Checklist template already exists for this project.");
            }

            var project = await _unitOfWork.SystematicReviewProjects.FindSingleAsync(p => p.Id == projectId, cancellationToken: cancellationToken);
            if (project == null)
            {
                throw new InvalidOperationException($"Project with ID {projectId} not found.");
            }

            var template = new StudySelectionChecklistTemplate
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                Name = request.Name,
                Description = request.Description,
                Version = 1,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.StudySelectionChecklistTemplates.AddAsync(template, cancellationToken);

            foreach (var sectionReq in request.Sections)
            {
                var section = new StudySelectionChecklistTemplateSection
                {
                    Id = Guid.NewGuid(),
                    TemplateId = template.Id,
                    Title = sectionReq.Title,
                    Description = sectionReq.Description,
                    Order = sectionReq.Order,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                };
                await _unitOfWork.StudySelectionChecklistTemplateSections.AddAsync(section, cancellationToken);

                foreach (var itemReq in sectionReq.Items)
                {
                    var item = new StudySelectionChecklistTemplateItem
                    {
                        Id = Guid.NewGuid(),
                        SectionId = section.Id,
                        Text = itemReq.Text,
                        Order = itemReq.Order,
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = DateTimeOffset.UtcNow
                    };
                    await _unitOfWork.StudySelectionChecklistTemplateItems.AddAsync(item, cancellationToken);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Fetch with details for mapping
            var createdTemplate = await _unitOfWork.StudySelectionChecklistTemplates.GetWithDetailsAsync(projectId, cancellationToken);
            return createdTemplate!.MapToDto();
        }

        public async Task<StudySelectionChecklistTemplateDto> UpdateTemplateAsync(Guid projectId, UpdateStudySelectionChecklistTemplateRequest request, CancellationToken cancellationToken = default)
        {
            var template = await _unitOfWork.StudySelectionChecklistTemplates.GetWithDetailsAsync(projectId, cancellationToken);
            if (template == null)
            {
                throw new InvalidOperationException($"Checklist template for project {projectId} not found.");
            }

            template.Name = request.Name;
            template.Description = request.Description;
            template.Version++; // Increment version on update
            template.ModifiedAt = DateTimeOffset.UtcNow;

            // Remove existing sections (and items via cascade or manual if necessary)
            foreach (var section in template.Sections.ToList())
            {
                await _unitOfWork.StudySelectionChecklistTemplateSections.RemoveAsync(section, cancellationToken);
            }

            // Add new sections and items
            foreach (var sectionReq in request.Sections)
            {
                var section = new StudySelectionChecklistTemplateSection
                {
                    Id = Guid.NewGuid(),
                    TemplateId = template.Id,
                    Title = sectionReq.Title,
                    Description = sectionReq.Description,
                    Order = sectionReq.Order,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                };
                await _unitOfWork.StudySelectionChecklistTemplateSections.AddAsync(section, cancellationToken);

                foreach (var itemReq in sectionReq.Items)
                {
                    var item = new StudySelectionChecklistTemplateItem
                    {
                        Id = Guid.NewGuid(),
                        SectionId = section.Id,
                        Text = itemReq.Text,
                        Order = itemReq.Order,
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = DateTimeOffset.UtcNow
                    };
                    await _unitOfWork.StudySelectionChecklistTemplateItems.AddAsync(item, cancellationToken);
                }
            }

            await _unitOfWork.StudySelectionChecklistTemplates.UpdateAsync(template, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var updatedTemplate = await _unitOfWork.StudySelectionChecklistTemplates.GetWithDetailsAsync(projectId, cancellationToken);
            return updatedTemplate!.MapToDto();
        }

        public async Task<StudySelectionChecklistTemplateDto> GetTemplateByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            var template = await _unitOfWork.StudySelectionChecklistTemplates.GetWithDetailsAsync(projectId, cancellationToken);
            if (template == null)
            {
                throw new InvalidOperationException($"Checklist template for project {projectId} not found.");
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
    }
}
