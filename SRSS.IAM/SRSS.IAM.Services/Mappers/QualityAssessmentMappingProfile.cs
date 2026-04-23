using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Services.DTOs.QualityAssessment;
using SRSS.IAM.Services.DTOs.SelectionCriteria;
using SRSS.IAM.Services.DTOs.StudySelection;

namespace SRSS.IAM.Services.Mappers
{
    public static class QualityAssessmentMappingExtension
    {
        // ==================== QualityAssessmentStrategy ====================
        public static QualityAssessmentStrategyDto ToDto(this QualityAssessmentStrategy entity)
        {
            return new QualityAssessmentStrategyDto
            {
                QaStrategyId = entity.Id,
                ProjectId = entity.ProjectId,
                Description = entity.Description ?? string.Empty,
                Checklists = entity.Checklists?.Select(c => c.ToDtoWithCriteria()).ToList() ?? new List<QualityAssessmentChecklistDto>()
            };
        }

        public static QualityAssessmentChecklistDto ToDtoWithCriteria(this QualityChecklist entity)
        {
            var dto = entity.ToDto();
            dto.Criteria = entity.Criteria?.Select(c => c.ToDto()).ToList() ?? new List<QualityAssessmentCriterionDto>();
            return dto;
        }

        public static QualityAssessmentStrategy ToEntity(this QualityAssessmentStrategyDto dto)
        {
            return new QualityAssessmentStrategy
            {
                Id = dto.QaStrategyId ?? Guid.Empty,
                ProjectId = dto.ProjectId,
                Description = dto.Description ?? string.Empty
            };
        }

        public static void UpdateEntity(this QualityAssessmentStrategyDto dto, QualityAssessmentStrategy entity)
        {
            entity.ProjectId = dto.ProjectId;
            entity.Description = dto.Description ?? string.Empty;
        }

        // ==================== QualityChecklist ====================
        public static QualityAssessmentChecklistDto ToDto(this QualityChecklist entity)
        {
            return new QualityAssessmentChecklistDto
            {
                ChecklistId = entity.Id,
                QaStrategyId = entity.QaStrategyId,
                Name = entity.Name
            };
        }

        public static QualityChecklist ToEntity(this QualityAssessmentChecklistDto dto)
        {
            return new QualityChecklist
            {
                Id = dto.ChecklistId ?? Guid.Empty,
                QaStrategyId = dto.QaStrategyId,
                Name = dto.Name
            };
        }

        public static void UpdateEntity(this QualityAssessmentChecklistDto dto, QualityChecklist entity)
        {
            entity.Id = dto.ChecklistId ?? entity.Id; // Preserve existing ID if not provided
            entity.QaStrategyId = dto.QaStrategyId;
            entity.Name = dto.Name;
        }

        // ==================== QualityCriterion ====================
        public static QualityAssessmentCriterionDto ToDto(this QualityCriterion entity)
        {
            return new QualityAssessmentCriterionDto
            {
                CriterionId = entity.Id,
                ChecklistId = entity.ChecklistId,
                Question = entity.Question,
                Weight = entity.Weight
            };
        }

        public static QualityCriterion ToEntity(this QualityAssessmentCriterionDto dto)
        {
            return new QualityCriterion
            {
                Id = dto.CriterionId ?? Guid.Empty,
                ChecklistId = dto.ChecklistId,
                Question = dto.Question,
                Weight = dto.Weight
            };
        }

        public static void UpdateEntity(this QualityAssessmentCriterionDto dto, QualityCriterion entity)
        {
            entity.Id = dto.CriterionId ?? entity.Id; // Preserve existing ID if not provided
            entity.ChecklistId = dto.ChecklistId;
            entity.Question = dto.Question;
            entity.Weight = dto.Weight;
        }

        // ==================== List Mapping ====================
        public static List<QualityAssessmentStrategyDto> ToDtoList(this IEnumerable<QualityAssessmentStrategy> entities)
        {
            return entities.Select(e => e.ToDto()).ToList();
        }

