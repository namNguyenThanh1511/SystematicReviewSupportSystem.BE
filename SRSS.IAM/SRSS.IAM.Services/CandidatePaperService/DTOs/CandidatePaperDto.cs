using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Services.DTOs.Common;
using System;

namespace SRSS.IAM.Services.CandidatePaperService.DTOs
{
    public class CandidatePaperDto
    {
        public Guid CandidateId { get; set; }
        public Guid ReviewProcessId { get; set; }
        public Guid OriginPaperId { get; set; }
        public string? OriginPaperTitle { get; set; }
        public string? OriginPaperAuthors { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Authors { get; set; }
        public string? PublicationYear { get; set; }
        public string? DOI { get; set; }
        public string? RawReference { get; set; }
        public string? NormalizedReference { get; set; }
        public CandidateStatus Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public decimal ConfidenceScore { get; set; }
        public decimal ExtractionQualityScore { get; set; }
        public decimal MatchConfidenceScore { get; set; }
        public bool IsSelectedInScreening { get; set; }
        public string? ValidationNote { get; set; }
    }

    public class GetCandidatePapersRequest : PaginationRequest
    {
        public string? SearchTerm { get; set; }
        public CandidateStatus? Status { get; set; }
        public string? Year { get; set; }
    }

    public class GetPapersRequest : PaginationRequest
    {
        public string? SearchTerm { get; set; }
        public string? Year { get; set; }
    }

    public class SelectCandidatePaperRequest
    {
        public List<Guid> CandidateIds { get; set; } = new List<Guid>();
    }

    public class RejectCandidatePaperRequest
    {
        public List<Guid> CandidateIds { get; set; } = new List<Guid>();
    }
}
