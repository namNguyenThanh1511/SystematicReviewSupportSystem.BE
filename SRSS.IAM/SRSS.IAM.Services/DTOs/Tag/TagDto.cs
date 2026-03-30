using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Services.DTOs.Tag
{
    // ============================================
    // PAPER TAG DTOs
    // ============================================

    public class AddPaperTagRequest
    {
        public ProcessPhase Phase { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    public class PaperTagResponse
    {
        public Guid Id { get; set; }
        public Guid PaperId { get; set; }
        public Guid UserId { get; set; }
        public ProcessPhase Phase { get; set; }
        public string PhaseText { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
    }

    // ============================================
    // USER TAG INVENTORY DTOs
    // ============================================

    public class AddUserTagRequest
    {
        public ProcessPhase Phase { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    public class UserTagInventoryResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public ProcessPhase Phase { get; set; }
        public string PhaseText { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public int UsageCount { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
