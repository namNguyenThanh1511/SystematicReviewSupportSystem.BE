namespace SRSS.IAM.Services.GrobidClient;

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public interface IPdfProcessingService
{
    Task<string> ProcessPdfFulltextAsync(string filePath);
}

public class PdfProcessingService : IPdfProcessingService
{
    private readonly IGrobidClient _grobidClient;
    private readonly ILogger<PdfProcessingService> _logger;

    public PdfProcessingService(IGrobidClient grobidClient, ILogger<PdfProcessingService> logger)
    {
        _grobidClient = grobidClient ?? throw new ArgumentNullException(nameof(grobidClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> ProcessPdfFulltextAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            throw new ArgumentException("Invalid file path.", nameof(filePath));
        }

        try
        {
            await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            
            _logger.LogInformation("Processing PDF fulltext via GROBID: {FilePath}", filePath);
            
            // Example configuration: consolidating header, citations, and funders
            var teiXml = await _grobidClient.ProcessFulltextDocumentAsync(
                pdfStream: fileStream,
                consolidateHeader: 1,
                consolidateCitations: 1,
                consolidateFunders: 1,
                segmentSentences: true);
                
            _logger.LogInformation("Successfully converted PDF to TEI format.");
            return teiXml;
        }
        catch (GrobidInvalidPdfException ex)
        {
            _logger.LogError(ex, "The provided PDF is invalid or cannot be processed.");
            throw;
        }
        catch (GrobidTimeoutException ex)
        {
            _logger.LogError(ex, "GROBID processing timed out.");
            throw;
        }
        catch (GrobidException ex)
        {
            _logger.LogError(ex, "An error occurred during GROBID processing.");
            throw;
        }
    }
}