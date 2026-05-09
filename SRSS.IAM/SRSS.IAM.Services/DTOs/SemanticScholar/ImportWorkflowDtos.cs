using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace SRSS.IAM.Services.DTOs.SemanticScholar
{
    public class StartImportSessionRequest
    {
        public Guid ProjectId { get; set; }
        public Guid? SearchSourceId { get; set; }
    }

    public class ImportSessionResponse
    {
        public Guid SessionId { get; set; }
        public string Status { get; set; }
    }

    public class ImportRisWorkflowRequest
    {
        public Guid SessionId { get; set; }
    }

    public class PaperImportMappingDto
    {
        public Guid InternalId { get; set; }
        public string SourceId { get; set; }
    }

    public class ImportRisWorkflowResponse
    {
        public Guid SessionId { get; set; }
        public List<Guid> ValidPaperIds { get; set; } = new();
        public List<Guid> DuplicatePaperIds { get; set; } = new();
        public List<PaperImportMappingDto> PaperMappings { get; set; } = new();
    }

    public class ImportSessionStatusResponse
    {
        public List<Guid> ValidPaperIds { get; set; } = new();
        public List<Guid> DuplicatePaperIds { get; set; } = new();
        public List<PaperImportMappingDto> PaperMappings { get; set; } = new();
        public string State { get; set; }
    }

    public class UploadRisWorkflowRequest
    {
        public IFormFile File { get; set; }
        public Guid SessionId { get; set; }
        public Guid? SearchSourceId { get; set; }
    }


    public class ImportSessionRedisModel
    {
        public string UserId { get; set; }
        public Guid ProjectId { get; set; }
        public List<Guid> ValidPaperIds { get; set; } = new();
        public List<Guid> DuplicatePaperIds { get; set; } = new();
        public List<PaperImportMappingDto> PaperMappings { get; set; } = new();
        public string State { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
