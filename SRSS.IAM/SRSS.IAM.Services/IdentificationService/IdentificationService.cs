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

            return MapToSearchExecutionResponse(searchExecution);
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

            return MapToSearchExecutionResponse(searchExecution);
        }

        public async Task<List<SearchExecutionResponse>> GetSearchExecutionsByIdentificationProcessIdAsync(
            Guid identificationProcessId,
            CancellationToken cancellationToken = default)
        {
            var searchExecutions = await _unitOfWork.SearchExecutions.FindAllAsync(
                se => se.IdentificationProcessId == identificationProcessId,
                cancellationToken: cancellationToken);

            return searchExecutions.Select(MapToSearchExecutionResponse).ToList();
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

            return MapToSearchExecutionResponse(searchExecution);
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

            await _unitOfWork.SearchExecutions.RemoveAsync(searchExecution, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
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
                        
                        // PRISMA workflow
                        CurrentSelectionStatus = SelectionStatus.Pending,
                        IsIncludedFinal = false,
                        
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
                result.Errors.Add($"IdentificationProcess with ID {identificationProcessId} not found.");
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return result;
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
                                result.Errors.Add($"SearchExecution with ID {searchExecutionId.Value} not found.");
                                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                                return result;
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
                            result.Errors.Add($"Failed to parse RIS file: {ex.Message}");
                            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                            return result;
                        }

                        if (!risPapers.Any())
                        {
                            result.Errors.Add("No valid records found in RIS file.");
                            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                            return result;
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
                                    existingPaper = await _unitOfWork.Papers.GetByDoiAndProjectAsync(risPaper.DOI, identificationProcess.ReviewProcess.ProjectId, cancellationToken);
                                }

                                if (existingPaper != null)
                                {
                                    // Handle duplicate: Update if needed
                                    bool needsUpdate = false;

                                    if (string.IsNullOrWhiteSpace(existingPaper.RawReference) && !string.IsNullOrWhiteSpace(risPaper.RawReference))
                                    {
                                        existingPaper.RawReference = risPaper.RawReference;
                                        needsUpdate = true;
                                    }

                                    // Link to current import batch if not already linked
                                    if (existingPaper.ImportBatchId == null)
                                    {
                                        existingPaper.ImportBatchId = importBatch.Id;
                                        needsUpdate = true;
                                    }

                                    if (needsUpdate)
                                    {
                                        existingPaper.ModifiedAt = DateTimeOffset.UtcNow;
                                        await _unitOfWork.Papers.UpdateAsync(existingPaper, cancellationToken);
                                        result.UpdatedRecords++;
                                    }
                                    else
                                    {
                                        result.DuplicateRecords++;
                                    }
                                }
                                else
                                {
                                    // Parse PublicationYear to int
                                    int? publicationYearInt = null;
                                    if (!string.IsNullOrWhiteSpace(risPaper.PublicationYear) &&
                                        int.TryParse(risPaper.PublicationYear, out var year))
                                    {
                                        publicationYearInt = year;
                                    }

                                    // Insert new paper
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
                                        
                                        // Import tracking - Link to ImportBatch only
                                        Source = "RIS",
                                        ImportBatchId = importBatch.Id,
                                        ImportedAt = importBatch.ImportedAt,
                                        ImportedBy = importBatch.ImportedBy,

                                        // PRISMA workflow
                                        CurrentSelectionStatus = SelectionStatus.Pending,
                                        IsIncludedFinal = false,

                                        // Audit fields
                                        CreatedAt = DateTimeOffset.UtcNow,
                                        ModifiedAt = DateTimeOffset.UtcNow
                                    };

                                    await _unitOfWork.Papers.AddAsync(newPaper, cancellationToken);
                                    result.ImportedRecords++;
                                    result.ImportedPaperIds.Add(newPaper.Id);
                                }
                            }
                            catch (Exception ex)
                            {
                                result.SkippedRecords++;
                                result.Errors.Add($"Error processing paper '{risPaper.Title}': {ex.Message}");
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
                        result.Errors.Add($"Transaction failed: {ex.Message}");
                        result.ImportBatchId = null; // No batch was persisted
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Unexpected error during import: {ex.Message}");
                }

            return result;
        }
    }
}

