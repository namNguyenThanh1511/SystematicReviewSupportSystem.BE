using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;
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
    }

    public class GetCandidatePapersRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SearchTerm { get; set; }
        public CandidateStatus? Status { get; set; }
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