        public static List<QualityAssessmentChecklistDto> ToDtoList(this IEnumerable<QualityChecklist> entities)
        {
            return entities.Select(e => e.ToDto()).ToList();
        }

        public static List<QualityAssessmentCriterionDto> ToDtoList(this IEnumerable<QualityCriterion> entities)
        {
            return entities.Select(e => e.ToDto()).ToList();
        }

        // ============================== Paper ============================
        public static QAPaperResponse ToResponse(this Paper entity)
        {
            if (entity == null) return null!;

            return new QAPaperResponse
            {
                Id = entity.Id,
                Title = entity.Title,
                Authors = entity.Authors,
                Abstract = entity.Abstract,
                DOI = entity.DOI,
                PublicationType = entity.PublicationType,
                PublicationYear = entity.PublicationYear,
                PublicationYearInt = entity.PublicationYearInt,
                PublicationDate = entity.PublicationDate,
                Volume = entity.Volume,
                Issue = entity.Issue,
                Pages = entity.Pages,
                Publisher = entity.Publisher,
                Language = entity.Language,
                Keywords = entity.Keywords,
                Url = entity.Url,
                ConferenceName = entity.ConferenceName,
                ConferenceLocation = entity.ConferenceLocation,
                ConferenceCountry = entity.ConferenceCountry,
                ConferenceYear = entity.ConferenceYear,
                Journal = entity.Journal,
                JournalIssn = entity.JournalIssn,
                Source = entity.Source,
                ImportedAt = entity.ImportedAt,
                ImportedBy = entity.ImportedBy,
                PdfUrl = entity.PdfUrl,
                FullTextAvailable = entity.FullTextAvailable,
                CreatedAt = entity.CreatedAt,
                ModifiedAt = entity.ModifiedAt
            };
        }

        // ==================== QualityAssessmentProcess ====================
        public static QualityAssessmentProcess ToEntity(this CreateQualityAssessmentProcessDto dto)
        {
            return new QualityAssessmentProcess
            {
                ReviewProcessId = dto.ReviewProcessId,
                Notes = dto.Notes,
                Status = QualityAssessmentProcessStatus.NotStarted
            };
        }

        public static void UpdateEntity(this UpdateQualityAssessmentProcessRequest dto, QualityAssessmentProcess entity)
        {
            entity.Notes = dto.Notes;
        }

        public static QualityAssessmentProcessResponse ToResponse(this QualityAssessmentProcess entity)
        {
            if (entity == null) return null!;

            return new QualityAssessmentProcessResponse
            {
                Id = entity.Id,
                ReviewProcessId = entity.ReviewProcessId,
                Notes = entity.Notes,
                Status = entity.Status,
                StatusText = entity.Status.ToString(),
                StartedAt = entity.StartedAt,
                CompletedAt = entity.CompletedAt,
                CreatedAt = entity.CreatedAt,
                ModifiedAt = entity.ModifiedAt
            };
        }

        // ==================== QualityAssessmentDecision ====================
        public static QualityAssessmentDecision ToEntity(this CreateQualityAssessmentDecisionRequest dto, Guid reviewerId, Guid paperId)
        {
            return new QualityAssessmentDecision
            {
                QualityAssessmentProcessId = dto.QualityAssessmentProcessId,
                ReviewerId = reviewerId,
                PaperId = paperId,
                Score = dto.Score,
                // Notes = dto.Notes
            };
        }

        public static QualityAssessmentDecisionItem ToEntity(this CreateQualityAssessmentDecisionItemRequest dto)
        {
            return new QualityAssessmentDecisionItem
            {
                QualityCriterionId = dto.QualityCriterionId,
                Value = dto.Value,
                Comment = dto.Comment,
                PdfHighlightCoordinates = dto.PdfHighlightCoordinates
            };
        }

