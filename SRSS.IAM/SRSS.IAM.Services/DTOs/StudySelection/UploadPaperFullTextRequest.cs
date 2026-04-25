using Microsoft.AspNetCore.Http;

namespace SRSS.IAM.Services.DTOs.StudySelection
{
    public class UploadPaperFullTextRequest
    {
        public IFormFile File { get; set; }
        public Guid ProjectId { get; set; }
        public Guid PaperId { get; set; }
        public bool ExtractWithGrobid { get; set; } = false;
    }
}
