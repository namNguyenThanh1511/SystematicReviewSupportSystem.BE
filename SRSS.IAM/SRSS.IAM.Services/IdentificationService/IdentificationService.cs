using Shared.Exceptions;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.Identification;
using SRSS.IAM.Services.Utils;

namespace SRSS.IAM.Services.IdentificationService
{
    public class IdentificationService : IIdentificationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public IdentificationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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

            identificationProcess.Complete();

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

            var allImportBatches = await _unitOfWork.ImportBatches.FindAllAsync(
                ib => ib.SearchExecutionId != null && searchExecutionIds.Contains(ib.SearchExecutionId.Value),
                cancellationToken: cancellationToken);

            var importBatchList = allImportBatches.ToList();
            var totalRecordsImported = importBatchList.Sum(ib => ib.TotalRecords);

            // Query actual unique paper count using the same logic as the unique papers endpoint
            var (_, uniqueRecords) = await _unitOfWork.Papers.GetUniquePapersByIdentificationProcessAsync(
                identificationProcessId,
                search: null,
                year: null,
                pageNumber: 1,
                pageSize: 1,
                cancellationToken);

            // Derive duplicate count from total minus unique to account for ALL removed papers
            // (both skipped-during-import duplicates and tracked DeduplicationResult duplicates)
            var duplicateRecords = totalRecordsImported - uniqueRecords;

            return new PrismaStatisticsResponse
            {
                TotalRecordsImported = totalRecordsImported,
                DuplicateRecords = duplicateRecords,
                UniqueRecords = uniqueRecords,
                ImportBatchCount = importBatchList.Count
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
                ImportedAt = paper.ImportedAt,
                ImportedBy = paper.ImportedBy,
                PdfUrl = paper.PdfUrl,
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

            var searchExecution = new SearchExecution
            {
                Id = Guid.NewGuid(),
                IdentificationProcessId = request.IdentificationProcessId,
                SearchSource = request.SearchSource,
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
            var searchExecutions = await _unitOfWork.SearchExecutions.FindAllAsync(
                se => se.IdentificationProcessId == identificationProcessId,
                cancellationToken: cancellationToken);

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

            if (!string.IsNullOrWhiteSpace(request.SearchSource))
            {
                searchExecution.SearchSource = request.SearchSource;
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
                SearchSource = searchExecution.SearchSource,
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
                SearchSource = searchExecution.SearchSource,
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
            string? source,
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
                                SearchSource = source ?? "Manual Import",
                                ExecutedAt = DateTimeOffset.UtcNow,
                                ResultCount = 0,
                                Type = SearchExecutionType.ManualImport,
                                CreatedAt = DateTimeOffset.UtcNow,
                                ModifiedAt = DateTimeOffset.UtcNow
                            };

                            await _unitOfWork.SearchExecutions.AddAsync(searchExecution, cancellationToken);
                            searchExecutionId = searchExecution.Id;
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
                            Source = source ?? "Manual Upload",
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
                        foreach (var risPaper in risPapers)
                        {
                            try
                            {
                                // Validate required field (Title)
                                if (string.IsNullOrWhiteSpace(risPaper.Title))
                                {
                                    result.SkippedRecords++;
                                    result.Errors.Add("Skipped record: Missing title.");
                                    continue;
                                }

                                // Check for duplicate by DOI
                                Paper? existingPaper = null;
                                if (!string.IsNullOrWhiteSpace(risPaper.DOI))
                                {
                                    // When importing with a search execution ID, check within that search execution
                                    if (searchExecutionId.HasValue)
                                    {
                                        existingPaper = await _unitOfWork.Papers.GetByDoiAndSearchExecutionAsync(
                                            risPaper.DOI,
                                            searchExecutionId.Value,
                                            cancellationToken);

                                        if (existingPaper != null)
                                        {
                                            throw new InvalidOperationException("Duplicate record detected within the same search execution based on DOI. Import aborted to prevent duplicates. Please review the RIS file and remove duplicates before importing.");
                                        }
                       
                                    }
                                    //get orginal paper for deduplication 
                                    existingPaper = await _unitOfWork.Papers.GetByDoiAndProjectAsync(
                                            risPaper.DOI,
                                            identificationProcess.ReviewProcess.ProjectId,
                                            cancellationToken);
                                    
                                }

                                // Parse PublicationYear to int
                                int? publicationYearInt = null;
                                if (!string.IsNullOrWhiteSpace(risPaper.PublicationYear) &&
                                    int.TryParse(risPaper.PublicationYear, out var year))
                                {
                                    publicationYearInt = year;
                                }

                                // Create new paper (or track duplication when no search execution ID)
                                var newPaper = new Paper
                                {
                                    Id = Guid.NewGuid(),
                                    Title = risPaper.Title,
                                    Authors = risPaper.Authors,
                                    Abstract = risPaper.Abstract,
                                    DOI = risPaper.DOI,
                                    PublicationType = risPaper.PublicationType,
                                    PublicationYear = risPaper.PublicationYear,
                                    PublicationYearInt = publicationYearInt,
                                    PublicationDate = risPaper.PublicationDate,
                                    Volume = risPaper.Volume,
                                    Issue = risPaper.Issue,
                                    Pages = risPaper.Pages,
                                    Publisher = risPaper.Publisher,
                                    ConferenceLocation = risPaper.ConferenceLocation,
                                    ConferenceName = risPaper.ConferenceName,
                                    Journal = risPaper.Journal,
                                    JournalIssn = risPaper.JournalIssn,
                                    Url = risPaper.Url,
                                    Keywords = risPaper.Keywords,
                                    RawReference = risPaper.RawReference,

                                    // Link to Project
                                    ProjectId = identificationProcess.ReviewProcess.ProjectId,

                                    // Import tracking
                                    Source = "RIS",
                                    ImportBatchId = importBatch.Id,
                                    ImportedAt = importBatch.ImportedAt,
                                    ImportedBy = importBatch.ImportedBy,

                                    // Paper is immutable bibliographic record - no workflow state
                                    // Audit fields
                                    CreatedAt = DateTimeOffset.UtcNow,
                                    ModifiedAt = DateTimeOffset.UtcNow
                                };

                                await _unitOfWork.Papers.AddAsync(newPaper, cancellationToken);
                                result.ImportedRecords++;
                                result.ImportedPaperIds.Add(newPaper.Id);

                                // If duplicate detected (no search execution ID case), create deduplication result
                                if (existingPaper != null)
                                {
                                    var deduplicationResult = new DeduplicationResult
                                    {
                                        Id = Guid.NewGuid(),
                                        IdentificationProcessId = identificationProcessId,
                                        PaperId = newPaper.Id,
                                        DuplicateOfPaperId = existingPaper.Id,
                                        Method = DeduplicationMethod.DOI_MATCH,
                                        ConfidenceScore = 1.0m, // DOI match = 100% confidence
                                        Notes = $"Duplicate detected by DOI: {risPaper.DOI}",
                                        CreatedAt = DateTimeOffset.UtcNow,
                                        ModifiedAt = DateTimeOffset.UtcNow
                                    };

                                    await _unitOfWork.DeduplicationResults.AddAsync(deduplicationResult, cancellationToken);
                                    result.DuplicateRecords++;
                                }
                            }
                            catch (Exception ex)
                            {
                               throw new InvalidOperationException($"Error importing record with title '{risPaper.Title}': {ex.Message}", ex);
                        }
                        }

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
                        throw new Exception("An error occurred during RIS import. See inner exception for details.", ex);
             
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("An unexpected error occurred during RIS import. See inner exception for details.", ex);

            }

            return result;
        }
    }
}