        public static void UpdateEntity(this UpdateQualityAssessmentDecisionItemRequest dto, QualityAssessmentDecisionItem entity)
        {
            entity.Id = dto.Id ?? entity.Id; // Preserve existing ID if not provided
            entity.Value = dto.Value;
            entity.Comment = dto.Comment;
            entity.PdfHighlightCoordinates = dto.PdfHighlightCoordinates;
        }

        public static QualityAssessmentDecisionItem ToEntity(this UpdateQualityAssessmentDecisionItemRequest dto)
        {
            return new QualityAssessmentDecisionItem
            {
                QualityCriterionId = dto.QualityCriterionId ?? Guid.Empty,
                Value = dto.Value,
                Comment = dto.Comment,
                PdfHighlightCoordinates = dto.PdfHighlightCoordinates
            };
        }

        public static void UpdateEntity(this UpdateQualityAssessmentDecisionRequest dto, QualityAssessmentDecision entity)
        {
            entity.Id = dto.Id ?? entity.Id; // Preserve existing ID if not provided
            entity.Score = dto.Score;
        }

        public static QualityAssessmentDecisionItemResponse ToDto(this QualityAssessmentDecisionItem entity)
        {
            if (entity == null) return null!;

            return new QualityAssessmentDecisionItemResponse
            {
                Id = entity.Id,
                QualityCriterionId = entity.QualityCriterionId,
                CriterionQuestion = entity.QualityCriterion?.Question,
                Value = entity.Value,
                Comment = entity.Comment,
                PdfHighlightCoordinates = entity.PdfHighlightCoordinates,
            };
        }

        public static QualityAssessmentDecisionResponse ToDto(this QualityAssessmentDecision entity)
        {
            if (entity == null) return null!;

            return new QualityAssessmentDecisionResponse
            {
                Id = entity.Id,
                ReviewerId = entity.ReviewerId,
                ReviewerName = entity.Reviewer?.FullName ?? entity.Reviewer?.Username,
                PaperId = entity.PaperId,
                Score = entity.Score,
                DecisionItems = entity.DecisionItems?.Select(i => i.ToDto()).ToList() ?? new List<QualityAssessmentDecisionItemResponse>()
            };
        }

        // ==================== QualityAssessmentResolution ====================
        public static QualityAssessmentResolution ToEntity(this CreateQualityAssessmentResolutionRequest dto, Guid resolvedBy)
        {
            return new QualityAssessmentResolution
            {
                QualityAssessmentProcessId = dto.QualityAssessmentProcessId,
                PaperId = dto.PaperId,
                ResolvedBy = resolvedBy,
                FinalDecision = dto.FinalDecision,
                FinalScore = dto.FinalScore,
                ResolutionNotes = dto.ResolutionNotes,
                ResolvedAt = DateTimeOffset.UtcNow
            };
        }

        public static void UpdateEntity(this UpdateQualityAssessmentResolutionRequest dto, QualityAssessmentResolution entity)
        {
            entity.Id = dto.Id ?? entity.Id; // Preserve existing ID if not provided
            entity.FinalDecision = dto.FinalDecision;
            entity.FinalScore = dto.FinalScore;
            entity.ResolutionNotes = dto.ResolutionNotes;
        }

        public static QualityAssessmentResolutionResponse ToResponse(this QualityAssessmentResolution entity, string? resolvedByName = null)
        {
            if (entity == null) return null!;

            return new QualityAssessmentResolutionResponse
            {
                Id = entity.Id,
                QualityAssessmentProcessId = entity.QualityAssessmentProcessId,
                PaperId = entity.PaperId,
                FinalDecision = entity.FinalDecision,
                FinalScore = entity.FinalScore,
                ResolutionNotes = entity.ResolutionNotes,
                ResolvedBy = entity.ResolvedBy,
                ResolvedByName = resolvedByName ?? entity.ResolvedBy.ToString(),
                ResolvedAt = entity.ResolvedAt
            };
        }

