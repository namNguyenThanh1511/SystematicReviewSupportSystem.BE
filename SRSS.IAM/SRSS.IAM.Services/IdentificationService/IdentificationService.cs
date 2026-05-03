using Shared.Exceptions;
using Pgvector;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.Identification;
using SRSS.IAM.Services.PaperEnrichmentService;
using SRSS.IAM.Services.ReferenceMatchingService;
using SRSS.IAM.Services.Utils;
using SRSS.IAM.Services.RagService;
using SRSS.IAM.Services.ReferenceMatchingService.DTOs;
using SRSS.IAM.Services.DTOs.Paper;
using Microsoft.EntityFrameworkCore;
using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Services.DTOs.PrismaReport;
using SRSS.IAM.Services.UserService;
using SRSS.IAM.Services.DTOs.Crossref;
using SRSS.IAM.Services.Parsers;
using Microsoft.Extensions.Logging;

namespace SRSS.IAM.Services.IdentificationService
{
    public class IdentificationService : IIdentificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IReferenceMatchingService _matchingService;
        private readonly ILocalEmbeddingService _embeddingService;
        private readonly IPaperEnrichmentOrchestrator _enrichmentOrchestrator;
        private readonly ICurrentUserService _currentUserService;
        private readonly IRisParser _risParser;
        private readonly IDoiParser _doiParser;
        private readonly IApiParser<CrossrefQueryParameters> _apiParser;
        private readonly ILogger<IdentificationService> _logger;

        public IdentificationService(
            IUnitOfWork unitOfWork,
            IReferenceMatchingService matchingService,
            ILocalEmbeddingService embeddingService,
            IPaperEnrichmentOrchestrator enrichmentOrchestrator,
            ICurrentUserService currentUserService,
            IRisParser risParser,
            IDoiParser doiParser,
            IApiParser<CrossrefQueryParameters> apiParser,
            ILogger<IdentificationService> logger)
        {
            _unitOfWork = unitOfWork;
            _matchingService = matchingService;
            _embeddingService = embeddingService;
            _enrichmentOrchestrator = enrichmentOrchestrator;
            _currentUserService = currentUserService;
            _risParser = risParser;
            _doiParser = doiParser;
            _apiParser = apiParser;
            _logger = logger;
        }

        public async Task<IdentificationProcessResponse> CreateIdentificationProcessAsync(
            CreateIdentificationProcessRequest request,
            CancellationToken cancellationToken = default)
        {
            var reviewProcess = await _unitOfWork.ReviewProcesses.FindSingleAsync(
                rp => rp.Id == request.ReviewProcessId,
                isTracking: true,
                cancellationToken);

            if (reviewProcess == null)
            {
                throw new InvalidOperationException($"ReviewProcess with ID {request.ReviewProcessId} not found.");
            }

            reviewProcess.EnsureCanCreateIdentificationProcess();

            var identificationProcess = new IdentificationProcess
            {
                Id = Guid.NewGuid(),
                ReviewProcessId = request.ReviewProcessId,
                Notes = request.Notes,
                Status = IdentificationStatus.NotStarted,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.IdentificationProcesses.AddAsync(identificationProcess, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new IdentificationProcessResponse
            {
                Id = identificationProcess.Id,
                ReviewProcessId = identificationProcess.ReviewProcessId,
                Notes = identificationProcess.Notes,
                StartedAt = identificationProcess.StartedAt,
                CompletedAt = identificationProcess.CompletedAt,
                Status = identificationProcess.Status,
                CreatedAt = identificationProcess.CreatedAt,
                ModifiedAt = identificationProcess.ModifiedAt
            };
        }

        public async Task<IdentificationProcessResponse> GetIdentificationProcessByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var identificationProcess = await _unitOfWork.IdentificationProcesses.FindSingleAsync(
                ip => ip.Id == id,
                cancellationToken: cancellationToken);

            if (identificationProcess == null)
            {
                throw new InvalidOperationException($"IdentificationProcess with ID {id} not found.");
            }

            return MapToIdentificationProcessResponse(identificationProcess);
        }

        public async Task<IdentificationProcessResponse> StartIdentificationProcessAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var identificationProcess = await _unitOfWork.IdentificationProcesses.FindSingleAsync(
                ip => ip.Id == id,
                isTracking: true,
                cancellationToken);

            if (identificationProcess == null)
            {
                throw new InvalidOperationException($"IdentificationProcess with ID {id} not found.");
            }

            var reviewProcess = await _unitOfWork.ReviewProcesses.FindSingleAsync(
                rp => rp.Id == identificationProcess.ReviewProcessId,
                cancellationToken: cancellationToken);
            if (reviewProcess == null)
            {
                throw new InvalidOperationException($"Associated ReviewProcess with ID {identificationProcess.ReviewProcessId} not found.");
            }
            reviewProcess.CurrentPhase = ProcessPhase.Identification;
            identificationProcess.Start();

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToIdentificationProcessResponse(identificationProcess);
        }

        public async Task<IdentificationProcessResponse> CompleteIdentificationProcessAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var identificationProcess = await _unitOfWork.IdentificationProcesses.FindSingleAsync(
                ip => ip.Id == id,
                isTracking: true,
                cancellationToken);

