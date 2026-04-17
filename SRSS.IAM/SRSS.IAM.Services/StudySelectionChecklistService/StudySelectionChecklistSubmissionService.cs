using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.StudySelectionChecklist;
using SRSS.IAM.Services.Mappers;
using Microsoft.EntityFrameworkCore;

namespace SRSS.IAM.Services.StudySelectionChecklists
{
    public class StudySelectionChecklistSubmissionService : IStudySelectionChecklistSubmissionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StudySelectionChecklistSubmissionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ChecklistSubmissionDto> CreateSubmissionAsync(CreateSubmissionRequest request, CancellationToken cancellationToken = default)
        {
            var decision = await _unitOfWork.ScreeningDecisions.FindSingleAsync(d => d.Id == request.ScreeningDecisionId, cancellationToken: cancellationToken);
            if (decision == null) throw new InvalidOperationException("Screening decision not found.");

            var template = await _unitOfWork.StudySelectionChecklistTemplates.FindSingleAsync(t => t.Id == request.ChecklistTemplateId, cancellationToken: cancellationToken);
            if (template == null) throw new InvalidOperationException("Checklist template not found.");

            var existing = await _unitOfWork.StudySelectionChecklistSubmissions.GetByDecisionIdAsync(request.ScreeningDecisionId, cancellationToken);
            if (existing != null)
            {
                return existing.MapToDto();
            }

            var submission = new StudySelectionChecklistSubmission
            {
                Id = Guid.NewGuid(),
                ScreeningDecisionId = request.ScreeningDecisionId,
                ChecklistTemplateId = request.ChecklistTemplateId,
                SubmittedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.StudySelectionChecklistSubmissions.AddAsync(submission, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return submission.MapToDto();
        }

        public async Task<ChecklistSubmissionDto> GetSubmissionAsync(Guid submissionId, CancellationToken cancellationToken = default)
        {
            var submission = await _unitOfWork.StudySelectionChecklistSubmissions.FindSingleAsync(s => s.Id == submissionId, cancellationToken: cancellationToken);
            if (submission == null) throw new InvalidOperationException("Submission not found.");
            return submission.MapToDto();
        }

        public async Task<ChecklistSubmissionDto?> GetSubmissionByDecisionIdAsync(Guid decisionId, CancellationToken cancellationToken = default)
        {
            var submission = await _unitOfWork.StudySelectionChecklistSubmissions.GetByDecisionIdAsync(decisionId, cancellationToken);
            return submission?.MapToDto();
        }

        public async Task<ChecklistSubmissionDto?> GetSubmissionByPaperAndPhaseAsync(Guid paperId, ScreeningPhase phase, CancellationToken cancellationToken = default)
        {
            var submission = await _unitOfWork.StudySelectionChecklistSubmissions.GetByPaperAndPhaseAsync(paperId, phase, cancellationToken);
            return submission?.MapToDto();
        }
    }
}
