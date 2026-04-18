using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.StudySelectionChecklist;
using SRSS.IAM.Services.Mappers;
using Shared.Exceptions;
using SRSS.IAM.Services.UserService;

namespace SRSS.IAM.Services.StudySelectionChecklists
{
    public class StudySelectionChecklistSubmissionService : IStudySelectionChecklistSubmissionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public StudySelectionChecklistSubmissionService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<ChecklistSubmissionDto> CreateSubmissionAsync(CreateSubmissionRequest request, CancellationToken cancellationToken = default)
        {
            var template = await _unitOfWork.StudySelectionChecklistTemplates.FindSingleAsync(t => t.Id == request.ChecklistTemplateId, cancellationToken: cancellationToken);
            if (template == null) throw new NotFoundException($"Checklist template with ID {request.ChecklistTemplateId} not found.");

            if (!template.IsActive) throw new BadRequestException("Template is not active, please reload page or contact leader to update template.");

            var existing = await _unitOfWork.StudySelectionChecklistSubmissions.GetByContextWithAnswersAsync(
                request.StudySelectionProcessId,
                request.PaperId,
                request.ReviewerId,
                request.Phase,
                cancellationToken);

            StudySelectionChecklistSubmission submission;

            if (existing != null)
            {
                submission = existing;
                submission.ChecklistTemplateId = request.ChecklistTemplateId;
                submission.SubmittedAt = DateTimeOffset.UtcNow;
                submission.ModifiedAt = DateTimeOffset.UtcNow;

                // Clear existing answers to replace with new ones
                var existingSections = await _unitOfWork.StudySelectionChecklistSubmissionSectionAnswers.FindAllAsync(a => a.SubmissionId == submission.Id);
                foreach (var s in existingSections) await _unitOfWork.StudySelectionChecklistSubmissionSectionAnswers.RemoveAsync(s);

                var existingItems = await _unitOfWork.StudySelectionChecklistSubmissionItemAnswers.FindAllAsync(a => a.SubmissionId == submission.Id);
                foreach (var i in existingItems) await _unitOfWork.StudySelectionChecklistSubmissionItemAnswers.RemoveAsync(i);
            }
            else
            {
                submission = new StudySelectionChecklistSubmission
                {
                    Id = Guid.NewGuid(),
                    StudySelectionProcessId = request.StudySelectionProcessId,
                    PaperId = request.PaperId,
                    ReviewerId = request.ReviewerId,
                    Phase = request.Phase,
                    ChecklistTemplateId = request.ChecklistTemplateId,
                    SubmittedAt = DateTimeOffset.UtcNow,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                };
                await _unitOfWork.StudySelectionChecklistSubmissions.AddAsync(submission, cancellationToken);
            }

            // Add new answers (ensure uniqueness at domain level by using a set or checking duplicates is not required as per prompt, 
            // but we can ensure we don't map same section/item twice from request)
            var sectionIds = new HashSet<Guid>();
            foreach (var sa in request.SectionAnswers)
            {
                if (!sectionIds.Add(sa.SectionId)) continue;
                await _unitOfWork.StudySelectionChecklistSubmissionSectionAnswers.AddAsync(new StudySelectionChecklistSubmissionSectionAnswer
                {
                    Id = Guid.NewGuid(),
                    SubmissionId = submission.Id,
                    SectionId = sa.SectionId,
                    IsChecked = sa.IsChecked,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                }, cancellationToken);
            }

