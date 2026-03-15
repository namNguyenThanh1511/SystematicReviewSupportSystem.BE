using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SRSS.IAM.Services.GrobidClient.DTOs;

namespace SRSS.IAM.Services.GrobidClient
{
    public interface IGrobidService
    {
        Task<GrobidHeaderExtractionDto> ExtractHeaderAsync(Stream pdfStream, string fileName, CancellationToken cancellationToken = default);
    }
}
