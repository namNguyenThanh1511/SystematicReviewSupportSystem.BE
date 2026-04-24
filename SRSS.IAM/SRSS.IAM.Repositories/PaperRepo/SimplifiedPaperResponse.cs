using System;

namespace SRSS.IAM.Repositories.PaperRepo
{
    public class SimplifiedPaperResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Author { get; set; }
        public string? Year { get; set; }
        public string? Source { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsAssigned { get; set; }
    }
}
