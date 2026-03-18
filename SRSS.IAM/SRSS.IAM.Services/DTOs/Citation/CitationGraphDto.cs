using System.Collections.Generic;

namespace SRSS.IAM.Services.DTOs.Citation
{
    public class CitationGraphDto
    {
        public List<PaperNodeDto> Nodes { get; set; } = new List<PaperNodeDto>();
        public List<CitationEdgeDto> Edges { get; set; } = new List<CitationEdgeDto>();
    }
}