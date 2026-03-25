using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SRSS.IAM.Services.GrobidClient.DTOs;

namespace SRSS.IAM.Services.GrobidClient
{
    public interface IGrobidService
    {
        Task<GrobidHeaderExtractionDto> ExtractHeaderAsync(Stream pdfStream, string fileName, CancellationToken cancellationToken = default);
        Task<List<GrobidReferenceDto>> ExtractReferencesAsync(Stream pdfStream, string fileName, CancellationToken cancellationToken = default);
        Task<string> ProcessFulltextDocumentAsync(Stream pdfStream, CancellationToken cancellationToken = default);
    }
}
