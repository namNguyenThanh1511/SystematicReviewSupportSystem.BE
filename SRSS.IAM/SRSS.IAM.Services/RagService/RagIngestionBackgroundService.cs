using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SRSS.IAM.Repositories;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.GrobidClient;

namespace SRSS.IAM.Services.RagService
{
    public class RagIngestionBackgroundService : BackgroundService
    {
        private readonly IRagIngestionQueue _queue;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RagIngestionBackgroundService> _logger;

        public RagIngestionBackgroundService(
            IRagIngestionQueue queue,
            IServiceProvider serviceProvider,
            ILogger<RagIngestionBackgroundService> logger)
        {
            _queue = queue;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RAG Ingestion Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                RagIngestionWorkItem workItem = default!;
                try
                {
                    workItem = await _queue.DequeueAsync(stoppingToken);

                    // Process each paper in its own DI scope
                    using var scope = _serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var grobidService = scope.ServiceProvider.GetRequiredService<IGrobidService>();
                    var embeddingService = scope.ServiceProvider.GetRequiredService<ILocalEmbeddingService>();
                    var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

                    await ProcessPaperAsync(workItem, dbContext, grobidService, embeddingService, httpClientFactory, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Prevent throwing if stopping token was signaled
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred processing paper {PaperId} in RAG Ingestion pipeline.", workItem?.PaperId);
                }
            }

            _logger.LogInformation("RAG Ingestion Background Service is stopping.");
        }

        private async Task ProcessPaperAsync(
            RagIngestionWorkItem workItem,
            AppDbContext dbContext,
            IGrobidService grobidService,
            ILocalEmbeddingService embeddingService,
            IHttpClientFactory httpClientFactory,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting RAG ingestion for PaperId {PaperId}", workItem.PaperId);

            // 1. Download PDF stream
            using var httpClient = httpClientFactory.CreateClient();
            using var response = await httpClient.GetAsync(workItem.PdfUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var pdfStream = await response.Content.ReadAsStreamAsync(cancellationToken);

            // 2. Fetch the pre-parsed TEI XML stored by Grobid from the database.
            //    .Select() projects only the scalar string — no .Include() needed.
            var teiXml = await dbContext.PaperFullTexts
                .Where(x => x.PaperPdf.PaperId == workItem.PaperId)
                .Select(x => x.RawXml)
                .FirstOrDefaultAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(teiXml))
            {
                _logger.LogWarning("PaperFullText not found for PaperId {PaperId}", workItem.PaperId);
                return;
            }

            // 3. Parse XML into PaperChunks
            var chunks = ParseTeiXmlToChunks(teiXml, workItem.PaperId);

            if (chunks.Count == 0)
            {
                _logger.LogWarning("No extractable chunks found for PaperId {PaperId}", workItem.PaperId);
                return;
            }

            // 4. Deduplication guard: remove any existing chunks for this paper so re-ingestion
            //    (e.g., after a model upgrade) does not create duplicate vectors.
            var existingChunks = await dbContext.PaperChunks
                .Where(c => c.PaperId == workItem.PaperId)
                .ToListAsync(cancellationToken);

            if (existingChunks.Count > 0)
            {
                _logger.LogInformation(
                    "Removing {Count} stale chunks before re-ingesting PaperId {PaperId}",
                    existingChunks.Count, workItem.PaperId);
                dbContext.PaperChunks.RemoveRange(existingChunks);
            }

            // 5. Generate embeddings and stamp model provenance on every chunk.
            var textContexts = chunks.Select(c => c.TextContent).ToList();
            var embeddings = embeddingService.GetEmbeddingsBatch(textContexts);

            for (int i = 0; i < chunks.Count; i++)
            {
                chunks[i].Embedding = embeddings[i];
                chunks[i].EmbeddingModel = embeddingService.ModelName;
                chunks[i].EmbeddingDimensions = embeddingService.Dimensions;
                chunks[i].EmbeddingProvider = embeddingService.Provider;
            }

            // 6. Save all changes (deletions + insertions) in a single transaction.
            await dbContext.PaperChunks.AddRangeAsync(chunks, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully ingested {ChunkCount} RAG chunks for PaperId {PaperId} using model '{Model}'",
                chunks.Count, workItem.PaperId, embeddingService.ModelName);
        }

        /// <summary>
        /// Parses the TEI XML output from Grobid and extracts text and coordinate information.
        /// Extracts from both &lt;head&gt; and &lt;p&gt; tags under the &lt;body&gt; node.
        /// </summary>
        private List<PaperChunk> ParseTeiXmlToChunks(string teiXml, Guid paperId)
        {
            var chunks = new List<PaperChunk>();
            XDocument doc;

            try
            {
                doc = XDocument.Parse(teiXml);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse TEI XML for PaperId {PaperId}", paperId);
                return chunks;
            }

            var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;
            var bodyElement = doc.Descendants(ns + "body").FirstOrDefault();

            if (bodyElement == null)
                return chunks;

            var timestamp = DateTimeOffset.UtcNow;

            // Iterate over interesting nodes: headings and paragraphs
            foreach (var element in bodyElement.Descendants().Where(e => e.Name.LocalName == "p" || e.Name.LocalName == "head"))
            {
                var text = element.Value?.Trim();

                // Skip extremely short chunks (likely noise or single characters)
                if (string.IsNullOrWhiteSpace(text) || text.Length < 30)
                    continue;

                // Extract coordinates attribute (format: page,x,y,w,h;page,x,y,w,h...)
                var coordsAttr = element.Attribute("coords")?.Value;
                string? coordsJson = ParseCoordinatesToJson(coordsAttr);

                chunks.Add(new PaperChunk
                {
                    Id = Guid.NewGuid(),
                    PaperId = paperId,
                    TextContent = text,
                    CoordinatesJson = coordsJson,
                    CreatedAt = timestamp
                });
            }

            return chunks;
        }

        /// <summary>
        /// Converts Grobid's 'coords' string into a standardized JSON array representation.
        /// Example input: "1,57.99,61.05,496.07,21.52;2,57.99,61.05,496.07,21.52"
        /// Example output: [{"page": 1, "x": 57.99, "y": 61.05, "w": 496.07, "h": 21.52}, ...]
        /// </summary>
        private string? ParseCoordinatesToJson(string? coordsString)
        {
            if (string.IsNullOrWhiteSpace(coordsString)) return null;

            var result = new List<object>();
            var segments = coordsString.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (var segment in segments)
            {
                var parts = segment.Split(',');
                if (parts.Length == 5 &&
                    int.TryParse(parts[0], out int page) &&
                    double.TryParse(parts[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double x) &&
                    double.TryParse(parts[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double y) &&
                    double.TryParse(parts[3], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double w) &&
                    double.TryParse(parts[4], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double h))
                {
                    result.Add(new { page, x, y, w, h });
                }
            }
            return result.Count == 0 ? null : JsonSerializer.Serialize(result);
        }
    }
}
