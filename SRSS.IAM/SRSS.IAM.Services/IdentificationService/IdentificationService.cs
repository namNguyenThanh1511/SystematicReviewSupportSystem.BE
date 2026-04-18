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

namespace SRSS.IAM.Services.IdentificationService
{
    public class IdentificationService : IIdentificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IReferenceMatchingService _matchingService;
        private readonly ILocalEmbeddingService _embeddingService;
        private readonly IPaperEnrichmentOrchestrator _enrichmentOrchestrator;
        private readonly ICurrentUserService _currentUserService;

        public IdentificationService(
            IUnitOfWork unitOfWork,
            IReferenceMatchingService matchingService,
            ILocalEmbeddingService embeddingService,
            IPaperEnrichmentOrchestrator enrichmentOrchestrator,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _matchingService = matchingService;
            _embeddingService = embeddingService;
            _enrichmentOrchestrator = enrichmentOrchestrator;
            _currentUserService = currentUserService;
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

            // Guard: Prevent completion if there are unresolved (Pending) deduplication results
            var hasPendingDedup = await _unitOfWork.DeduplicationResults.AnyAsync(
                dr => dr.IdentificationProcessId == id && dr.ReviewStatus == DeduplicationReviewStatus.Pending,
                cancellationToken: cancellationToken);

            if (hasPendingDedup)
            {
                throw new InvalidOperationException(
                    "Cannot complete identification process with unresolved duplicate pairs. Please resolve all pending deduplication results first.");
            }

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
            Guid identificationProcessId,
            CancellationToken cancellationToken = default)
        {
            var identificationProcess = await _unitOfWork.IdentificationProcesses.FindSingleAsync(
                ip => ip.Id == identificationProcessId,
                cancellationToken: cancellationToken);

            if (identificationProcess == null)
            {
                throw new InvalidOperationException($"IdentificationProcess with ID {identificationProcessId} not found.");
            }

            var searchExecutions = await _unitOfWork.SearchExecutions.FindAllAsync(
                se => se.IdentificationProcessId == identificationProcessId,
                cancellationToken: cancellationToken);

            var searchExecutionIds = searchExecutions.Select(se => se.Id).ToHashSet();

            var importBatchList = await _unitOfWork.ImportBatches.GetBySearchExecutionIdsWithSourceAsync(
                searchExecutionIds,
                cancellationToken: cancellationToken);
            var totalRecordsImported = importBatchList.Sum(ib => ib.TotalRecords);

            // Query actual unique paper count from the frozen snapshot
            var uniquePaperIds = await _unitOfWork.IdentificationProcessPapers.GetIncludedPaperIdsByProcessAsync(identificationProcessId, cancellationToken);
            var uniqueRecordsCount = uniquePaperIds.Count;

            // Get total deduplication result count for this process
            var duplicateRecords = await _unitOfWork.DeduplicationResults.CountDuplicatesByProcessAsync(identificationProcessId, cancellationToken);

            // Breakdown by Database Source
            var identifiedBreakdown = importBatchList
                .GroupBy(ib => ib.SearchExecution?.SearchSource?.Name ?? "Manual")
                .Select(g => new PrismaBreakdownResponse { Label = g.Key, Count = g.Sum(ib => ib.TotalRecords) })
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
            var deduplicationQuery = _unitOfWork.DeduplicationResults.GetQueryable();
            var snapshotQuery = _unitOfWork.IdentificationProcessPapers.GetQueryable();

            var pendingSelectionCount = await _unitOfWork.Papers.GetQueryable()
                .Where(p => p.ImportBatchId != null && 
                            searchExecutionIds.Contains(p.ImportBatch.SearchExecutionId ?? Guid.Empty))
                // Not a duplicate (CANCEL) and not pending review
                .Where(p => !deduplicationQuery.Any(d => d.PaperId == p.Id && 
                                                        d.IdentificationProcessId == identificationProcessId && 
                                                        (d.ResolvedDecision == DuplicateResolutionDecision.CANCEL || 
                                                         d.ReviewStatus == DeduplicationReviewStatus.Pending)))
                // Not already in the snapshot
                .Where(p => !snapshotQuery.Any(i => i.IdentificationProcessId == identificationProcessId && 
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
            var searchExecution = await _unitOfWork.SearchExecutions.FindSingleAsync(
                se => se.Id == request.SearchExecutionId,
                isTracking: true,
                cancellationToken);

            if (searchExecution == null)
            {
                throw new InvalidOperationException($"SearchExecution with ID {request.SearchExecutionId} not found.");
            }

            await EnsureIdentificationProcessCanBeEditedAsync(searchExecution.IdentificationProcessId, cancellationToken);

            var importBatch = new ImportBatch
            {
                Id = Guid.NewGuid(),
                SearchExecutionId = request.SearchExecutionId,
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

        public async Task<List<ImportBatchResponse>> GetImportBatchesBySearchExecutionIdAsync(
            Guid searchExecutionId,
            CancellationToken cancellationToken = default)
        {
            var importBatches = await _unitOfWork.ImportBatches.FindAllAsync(
                ib => ib.SearchExecutionId == searchExecutionId,
                cancellationToken: cancellationToken);

            return importBatches.Select(MapToImportBatchResponse).ToList();
        }

        public async Task<List<ImportBatchResponse>> GetImportBatchesByIdentificationProcessIdAsync(
            Guid identificationProcessId,
            CancellationToken cancellationToken = default)
        {
            var searchExecutions = await _unitOfWork.SearchExecutions.FindAllAsync(
                se => se.IdentificationProcessId == identificationProcessId,
                cancellationToken: cancellationToken);

            var searchExecutionIds = searchExecutions.Select(se => se.Id).ToHashSet();

            var importBatches = await _unitOfWork.ImportBatches.FindAllAsync(
                ib => ib.SearchExecutionId != null && searchExecutionIds.Contains(ib.SearchExecutionId.Value),
                cancellationToken: cancellationToken);

            return importBatches.Select(MapToImportBatchResponse).ToList();
        }

        public async Task<ImportBatchResponse> UpdateImportBatchAsync(
            UpdateImportBatchRequest request,
            CancellationToken cancellationToken = default)
        {
            var importBatch = await _unitOfWork.ImportBatches.FindSingleAsync(
                ib => ib.Id == request.Id,
                isTracking: true,
                cancellationToken);

            if (importBatch == null)
            {
                throw new InvalidOperationException($"ImportBatch with ID {request.Id} not found.");
            }

            if (importBatch.SearchExecutionId.HasValue)
            {
                var searchExecution = await _unitOfWork.SearchExecutions.FindSingleAsync(
                    se => se.Id == importBatch.SearchExecutionId.Value,
                    cancellationToken: cancellationToken);

                if (searchExecution != null)
                {
                    await EnsureIdentificationProcessCanBeEditedAsync(
                        searchExecution.IdentificationProcessId,
                        cancellationToken);
                }
            }

            if (!string.IsNullOrWhiteSpace(request.FileName))
            {
                importBatch.FileName = request.FileName;
            }

            if (!string.IsNullOrWhiteSpace(request.FileType))
            {
                importBatch.FileType = request.FileType;
            }

            if (!string.IsNullOrWhiteSpace(request.Source))
            {
                importBatch.Source = request.Source;
            }

            if (request.TotalRecords.HasValue)
            {
                importBatch.TotalRecords = request.TotalRecords.Value;
            }

            if (!string.IsNullOrWhiteSpace(request.ImportedBy))
            {
                importBatch.ImportedBy = request.ImportedBy;
            }

            importBatch.ModifiedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.ImportBatches.UpdateAsync(importBatch, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToImportBatchResponse(importBatch);
        }

        public async Task<bool> DeleteImportBatchAsync(
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

            if (importBatch.SearchExecutionId.HasValue)
            {
                var searchExecution = await _unitOfWork.SearchExecutions.FindSingleAsync(
                    se => se.Id == importBatch.SearchExecutionId.Value,
                    cancellationToken: cancellationToken);

                if (searchExecution != null)
                {
                    await EnsureIdentificationProcessCanBeEditedAsync(
                        searchExecution.IdentificationProcessId,
                        cancellationToken);
                }
            }

            await _unitOfWork.ImportBatches.RemoveAsync(importBatch, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
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
                ModifiedAt = paper.ModifiedAt
            };
        }

        private static ImportBatchResponse MapToImportBatchResponse(ImportBatch importBatch)
        {
            return new ImportBatchResponse
            {
                Id = importBatch.Id,
                SearchExecutionId = importBatch.SearchExecutionId,
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

        public async Task<SearchExecutionResponse> CreateSearchExecutionAsync(
            CreateSearchExecutionRequest request,
            CancellationToken cancellationToken = default)
        {
            var identificationProcess = await _unitOfWork.IdentificationProcesses.FindSingleAsync(
                ip => ip.Id == request.IdentificationProcessId,
                isTracking: true,
                cancellationToken);

            if (identificationProcess == null)
            {
                throw new InvalidOperationException($"IdentificationProcess with ID {request.IdentificationProcessId} not found.");
            }

            await EnsureIdentificationProcessCanBeEditedAsync(identificationProcess, cancellationToken);

            var searchSource = await _unitOfWork.SearchSources.FindSingleAsync(s => s.Id == request.SearchSourceId, cancellationToken: cancellationToken);
            if (searchSource == null)
            {
                throw new InvalidOperationException($"SearchSource with ID {request.SearchSourceId} not found.");
            }

            var searchExecution = new SearchExecution
            {
                Id = Guid.NewGuid(),
                IdentificationProcessId = request.IdentificationProcessId,
                SearchSourceId = request.SearchSourceId,
                SearchQuery = request.SearchQuery,
                ExecutedAt = DateTimeOffset.UtcNow,
                ResultCount = 0,
                Type = request.Type,
                Notes = request.Notes,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.SearchExecutions.AddAsync(searchExecution, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return await MapToSearchExecutionResponseAsync(searchExecution, cancellationToken);
        }

        public async Task<SearchExecutionResponse> GetSearchExecutionByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var searchExecution = await _unitOfWork.SearchExecutions.FindSingleAsync(
                se => se.Id == id,
                cancellationToken: cancellationToken);

            if (searchExecution == null)
            {
                throw new NotFoundException("SearchExecution not found.");
            }

            return await MapToSearchExecutionResponseAsync(searchExecution, cancellationToken);
        }

        public async Task<List<SearchExecutionResponse>> GetSearchExecutionsByIdentificationProcessIdAsync(
            Guid identificationProcessId,
            CancellationToken cancellationToken = default)
        {
            var searchExecutions = await _unitOfWork.SearchExecutions.GetByProcessIdWithSourceAsync(identificationProcessId, cancellationToken);

            var responses = new List<SearchExecutionResponse>();
            foreach (var searchExecution in searchExecutions)
            {
                responses.Add(await MapToSearchExecutionResponseAsync(searchExecution, cancellationToken));
            }

            return responses;
        }

        public async Task<SearchExecutionResponse> UpdateSearchExecutionAsync(
            UpdateSearchExecutionRequest request,
            CancellationToken cancellationToken = default)
        {
            var searchExecution = await _unitOfWork.SearchExecutions.FindSingleAsync(
                se => se.Id == request.Id,
                isTracking: true,
                cancellationToken);

            if (searchExecution == null)
            {
                throw new InvalidOperationException($"SearchExecution with ID {request.Id} not found.");
            }

            var identificationProcess = await _unitOfWork.IdentificationProcesses.FindSingleAsync(
                ip => ip.Id == searchExecution.IdentificationProcessId,
                cancellationToken: cancellationToken);

            if (identificationProcess == null)
            {
                throw new InvalidOperationException($"IdentificationProcess with ID {searchExecution.IdentificationProcessId} not found.");
            }

            await EnsureIdentificationProcessCanBeEditedAsync(identificationProcess, cancellationToken);

            if (request.SearchSourceId.HasValue)
            {
                var searchSource = await _unitOfWork.SearchSources.FindSingleAsync(s => s.Id == request.SearchSourceId.Value, cancellationToken: cancellationToken);
                if (searchSource == null)
                {
                    throw new InvalidOperationException($"SearchSource with ID {request.SearchSourceId.Value} not found.");
                }
                searchExecution.SearchSourceId = request.SearchSourceId.Value;
            }

            if (request.SearchQuery != null)
            {
                searchExecution.SearchQuery = request.SearchQuery;
            }

            if (request.Type.HasValue)
            {
                searchExecution.Type = request.Type.Value;
            }

            if (request.Notes != null)
            {
                searchExecution.Notes = request.Notes;
            }

            searchExecution.ModifiedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SearchExecutions.UpdateAsync(searchExecution, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return await MapToSearchExecutionResponseAsync(searchExecution, cancellationToken);
        }

        public async Task<bool> DeleteSearchExecutionAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var searchExecution = await _unitOfWork.SearchExecutions.FindSingleAsync(
                se => se.Id == id,
                cancellationToken: cancellationToken);

            if (searchExecution == null)
            {
                throw new NotFoundException("SearchExecution not found.");
            }

            var identificationProcess = await _unitOfWork.IdentificationProcesses.FindSingleAsync(
                ip => ip.Id == searchExecution.IdentificationProcessId,
                cancellationToken: cancellationToken);

            if (identificationProcess == null)
            {
                throw new InvalidOperationException($"IdentificationProcess with ID {searchExecution.IdentificationProcessId} not found.");
            }

            await EnsureIdentificationProcessCanBeEditedAsync(identificationProcess, cancellationToken);

            var hasImportBatches = await _unitOfWork.ImportBatches.AnyAsync(
                ib => ib.SearchExecutionId == id,
                cancellationToken: cancellationToken);

            if (hasImportBatches)
            {
                throw new InvalidOperationException(
                    "Cannot delete search execution with existing import batches. Please delete all import batches first."
                );
            }

            await _unitOfWork.SearchExecutions.RemoveAsync(searchExecution, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }

        private async Task<SearchExecutionResponse> MapToSearchExecutionResponseAsync(
            SearchExecution searchExecution,
            CancellationToken cancellationToken = default)
        {
            var importBatches = await _unitOfWork.ImportBatches.FindAllAsync(
                ib => ib.SearchExecutionId == searchExecution.Id,
                isTracking: false,
                cancellationToken: cancellationToken);

            var importBatchCount = importBatches.Count();

            return new SearchExecutionResponse
            {
                Id = searchExecution.Id,
                IdentificationProcessId = searchExecution.IdentificationProcessId,
                SearchSource = searchExecution.SearchSource?.Name ?? string.Empty,
                SearchQuery = searchExecution.SearchQuery,
                ExecutedAt = searchExecution.ExecutedAt,
                ResultCount = searchExecution.ResultCount,
                Type = searchExecution.Type,
                TypeText = searchExecution.Type.ToString(),
                Notes = searchExecution.Notes,
                ImportBatchCount = importBatchCount,
                CreatedAt = searchExecution.CreatedAt,
                ModifiedAt = searchExecution.ModifiedAt
            };
        }

        private static SearchExecutionResponse MapToSearchExecutionResponse(SearchExecution searchExecution)
        {
            return new SearchExecutionResponse
            {
                Id = searchExecution.Id,
                IdentificationProcessId = searchExecution.IdentificationProcessId,
                SearchSource = searchExecution.SearchSource?.Name ?? string.Empty,
                SearchQuery = searchExecution.SearchQuery,
                ExecutedAt = searchExecution.ExecutedAt,
                ResultCount = searchExecution.ResultCount,
                Type = searchExecution.Type,
                TypeText = searchExecution.Type.ToString(),
                Notes = searchExecution.Notes,
                ImportBatchCount = 0,
                CreatedAt = searchExecution.CreatedAt,
                ModifiedAt = searchExecution.ModifiedAt
            };
        }

        public async Task<ImportPaperResponse> ImportPaperAsync(
            ImportPaperRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request.Papers == null || !request.Papers.Any())
            {
                throw new ArgumentException("No papers provided for import.", nameof(request.Papers));
            }

            // Begin transaction to ensure atomicity
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // Validate SearchExecution if provided
                SearchExecution? searchExecution = null;
                if (request.SearchExecutionId.HasValue)
                {
                    searchExecution = await _unitOfWork.SearchExecutions.FindSingleAsync(
                        se => se.Id == request.SearchExecutionId.Value,
                        isTracking: true,
                        cancellationToken);

                    if (searchExecution == null)
                    {
                        await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                        throw new InvalidOperationException($"SearchExecution with ID {request.SearchExecutionId.Value} not found.");
                    }

                    await EnsureIdentificationProcessCanBeEditedAsync(
                        searchExecution.IdentificationProcessId,
                        cancellationToken);
                }

                // Create ImportBatch for this manual import
                var importBatch = new ImportBatch
                {
                    Id = Guid.NewGuid(),
                    FileName = "Manual Import",
                    FileType = "JSON",
                    Source = "Manual Entry",
                    TotalRecords = request.Papers.Count,
                    ImportedBy = request.ImportedBy,
                    ImportedAt = DateTimeOffset.UtcNow,
                    SearchExecutionId = request.SearchExecutionId,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                };

                await _unitOfWork.ImportBatches.AddAsync(importBatch, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);


                // Create papers linked to ImportBatch
                var papers = new List<Paper>();
                foreach (var paperDto in request.Papers)
                {
                    var paper = new Paper
                    {
                        Id = Guid.NewGuid(),
                        Title = paperDto.Title,
                        Authors = paperDto.Authors,
                        Abstract = paperDto.Abstract,
                        DOI = paperDto.DOI,
                        PublicationYear = paperDto.PublicationYear,
                        Journal = paperDto.Journal,
                        Url = paperDto.Url,
                        Keywords = paperDto.Keywords,

                        // Import tracking - Link to ImportBatch only
                        ImportBatchId = importBatch.Id,
                        SearchSourceId = searchExecution?.SearchSourceId,
                        Source = "Manual",
                        ImportedAt = importBatch.ImportedAt,
                        ImportedBy = importBatch.ImportedBy,
                        // TODO: Add ProjectId to ImportPaperRequest
                        // ProjectId = request.ProjectId,

                        // Paper is immutable bibliographic record - no workflow state
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = DateTimeOffset.UtcNow
                    };

                    papers.Add(paper);
                }

                await _unitOfWork.Papers.AddRangeAsync(papers, cancellationToken);
                // Update SearchExecution result count if provided
                if (searchExecution != null)
                {
                    searchExecution.ResultCount += papers.Count;
                    searchExecution.ModifiedAt = DateTimeOffset.UtcNow;
                    await _unitOfWork.SearchExecutions.UpdateAsync(searchExecution, cancellationToken);
                }

                // Commit transaction
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return new ImportPaperResponse
                {
                    TotalImported = papers.Count,
                    ImportedPaperIds = papers.Select(p => p.Id).ToList()
                };
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task<RisImportResultDto> ImportRisFileAsync(
            Stream fileStream,
            string fileName,
            Guid? searchSourceId,
            string? importedBy,
            Guid? searchExecutionId,
            Guid identificationProcessId,
            CancellationToken cancellationToken = default)
        {
            var result = new RisImportResultDto();

            // Validate IdentificationProcess 
            IdentificationProcess identificationProcess = await _unitOfWork.IdentificationProcesses.GetByIdWithProjectAsync(identificationProcessId, cancellationToken);

            if (identificationProcess == null)
            {

                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw new InvalidOperationException($"IdentificationProcess with ID {identificationProcessId} not found.");
            }

            await EnsureIdentificationProcessCanBeEditedAsync(identificationProcess, cancellationToken);

            try
            {
                // Begin transaction to ensure atomicity
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                try
                {
                    // Validate SearchExecution if provided
                    SearchExecution? searchExecution = null;
                    if (searchExecutionId.HasValue)
                    {
                        searchExecution = await _unitOfWork.SearchExecutions.GetByIdWithProjectAsync(
                            searchExecutionId.Value,
                            cancellationToken);

                        if (searchExecution == null)
                        {

                            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                            throw new InvalidOperationException($"SearchExecution with ID {searchExecutionId.Value} not found.");
                        }
                    }
                    else
                    {
                        // Create a new SearchExecution for this import
                        searchExecution = new SearchExecution
                        {
                            Id = Guid.NewGuid(),
                            IdentificationProcessId = identificationProcessId,
                            SearchSourceId = searchSourceId ?? Guid.Empty, // Requires a valid source ID
                            ExecutedAt = DateTimeOffset.UtcNow,
                            ResultCount = 0,
                            Type = SearchExecutionType.ManualImport,
                            CreatedAt = DateTimeOffset.UtcNow,
                            ModifiedAt = DateTimeOffset.UtcNow
                        };

                        await _unitOfWork.SearchExecutions.AddAsync(searchExecution, cancellationToken);
                    }

                    // Parse RIS file before creating ImportBatch
                    List<RisPaperDto> risPapers;
                    try
                    {
                        risPapers = RisParser.Parse(fileStream);
                    }
                    catch (Exception ex)
                    {

                        await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                        throw new InvalidOperationException($"Failed to parse RIS file: {ex.Message}", ex);

                    }

                    if (!risPapers.Any())
                    {

                        await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                        throw new InvalidOperationException("No valid records found in the RIS file.");

                    }

                    result.TotalRecords = risPapers.Count;

                    // Create ImportBatch to track this import
                    var importBatch = new ImportBatch
                    {
                        Id = Guid.NewGuid(),
                        FileName = fileName,
                        FileType = "RIS",
                        Source = searchExecution?.SearchSource?.Name ?? "Manual Upload",
                        TotalRecords = risPapers.Count,
                        ImportedBy = importedBy,
                        ImportedAt = DateTimeOffset.UtcNow,
                        SearchExecutionId = searchExecutionId,
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = DateTimeOffset.UtcNow
                    };

                    await _unitOfWork.ImportBatches.AddAsync(importBatch, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    result.ImportBatchId = importBatch.Id;

                    // Process each paper
                    await ProcessPapersAsync(
                        risPapers,
                        identificationProcess,
                        importBatch,
                        result,
                        searchExecution,
                        cancellationToken);

                    // Update SearchExecution result count if provided
                    if (searchExecution != null)
                    {
                        searchExecution.ResultCount += result.ImportedRecords;
                        searchExecution.ModifiedAt = DateTimeOffset.UtcNow;
                        await _unitOfWork.SearchExecutions.UpdateAsync(searchExecution, cancellationToken);
                    }

                    // Update ImportBatch with final statistics
                    importBatch.TotalRecords = result.TotalRecords;
                    importBatch.ModifiedAt = DateTimeOffset.UtcNow;
                    await _unitOfWork.ImportBatches.UpdateAsync(importBatch, cancellationToken);

                    // Commit transaction - all changes persisted atomically
                    await _unitOfWork.CommitTransactionAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    // Rollback transaction on any error
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw new Exception(ex.Message, ex);

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);

            }

            return result;
        }
        private async Task ProcessPapersAsync(
            List<RisPaperDto> risPapers,
            IdentificationProcess identificationProcess,
            ImportBatch importBatch,
            RisImportResultDto result,
            SearchExecution? searchExecution,
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

            // STEP 2 & 3: Fast Match (DOI, Exact title) & Fuzzy match (cheap)
            // MatchBatchAsync without embeddings will perform these steps
            var preliminaryMatches = (await _matchingService.MatchBatchAsync(
                references,
                identificationProcess.Id,
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

            // STEP 6: Semantic matching
            // We only need to re-match the items we just embedded
            // For simplicity and to handle intra-batch matches, we re-run MatchBatchAsync
            // with the now-enriched references.
            var finalMatches = (await _matchingService.MatchBatchAsync(
                references,
                identificationProcess.Id,
                cancellationToken)).ToList();

            // STEP 7: Persist DB (batch)
            var papersToSource = new List<Paper>();
            var deduplicationResults = new List<DeduplicationResult>();
            var processedReferences = new List<ProcessedReference>();

            for (int i = 0; i < risPapers.Count; i++)
            {
                var risPaper = risPapers[i];
                var reference = references[i];
                var bestMatch = finalMatches[i];

                if (string.IsNullOrWhiteSpace(risPaper.Title)) continue;

                // Intra-batch detection (match against already processed papers in this batch)
                var batchMatch = _matchingService.MatchAgainstProcessed(reference, processedReferences);

                // If batch match is better than DB match, use it
                bool isBatchLevelMatch = false;
                if (batchMatch != null && batchMatch.ConfidenceScore > bestMatch.ConfidenceScore)
                {
                    bestMatch = batchMatch;
                    isBatchLevelMatch = true;
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
                    ProjectId = identificationProcess.ReviewProcess.ProjectId,
                    Source = searchExecution?.SearchSource?.Name ?? "Manual Upload",
                    ImportBatchId = importBatch.Id,
                    SearchSourceId = searchExecution?.SearchSourceId,
                    ImportedAt = importBatch.ImportedAt,
                    ImportedBy = importBatch.ImportedBy,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
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
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = DateTimeOffset.UtcNow
                    };
                }

                papersToSource.Add(newPaper);
                result.ImportedRecords++;
                result.ImportedPaperIds.Add(newPaper.Id);

                // Handle duplication
                if (bestMatch != null && bestMatch.ConfidenceScore >= 0.7m)
                {
                    var deduplicationResult = new DeduplicationResult
                    {
                        Id = Guid.NewGuid(),
                        IdentificationProcessId = identificationProcess.Id,
                        PaperId = newPaper.Id,
                        DuplicateOfPaperId = isBatchLevelMatch ? (bestMatch.MatchedPaperId ?? Guid.Empty) : (bestMatch.MatchedPaper?.Id ?? bestMatch.MatchedPaperId ?? Guid.Empty),
                        Method = bestMatch.Strategy switch
                        {
                            MatchStrategy.DOI => DeduplicationMethod.DOI_MATCH,
                            MatchStrategy.TitleExact => DeduplicationMethod.TITLE_AUTHOR,
                            MatchStrategy.TitleFuzzy => DeduplicationMethod.TITLE_FUZZY,
                            MatchStrategy.Semantic => DeduplicationMethod.SEMANTIC,
                            _ => DeduplicationMethod.HYBRID
                        },
                        ConfidenceScore = bestMatch.ConfidenceScore,
                        Notes = isBatchLevelMatch
                            ? $"Intra-batch duplicate detected ({bestMatch.Strategy} match)."
                            : $"Database duplicate detected ({bestMatch.Strategy} match).",
                        ReviewStatus = bestMatch.ConfidenceScore >= 0.95m
                            ? DeduplicationReviewStatus.Confirmed
                            : DeduplicationReviewStatus.Pending,
                        ResolvedDecision = bestMatch.ConfidenceScore >= 0.95m
                            ? DuplicateResolutionDecision.CANCEL
                            : null,
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = DateTimeOffset.UtcNow
                    };

                    deduplicationResults.Add(deduplicationResult);
                    result.DuplicateRecords++;
                }

                processedReferences.Add(new ProcessedReference { Reference = reference, PaperId = newPaper.Id });
            }

            // Perform batched inserts
            if (papersToSource.Any())
            {
                await _unitOfWork.Papers.AddRangeAsync(papersToSource, cancellationToken);
            }

            if (deduplicationResults.Any())
            {
                await _unitOfWork.DeduplicationResults.AddRangeAsync(deduplicationResults, cancellationToken);
            }
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

        public async Task MarkAsDuplicateAsync(
            Guid identificationProcessId,
            Guid paperId,
            MarkAsDuplicateRequest request,
            CancellationToken cancellationToken = default)
        {
            var identificationProcess = await _unitOfWork.IdentificationProcesses.FindSingleAsync(
                ip => ip.Id == identificationProcessId,
                cancellationToken: cancellationToken);

            if (identificationProcess == null)
            {
                throw new InvalidOperationException($"IdentificationProcess with ID {identificationProcessId} not found.");
            }

            await EnsureIdentificationProcessCanBeEditedAsync(identificationProcess, cancellationToken);

            var paper = await _unitOfWork.Papers.FindSingleAsync(
                p => p.Id == paperId,
                cancellationToken: cancellationToken);

            if (paper == null)
            {
                throw new InvalidOperationException($"Paper with ID {paperId} not found.");
            }

            var duplicateOfPaper = await _unitOfWork.Papers.FindSingleAsync(
                p => p.Id == request.DuplicateOfPaperId,
                cancellationToken: cancellationToken);

            if (duplicateOfPaper == null)
            {
                throw new InvalidOperationException($"Original paper with ID {request.DuplicateOfPaperId} not found.");
            }

            var deduplicationResult = new DeduplicationResult
            {
                Id = Guid.NewGuid(),
                IdentificationProcessId = identificationProcessId,
                PaperId = paperId,
                DuplicateOfPaperId = request.DuplicateOfPaperId,
                Method = DeduplicationMethod.MANUAL,
                ReviewStatus = DeduplicationReviewStatus.Confirmed,
                ResolvedDecision = DuplicateResolutionDecision.CANCEL,
                ConfidenceScore = 1.0m,
                Notes = request.Reason,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.DeduplicationResults.AddAsync(deduplicationResult, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
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

            // Get base query for unique papers in this process
            var query = _unitOfWork.Papers.GetQueryable()
                .AsNoTracking()
                .Where(p =>
                    p.ImportBatch != null &&
                    p.ImportBatch.SearchExecution != null &&
                    p.ImportBatch.SearchExecution.IdentificationProcessId == identificationProcessId &&
                    // Exclude duplicates and pending resolutions
                    !deduplicationQuery.Any(dr =>
                        dr.PaperId == p.Id &&
                        dr.IdentificationProcessId == identificationProcessId && (
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
                query = query.Where(p =>
                    p.SearchSourceId == sourceId ||
                    (p.SearchSourceId == null &&
                     p.ImportBatch != null &&
                     p.ImportBatch.SearchExecution != null &&
                     p.ImportBatch.SearchExecution.SearchSourceId == sourceId));
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

        public async Task AddPapersToIdentificationSnapshotAsync(
            Guid identificationProcessId,
            List<Guid> paperIds,
            CancellationToken cancellationToken = default)
        {
            if (paperIds == null || !paperIds.Any())
            {
                return;
            }

            // Validate IdentificationProcess
            var identificationProcess = await _unitOfWork.IdentificationProcesses.FindSingleAsync(
                ip => ip.Id == identificationProcessId,
                cancellationToken: cancellationToken);

            if (identificationProcess == null)
            {
                throw new InvalidOperationException($"IdentificationProcess with ID {identificationProcessId} not found.");
            }

            await EnsureIdentificationProcessCanBeEditedAsync(identificationProcess, cancellationToken);

            // Get existing paper IDs in snapshot to avoid duplicates
            var existingPaperIds = await _unitOfWork.IdentificationProcessPapers.GetIncludedPaperIdsByProcessAsync(
                identificationProcessId, cancellationToken);

            var existingPaperIdsSet = existingPaperIds.ToHashSet();

            // Filter out papers already in snapshot
            var newPaperIds = paperIds.Distinct().Where(id => !existingPaperIdsSet.Contains(id)).ToList();

            if (!newPaperIds.Any())
            {
                return;
            }

            // Optional: Validate that these papers belong to the process and are not duplicates
            // For now, we trust the bulk select if it came from the "Ready" list

            var snapshotRecords = newPaperIds.Select(paperId => new IdentificationProcessPaper
            {
                Id = Guid.NewGuid(),
                IdentificationProcessId = identificationProcessId,
                PaperId = paperId,
                IncludedAfterDedup = true,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            }).ToList();

            await _unitOfWork.IdentificationProcessPapers.AddRangeAsync(snapshotRecords, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

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
                .Where(ipp => ipp.IdentificationProcessId == identificationProcessId);

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
                query = query.Where(ipp =>
                    ipp.Paper.SearchSourceId == sourceId ||
                    (ipp.Paper.SearchSourceId == null &&
                     ipp.Paper.ImportBatch != null &&
                     ipp.Paper.ImportBatch.SearchExecution != null &&
                     ipp.Paper.ImportBatch.SearchExecution.SearchSourceId == sourceId));
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