            var itemIds = new HashSet<Guid>();
            foreach (var ia in request.ItemAnswers)
            {
                if (!itemIds.Add(ia.ItemId)) continue;
                await _unitOfWork.StudySelectionChecklistSubmissionItemAnswers.AddAsync(new StudySelectionChecklistSubmissionItemAnswer
                {
                    Id = Guid.NewGuid(),
                    SubmissionId = submission.Id,
                    ItemId = ia.ItemId,
                    IsChecked = ia.IsChecked,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                }, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return submission.MapToDto();
        }

        public async Task<ChecklistSubmissionDto> GetSubmissionAsync(Guid submissionId, CancellationToken cancellationToken = default)
        {
            var submission = await _unitOfWork.StudySelectionChecklistSubmissions.FindSingleAsync(s => s.Id == submissionId, cancellationToken: cancellationToken);
            if (submission == null) throw new NotFoundException($"Submission with ID {submissionId} not found.");
            return submission.MapToDto();
        }

        public async Task<ChecklistReviewDto> GetChecklistForReviewByContextAsync(Guid processId, Guid paperId, Guid reviewerId, ScreeningPhase phase, CancellationToken cancellationToken = default)
        {
            // 1. Load Process context once
            var process = await _unitOfWork.StudySelectionProcesses.FindSingleAsync(p => p.Id == processId, cancellationToken: cancellationToken);
            if (process == null) throw new NotFoundException($"Study selection process with ID {processId} not found.");

            var rProcess = await _unitOfWork.ReviewProcesses.FindSingleAsync(p => p.Id == process.ReviewProcessId, cancellationToken: cancellationToken);
            if (rProcess == null) throw new NotFoundException($"Review process for ID {process.ReviewProcessId} not found.");

            // 2. Try to find existing submission with answers
            var submission = await _unitOfWork.StudySelectionChecklistSubmissions.GetByContextWithAnswersAsync(processId, paperId, reviewerId, phase, cancellationToken);

            StudySelectionChecklistTemplate? template = null;

            if (submission != null)
            {
                // 3. Load EXACT template linked to the submission
                template = await _unitOfWork.StudySelectionChecklistTemplates.GetByIdWithDetailsAsync(submission.ChecklistTemplateId, rProcess.ProjectId, cancellationToken);

                if (template == null)
                {
                    throw new NotFoundException($"Checklist template with ID {submission.ChecklistTemplateId} linked to existing submission not found.");
                }
            }
            else
            {
                // 4. Load active template for new submission
                template = await _unitOfWork.StudySelectionChecklistTemplates.GetActiveWithDetailsAsync(rProcess.ProjectId, cancellationToken);

                if (template == null)
                {
                    throw new NotFoundException($"No active checklist template found for project {rProcess.ProjectId}.");
                }
            }

            // 5. Map and return fully hydrated model
            return template.MapToReviewDto(submission);
        }

        public async Task<ChecklistReviewDto> GetSubmissionByContextAsync(Guid processId, Guid paperId, Guid reviewerId, ScreeningPhase phase, CancellationToken cancellationToken = default)
        {
            // 1. Try to find existing submission with answers
            var submission = await _unitOfWork.StudySelectionChecklistSubmissions.GetByContextWithAnswersAsync(processId, paperId, reviewerId, phase, cancellationToken);
            if (submission == null)
            {
                throw new NotFoundException($"Submission not found for reviewer {reviewerId} on paper {paperId}.");
            }

            // 2. Load Process context to get project ID
            var process = await _unitOfWork.StudySelectionProcesses.FindSingleAsync(p => p.Id == processId, cancellationToken: cancellationToken);
            if (process == null) throw new NotFoundException($"Study selection process with ID {processId} not found.");

            var rProcess = await _unitOfWork.ReviewProcesses.FindSingleAsync(p => p.Id == process.ReviewProcessId, cancellationToken: cancellationToken);
            if (rProcess == null) throw new NotFoundException($"Review process for ID {process.ReviewProcessId} not found.");

            // 3. Load EXACT template linked to the submission
            var template = await _unitOfWork.StudySelectionChecklistTemplates.GetByIdWithDetailsAsync(submission.ChecklistTemplateId, rProcess.ProjectId, cancellationToken);
            if (template == null)
            {
                throw new NotFoundException($"Checklist template with ID {submission.ChecklistTemplateId} linked to existing submission not found.");
            }

            // 4. Map and return
            return template.MapToReviewDto(submission);
        }


    }
}