            if (identificationProcess == null)
            {
                throw new InvalidOperationException($"IdentificationProcess with ID {id} not found.");
            }

            await EnsureIdentificationProcessCanBeEditedAsync(identificationProcess, cancellationToken);




            identificationProcess.Complete();

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Trigger downstream enrichment for final dataset papers (fire-and-forget via Channel)
            await _enrichmentOrchestrator.TriggerForIdentificationProcessAsync(id, cancellationToken);

            return MapToIdentificationProcessResponse(identificationProcess);
        }

        public async Task<IdentificationProcessResponse> ReopenIdentificationProcessAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var identificationProcess = await _unitOfWork.IdentificationProcesses.FindSingleAsync(
                ip => ip.Id == id,
                isTracking: true,
                cancellationToken);

            if (identificationProcess == null)
            {
                throw new InvalidOperationException($"IdentificationProcess with ID {id} not found.");
            }

            identificationProcess.Reopen();

            var reviewProcess = await _unitOfWork.ReviewProcesses.FindSingleAsync(
                rp => rp.Id == identificationProcess.ReviewProcessId,
                isTracking: true,
                cancellationToken);

            if (reviewProcess == null)
            {
                throw new InvalidOperationException($"Associated ReviewProcess with ID {identificationProcess.ReviewProcessId} not found.");
            }

            reviewProcess.CurrentPhase = ProcessPhase.Identification;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToIdentificationProcessResponse(identificationProcess);
        }


        public async Task<PrismaStatisticsResponse> GetPrismaStatisticsAsync(
            Guid ReviewProcessId,
            CancellationToken cancellationToken = default)
        {
            var reviewProcess = await _unitOfWork.ReviewProcesses.FindSingleAsync(
                rp => rp.Id == ReviewProcessId,
                cancellationToken: cancellationToken);
            if (reviewProcess == null)
            {
                throw new InvalidOperationException($"ReviewProcess for Project ID {ReviewProcessId} not found.");
            }
            var projectId = reviewProcess.ProjectId;
            var identificationProcess = await _unitOfWork.IdentificationProcesses.FindSingleAsync(
                ip => ip.ReviewProcessId == reviewProcess.Id,
                cancellationToken: cancellationToken);

            if (identificationProcess == null)
            {
                throw new InvalidOperationException($"IdentificationProcess not found.");
            }

            var importBatchList = await _unitOfWork.ImportBatches.GetByProjectIdsAsync(projectId, cancellationToken);
            var totalRecordsImported = importBatchList.Sum(ib => ib.TotalRecords);

            // Query actual unique paper count from the frozen snapshot
            var uniquePaperIds = await _unitOfWork.IdentificationProcessPapers.GetIncludedPaperIdsByProcessAsync(identificationProcess.Id, cancellationToken);
            var uniqueRecordsCount = uniquePaperIds.Count;

            // Get total deduplication result count for this process
            var duplicateRecords = await _unitOfWork.DeduplicationResults.CountDuplicatesByProjectAsync(projectId, cancellationToken);

            var searchSourceIds = await _unitOfWork.SearchSources.GetByProjectIdAsync(projectId, cancellationToken);

            var paperToSearchSource = await _unitOfWork.Papers.GetQueryable()
                .Where(p => p.ProjectId == projectId)
                .Select(p => new { p.Id, p.SearchSourceId })
                .ToListAsync(cancellationToken);

            // Breakdown by Database Source
            var identifiedBreakdown =
                paperToSearchSource
                .GroupBy(ip => ip.SearchSourceId)
                .Select(g =>
                {
                    var sourceName = searchSourceIds.FirstOrDefault(s => s.Id == g.Key)?.Name ?? "Others";
                    return new PrismaBreakdownResponse { Label = sourceName, Count = g.Count() };
                })
                .ToList();

            // 2. Identification: Snowballing
            var snowballingPapers = await _unitOfWork.IdentificationProcessPapers.FindAllAsync(
                ipp => ipp.IdentificationProcessId == identificationProcess.Id && ipp.SourceType == PaperSourceType.Snowballing,
                isTracking: false,
                cancellationToken: cancellationToken);

            var totalFromSnowballing = snowballingPapers.Count();
            if (totalFromSnowballing > 0)
            {
                identifiedBreakdown.Add(new PrismaBreakdownResponse { Label = "Snowballing", Count = totalFromSnowballing });
            }

            // 3. Pending Selection (Ready but NOT in snapshot)
            // var hasPendingDedup = await _unitOfWork.DeduplicationResults.AnyAsync(
            //     dr => dr.ProjectId == reviewProcess.ProjectId && dr.ReviewStatus == DeduplicationReviewStatus.Pending,
            //     cancellationToken: cancellationToken);

            var deduplicationQuery = _unitOfWork.DeduplicationResults.GetQueryable();
            var snapshotQuery = _unitOfWork.IdentificationProcessPapers.GetQueryable()
                .Where(ipp => ipp.IdentificationProcessId == identificationProcess.Id);
            var pendingSelectionCount = await _unitOfWork.Papers.GetQueryable()
                .Where(p => p.ProjectId == reviewProcess.ProjectId)
                // Not a duplicate (CANCEL) and not pending review
                .Where(p => !deduplicationQuery.Any(d => d.PaperId == p.Id &&
                                                        d.ProjectId == reviewProcess.ProjectId &&
                                                        (d.ResolvedDecision == DuplicateResolutionDecision.CANCEL ||
                                                         d.ReviewStatus == DeduplicationReviewStatus.Pending)))
                // Not already in the snapshot
                .Where(p => !snapshotQuery.Any(i => i.IdentificationProcessId == identificationProcess.Id &&
                                                     i.PaperId == p.Id))
                .CountAsync(cancellationToken);

            return new PrismaStatisticsResponse
            {
                TotalRecordsImported = totalRecordsImported + totalFromSnowballing,
                DuplicateRecords = duplicateRecords,
                UniqueRecords = uniqueRecordsCount,
                PendingSelectionCount = pendingSelectionCount,
                ImportBatchCount = importBatchList.Count(),
                IdentifiedBreakdown = identifiedBreakdown
            };
        }

        public async Task<ImportBatchResponse> CreateImportBatchAsync(
            CreateImportBatchRequest request,
            CancellationToken cancellationToken = default)
        {
            var project = await _unitOfWork.SystematicReviewProjects.FindSingleAsync(
                p => p.Id == request.ProjectId,
                cancellationToken: cancellationToken);


            var importBatch = new ImportBatch
            {
                Id = Guid.NewGuid(),
                ProjectId = request.ProjectId,
                FileName = request.FileName,
                FileType = request.FileType,
                Source = request.Source,
                TotalRecords = request.TotalRecords,
                ImportedBy = request.ImportedBy,
                ImportedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.ImportBatches.AddAsync(importBatch, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToImportBatchResponse(importBatch);
        }

        public async Task<ImportBatchResponse> GetImportBatchByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var importBatch = await _unitOfWork.ImportBatches.FindSingleAsync(
                ib => ib.Id == id,
                cancellationToken: cancellationToken);

            if (importBatch == null)
            {
                throw new NotFoundException("ImportBatch not found.");
            }

            return MapToImportBatchResponse(importBatch);
        }



        public async Task<List<ImportBatchResponse>> GetImportBatchesByProjectIdAsync(
            Guid projectId,
            CancellationToken cancellationToken = default)
        {
            var importBatches = await _unitOfWork.ImportBatches.FindAllAsync(
                ib => ib.ProjectId == projectId,
                cancellationToken: cancellationToken);



            return importBatches.Select(MapToImportBatchResponse).ToList();
        }


        public async Task<List<DTOs.Paper.PaperResponse>> GetPapersByImportBatchIdAsync(
            Guid importBatchId,
            CancellationToken cancellationToken = default)
        {
            var importBatch = await _unitOfWork.ImportBatches.FindSingleAsync(
                ib => ib.Id == importBatchId,
                cancellationToken: cancellationToken);

            if (importBatch == null)
            {
                throw new NotFoundException($"ImportBatch with ID {importBatchId} not found.");
            }

            var papers = await _unitOfWork.Papers.FindAllAsync(
                p => p.ImportBatchId == importBatchId,
                cancellationToken: cancellationToken);

            return papers.Select(MapToPaperResponse).ToList();
        }

        private static DTOs.Paper.PaperResponse MapToPaperResponse(Paper paper)
        {
            return new DTOs.Paper.PaperResponse
            {
                Id = paper.Id,
                Title = paper.Title,
                Authors = paper.Authors,
                Abstract = paper.Abstract,
                DOI = paper.DOI,
                PublicationType = paper.PublicationType,
                PublicationYear = paper.PublicationYear,
                PublicationYearInt = paper.PublicationYearInt,
                PublicationDate = paper.PublicationDate,
                Volume = paper.Volume,
                Issue = paper.Issue,
                Pages = paper.Pages,
                Publisher = paper.Publisher,
                Language = paper.Language,
                Keywords = paper.Keywords,
                Url = paper.Url,
                ConferenceName = paper.ConferenceName,
                ConferenceLocation = paper.ConferenceLocation,
                ConferenceCountry = paper.ConferenceCountry,
                ConferenceYear = paper.ConferenceYear,
                Journal = paper.Journal,
                JournalIssn = paper.JournalIssn,
                Source = paper.Source,
                SearchSourceId = paper.SearchSourceId,
                ImportedAt = paper.ImportedAt,
                ImportedBy = paper.ImportedBy,
                PdfUrl = paper.PdfUrl,
                FullTextRetrievalStatus = paper.FullTextRetrievalStatus,
                FullTextRetrievalStatusText = paper.FullTextRetrievalStatus.ToString(),
                FullTextAvailable = paper.FullTextAvailable,
                AccessType = paper.AccessType,
                AccessTypeText = paper.AccessType?.ToString(),
                CreatedAt = paper.CreatedAt,
                ModifiedAt = paper.ModifiedAt,
                IsDuplicated = paper.IsDuplicated
            };
        }

        private static ImportBatchResponse MapToImportBatchResponse(ImportBatch importBatch)
        {
            return new ImportBatchResponse
            {
                Id = importBatch.Id,
                FileName = importBatch.FileName,
                FileType = importBatch.FileType,
                Source = importBatch.Source,
                TotalRecords = importBatch.TotalRecords,
                ImportedBy = importBatch.ImportedBy,
                ImportedAt = importBatch.ImportedAt,
                CreatedAt = importBatch.CreatedAt,
                ModifiedAt = importBatch.ModifiedAt
            };
        }

        private static IdentificationProcessResponse MapToIdentificationProcessResponse(IdentificationProcess identificationProcess)
        {
            return new IdentificationProcessResponse
            {
                Id = identificationProcess.Id,
                ReviewProcessId = identificationProcess.ReviewProcessId,
                Notes = identificationProcess.Notes,
                StartedAt = identificationProcess.StartedAt,
                CompletedAt = identificationProcess.CompletedAt,
                Status = identificationProcess.Status,
                StatusText = identificationProcess.Status.ToString(),
                CreatedAt = identificationProcess.CreatedAt,
                ModifiedAt = identificationProcess.ModifiedAt
            };
        }


        // ─── Import entry points ────────────────────────────────────────────────────

        public async Task<RisImportResultDto> ImportRisFileAsync(
            Stream fileStream,
            string fileName,
            Guid? searchSourceId,
            Guid projectId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting RIS file import. File: {FileName}, Project: {ProjectId}", fileName, projectId);

            List<RisPaperDto> papers;
            try
            {
                papers = _risParser.Parse(fileStream);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to parse RIS file '{fileName}': {ex.Message}", ex);
            }

            return await ImportAsync(
                papers,
                sourceName: null,      // resolved inside ImportAsync from searchSource.Name
                fileName: fileName,
                fileType: "RIS",
                searchSourceId: searchSourceId,
                projectId: projectId,
                cancellationToken: cancellationToken);
        }

        public async Task<RisImportResultDto> ImportFromDoiAsync(
            string doi,
            Guid? searchSourceId,
            Guid projectId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting DOI import. DOI: {Doi}, Project: {ProjectId}", doi, projectId);

            var papers = await _doiParser.ParseAsync(doi, cancellationToken);

            return await ImportAsync(
                papers,
                sourceName: "Crossref DOI",
                fileName: $"doi:{doi}",
                fileType: "DOI",
                searchSourceId: searchSourceId,
                projectId: projectId,
                cancellationToken: cancellationToken);
        }

        public async Task<RisImportResultDto> ImportFromApiAsync(
            CrossrefQueryParameters query,
            Guid? searchSourceId,
            Guid projectId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting Crossref API import. Query: {@Query}, Project: {ProjectId}", query, projectId);

            var papers = await _apiParser.ParseAsync(query, cancellationToken);

            return await ImportAsync(
                papers,
                sourceName: "Crossref API",
                fileName: $"crossref-query:{query.Query ?? query.QueryTitle ?? "custom"}",
                fileType: "API",
                searchSourceId: searchSourceId,
                projectId: projectId,
                cancellationToken: cancellationToken);
        }

        // ─── Shared import core ─────────────────────────────────────────────────────

        /// <summary>
        /// Creates an <see cref="ImportBatch"/>, runs <see cref="ProcessPapersAsync"/>,
        /// and wraps everything in a database transaction.
        /// All import entry points (RIS, DOI, API) converge here.
        /// </summary>
        private async Task<RisImportResultDto> ImportAsync(
            List<RisPaperDto> papers,
            string? sourceName,
            string fileName,
            string fileType,
            Guid? searchSourceId,
            Guid projectId,
            CancellationToken cancellationToken)
        {
            var result = new RisImportResultDto();

            // Resolve current user
            var currentUser = _currentUserService.GetCurrentUser();
            var user = await _unitOfWork.Users.FindSingleAsync(
                u => u.Id == Guid.Parse(currentUser.userId),
                cancellationToken: cancellationToken);

            if (user == null)
                throw new NotFoundException($"User with ID {currentUser.userId} not found.");

            var importedBy = user.Username;

            // Validate project
            var project = await _unitOfWork.SystematicReviewProjects.FindSingleAsync(
                p => p.Id == projectId,
                isTracking: false,
                cancellationToken);

            if (project == null)
                throw new InvalidOperationException($"Project with ID {projectId} not found.");

            // Optionally resolve search source
            SearchSource? searchSource = null;
            if (searchSourceId.HasValue)
            {
                searchSource = await _unitOfWork.SearchSources.FindSingleAsync(
                    s => s.Id == searchSourceId.Value,
                    cancellationToken: cancellationToken);

                if (searchSource == null)
                    throw new InvalidOperationException($"SearchSource with ID {searchSourceId.Value} not found.");
            }

            var resolvedSource = searchSource?.Name ?? sourceName ?? "Manual Upload";

            result.TotalRecords = papers.Count;

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var importBatch = new ImportBatch
                {
                    Id            = Guid.NewGuid(),
                    ProjectId     = projectId,
                    FileName      = fileName,
                    FileType      = fileType,
                    Source        = resolvedSource,
                    TotalRecords  = papers.Count,
                    ImportedBy    = importedBy,
                    ImportedAt    = DateTimeOffset.UtcNow,
                    CreatedAt     = DateTimeOffset.UtcNow,
                    ModifiedAt    = DateTimeOffset.UtcNow
                };

                await _unitOfWork.ImportBatches.AddAsync(importBatch, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                result.ImportBatchId = importBatch.Id;

                await ProcessPapersAsync(papers, projectId, importBatch, result, searchSource, cancellationToken);

                // Update batch totals after processing
                importBatch.TotalRecords = result.TotalRecords;
                importBatch.ModifiedAt   = DateTimeOffset.UtcNow;
                await _unitOfWork.ImportBatches.UpdateAsync(importBatch, cancellationToken);

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation(
                    "Import completed. Source: {Source}, Total: {Total}, Imported: {Imported}, Duplicates: {Dup}",
                    resolvedSource, result.TotalRecords, result.ImportedRecords, result.DuplicateRecords);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }

            return result;
        }

        private async Task ProcessPapersAsync(
            List<RisPaperDto> risPapers,
            Guid projectId,
            ImportBatch importBatch,
            RisImportResultDto result,
            SearchSource? searchSource,
            CancellationToken cancellationToken)
        {
            // STEP 1: Normalize data (DOI, Title)
            var references = risPapers.Select(p => new ExtractedReference
            {
                Title = p.Title,
                Authors = p.Authors,
                Abstract = p.Abstract,
                DOI = DoiHelper.Normalize(p.DOI) ?? p.DOI,
                PublishedYear = p.PublicationYear,
                Journal = p.Journal,
                RawReference = p.RawReference
            }).ToList();

            // STEP 2 & 3: Fast Match (DOI, Exact title) & Fuzzy match at project scope.
            var preliminaryMatches = (await _matchingService.MatchBatchInProjectAsync(
                references,
                projectId,
                cancellationToken)).ToList();

            // STEP 4: FILTER candidates needing embedding
            // Identify indices that need semantic matching
            var semanticMatchNeededIndices = new List<int>();
            for (int i = 0; i < references.Count; i++)
            {
                var match = preliminaryMatches[i];
                // Only embed if no high-confidence DOI/Exact match or if fuzzy match is weak
                if (match.ConfidenceScore < 0.95m && !string.IsNullOrWhiteSpace(references[i].Title))
                {
                    semanticMatchNeededIndices.Add(i);
                }
            }

            // STEP 5: BATCH EMBEDDING (only subset)
            if (semanticMatchNeededIndices.Any())
            {
                var inputsToEmbed = semanticMatchNeededIndices
                    .Select(idx => GenerateEmbeddingInput(
                        references[idx].Title,
                        references[idx].Abstract))
                    .ToList();

                var embeddings = _embeddingService.GetEmbeddingsBatch(inputsToEmbed);

                for (int i = 0; i < semanticMatchNeededIndices.Count; i++)
                {
                    var originalIdx = semanticMatchNeededIndices[i];
                    references[originalIdx].TitleEmbedding = embeddings[i].ToArray();
                }
            }

            // STEP 6: Semantic matching at project scope.
            var finalMatches = (await _matchingService.MatchBatchInProjectAsync(
                references,
                projectId,
                cancellationToken)).ToList();

            // STEP 7: Persist DB (batch) with canonical paper resolution.
            var papersToCreate = new List<Paper>();
            var deduplicationResults = new List<DeduplicationResult>();
            var processedReferences = new List<ProcessedReference>();
            var now = DateTimeOffset.UtcNow;

            for (int i = 0; i < risPapers.Count; i++)
            {
                var risPaper = risPapers[i];
                var reference = references[i];
                var bestMatch = finalMatches[i];

                if (string.IsNullOrWhiteSpace(risPaper.Title)) continue;

                // Intra-batch detection (match against already processed papers in this batch)
                var batchMatch = _matchingService.MatchAgainstProcessed(reference, processedReferences);

                // If batch match is better than DB match, use it
                if (batchMatch != null && batchMatch.ConfidenceScore > bestMatch.ConfidenceScore)
                {
                    bestMatch = batchMatch;
                }

                int? publicationYearInt = null;
                if (int.TryParse(risPaper.PublicationYear, out var year)) publicationYearInt = year;

                var newPaper = new Paper
                {
                    Id = Guid.NewGuid(),
                    Title = risPaper.Title,
                    Authors = risPaper.Authors,
                    Abstract = risPaper.Abstract,
                    DOI = reference.DOI,
                    PublicationType = risPaper.PublicationType,
                    PublicationYear = risPaper.PublicationYear,
                    PublicationYearInt = publicationYearInt,
                    Volume = risPaper.Volume,
                    Issue = risPaper.Issue,
                    Pages = risPaper.Pages,
                    Publisher = risPaper.Publisher,
                    Journal = risPaper.Journal,
                    JournalIssn = risPaper.JournalIssn,
                    Url = risPaper.Url,
                    Keywords = risPaper.Keywords,
                    RawReference = risPaper.RawReference,
                    ProjectId = projectId,
                    Source = searchSource?.Name ?? "Manual Upload",
                    ImportBatchId = importBatch.Id,
                    CreatedFromImportBatchId = importBatch.Id,
                    SearchSourceId = searchSource?.Id,
                    ImportedAt = importBatch.ImportedAt,
                    ImportedBy = importBatch.ImportedBy,
                    CreatedAt = now,
                    ModifiedAt = now,

                    // Extended Metadata
                    ConferenceName = risPaper.ConferenceName,
                    ConferenceLocation = risPaper.ConferenceLocation,
                    ConferenceDate = risPaper.ConferenceDate,
                    SecondaryTitle = risPaper.SecondaryTitle,
                    BookTitle = risPaper.BookTitle,
                    SourceRecordId = string.IsNullOrWhiteSpace(risPaper.Id) ? null : risPaper.Id
                };

                // Add embedding if exists
                if (reference.TitleEmbedding is { Length: > 0 })
                {
                    newPaper.TitleEmbedding = new PaperEmbedding
                    {
                        Id = Guid.NewGuid(),
                        PaperId = newPaper.Id,
                        Embedding = new Vector(reference.TitleEmbedding),
                        Model = _embeddingService.ModelName,
                        CreatedAt = now,
                        ModifiedAt = now
                    };
                }

                var duplicateOfPaperId = bestMatch?.MatchedPaper?.Id ?? bestMatch?.MatchedPaperId;
                if (bestMatch != null
                    && bestMatch.ConfidenceScore >= 0.7m
                    && duplicateOfPaperId.HasValue
                    && duplicateOfPaperId.Value != Guid.Empty
                    && duplicateOfPaperId.Value != newPaper.Id)
                {
                    deduplicationResults.Add(new DeduplicationResult
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = projectId,
                        PaperId = newPaper.Id,
                        DuplicateOfPaperId = duplicateOfPaperId.Value,
                        Method = MapDeduplicationMethod(bestMatch.Strategy),
                        ConfidenceScore = bestMatch.ConfidenceScore,
                        ReviewStatus = DeduplicationReviewStatus.Pending,
                        ResolvedDecision = null,
                        Notes = "Detected during project-level import",
                        CreatedAt = now,
                        ModifiedAt = now
                    });

                    result.DuplicateRecords++;
                    newPaper.IsDuplicated = true;
                }

                papersToCreate.Add(newPaper);
                result.ImportedRecords++;
                result.ImportedPaperIds.Add(newPaper.Id);

                processedReferences.Add(new ProcessedReference { Reference = reference, PaperId = newPaper.Id });
            }

            // Perform batched inserts
            if (papersToCreate.Any())
            {
                await _unitOfWork.Papers.AddRangeAsync(papersToCreate, cancellationToken);
            }

            if (deduplicationResults.Any())
            {
                await _unitOfWork.DeduplicationResults.AddRangeAsync(deduplicationResults, cancellationToken);
            }
        }

        private static DeduplicationMethod MapDeduplicationMethod(MatchStrategy strategy)
        {
            return strategy switch
            {
                MatchStrategy.DOI => DeduplicationMethod.DOI_MATCH,
                MatchStrategy.TitleFuzzy => DeduplicationMethod.TITLE_FUZZY,
                MatchStrategy.Semantic => DeduplicationMethod.SEMANTIC,
                MatchStrategy.TitleExact => DeduplicationMethod.TITLE_AUTHOR,
                _ => DeduplicationMethod.HYBRID
            };
        }

        private static MatchResult? SelectBestMatch(params MatchResult?[] matches)
        {
            var validMatches = matches
                .Where(m => m != null && m.ConfidenceScore > 0)
                .Select(m => m!)
                .ToList();

            if (!validMatches.Any())
            {
                return null;
            }

            var doiMatch = validMatches.FirstOrDefault(m => m.Strategy == MatchStrategy.DOI && m.ConfidenceScore >= 1.0m);
            if (doiMatch != null)
            {
                return doiMatch;
            }

            var titleExactMatch = validMatches.FirstOrDefault(m => m.Strategy == MatchStrategy.TitleExact && m.ConfidenceScore >= 1.0m);
            if (titleExactMatch != null)
            {
                return titleExactMatch;
            }

            return validMatches
                .Where(m => m.Strategy == MatchStrategy.TitleFuzzy || m.Strategy == MatchStrategy.Semantic)
                .OrderByDescending(m => m.ConfidenceScore)
                .FirstOrDefault();
        }

        /// <summary>
        /// Generates embedding input focused on semantic content.
        /// Uses Title + first 200 chars of Abstract to avoid noise from
        /// metadata fields (author, year, journal, publisher) that cause
        /// false positives with BERT-based embedding models.
        /// </summary>
        private string GenerateEmbeddingInput(string? title, string? abstractText)
        {
            var sb = new System.Text.StringBuilder();
            if (!string.IsNullOrWhiteSpace(title)) sb.Append(title.Trim());

            if (!string.IsNullOrWhiteSpace(abstractText))
            {
                var trimmedAbstract = abstractText.Trim();
                if (trimmedAbstract.Length > 200)
                {
                    trimmedAbstract = trimmedAbstract.Substring(0, 200);
                }
                sb.Append(" ");
                sb.Append(trimmedAbstract);
            }

            return sb.ToString().Trim();
        }



        private async Task EnsureIdentificationProcessCanBeEditedAsync(
            IdentificationProcess identificationProcess,
            CancellationToken cancellationToken)
        {
            if (identificationProcess.Status == IdentificationStatus.Completed)
            {
                throw new InvalidOperationException("Cannot modify identification data because the identification process is completed. Reopen the process to continue editing.");
            }

            var userIdValue = _currentUserService.GetUserId();
            if (!Guid.TryParse(userIdValue, out var currentUserId))
            {
                throw new UnauthorizedException("User not authenticated.");
            }

            var reviewProcess = await _unitOfWork.ReviewProcesses.FindSingleAsync(
                rp => rp.Id == identificationProcess.ReviewProcessId,
                cancellationToken: cancellationToken);

            if (reviewProcess == null)
            {
                throw new InvalidOperationException($"ReviewProcess with ID {identificationProcess.ReviewProcessId} not found.");
            }

            var membership = await _unitOfWork.SystematicReviewProjects.GetMembershipQueryable(currentUserId)
                .FirstOrDefaultAsync(m => m.ProjectId == reviewProcess.ProjectId, cancellationToken);

            if (membership == null || membership.Role != ProjectRole.Leader)
            {
                throw new UnauthorizedException("Only project leaders can perform this action.");
            }
        }

        private async Task EnsureIdentificationProcessCanBeEditedAsync(
            Guid identificationProcessId,
            CancellationToken cancellationToken)
        {
            var identificationProcess = await _unitOfWork.IdentificationProcesses.FindSingleAsync(
                ip => ip.Id == identificationProcessId,
                cancellationToken: cancellationToken);

            if (identificationProcess == null)
            {
                throw new InvalidOperationException($"IdentificationProcess with ID {identificationProcessId} not found.");
            }

            await EnsureIdentificationProcessCanBeEditedAsync(identificationProcess, cancellationToken);
        }

        public async Task<(List<PaperResponse> Papers, int TotalCount)> GetReadyPapersForSnapshotAsync(
            Guid identificationProcessId,
            string? search,
            int? year,
            Guid? searchSourceId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            // Define sub-queries outside the expression tree to avoid optional argument translation issues
            var deduplicationQuery = _unitOfWork.DeduplicationResults.GetQueryable(null, false);
            var snapshotQuery = _unitOfWork.IdentificationProcessPapers.GetQueryable(null, false);

            var identificationProcess = await _unitOfWork.IdentificationProcesses.GetByIdWithProjectAsync(
                identificationProcessId,
                cancellationToken);

            if (identificationProcess == null)
            {
                throw new InvalidOperationException($"IdentificationProcess with ID {identificationProcessId} not found.");
            }

            var projectId = identificationProcess.ReviewProcess.ProjectId;

            // Get base query for unique papers in this process from the project pool.
            var query = _unitOfWork.Papers.GetQueryable()
                .AsNoTracking()
                .Where(p =>
                    p.ProjectId == projectId &&
                    !p.IsDeleted &&
                    // Exclude duplicates and pending resolutions
                    !deduplicationQuery.Any(dr =>
                        dr.PaperId == p.Id &&
                        dr.ProjectId == projectId && (
                        dr.ResolvedDecision == DuplicateResolutionDecision.CANCEL || dr.ReviewStatus == DeduplicationReviewStatus.Pending)) &&
                    // Exclude papers already in the snapshot
                    !snapshotQuery.Any(ip =>
                        ip.IdentificationProcessId == identificationProcessId &&
                        ip.PaperId == p.Id));

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(p =>
                    (p.Title != null && p.Title.ToLower().Contains(searchLower)) ||
                    (p.DOI != null && p.DOI.ToLower().Contains(searchLower)) ||
                    (p.Authors != null && p.Authors.ToLower().Contains(searchLower)));
            }

            // Apply year filter
            if (year.HasValue)
            {
                query = query.Where(p => p.PublicationYearInt == year.Value);
            }

            if (searchSourceId.HasValue)
            {
                var sourceId = searchSourceId.Value;
                query = query.Where(p => p.SearchSourceId == sourceId);
            }

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply ordering and pagination
            var papers = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (papers.Select(MapToPaperResponse).ToList(), totalCount);
        }

        // public async Task AddPapersToIdentificationSnapshotAsync(
        //     Guid identificationProcessId,
        //     List<Guid> paperIds,
        //     CancellationToken cancellationToken = default)
        // {
        //     if (paperIds == null || !paperIds.Any())
        //     {
        //         return;
        //     }

        //     // Validate IdentificationProcess
        //     var identificationProcess = await _unitOfWork.IdentificationProcesses.FindSingleAsync(
        //         ip => ip.Id == identificationProcessId,
        //         cancellationToken: cancellationToken);

        //     if (identificationProcess == null)
        //     {
        //         throw new InvalidOperationException($"IdentificationProcess with ID {identificationProcessId} not found.");
        //     }

        //     await EnsureIdentificationProcessCanBeEditedAsync(identificationProcess, cancellationToken);

        //     var reviewProcess = await _unitOfWork.ReviewProcesses.FindSingleAsync(
        //         rp => rp.Id == identificationProcess.ReviewProcessId,
        //         isTracking: false,
        //         cancellationToken);

        //     if (reviewProcess == null)
        //     {
        //         throw new InvalidOperationException($"ReviewProcess with ID {identificationProcess.ReviewProcessId} not found.");
        //     }

        //     var projectId = reviewProcess.ProjectId;

        //     // Get existing paper IDs in snapshot to avoid duplicates
        //     var existingPaperIds = await _unitOfWork.IdentificationProcessPapers.GetIncludedPaperIdsByProcessAsync(
        //         identificationProcessId, cancellationToken);

        //     var existingPaperIdsSet = existingPaperIds.ToHashSet();

        //     // Filter out papers already in snapshot
        //     var newPaperIds = paperIds.Distinct().Where(id => !existingPaperIdsSet.Contains(id)).ToList();

        //     if (!newPaperIds.Any())
        //     {
        //         return;
        //     }

        //     var candidatePapers = (await _unitOfWork.Papers.FindAllAsync(
        //         p => newPaperIds.Contains(p.Id),
        //         isTracking: false,
        //         cancellationToken)).ToList();

        //     var references = candidatePapers.Select(p => new ExtractedReference
        //     {
        //         Title = p.Title,
        //         Authors = p.Authors,
        //         Abstract = p.Abstract,
        //         DOI = p.DOI,
        //         PublishedYear = p.PublicationYear,
        //         Journal = p.Journal,
        //         RawReference = p.RawReference,
        //         TitleEmbedding = p.TitleEmbedding?.Embedding?.ToArray()
        //     }).ToList();

        //     var processMatches = (await _matchingService.MatchBatchAsync(
        //         references,
        //         identificationProcessId,
        //         cancellationToken)).ToList();

        //     var deduplicationResults = new List<DeduplicationResult>();
        //     var now = DateTimeOffset.UtcNow;

        //     for (int i = 0; i < candidatePapers.Count; i++)
        //     {
        //         var candidate = candidatePapers[i];
        //         var match = processMatches[i];
        //         if (match == null || match.ConfidenceScore < 0.7m)
        //         {
        //             continue;
        //         }

        //         var duplicateOfPaperId = match.MatchedPaperId ?? match.MatchedPaper?.Id;
        //         if (!duplicateOfPaperId.HasValue || duplicateOfPaperId.Value == Guid.Empty || duplicateOfPaperId.Value == candidate.Id)
        //         {
        //             continue;
        //         }

        //         deduplicationResults.Add(new DeduplicationResult
        //         {
        //             Id = Guid.NewGuid(),
        //             ProjectId = projectId,
        //             PaperId = candidate.Id,
        //             DuplicateOfPaperId = duplicateOfPaperId.Value,
        //             Method = MapDeduplicationMethod(match.Strategy),
        //             ConfidenceScore = match.ConfidenceScore,
        //             Notes = $"Snapshot duplicate detected ({match.Strategy} match).",
        //             ReviewStatus = match.ConfidenceScore >= 0.95m
        //                 ? DeduplicationReviewStatus.Resolved
        //                 : DeduplicationReviewStatus.Pending,
        //             ResolvedDecision = match.ConfidenceScore >= 0.95m
        //                 ? DuplicateResolutionDecision.CANCEL
        //                 : null,
        //             CreatedAt = now,
        //             ModifiedAt = now
        //         });
        //     }

        //     var snapshotRecords = newPaperIds.Select(paperId => new IdentificationProcessPaper
        //     {
        //         Id = Guid.NewGuid(),
        //         IdentificationProcessId = identificationProcessId,
        //         PaperId = paperId,
        //         IncludedAfterDedup = !deduplicationResults.Any(dr => dr.PaperId == paperId && dr.ResolvedDecision == DuplicateResolutionDecision.CANCEL),
        //         CreatedAt = DateTimeOffset.UtcNow,
        //         ModifiedAt = DateTimeOffset.UtcNow
        //     }).ToList();

        //     if (deduplicationResults.Any())
        //     {
        //         await _unitOfWork.DeduplicationResults.AddRangeAsync(deduplicationResults, cancellationToken);
        //     }

        //     await _unitOfWork.IdentificationProcessPapers.AddRangeAsync(snapshotRecords, cancellationToken);
        //     await _unitOfWork.SaveChangesAsync(cancellationToken);
        // }

        public async Task<(List<PaperResponse> Papers, int TotalCount)> GetPaperIdentificationProcessSnapshotAsync(
            Guid identificationProcessId,
            string? search,
            int? year,
            Guid? searchSourceId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _unitOfWork.IdentificationProcessPapers.GetQueryable()
                .AsNoTracking()
                .Include(ipp => ipp.Paper)
                .Where(ipp => ipp.IdentificationProcessId == identificationProcessId && !ipp.Paper.IsDeleted);

            // Apply search filter on paper metadata
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(ipp =>
                    (ipp.Paper.Title != null && ipp.Paper.Title.ToLower().Contains(searchLower)) ||
                    (ipp.Paper.DOI != null && ipp.Paper.DOI.ToLower().Contains(searchLower)) ||
                    (ipp.Paper.Authors != null && ipp.Paper.Authors.ToLower().Contains(searchLower)));
            }

            // Apply year filter
            if (year.HasValue)
            {
                query = query.Where(ipp => ipp.Paper.PublicationYearInt == year.Value);
            }

            if (searchSourceId.HasValue)
            {
                var sourceId = searchSourceId.Value;
                query = query.Where(ipp => ipp.Paper.SearchSourceId == sourceId);
            }

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply ordering and pagination
            var papers = await query
                .OrderByDescending(ipp => ipp.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(ipp => ipp.Paper)
                .ToListAsync(cancellationToken);

            return (papers.Select(MapToPaperResponse).ToList(), totalCount);
        }
    }
}
