using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SRSS.IAM.Services.GrobidClient.DTOs;

namespace SRSS.IAM.Services.GrobidClient
{
    public class GrobidService : IGrobidService
    {
        private readonly IGrobidClient _grobidClient;
        private readonly ILogger<GrobidService> _logger;

        public GrobidService(IGrobidClient grobidClient, ILogger<GrobidService> logger)
        {
            _grobidClient = grobidClient;
            _logger = logger;
        }

        public async Task<GrobidHeaderExtractionDto> ExtractHeaderAsync(Stream pdfStream, string fileName, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting GROBID header extraction for file {FileName}", fileName);
            string teiXml = string.Empty;

            try
            {
                // Note: The retry logic (for status 503, max 3 retries, wait 2 sec) 
                // is already implemented inside GrobidClient's SendRequestWithRetryAsync method.
                teiXml = await _grobidClient.ProcessHeaderDocumentAsync(
                    pdfStream: pdfStream,
                    consolidateHeader: 1, // requested by instructions
                    includeRawAffiliations: true, // requested by instructions
                    includeRawCopyrights: false,
                    startPage: 1,
                    endPage: 5 // expanded end page slightly for header extraction
                );
                _logger.LogInformation("TEI XML from GROBID for {FileName}: {TeiXml}", fileName, teiXml);

                _logger.LogInformation("Successfully received TEI XML from GROBID for {FileName}", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract header from GROBID for file {FileName}", fileName);
                // Return an empty/error DTO if extraction completely fails
                return new GrobidHeaderExtractionDto();
            }

            var dto = GrobidTeiParser.Parse(teiXml);
            _logger.LogInformation("Successfully parsed TEI XML for {FileName}", fileName);

            return dto;
        }

        public async Task<List<GrobidReferenceDto>> ExtractReferencesAsync(Stream pdfStream, string fileName, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting GROBID reference extraction for file {FileName}", fileName);
            string teiXml = string.Empty;

            try
            {
                teiXml = await _grobidClient.ProcessReferencesAsync(
                    pdfStream: pdfStream,
                    consolidateCitations: 1,
                    includeRawCitations: true,
                    returnBibtex: false
                );

                _logger.LogInformation("Successfully received TEI XML references from GROBID for {FileName}", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract references from GROBID for file {FileName}", fileName);
                return new List<GrobidReferenceDto>();
            }

            var dtos = GrobidTeiParser.ParseReferences(teiXml);
            _logger.LogInformation("Successfully parsed {Count} references from TEI XML for {FileName}", dtos.Count, fileName);

            return dtos;
        }

        public async Task<string> ProcessFulltextDocumentAsync(Stream pdfStream, CancellationToken cancellationToken = default, IEnumerable<string>? teiCoordinates = null)
        {
            _logger.LogInformation("Starting GROBID fulltext extraction");
            try
            {
                var teiXml = await _grobidClient.ProcessFulltextDocumentAsync(
                    pdfStream: pdfStream,
                    consolidateHeader: 0,
                    consolidateCitations: 0,
                    consolidateFunders: 0,
                    segmentSentences: false,
                    teiCoordinates: teiCoordinates
                );

                _logger.LogInformation("Successfully received fulltext TEI XML from GROBID");
                return teiXml;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract fulltext from GROBID");
                return string.Empty;
            }
        }
    }
}