        // ==================== User Mapping ====================
        public static QualityAssessmentReviewerResponse ToQualityAssessmentReviewerResponse(this User entity)
        {
            if (entity == null) return null!;

            return new QualityAssessmentReviewerResponse
            {
                Id = entity.Id,
                Username = entity.Username,
                FullName = entity.FullName
            };
        }

        // ==================== Paper Mapping ====================
        public static QALeaderDashboardPaperResponse ToLeaderDashboardPaperResponse(
            this IncludedPaperResponse qaPaper,
            double percentage,
            QualityAssessmentResolution? resolution,
            List<User>? reviewers = null,
            List<QualityAssessmentDecision>? decisions = null,
            string? resolvedByName = null)
        {
            if (qaPaper == null) return null!;

            var response = new QALeaderDashboardPaperResponse
            {
                PaperId = qaPaper.PaperId,
                Title = qaPaper.Title,
                DOI = qaPaper.DOI,
                Authors = qaPaper.Authors,
                Abstract = qaPaper.Abstract,
                PublicationYear = qaPaper.PublicationYear,
                PublicationType = qaPaper.PublicationType,
                Journal = qaPaper.Journal,
                Source = qaPaper.Source,
                Keywords = qaPaper.Keywords,
                Url = qaPaper.Url,
                PdfUrl = qaPaper.PdfUrl,
                CreatedAt = qaPaper.CreatedAt,
                CompletionPercentage = Math.Round(percentage, 2),
                Status = resolution != null ? "resolved" : (percentage > 0 ? "in-progress" : "not-started"),
                Reviewers = reviewers?.Select(u => u.ToQualityAssessmentReviewerResponse()).ToList() ?? new List<QualityAssessmentReviewerResponse>(),
                Decisions = decisions?.Select(d => d.ToDto()).ToList() ?? new List<QualityAssessmentDecisionResponse>()
            };

            if (resolution != null)
            {
                response.Resolution = resolution.ToResponse(resolvedByName);
            }

            return response;
        }

        public static QAMemberDashboardPaperResponse ToMemberDashboardPaperResponse(
            this IncludedPaperResponse qaPaper,
            double percentage,
            QualityAssessmentResolution? resolution,
            QualityAssessmentDecision? userDecision,
            string? resolvedByName = null)
        {
            if (qaPaper == null) return null!;

            var response = new QAMemberDashboardPaperResponse
            {
                PaperId = qaPaper.PaperId,
                Title = qaPaper.Title,
                DOI = qaPaper.DOI,
                Authors = qaPaper.Authors,
                Abstract = qaPaper.Abstract,
                PublicationYear = qaPaper.PublicationYear,
                PublicationType = qaPaper.PublicationType,
                Journal = qaPaper.Journal,
                Source = qaPaper.Source,
                Keywords = qaPaper.Keywords,
                Url = qaPaper.Url,
                PdfUrl = qaPaper.PdfUrl,
                CreatedAt = qaPaper.CreatedAt,
                CompletionPercentage = Math.Round(percentage, 2),
                Status = resolution != null ? "resolved" : (percentage >= 100 ? "completed" : (percentage > 0 ? "in-progress" : "not-started")),
                Decisions = userDecision != null ? new List<QualityAssessmentDecisionResponse> { userDecision.ToDto() } : new List<QualityAssessmentDecisionResponse>()
            };


            if (resolution != null)
            {
                response.Resolution = resolution.ToResponse(resolvedByName);
            }

            return response;
        }

        // ==================== QualityAssessmentAssignment ====================
        public static QualityAssessmentAssignment ToEntity(this CreateQualityAssessmentAssignmentRequest dto, Guid userId)
        {
            return new QualityAssessmentAssignment
            {
                QualityAssessmentProcessId = dto.QualityAssessmentProcessId,
                UserId = userId,
                AssignedAt = DateTimeOffset.UtcNow
            };
        }
    }
}