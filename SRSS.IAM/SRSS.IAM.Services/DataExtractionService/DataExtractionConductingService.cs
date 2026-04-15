using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using SRSS.IAM.Services.GrobidClient;
using Shared.Models;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.DataExtraction;
using ClosedXML.Excel;
using SRSS.IAM.Services.UserService;
using SRSS.IAM.Services.RagService;
using System.Xml.Linq;

namespace SRSS.IAM.Services.DataExtractionService
{
    public class DataExtractionConductingService : IDataExtractionConductingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IGrobidService _grobidService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IRagRetrievalService _ragRetrievalService;
        private readonly IRagIngestionQueue _ragQueue;

        public DataExtractionConductingService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IGrobidService grobidService,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IRagRetrievalService ragRetrievalService,
            IRagIngestionQueue ragQueue)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _grobidService = grobidService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _ragRetrievalService = ragRetrievalService;
            _ragQueue = ragQueue;
        }

        private async Task SyncEligiblePapersAsync(Guid extractionProcessId, CancellationToken cancellationToken = default)
        {
            // 1. Lấy thông tin ReviewProcess để biết ID của StudySelectionProcess
            var extractionProcess = await _unitOfWork.DataExtractionProcesses.GetQueryable()
                .Include(dp => dp.ReviewProcess)
                    .ThenInclude(rp => rp.StudySelectionProcess)
                .FirstOrDefaultAsync(dp => dp.Id == extractionProcessId, cancellationToken);

            if (extractionProcess?.ReviewProcess?.StudySelectionProcess == null) return;

            var selectionProcessId = extractionProcess.ReviewProcess.StudySelectionProcess.Id;

            // 2. Lấy danh sách Paper đã PASS vòng Full-Text Screening (Include Paper để lấy PdfUrl)
            var eligiblePapers = await _unitOfWork.StudySelectionProcessPapers.GetWithPaperByProcessAsync(selectionProcessId, cancellationToken);

            // 3. Lấy danh sách Task đã tồn tại trong Data Extraction để tránh tạo trùng
            var existingTaskPaperIds = await _unitOfWork.ExtractionPaperTasks.GetQueryable()
                .Where(t => t.DataExtractionProcessId == extractionProcessId)
                .Select(t => t.PaperId)
                .ToListAsync(cancellationToken);

            // 4. Tìm ra những Paper mới được pass Screening nhưng chưa có Task ở Extraction
            var newPapers = eligiblePapers.Where(sr => !existingTaskPaperIds.Contains(sr.PaperId)).ToList();

            if (newPapers.Any())
            {
                var newTasks = newPapers.Select(sr => new ExtractionPaperTask
                {
                    DataExtractionProcessId = extractionProcessId,
                    PaperId = sr.PaperId,
                    Status = PaperExtractionStatus.NotStarted,
                    Reviewer1Status = ReviewerTaskStatus.NotStarted,
                    Reviewer2Status = ReviewerTaskStatus.NotStarted,
                    CreatedAt = DateTimeOffset.UtcNow
                }).ToList();

                // Lưu hàng loạt xuống DB
                await _unitOfWork.ExtractionPaperTasks.AddRangeAsync(newTasks);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // ==========================================
                // KÍCH HOẠT RAG INGESTION ĐÃ CHUYỂN SANG STUDY SELECTION COMPLETE
                // ==========================================
            }
        }

        public async Task<ExtractionDashboardResponseDto> GetDashboardAsync(Guid extractionProcessId, ExtractionDashboardFilterDto filter)
        {
            // --- Role-Based Filtering ---
            var currentUserIdStr = _currentUserService.GetUserId();
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
            {
                throw new UnauthorizedAccessException("Current user ID is invalid.");
            }

            var extractionProcess = await _unitOfWork.DataExtractionProcesses.GetQueryable()
                .Include(dp => dp.ReviewProcess)
                .FirstOrDefaultAsync(dp => dp.Id == extractionProcessId);

            if (extractionProcess?.ReviewProcess == null)
            {
                throw new InvalidOperationException($"DataExtractionProcess {extractionProcessId} or its ReviewProcess not found.");
            }

            var projectId = extractionProcess.ReviewProcess.ProjectId;

            var currentUserProjectMember = await _unitOfWork.SystematicReviewProjects.GetQueryable()
                .Where(p => p.Id == projectId)
                .SelectMany(p => p.ProjectMembers)
                .FirstOrDefaultAsync(pm => pm.UserId == currentUserId);

            if (currentUserProjectMember == null)
            {
                throw new UnauthorizedAccessException($"User is not a member of project {projectId}.");
            }

            var query = _unitOfWork.ExtractionPaperTasks.GetTasksByProcessQueryable(extractionProcessId);

            // Reviewers only see papers assigned to them
            if (currentUserProjectMember.Role == ProjectRole.Member)
            {
                query = query.Where(t => t.Reviewer1Id == currentUserId || t.Reviewer2Id == currentUserId);
            }

            // Calculate Summary on the (potentially filtered) dataset
            var totalIncluded = await query.CountAsync();
            var inProgressCount = await query.CountAsync(t => t.Status == PaperExtractionStatus.InProgress);
            var awaitingConsensusCount = await query.CountAsync(t => t.Status == PaperExtractionStatus.AwaitingConsensus);
            var completedCount = await query.CountAsync(t => t.Status == PaperExtractionStatus.Completed);

            // Apply SearchFilter
            if (!string.IsNullOrWhiteSpace(filter.SearchQuery))
            {
                var searchTerm = filter.SearchQuery.ToLower();
                query = query.Where(t =>
                    t.Paper.Title.ToLower().Contains(searchTerm) ||
                    (t.Paper.Authors != null && t.Paper.Authors.ToLower().Contains(searchTerm)));
            }

            // Apply StatusFilter
            if (!string.IsNullOrWhiteSpace(filter.StatusFilter) && filter.StatusFilter.ToLower() != "all")
            {
                var statusStr = filter.StatusFilter.ToLower();
                var statusEnum = statusStr switch
                {
                    "todo" => PaperExtractionStatus.NotStarted,
                    "in-progress" => PaperExtractionStatus.InProgress,
                    "awaiting-consensus" => PaperExtractionStatus.AwaitingConsensus,
                    "completed" => PaperExtractionStatus.Completed,
                    _ => (PaperExtractionStatus?)null
                };

                if (statusEnum.HasValue)
                {
                    query = query.Where(t => t.Status == statusEnum.Value);
                }
            }

            // Pagination
            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(t => t.Paper.Title)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(t => new ExtractionDashboardTaskDto
                {
                    TaskId = t.Id,
                    PaperId = t.PaperId,
                    Title = t.Paper.Title,
                    Authors = t.Paper.Authors,
                    PublicationYear = t.Paper.PublicationYearInt,
                    Reviewer1Id = t.Reviewer1Id,
                    Reviewer2Id = t.Reviewer2Id,
                    Status = t.Status.ToString(),
                    Reviewer1Status = t.Reviewer1Status.ToString(),
                    Reviewer2Status = t.Reviewer2Status.ToString(),
                    PdfUrl = t.Paper.PdfUrl
                })
                .ToListAsync();

            return new ExtractionDashboardResponseDto
            {
                Summary = new ExtractionDashboardSummaryDto
                {
                    TotalIncluded = totalIncluded,
                    InProgress = inProgressCount,
                    AwaitingConsensus = awaitingConsensusCount,
                    Completed = completedCount
                },
                Tasks = new PaginatedList<ExtractionDashboardTaskDto>(items, totalCount, filter.PageNumber, filter.PageSize)
            };
        }

        public async Task AssignReviewersAsync(Guid extractionProcessId, Guid paperId, AssignReviewersDto dto)
        {
            if (!dto.Reviewer1Id.HasValue || !dto.Reviewer2Id.HasValue)
            {
                throw new ArgumentException("Both Reviewer1 and Reviewer2 are required.");
            }

            if (dto.Reviewer1Id == dto.Reviewer2Id)
            {
                throw new ArgumentException("Reviewer1 and Reviewer2 cannot be the same user.");
            }

            var task = await _unitOfWork.ExtractionPaperTasks.GetQueryable()
                .FirstOrDefaultAsync(t => t.DataExtractionProcessId == extractionProcessId && t.PaperId == paperId);

            if (task == null)
            {
                throw new InvalidOperationException($"Extraction task for paper {paperId} in process {extractionProcessId} not found.");
            }

            // Validation: Fetch project info
            var extractionProcess = await _unitOfWork.DataExtractionProcesses.GetQueryable()
                .Include(dp => dp.ReviewProcess)
                .FirstOrDefaultAsync(dp => dp.Id == extractionProcessId);

            if (extractionProcess?.ReviewProcess == null)
            {
                throw new InvalidOperationException($"DataExtractionProcess {extractionProcessId} or its ReviewProcess not found.");
            }

            var projectId = extractionProcess.ReviewProcess.ProjectId;

            // Leader Authorization Check
            var currentUserIdStr = _currentUserService.GetUserId();
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
            {
                throw new UnauthorizedAccessException("Current user ID is invalid.");
            }

            var currentUserProjectMember = await _unitOfWork.SystematicReviewProjects.GetQueryable()
                .Where(p => p.Id == projectId)
                .SelectMany(p => p.ProjectMembers)
                .FirstOrDefaultAsync(pm => pm.UserId == currentUserId);

            if (currentUserProjectMember == null || currentUserProjectMember.Role != ProjectRole.Leader)
            {
                throw new UnauthorizedAccessException($"User is not authorized. Must be a Leader for project {projectId}.");
            }

            await ValidateReviewerInProjectAsync(projectId, dto.Reviewer1Id);
            await ValidateReviewerInProjectAsync(projectId, dto.Reviewer2Id);

            task.Reviewer1Id = dto.Reviewer1Id;
            task.Reviewer2Id = dto.Reviewer2Id;
            task.Reviewer1Status = ReviewerTaskStatus.InProgress;
            task.Reviewer2Status = ReviewerTaskStatus.InProgress;
            task.Status = PaperExtractionStatus.InProgress;
            task.ModifiedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync();
        }

        private async Task ValidateReviewerInProjectAsync(Guid projectId, Guid? reviewerId)
        {
            if (!reviewerId.HasValue) return;

            var project = await _unitOfWork.SystematicReviewProjects.GetQueryable()
                .Include(p => p.ProjectMembers)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            var pm = project?.ProjectMembers.FirstOrDefault(pm => pm.UserId == reviewerId.Value);

            if (pm == null)
                throw new ArgumentException($"User {reviewerId} is not a member of project {projectId}.");

            // Fix #8: Reviewers must be Members, not Leaders
            if (pm.Role != ProjectRole.Member)
                throw new ArgumentException($"User {reviewerId} must be a Member (not a Leader) to be assigned as a reviewer.");
        }

        public async Task<DataExtractionProcessResponse> StartAsync(Guid extractionProcessId)
        {
            var entity = await _unitOfWork.DataExtractionProcesses.GetQueryable()
                .Include(dp => dp.ReviewProcess)
                .FirstOrDefaultAsync(dp => dp.Id == extractionProcessId);

            if (entity == null)
                throw new InvalidOperationException($"Data extraction process {extractionProcessId} not found.");

            if (entity.Status == ExtractionProcessStatus.Completed)
                throw new InvalidOperationException("Data extraction process already completed.");

            // Fix #4: Leader authorization to start extraction
            var currentUserIdStr = _currentUserService.GetUserId();
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                throw new UnauthorizedAccessException("Current user ID is invalid.");

            var projectId = entity.ReviewProcess.ProjectId;
            var currentUserProjectMember = await _unitOfWork.SystematicReviewProjects.GetQueryable()
                .Where(p => p.Id == projectId)
                .SelectMany(p => p.ProjectMembers)
                .FirstOrDefaultAsync(pm => pm.UserId == currentUserId);

            if (currentUserProjectMember == null || currentUserProjectMember.Role != ProjectRole.Leader)
                throw new UnauthorizedAccessException($"Only Project Leaders can start the data extraction process.");

            if (entity.Status == ExtractionProcessStatus.NotStarted)
            {
                entity.Status = ExtractionProcessStatus.InProgress;
                entity.StartedAt ??= DateTimeOffset.UtcNow;
                entity.ModifiedAt = DateTimeOffset.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                await SyncEligiblePapersAsync(extractionProcessId);
            }

            return new DataExtractionProcessResponse
            {
                Id = entity.Id,
                ReviewProcessId = entity.ReviewProcessId,
                Status = entity.Status,
                StatusText = entity.Status.ToString(),
                StartedAt = entity.StartedAt,
                CompletedAt = entity.CompletedAt,
                Notes = entity.Notes,
                CreatedAt = entity.CreatedAt,
                ModifiedAt = entity.ModifiedAt
            };
        }

        public async Task SubmitExtractionAsync(Guid extractionProcessId, Guid paperId, SubmitExtractionRequestDto request)
        {
            // Authorization & Validation
            var currentUserIdStr = _currentUserService.GetUserId();
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
            {
                throw new UnauthorizedAccessException("Current user ID is invalid.");
            }

            var task = await _unitOfWork.ExtractionPaperTasks.GetQueryable()
                .FirstOrDefaultAsync(t => t.DataExtractionProcessId == extractionProcessId && t.PaperId == paperId);

            if (task == null)
            {
                throw new InvalidOperationException($"Extraction task for paper {paperId} in process {extractionProcessId} not found.");
            }

            if (currentUserId != task.Reviewer1Id && currentUserId != task.Reviewer2Id)
                throw new UnauthorizedAccessException("User is not authorized to submit extraction for this paper.");

            // Fix #12: Block re-submission on a completed task
            if (task.Status == PaperExtractionStatus.Completed)
                throw new InvalidOperationException("Cannot re-submit extraction for a task that is already completed.");

            // Data Upsert Logic
            var existingExtractedDataValues = await _unitOfWork.ExtractedDataValues.FindAllAsync(e => e.PaperId == paperId && e.ReviewerId == currentUserId);

            if (existingExtractedDataValues != null && existingExtractedDataValues.Any())
            {
                foreach (var val in existingExtractedDataValues)
                {
                    await _unitOfWork.ExtractedDataValues.RemoveAsync(val);
                }
            }

            var newExtractedValues = request.Values.Select(v => new ExtractedDataValue
            {
                Id = Guid.NewGuid(),
                PaperId = paperId,
                FieldId = v.FieldId,
                ReviewerId = currentUserId,
                OptionId = v.OptionId,
                StringValue = v.StringValue,
                NumericValue = v.NumericValue,
                BooleanValue = v.BooleanValue,
                MatrixColumnId = v.MatrixColumnId,
                MatrixRowIndex = v.MatrixRowIndex,
                IsConsensusFinal = false,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            }).ToList();

            await _unitOfWork.ExtractedDataValues.AddRangeAsync(newExtractedValues);

            // Status Update Logic
            if (currentUserId == task.Reviewer1Id)
            {
                task.Reviewer1Status = ReviewerTaskStatus.Completed;
            }
            else if (currentUserId == task.Reviewer2Id)
            {
                task.Reviewer2Status = ReviewerTaskStatus.Completed;
            }

            // Define Double Extraction vs Single Extraction logic
            // Based on instructions: task matches Double if there's both Reviewer1 and Reviewer2 assigned
            bool isDoubleExtraction = task.Reviewer1Id.HasValue && task.Reviewer2Id.HasValue;

            if (isDoubleExtraction)
            {
                if (task.Reviewer1Status == ReviewerTaskStatus.Completed && task.Reviewer2Status == ReviewerTaskStatus.Completed)
                {
                    task.Status = PaperExtractionStatus.AwaitingConsensus;
                }
                else
                {
                    task.Status = PaperExtractionStatus.InProgress;
                }
            }
            else
            {
                // Single extraction
                task.Status = PaperExtractionStatus.Completed;
            }

            task.ModifiedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<ConsensusWorkspaceDto> GetConsensusWorkspaceAsync(Guid extractionProcessId, Guid paperId)
        {
            // Verify extraction task
            var task = await _unitOfWork.ExtractionPaperTasks.GetQueryable()
                .FirstOrDefaultAsync(t => t.DataExtractionProcessId == extractionProcessId && t.PaperId == paperId);

            if (task == null)
                throw new InvalidOperationException($"Extraction task for paper {paperId} not found.");

            if (task.Status != PaperExtractionStatus.AwaitingConsensus && task.Status != PaperExtractionStatus.Completed)
                throw new InvalidOperationException($"Task is not in a valid state to view consensus. Current status: {task.Status}");

            // Determine extraction mode
            bool isDirectExtraction = task.AdjudicatorId.HasValue && !task.Reviewer1Id.HasValue && !task.Reviewer2Id.HasValue;
            bool isDoubleBlind = task.Reviewer1Id.HasValue && task.Reviewer2Id.HasValue;

            if (!isDirectExtraction && !isDoubleBlind)
                throw new InvalidOperationException("Cannot view consensus: task is neither a valid double-blind nor a direct extraction.");

            // Get protocol ID
            var extractionProcess = await _unitOfWork.DataExtractionProcesses.GetQueryable()
                .Include(dp => dp.ReviewProcess)
                .FirstOrDefaultAsync(dp => dp.Id == extractionProcessId);

            if (extractionProcess?.ReviewProcess?.ProtocolId == null)
                throw new InvalidOperationException("Protocol not found for this extraction process.");

            var protocolId = extractionProcess.ReviewProcess.ProtocolId.Value;

            var templateList = await _unitOfWork.ExtractionTemplates.FindAllAsync(t => t.ProtocolId == protocolId);
            var templateEntity = templateList.FirstOrDefault();

            if (templateEntity == null)
                throw new InvalidOperationException("Extraction template not found for this protocol.");

            var template = await _unitOfWork.ExtractionTemplates.GetByIdWithFieldsAsync(templateEntity.Id);

            if (template == null)
                throw new InvalidOperationException("Extraction template not found.");

            // Safe reviewer IDs: fall back to Guid.Empty for direct extraction (no-op in MapConsensusField)
            var r1Id = task.Reviewer1Id ?? Guid.Empty;
            var r2Id = task.Reviewer2Id ?? Guid.Empty;

            // Fetch extracted answers — for direct extraction only load final consensus records
            IEnumerable<ExtractedDataValue> extractedValues;
            if (isDirectExtraction)
            {
                extractedValues = await _unitOfWork.ExtractedDataValues.FindAllAsync(e =>
                    e.PaperId == paperId && e.IsConsensusFinal == true);
            }
            else
            {
                extractedValues = await _unitOfWork.ExtractedDataValues.FindAllAsync(e =>
                    e.PaperId == paperId &&
                    (e.ReviewerId == r1Id || e.ReviewerId == r2Id || e.IsConsensusFinal == true));
            }

            var valuesList = extractedValues.ToList();

            var dto = new ConsensusWorkspaceDto
            {
                PaperId = paperId,
                TemplateId = template.Id,
                Reviewer1Id = r1Id,
                Reviewer2Id = r2Id,
                Sections = template.Sections.OrderBy(s => s.OrderIndex).Select(s => new ConsensusSectionDto
                {
                    SectionId = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    SectionType = (int)s.SectionType,
                    OrderIndex = s.OrderIndex,
                    MatrixColumns = s.MatrixColumns?.OrderBy(c => c.OrderIndex).Select(c => new ExtractionMatrixColumnDto
                    {
                        ColumnId = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        OrderIndex = c.OrderIndex
                    }).ToList() ?? new List<ExtractionMatrixColumnDto>(),
                    Fields = s.Fields.Where(f => f.ParentFieldId == null).OrderBy(f => f.OrderIndex)
                        .Select(f => MapConsensusField(f, valuesList, r1Id, r2Id))
                        .ToList()
                }).ToList()
            };

            return dto;
        }

        private ConsensusFieldDto MapConsensusField(ExtractionField field, List<ExtractedDataValue> allValues, Guid r1Id, Guid r2Id)
        {
            var fieldDto = new ConsensusFieldDto
            {
                FieldId = field.Id,
                Name = field.Name,
                Instruction = field.Instruction,
                FieldType = (int)field.FieldType,
                IsRequired = field.IsRequired,
                OrderIndex = field.OrderIndex,
                Options = field.Options?.OrderBy(o => o.DisplayOrder).Select(o => new FieldOptionDto
                {
                    OptionId = o.Id,
                    FieldId = o.FieldId,
                    Value = o.Value,
                    DisplayOrder = o.DisplayOrder
                }).ToList() ?? new List<FieldOptionDto>(),
                SubFields = field.SubFields?.OrderBy(sf => sf.OrderIndex)
                    .Select(sf => MapConsensusField(sf, allValues, r1Id, r2Id))
                    .ToList() ?? new List<ConsensusFieldDto>()
            };

            // Map answers
            // Filter values for this field
            var fieldValues = allValues.Where(v => v.FieldId == field.Id).ToList();

            // Group by matrix coords (ColumnId, RowIndex). For flat forms these will be null.
            var groupedValues = fieldValues
                .GroupBy(v => new { v.MatrixColumnId, v.MatrixRowIndex })
                .ToList();

            foreach (var group in groupedValues)
            {
                // For MultiSelect, there might be multiple records per reviewer.
                var r1Records = group.Where(v => v.ReviewerId == r1Id && v.IsConsensusFinal != true).ToList();
                var r2Records = group.Where(v => v.ReviewerId == r2Id && v.IsConsensusFinal != true).ToList();
                var finalRecords = group.Where(v => v.IsConsensusFinal == true).ToList();

                fieldDto.Answers.Add(new ExtractedAnswerDto
                {
                    MatrixColumnId = group.Key.MatrixColumnId,
                    MatrixRowIndex = group.Key.MatrixRowIndex,
                    Reviewer1Answer = BuildAnswerDetail(r1Records, field),
                    Reviewer2Answer = BuildAnswerDetail(r2Records, field),
                    FinalAnswer = BuildAnswerDetail(finalRecords, field)
                });
            }

            // If no answers yet, maybe add an empty placeholder if needed, but UI can handle empty arrays.

            return fieldDto;
        }

        private AnswerDetailDto? BuildAnswerDetail(List<ExtractedDataValue> records, ExtractionField field)
        {
            if (!records.Any()) return null;

            // Handle MultiSelect which has multiple records
            if (field.FieldType == FieldType.MultiSelect)
            {
                // StringValue might hold the raw options, but OptionId is cleaner
                var optionIds = records.Where(r => r.OptionId.HasValue).Select(r => r.OptionId!.Value).ToList();
                var displayValues = records.Where(r => r.OptionId.HasValue)
                    .Select(r => field.Options?.FirstOrDefault(o => o.Id == r.OptionId!.Value)?.Value)
                    .Where(v => !string.IsNullOrEmpty(v))
                    .ToList();

                return new AnswerDetailDto
                {
                    // For multiselect, we might combine the string values for UI display
                    DisplayValue = string.Join(", ", displayValues),
                    StringValue = string.Join(",", optionIds) // using StringValue temporarily to hold multi-options
                };
            }

            // Single values
            var record = records.First();
            var dto = new AnswerDetailDto
            {
                OptionId = record.OptionId,
                StringValue = record.StringValue,
                NumericValue = record.NumericValue,
                BooleanValue = record.BooleanValue
            };

            // Build DisplayValue based on FieldType
            switch (field.FieldType)
            {
                case FieldType.Text:
                    dto.DisplayValue = record.StringValue;
                    break;
                case FieldType.Integer:
                case FieldType.Decimal:
                    dto.DisplayValue = record.NumericValue?.ToString("G29");
                    break;
                case FieldType.Boolean:
                    dto.DisplayValue = record.BooleanValue.HasValue ? (record.BooleanValue.Value ? "Yes" : "No") : null;
                    break;
                case FieldType.SingleSelect:
                    if (record.OptionId.HasValue)
                    {
                        dto.DisplayValue = field.Options?.FirstOrDefault(o => o.Id == record.OptionId.Value)?.Value;
                    }
                    break;
            }

            return dto;
        }

        public async Task SubmitConsensusAsync(Guid extractionProcessId, Guid paperId, SubmitConsensusRequestDto request)
        {
            // Authorization & Validation
            var currentUserIdStr = _currentUserService.GetUserId();
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
            {
                throw new UnauthorizedAccessException("Current user ID is invalid.");
            }

            var task = await _unitOfWork.ExtractionPaperTasks.GetQueryable()
                .FirstOrDefaultAsync(t => t.DataExtractionProcessId == extractionProcessId && t.PaperId == paperId);

            if (task == null)
                throw new InvalidOperationException($"Extraction task for paper {paperId} not found.");

            if (task.Status != PaperExtractionStatus.AwaitingConsensus && task.Status != PaperExtractionStatus.Completed)
                throw new InvalidOperationException($"Task is not in a valid state for consensus: {task.Status}");

            // Check Project Leader
            var extractionProcess = await _unitOfWork.DataExtractionProcesses.GetQueryable()
                .Include(dp => dp.ReviewProcess)
                .FirstOrDefaultAsync(dp => dp.Id == extractionProcessId);

            if (extractionProcess?.ReviewProcess == null)
                throw new InvalidOperationException($"DataExtractionProcess or ReviewProcess not found.");

            var projectId = extractionProcess.ReviewProcess.ProjectId;

            var currentUserProjectMember = await _unitOfWork.SystematicReviewProjects.GetQueryable()
                .Where(p => p.Id == projectId)
                .SelectMany(p => p.ProjectMembers)
                .FirstOrDefaultAsync(pm => pm.UserId == currentUserId);

            if (currentUserProjectMember == null || currentUserProjectMember.Role != ProjectRole.Leader)
            {
                throw new UnauthorizedAccessException($"User is not authorized. Must be a Leader for project {projectId}.");
            }

            // Remove old consensus values
            var existingConsensus = await _unitOfWork.ExtractedDataValues.FindAllAsync(e =>
                e.PaperId == paperId && e.IsConsensusFinal == true);

            if (existingConsensus != null && existingConsensus.Any())
            {
                foreach (var val in existingConsensus)
                {
                    await _unitOfWork.ExtractedDataValues.RemoveAsync(val);
                }
            }

            // Add new consensus values
            var consensusValues = request.Values.Select(v => new ExtractedDataValue
            {
                Id = Guid.NewGuid(),
                PaperId = paperId,
                FieldId = v.FieldId,
                ReviewerId = currentUserId, // the adjudicator
                OptionId = v.OptionId,
                StringValue = v.StringValue,
                NumericValue = v.NumericValue,
                BooleanValue = v.BooleanValue,
                MatrixColumnId = v.MatrixColumnId,
                MatrixRowIndex = v.MatrixRowIndex,
                IsConsensusFinal = true,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            }).ToList();

            await _unitOfWork.ExtractedDataValues.AddRangeAsync(consensusValues);

            // Update Task Status
            task.AdjudicatorId = currentUserId;
            task.Status = PaperExtractionStatus.Completed;
            task.ModifiedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<ExtractionPreviewDto> GetPivotedExtractionDataAsync(Guid extractionProcessId)
        {
            // 1. Fetch extraction process and protocol
            var extractionProcess = await _unitOfWork.DataExtractionProcesses.GetQueryable()
                .Include(dp => dp.ReviewProcess)
                .FirstOrDefaultAsync(dp => dp.Id == extractionProcessId);

            if (extractionProcess?.ReviewProcess?.ProtocolId == null)
                throw new InvalidOperationException("Protocol not found for this extraction process.");

            var protocolId = extractionProcess.ReviewProcess.ProtocolId.Value;

            // 2. Fetch Template
            var templateList = await _unitOfWork.ExtractionTemplates.FindAllAsync(t => t.ProtocolId == protocolId);
            var templateEntity = templateList.FirstOrDefault();

            if (templateEntity == null)
                throw new InvalidOperationException("Extraction template not found for this protocol.");

            var template = await _unitOfWork.ExtractionTemplates.GetByIdWithFieldsAsync(templateEntity.Id);

            if (template == null)
                throw new InvalidOperationException("Extraction template not found.");

            // 3. Fetch completed tasks
            var completedTasks = await _unitOfWork.ExtractionPaperTasks.GetQueryable()
                .Include(t => t.Paper)
                .Where(t => t.DataExtractionProcessId == extractionProcessId && t.Status == PaperExtractionStatus.Completed)
                .ToListAsync();

            var completedPaperIds = completedTasks.Select(t => t.PaperId).ToList();

            if (!completedPaperIds.Any())
                throw new InvalidOperationException("No completed papers found for this extraction process to export.");

            // 4. Fetch extracted answers
            var allValues = await _unitOfWork.ExtractedDataValues.FindAllAsync(e => completedPaperIds.Contains(e.PaperId));
            var valuesList = allValues.ToList();

            // 5. Build headers list and column map
            var headers = new List<string> { "Study ID (System)", "Citation", "Title", "Authors", "Year" };
            var colMap = new Dictionary<(Guid FieldId, Guid? MatrixColumnId), string>();

            Action<ExtractionField, List<ExtractionField>> flatten = null!;
            flatten = (f, list) =>
            {
                list.Add(f);
                if (f.SubFields != null && f.SubFields.Any())
                {
                    foreach (var sf in f.SubFields.OrderBy(x => x.OrderIndex))
                        flatten(sf, list);
                }
            };

            foreach (var section in template.Sections.OrderBy(s => s.OrderIndex))
            {
                var orderedFields = section.Fields?.Where(f => f.ParentFieldId == null).OrderBy(f => f.OrderIndex).ToList() ?? new List<ExtractionField>();
                var sectionFields = new List<ExtractionField>();
                foreach (var f in orderedFields)
                {
                    flatten(f, sectionFields);
                }

                if (section.SectionType == SectionType.FlatForm)
                {
                    foreach (var f in sectionFields)
                    {
                        string headerName = $"{section.Name} - {f.Name}";
                        headers.Add(headerName);
                        colMap[(f.Id, null)] = headerName;
                    }
                }
                else if (section.SectionType == SectionType.MatrixGrid)
                {
                    var matrixColumns = section.MatrixColumns?.OrderBy(c => c.OrderIndex).ToList() ?? new List<ExtractionMatrixColumn>();
                    foreach (var mc in matrixColumns)
                    {
                        foreach (var f in sectionFields)
                        {
                            string headerName = $"{section.Name} - {mc.Name} - {f.Name}";
                            headers.Add(headerName);
                            colMap[(f.Id, mc.Id)] = headerName;
                        }
                    }
                }
            }

            // 6. Build unified fields dictionary
            var allTemplateFields = template.Sections.SelectMany(s => s.Fields).ToList();
            var allSubFields = allTemplateFields.SelectMany(f => f.SubFields ?? new List<ExtractionField>()).ToList();
            var unifiedFieldsDict = allTemplateFields.Concat(allSubFields).ToDictionary(f => f.Id);

            // 7. Build rows
            var rows = new List<Dictionary<string, string>>();

            foreach (var task in completedTasks)
            {
                var paperValues = valuesList.Where(v => v.PaperId == task.PaperId).ToList();

                // Golden Record filtering
                if (paperValues.Any(v => v.IsConsensusFinal))
                {
                    paperValues = paperValues.Where(v => v.IsConsensusFinal).ToList();
                }
                else
                {
                    paperValues = paperValues.Where(v => v.ReviewerId == task.Reviewer1Id).ToList();
                }

                // Generate Citation string
                string firstAuthor = "Unknown";
                if (!string.IsNullOrWhiteSpace(task.Paper.Authors))
                {
                    var authorParts = task.Paper.Authors.Split(',');
                    if (authorParts.Any() && !string.IsNullOrWhiteSpace(authorParts[0]))
                    {
                        firstAuthor = authorParts[0].Trim();
                    }
                }
                string yearStr = task.Paper.PublicationYearInt?.ToString() ?? "Unknown";
                string citationStr = $"{firstAuthor} et al., {yearStr}";

                var row = new Dictionary<string, string>
                {
                    ["Study ID (System)"] = task.PaperId.ToString(),
                    ["Citation"] = citationStr,
                    ["Title"] = task.Paper.Title ?? "",
                    ["Authors"] = task.Paper.Authors ?? "",
                    ["Year"] = task.Paper.PublicationYearInt?.ToString() ?? ""
                };

                // Group by FieldId and MatrixColumnId, ignoring MatrixRowIndex,
                // so that all data for a single paper collapses into one row.
                var rowGroups = paperValues.GroupBy(v => new { v.FieldId, v.MatrixColumnId }).ToList();

                foreach (var group in rowGroups)
                {
                    if (colMap.TryGetValue((group.Key.FieldId, group.Key.MatrixColumnId), out string? headerName))
                    {
                        if (!unifiedFieldsDict.TryGetValue(group.Key.FieldId, out var field))
                            continue;

                        string valueStr = "";

                        if (field.FieldType == FieldType.MultiSelect)
                        {
                            var optionIds = group.Where(v => v.OptionId.HasValue).Select(v => v.OptionId!.Value).ToList();
                            var optionValues = field.Options?
                                .Where(o => optionIds.Contains(o.Id))
                                .Select(o => o.Value)
                                .ToList() ?? new List<string>();

                            valueStr = string.Join(", ", optionValues);
                        }
                        else
                        {
                            var record = group.First();
                            switch (field.FieldType)
                            {
                                case FieldType.Text:
                                    valueStr = record.StringValue ?? "";
                                    break;
                                case FieldType.Integer:
                                case FieldType.Decimal:
                                    valueStr = record.NumericValue?.ToString("G29") ?? "";
                                    break;
                                case FieldType.Boolean:
                                    valueStr = record.BooleanValue.HasValue ? (record.BooleanValue.Value ? "Yes" : "No") : "";
                                    break;
                                case FieldType.SingleSelect:
                                    if (record.OptionId.HasValue)
                                    {
                                        valueStr = field.Options?.FirstOrDefault(o => o.Id == record.OptionId.Value)?.Value ?? "";
                                    }
                                    break;
                            }
                        }

                        row[headerName] = valueStr;
                    }
                }

                rows.Add(row);
            }

            return new ExtractionPreviewDto
            {
                Headers = headers,
                Rows = rows
            };
        }

        public async Task<byte[]> ExportExtractedDataAsync(Guid extractionProcessId)
        {
            var data = await GetPivotedExtractionDataAsync(extractionProcessId);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Extracted Data");

            // Write headers
            for (int i = 0; i < data.Headers.Count; i++)
            {
                worksheet.Cell(1, i + 1).Value = data.Headers[i];
            }

            // Style headers
            var headerRow = worksheet.Row(1);
            headerRow.Style.Font.Bold = true;

            // Write rows
            for (int r = 0; r < data.Rows.Count; r++)
            {
                var row = data.Rows[r];
                for (int c = 0; c < data.Headers.Count; c++)
                {
                    var header = data.Headers[c];
                    if (row.TryGetValue(header, out var value))
                    {
                        worksheet.Cell(r + 2, c + 1).Value = value;
                    }
                }
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportExtractedDataCsvAsync(Guid extractionProcessId, CancellationToken cancellationToken = default)
        {
            var data = await GetPivotedExtractionDataAsync(extractionProcessId);

            var sb = new System.Text.StringBuilder();

            // Write headers
            var headerLine = string.Join(",", data.Headers.Select(EscapeCsvValue));
            sb.AppendLine(headerLine);

            // Write rows
            foreach (var row in data.Rows)
            {
                var rowValues = data.Headers.Select(h => row.TryGetValue(h, out var val) ? EscapeCsvValue(val) : "");
                sb.AppendLine(string.Join(",", rowValues));
            }

            // Include UTF-8 BOM
            var bom = new byte[] { 0xEF, 0xBB, 0xBF };
            var csvBytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());

            var result = new byte[bom.Length + csvBytes.Length];
            Buffer.BlockCopy(bom, 0, result, 0, bom.Length);
            Buffer.BlockCopy(csvBytes, 0, result, bom.Length, csvBytes.Length);

            return result;
        }

        private string EscapeCsvValue(string? value)
        {
            if (string.IsNullOrEmpty(value)) return "";

            bool requiresQuotes = value.Contains(",") || value.Contains("\"") || value.Contains("\r") || value.Contains("\n");
            
            if (requiresQuotes)
            {
                value = value.Replace("\"", "\"\"");
                return $"\"{value}\"";
            }
            
            return value;
        }

        public async Task ReopenExtractionAsync(Guid extractionProcessId, Guid paperId, ReopenExtractionRequestDto request)
        {
            // 1. Authorization: Ensure the current user is the Project Leader
            var currentUserIdStr = _currentUserService.GetUserId();
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
            {
                throw new UnauthorizedAccessException("Current user ID is invalid.");
            }

            var extractionProcess = await _unitOfWork.DataExtractionProcesses.GetQueryable()
                .Include(dp => dp.ReviewProcess)
                .FirstOrDefaultAsync(dp => dp.Id == extractionProcessId);

            if (extractionProcess?.ReviewProcess == null)
            {
                throw new InvalidOperationException($"DataExtractionProcess {extractionProcessId} or its ReviewProcess not found.");
            }

            var projectId = extractionProcess.ReviewProcess.ProjectId;

            var currentUserProjectMember = await _unitOfWork.SystematicReviewProjects.GetQueryable()
                .Where(p => p.Id == projectId)
                .SelectMany(p => p.ProjectMembers)
                .FirstOrDefaultAsync(pm => pm.UserId == currentUserId);

            if (currentUserProjectMember == null || currentUserProjectMember.Role != ProjectRole.Leader)
            {
                throw new UnauthorizedAccessException($"User is not authorized. Must be a Leader for project {projectId}.");
            }

            // 2. Validation: Verify the ExtractionPaperTask exists
            var task = await _unitOfWork.ExtractionPaperTasks.GetQueryable()
                .FirstOrDefaultAsync(t => t.DataExtractionProcessId == extractionProcessId && t.PaperId == paperId);

            if (task == null)
            {
                throw new InvalidOperationException($"Extraction task for paper {paperId} in process {extractionProcessId} not found.");
            }

            if (task.Status != PaperExtractionStatus.AwaitingConsensus && task.Status != PaperExtractionStatus.Completed && task.Status != PaperExtractionStatus.InProgress)
            {
                throw new InvalidOperationException($"Cannot reopen extraction from status '{task.Status}'.");
            }

            if (request.Target == TargetReviewer.Reviewer1 && task.Reviewer1Status != ReviewerTaskStatus.Completed)
            {
                throw new InvalidOperationException($"Cannot reopen extraction for Reviewer 1. Reviewer 1 is not completed.");
            }

            if (request.Target == TargetReviewer.Reviewer2 && task.Reviewer2Status != ReviewerTaskStatus.Completed)
            {
                throw new InvalidOperationException($"Cannot reopen extraction for Reviewer 2. Reviewer 2 is not completed.");
            }

            if (request.Target == TargetReviewer.Both && task.Reviewer1Status != ReviewerTaskStatus.Completed && task.Reviewer2Status != ReviewerTaskStatus.Completed)
            {
                throw new InvalidOperationException($"Cannot reopen extraction for both Reviewers. Both Reviewers are not completed.");
            }

            // 3. State Reversal
            if (request.Target == TargetReviewer.Direct)
            {
                task.AdjudicatorId = null;
                task.Status = PaperExtractionStatus.NotStarted;
                task.Reviewer1Status = ReviewerTaskStatus.NotStarted;
                task.Reviewer2Status = ReviewerTaskStatus.NotStarted;
            }
            else
            {
                if (request.Target == TargetReviewer.Reviewer1 || request.Target == TargetReviewer.Both)
                {
                    task.Reviewer1Status = ReviewerTaskStatus.InProgress;
                }

                if (request.Target == TargetReviewer.Reviewer2 || request.Target == TargetReviewer.Both)
                {
                    task.Reviewer2Status = ReviewerTaskStatus.InProgress;
                }

                task.Status = PaperExtractionStatus.InProgress;
            }

            // 4. Data Cleanup: Remove consensus data AND the targeted reviewer's data
            var allExtractedValues = await _unitOfWork.ExtractedDataValues
                .FindAllAsync(e => e.PaperId == paperId);

            var valuesToDelete = new List<ExtractedDataValue>();

            // Always delete consensus data
            valuesToDelete.AddRange(allExtractedValues.Where(e => e.IsConsensusFinal == true));

            if (request.Target == TargetReviewer.Direct)
            {
                // Clear the adjudicator's own submitted data
                if (task.AdjudicatorId.HasValue)
                    valuesToDelete.AddRange(allExtractedValues.Where(e => e.ReviewerId == task.AdjudicatorId.Value));
            }
            else
            {
                if (request.Target == TargetReviewer.Reviewer1 || request.Target == TargetReviewer.Both)
                {
                    if (task.Reviewer1Id.HasValue)
                        valuesToDelete.AddRange(allExtractedValues.Where(e =>
                            e.ReviewerId == task.Reviewer1Id.Value && e.IsConsensusFinal == false));
                }

                if (request.Target == TargetReviewer.Reviewer2 || request.Target == TargetReviewer.Both)
                {
                    if (task.Reviewer2Id.HasValue)
                        valuesToDelete.AddRange(allExtractedValues.Where(e =>
                            e.ReviewerId == task.Reviewer2Id.Value && e.IsConsensusFinal == false));
                }
            }

            foreach (var val in valuesToDelete.DistinctBy(v => v.Id))
            {
                await _unitOfWork.ExtractedDataValues.RemoveAsync(val);
            }

            // 5. Save
            task.ModifiedAt = DateTimeOffset.UtcNow;
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<ExtractedValueDto>> AutoExtractWithAiAsync(Guid extractionProcessId, Guid paperId)
        {
            // 1. Fetch paper & PDF URL
            var extractionTask = await _unitOfWork.ExtractionPaperTasks.GetQueryable()
                .Include(t => t.Paper)
                .FirstOrDefaultAsync(t => t.DataExtractionProcessId == extractionProcessId && t.PaperId == paperId);

            if (extractionTask == null)
            {
                throw new InvalidOperationException($"Extraction task for paper {paperId} not found.");
            }

            var pdfUrl = extractionTask.Paper.PdfUrl;
            if (string.IsNullOrWhiteSpace(pdfUrl))
            {
                throw new InvalidOperationException("Paper does not have a valid PdfUrl.");
            }

            // 2. Download PDF
            var httpClient = _httpClientFactory.CreateClient();
            var pdfResponse = await httpClient.GetAsync(pdfUrl);
            if (!pdfResponse.IsSuccessStatusCode)
            {
                throw new InvalidOperationException("Failed to download PDF from url.");
            }
            using var networkStream = await pdfResponse.Content.ReadAsStreamAsync();

            // Copy sang MemoryStream để an toàn tuyệt đối
            using var memoryStream = new MemoryStream();
            await networkStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0; // Đặt con trỏ về đầu file

            // 3. Extract text from Grobid (Dùng memoryStream)
            var paperText = await _grobidService.ProcessFulltextDocumentAsync(memoryStream);

            if (string.IsNullOrWhiteSpace(paperText))
            {
                throw new InvalidOperationException("Failed to extract full text from the PDF using Grobid. Check Backend Console Logs for details.");
            }

            var doc = XDocument.Parse(paperText);

            var sb = new System.Text.StringBuilder();

            // 1. Extract Title
            var title = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "title")?.Value;
            if (!string.IsNullOrWhiteSpace(title)) sb.AppendLine($"TITLE: {title}\n");

            // 2. Extract Authors
            var teiHeaderNode = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "teiHeader");

            var authorNodes = teiHeaderNode != null
                ? teiHeaderNode.Descendants().Where(x => x.Name.LocalName == "author")
                : Enumerable.Empty<XElement>();

            var authorsList = new List<string>();
            foreach (var author in authorNodes)
            {
                var names = author.Descendants()
                                .Where(x => x.Name.LocalName == "forename" || x.Name.LocalName == "surname")
                                .Select(x => x.Value);

                if (names.Any())
                {
                    authorsList.Add(string.Join(" ", names));
                }
            }

            if (authorsList.Any())
            {
                sb.AppendLine($"AUTHORS: {string.Join(", ", authorsList)}\n");
            }

            // 3. Extract Publication Date/Year
            var dateNode = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "date" && x.Attribute("type")?.Value == "published");
            var year = dateNode?.Attribute("when")?.Value ?? dateNode?.Value;
            if (!string.IsNullOrWhiteSpace(year)) sb.AppendLine($"PUBLICATION DATE: {year}\n");

            // 4. Extract Abstract
            var abstractText = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "abstract")?.Value;
            if (!string.IsNullOrWhiteSpace(abstractText)) sb.AppendLine($"ABSTRACT:\n{abstractText}\n");

            // 5. Extract Main Body Text (Only from inside the <text> node)
            sb.AppendLine("MAIN TEXT:");
            var textBodyNodes = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "text")?.Descendants()
                                    .Where(x => x.Name.LocalName == "p" || x.Name.LocalName == "head")
                                    .Select(x => x.Value);

            if (textBodyNodes != null && textBodyNodes.Any())
            {
                sb.AppendLine(string.Join("\n\n", textBodyNodes));
            }

            var cleanPaperText = sb.ToString();

            // 4. Fetch Schema
            var extractionProcess = await _unitOfWork.DataExtractionProcesses.GetQueryable()
                .Include(dp => dp.ReviewProcess)
                .FirstOrDefaultAsync(dp => dp.Id == extractionProcessId);

            if (extractionProcess?.ReviewProcess?.ProtocolId == null)
                throw new InvalidOperationException("Protocol not found.");

            var templateList = await _unitOfWork.ExtractionTemplates.FindAllAsync(t => t.ProtocolId == extractionProcess.ReviewProcess.ProtocolId.Value);
            var templateEntity = templateList.FirstOrDefault();

            if (templateEntity == null)
                throw new InvalidOperationException("Extraction template not found.");

            var template = await _unitOfWork.ExtractionTemplates.GetByIdWithFieldsAsync(templateEntity.Id);
            if (template == null)
                throw new InvalidOperationException("Extraction template details not found.");

            // Flatten and build schema for LLM
            var schemaObj = new
            {
                TemplateName = template.Name,
                Sections = template.Sections.OrderBy(s => s.OrderIndex).Select(s => new
                {
                    SectionName = s.Name,
                    Description = s.Description,
                    SectionType = s.SectionType.ToString(),
                    MatrixColumns = s.MatrixColumns?.OrderBy(c => c.OrderIndex).Select(c => new { c.Id, c.Name }).ToList(),
                    Fields = GetSimplifiedFields(s.Fields.Where(f => f.ParentFieldId == null).OrderBy(f => f.OrderIndex).ToList())
                })
            };

            var schemaJson = JsonSerializer.Serialize(schemaObj, new JsonSerializerOptions { WriteIndented = true });

            // 5. Call Gemini
            var apiKey = _configuration["Gemini:ApiKey"] ?? Environment.GetEnvironmentVariable("Gemini:ApiKey");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("Gemini API key is not configured.");
            }

            Console.WriteLine("clear text: " + cleanPaperText);

            string prompt = $@"
You are an expert academic researcher. 
I will provide you with a PAPER TEXT and an extraction SCHEMA.
Your task is to extract the correct answers from the PAPER TEXT according to the SCHEMA.
For SingleSelect or MultiSelect fields, you MUST map your answer to the exact 'OptionId' provided in the schema.
You MUST return ONLY a JSON array that exactly matches the following structure:
[
  {{
    ""FieldId"": ""uuid"",
    ""OptionId"": ""uuid or null"",
    ""StringValue"": ""extracted text or null"",
    ""NumericValue"": decimal or null,
    ""BooleanValue"": boolean or null,
    ""MatrixColumnId"": ""uuid or null"",
    ""MatrixRowIndex"": int or null
  }}
]
Do not include any other markdown formatting like ```json or comments. 
If no data is found for a field, omit it or leave values as null.

=== SCHEMA ===
{schemaJson}

=== PAPER TEXT ===
{cleanPaperText}
";

            var requestPayload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    responseMimeType = "application/json"
                }
            };

            var geminiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";
            var geminiResponse = await httpClient.PostAsJsonAsync(geminiUrl, requestPayload);

            if (!geminiResponse.IsSuccessStatusCode)
            {
                var errorStr = await geminiResponse.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Gemini API error: {errorStr}");
            }

            var geminiResult = await geminiResponse.Content.ReadFromJsonAsync<JsonDocument>();
            try
            {
                var responseText = geminiResult?.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text").GetString();

                if (string.IsNullOrWhiteSpace(responseText))
                    return new List<ExtractedValueDto>();

                responseText = responseText.Trim();
                if (responseText.StartsWith("```json"))
                {
                    responseText = responseText.Substring(7);
                }
                if (responseText.StartsWith("```"))
                {
                    responseText = responseText.Substring(3);
                }
                if (responseText.EndsWith("```"))
                {
                    responseText = responseText.Substring(0, responseText.Length - 3);
                }
                responseText = responseText.Trim();

                var extractedValues = JsonSerializer.Deserialize<List<ExtractedValueDto>>(responseText, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // var extractedValues = JsonSerializer.Deserialize<List<ExtractedValueDto>>(responseText, new JsonSerializerOptions
                // {
                //     PropertyNameCaseInsensitive = true
                // });

                return extractedValues ?? new List<ExtractedValueDto>();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to parse Gemini response.", ex);
            }
        }

        public async Task<ExtractedValueDto?> AskAiSingleFieldAsync(Guid extractionProcessId, AskAiFieldRequestDto request, CancellationToken cancellationToken = default)
        {
            var extractionTask = await _unitOfWork.ExtractionPaperTasks.GetQueryable()
                .Include(t => t.Paper)
                .FirstOrDefaultAsync(t => t.DataExtractionProcessId == extractionProcessId && t.PaperId == request.PaperId, cancellationToken);

            if (extractionTask == null)
                throw new InvalidOperationException($"Extraction task for paper {request.PaperId} not found.");

            // string searchQuestion = $"{request.FieldName}. {request.FieldInstruction}".Trim();

            string searchQuestion = request.FieldName;
            if (!string.IsNullOrWhiteSpace(request.FieldInstruction))
            {
                searchQuestion += $". {request.FieldInstruction}";
            }
            searchQuestion += ". Identify related methods, techniques, approaches, or strategies.";

            var relevantChunks = await _ragRetrievalService.GetRelevantChunksAsync(request.PaperId, searchQuestion, topK: 12, cancellationToken);
            if (relevantChunks == null || !relevantChunks.Any())
                return null;

            var coordinatesList = new List<object>();
            var textContextBuilder = new System.Text.StringBuilder();

            foreach (var chunk in relevantChunks)
            {
                textContextBuilder.AppendLine(chunk.TextContent);
                textContextBuilder.AppendLine("---");

                if (!string.IsNullOrWhiteSpace(chunk.CoordinatesJson))
                {
                    try
                    {
                        var coords = JsonSerializer.Deserialize<List<object>>(chunk.CoordinatesJson);
                        if (coords != null)
                        {
                            coordinatesList.AddRange(coords);
                        }
                    }
                    catch { /* ignore invalid json */ }
                }
            }

            string combinedCoordinates = JsonSerializer.Serialize(coordinatesList);
            string combinedContext = textContextBuilder.ToString();

            var apiKey = _configuration["Gemini:ApiKey"] ?? Environment.GetEnvironmentVariable("Gemini:ApiKey");
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("Gemini API key is not configured.");

            string optionsInstruction = string.IsNullOrWhiteSpace(request.OptionsJson)
                ? ""
                : $"\nValid Options (JSON format: [OptionId, Value]): {request.OptionsJson}\nYou MUST match your extracted answer to ONE of the OptionIds provided above if applicable.\n";

            string prompt = $@"
You are an expert academic researcher. 
Your task is to extract a SINGLE specific field from the provided PAPER CONTEXT.

Field Name: {request.FieldName}
Field Type: {request.FieldType}
Instructions: {request.FieldInstruction}
{optionsInstruction}

You MUST return ONLY a JSON object that exactly matches the following structure (ExtractedValueDto):
{{
  ""FieldId"": ""{request.FieldId}"",
  ""OptionId"": ""uuid or null"",
  ""StringValue"": ""extracted text or null"",
  ""NumericValue"": decimal or null,
  ""BooleanValue"": boolean or null
}}

Do not include any other markdown formatting like ```json or comments. 
If no relevant data is found in the context, return a JSON object with null values for OptionId, StringValue, NumericValue, and BooleanValue.

=== PAPER CONTEXT ===
{combinedContext}
";

            var requestPayload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[] { new { text = prompt } }
                    }
                },
                generationConfig = new { responseMimeType = "application/json" }
            };

            var httpClient = _httpClientFactory.CreateClient();
            var geminiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";
            var geminiResponse = await httpClient.PostAsJsonAsync(geminiUrl, requestPayload, cancellationToken);

            if (!geminiResponse.IsSuccessStatusCode)
            {
                var errorStr = await geminiResponse.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException($"Gemini API error: {errorStr}");
            }

            var geminiResult = await geminiResponse.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: cancellationToken);
            try
            {
                var responseText = geminiResult?.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text").GetString();

                if (string.IsNullOrWhiteSpace(responseText))
                    return null;

                responseText = responseText.Trim();
                if (responseText.StartsWith("```json")) responseText = responseText.Substring(7);
                if (responseText.StartsWith("```")) responseText = responseText.Substring(3);
                if (responseText.EndsWith("```")) responseText = responseText.Substring(0, responseText.Length - 3);
                responseText = responseText.Trim();

                var extractedValue = JsonSerializer.Deserialize<ExtractedValueDto>(responseText, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (extractedValue != null)
                {
                    extractedValue.EvidenceCoordinates = combinedCoordinates;
                    extractedValue.MatrixColumnId = request.MatrixColumnId;
                    extractedValue.MatrixRowIndex = request.MatrixRowIndex;
                }

                return extractedValue;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to parse Gemini response for single field.", ex);
            }
        }

        public async Task DirectExtractByLeaderAsync(Guid extractionProcessId, Guid paperId, SubmitExtractionRequestDto payload, CancellationToken cancellationToken)
        {
            // 1. Resolve & validate current user
            var currentUserIdStr = _currentUserService.GetUserId();
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                throw new UnauthorizedAccessException("Current user ID is invalid.");

            // 2. Load the extraction process to get the project context
            var extractionProcess = await _unitOfWork.DataExtractionProcesses.GetQueryable()
                .Include(dp => dp.ReviewProcess)
                .FirstOrDefaultAsync(dp => dp.Id == extractionProcessId, cancellationToken);

            if (extractionProcess?.ReviewProcess == null)
                throw new InvalidOperationException($"DataExtractionProcess {extractionProcessId} or its ReviewProcess not found.");

            var projectId = extractionProcess.ReviewProcess.ProjectId;

            // 3. Authorization: current user must be a Project Leader
            var currentUserProjectMember = await _unitOfWork.SystematicReviewProjects.GetQueryable()
                .Where(p => p.Id == projectId)
                .SelectMany(p => p.ProjectMembers)
                .FirstOrDefaultAsync(pm => pm.UserId == currentUserId, cancellationToken);

            if (currentUserProjectMember == null || currentUserProjectMember.Role != ProjectRole.Leader)
                throw new UnauthorizedAccessException($"User is not authorized. Must be a Leader for project {projectId}.");

            // 4. Find the ExtractionPaperTask for this paper
            var task = await _unitOfWork.ExtractionPaperTasks.GetQueryable()
                .FirstOrDefaultAsync(t => t.DataExtractionProcessId == extractionProcessId && t.PaperId == paperId, cancellationToken);

            if (task == null)
                throw new InvalidOperationException($"Extraction task for paper {paperId} in process {extractionProcessId} not found.");

            if (task.Status == PaperExtractionStatus.Completed)
                throw new InvalidOperationException($"Extraction task for paper {paperId} is already Completed. Use Reopen to make changes.");

            // 5. Replace-semantics: remove any existing consensus values for this paper
            var existingConsensusFinalValues = await _unitOfWork.ExtractedDataValues.FindAllAsync(
                e => e.PaperId == paperId && e.IsConsensusFinal == true);

            if (existingConsensusFinalValues != null && existingConsensusFinalValues.Any())
            {
                foreach (var val in existingConsensusFinalValues)
                    await _unitOfWork.ExtractedDataValues.RemoveAsync(val);
            }

            // 6. Persist the payload directly as final consensus records (IsConsensusFinal = true)
            var directValues = payload.Values.Select(v => new ExtractedDataValue
            {
                Id = Guid.NewGuid(),
                PaperId = paperId,
                FieldId = v.FieldId,
                ReviewerId = currentUserId,   // the adjudicating leader
                OptionId = v.OptionId,
                StringValue = v.StringValue,
                NumericValue = v.NumericValue,
                BooleanValue = v.BooleanValue,
                MatrixColumnId = v.MatrixColumnId,
                MatrixRowIndex = v.MatrixRowIndex,
                IsConsensusFinal = true,       // CRITICAL: marks as final, bypassing reviewers
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            }).ToList();

            await _unitOfWork.ExtractedDataValues.AddRangeAsync(directValues);

            // 7. Force task to Completed, bypassing InProgress / AwaitingConsensus
            task.AdjudicatorId = currentUserId;
            task.Reviewer1Status = ReviewerTaskStatus.Completed;
            task.Reviewer2Status = ReviewerTaskStatus.Completed;
            task.Status = PaperExtractionStatus.Completed;
            task.ModifiedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync();
        }

        private object GetSimplifiedFields(IEnumerable<ExtractionField> fields)
        {
            var result = new List<object>();
            foreach (var f in fields)
            {
                result.Add(new
                {
                    f.Id,
                    f.Name,
                    f.Instruction,
                    FieldType = f.FieldType.ToString(),
                    Options = f.Options?.OrderBy(o => o.DisplayOrder).Select(o => new { o.Id, o.Value }).ToList(),
                    SubFields = f.SubFields != null && f.SubFields.Any() ? GetSimplifiedFields(f.SubFields.OrderBy(sf => sf.OrderIndex)) : null
                });
            }
            return result;
        }

        public async Task<ExtractionWorkloadSummaryDto> GetWorkloadSummaryAsync(Guid extractionProcessId, CancellationToken cancellationToken)
        {
            // 1. Resolve current user
            var currentUserIdStr = _currentUserService.GetUserId();
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                throw new UnauthorizedAccessException("Current user ID is invalid.");

            // 2. Resolve the project tied to this extraction process
            var extractionProcess = await _unitOfWork.DataExtractionProcesses.GetQueryable()
                .Include(dp => dp.ReviewProcess)
                .FirstOrDefaultAsync(dp => dp.Id == extractionProcessId, cancellationToken);

            if (extractionProcess?.ReviewProcess == null)
                throw new InvalidOperationException($"DataExtractionProcess {extractionProcessId} or its ReviewProcess not found.");

            var projectId = extractionProcess.ReviewProcess.ProjectId;

            // 3. Validate membership and determine role
            var allMembers = await _unitOfWork.SystematicReviewProjects.GetQueryable()
                .Where(p => p.Id == projectId)
                .SelectMany(p => p.ProjectMembers)
                .Include(pm => pm.User)
                .ToListAsync(cancellationToken);

            var currentMember = allMembers.FirstOrDefault(pm => pm.UserId == currentUserId);
            if (currentMember == null)
                throw new UnauthorizedAccessException($"User is not a member of project {projectId}.");

            var isLeader = currentMember.Role == ProjectRole.Leader;

            // 4. Build userId → display name lookup from project members
            var userNameLookup = allMembers.ToDictionary(
                pm => pm.UserId,
                pm => string.IsNullOrWhiteSpace(pm.User?.FullName) ? pm.User?.Username ?? pm.UserId.ToString() : pm.User.FullName);

            // 5. Fetch all ExtractionPaperTask records for this process
            var allTasks = await _unitOfWork.ExtractionPaperTasks.GetQueryable()
                .Where(t => t.DataExtractionProcessId == extractionProcessId)
                .ToListAsync(cancellationToken);

            // 6. Compute global stats
            int totalPapers = allTasks.Count;
            int fullyCompleted = allTasks.Count(t => t.Status == PaperExtractionStatus.Completed);
            double overallPct = totalPapers == 0 ? 0 : Math.Round((double)fullyCompleted / totalPapers * 100, 2);

            // 7. Per-reviewer stats using efficient LINQ grouping
            // A reviewer contributes to a task if they are Reviewer1 OR Reviewer2.
            // We emit two "participation records" per task when both slots are filled.
            var participations = allTasks
                .SelectMany(t =>
                {
                    var slots = new List<(Guid ReviewerId, ReviewerTaskStatus Status)>();
                    if (t.Reviewer1Id.HasValue)
                        slots.Add((t.Reviewer1Id.Value, t.Reviewer1Status));
                    if (t.Reviewer2Id.HasValue)
                        slots.Add((t.Reviewer2Id.Value, t.Reviewer2Status));
                    return slots;
                })
                .GroupBy(x => x.ReviewerId)
                .Select(g => new ReviewerWorkloadDto
                {
                    ReviewerId = g.Key,
                    ReviewerName = userNameLookup.TryGetValue(g.Key, out var name) ? name : g.Key.ToString(),
                    TotalAssigned = g.Count(),
                    Completed = g.Count(x => x.Status == ReviewerTaskStatus.Completed),
                    InProgress = g.Count(x => x.Status == ReviewerTaskStatus.InProgress),
                    NotStarted = g.Count(x => x.Status == ReviewerTaskStatus.NotStarted)
                })
                .ToList();

            // 8. Role-based filtering: Members see only their own workload
            List<ReviewerWorkloadDto> reviewerWorkloads;
            if (isLeader)
            {
                reviewerWorkloads = participations;
            }
            else
            {
                var myWorkload = participations.FirstOrDefault(r => r.ReviewerId == currentUserId);
                reviewerWorkloads = myWorkload is not null ? new List<ReviewerWorkloadDto> { myWorkload } : new List<ReviewerWorkloadDto>();
            }

            return new ExtractionWorkloadSummaryDto
            {
                TotalPapers = totalPapers,
                FullyCompletedPapers = fullyCompleted,
                OverallProgressPercentage = overallPct,
                ReviewerWorkloads = reviewerWorkloads
            };
        }
        public async Task CompleteAsync(Guid extractionProcessId, CancellationToken cancellationToken)
        {
            // 1. Fetch Process
            var extractionProcess = await _unitOfWork.DataExtractionProcesses.GetQueryable()
                .Include(dp => dp.ReviewProcess)
                .FirstOrDefaultAsync(dp => dp.Id == extractionProcessId, cancellationToken);

            if (extractionProcess?.ReviewProcess == null)
            {
                throw new InvalidOperationException($"DataExtractionProcess {extractionProcessId} or its ReviewProcess not found.");
            }

            var projectId = extractionProcess.ReviewProcess.ProjectId;

            // 2. Authorization
            var currentUserIdStr = _currentUserService.GetUserId();
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
            {
                throw new UnauthorizedAccessException("Current user ID is invalid.");
            }

            var currentUserProjectMember = await _unitOfWork.SystematicReviewProjects.GetQueryable()
                .Where(p => p.Id == projectId)
                .SelectMany(p => p.ProjectMembers)
                .FirstOrDefaultAsync(pm => pm.UserId == currentUserId, cancellationToken);

            if (currentUserProjectMember == null || currentUserProjectMember.Role != ProjectRole.Leader)
            {
                throw new UnauthorizedAccessException($"User is not authorized. Must be a Leader for project {projectId}.");
            }

            // 3. Validation
            var tasks = await _unitOfWork.ExtractionPaperTasks.GetQueryable()
                .Where(t => t.DataExtractionProcessId == extractionProcessId)
                .ToListAsync(cancellationToken);

            // if (tasks.Any() && tasks.Any(t => t.Status != PaperExtractionStatus.Completed))
            // {
            //     throw new InvalidOperationException("Cannot complete phase. All papers must be completely extracted and resolved.");
            // }

            // 4. Update Status
            extractionProcess.Status = ExtractionProcessStatus.Completed;
            extractionProcess.CompletedAt = DateTimeOffset.UtcNow;
            extractionProcess.ModifiedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<ExtractionEditableGridDto> GetEditableExtractionGridAsync(Guid extractionProcessId)
        {
            // 1. Fetch extraction process and protocol
            var extractionProcess = await _unitOfWork.DataExtractionProcesses.GetQueryable()
                .Include(dp => dp.ReviewProcess)
                .FirstOrDefaultAsync(dp => dp.Id == extractionProcessId);

            if (extractionProcess?.ReviewProcess?.ProtocolId == null)
                throw new InvalidOperationException("Protocol not found for this extraction process.");

            var protocolId = extractionProcess.ReviewProcess.ProtocolId.Value;

            // 2. Fetch Template
            var templateList = await _unitOfWork.ExtractionTemplates.FindAllAsync(t => t.ProtocolId == protocolId);
            var templateEntity = templateList.FirstOrDefault();

            if (templateEntity == null)
                throw new InvalidOperationException("Extraction template not found for this protocol.");

            var template = await _unitOfWork.ExtractionTemplates.GetByIdWithFieldsAsync(templateEntity.Id);

            if (template == null)
                throw new InvalidOperationException("Extraction template not found.");

            // 3. Fetch completed tasks
            var completedTasks = await _unitOfWork.ExtractionPaperTasks.GetQueryable()
                .Include(t => t.Paper)
                .Where(t => t.DataExtractionProcessId == extractionProcessId && t.Status == PaperExtractionStatus.Completed)
                .ToListAsync();

            var completedPaperIds = completedTasks.Select(t => t.PaperId).ToList();

            if (!completedPaperIds.Any())
                throw new InvalidOperationException("No completed papers found for this extraction process to export.");

            // 4. Fetch extracted answers (only IsConsensusFinal)
            var allValues = await _unitOfWork.ExtractedDataValues.FindAllAsync(e => completedPaperIds.Contains(e.PaperId) && e.IsConsensusFinal);
            var valuesList = allValues.ToList();

            // 5. Build columns list and column map
            var columns = new List<ExtractionGridColumnMetaDto>();
            var colMap = new Dictionary<(Guid FieldId, Guid? MatrixColumnId), string>();

            Action<ExtractionField, List<ExtractionField>> flatten = null!;
            flatten = (f, list) =>
            {
                list.Add(f);
                if (f.SubFields != null && f.SubFields.Any())
                {
                    foreach (var sf in f.SubFields.OrderBy(x => x.OrderIndex))
                        flatten(sf, list);
                }
            };

            foreach (var section in template.Sections.OrderBy(s => s.OrderIndex))
            {
                var orderedFields = section.Fields?.Where(f => f.ParentFieldId == null).OrderBy(f => f.OrderIndex).ToList() ?? new List<ExtractionField>();
                var sectionFields = new List<ExtractionField>();
                foreach (var f in orderedFields)
                {
                    flatten(f, sectionFields);
                }

                if (section.SectionType == SectionType.FlatForm)
                {
                    foreach (var f in sectionFields)
                    {
                        string headerName = $"{section.Name} - {f.Name}";
                        var colMeta = new ExtractionGridColumnMetaDto
                        {
                            FieldId = f.Id,
                            HeaderName = headerName,
                            FieldType = f.FieldType.ToString(),
                            Options = f.Options?.OrderBy(o => o.DisplayOrder).Select(o => new GridFieldOptionDto { OptionId = o.Id, Value = o.Value }).ToList() ?? new List<GridFieldOptionDto>()
                        };
                        columns.Add(colMeta);
                        colMap[(f.Id, null)] = headerName;
                    }
                }
                else if (section.SectionType == SectionType.MatrixGrid)
                {
                    var matrixColumns = section.MatrixColumns?.OrderBy(c => c.OrderIndex).ToList() ?? new List<ExtractionMatrixColumn>();
                    foreach (var mc in matrixColumns)
                    {
                        foreach (var f in sectionFields)
                        {
                            string headerName = $"{section.Name} - {mc.Name} - {f.Name}";
                            var colMeta = new ExtractionGridColumnMetaDto
                            {
                                FieldId = f.Id,
                                HeaderName = headerName,
                                FieldType = f.FieldType.ToString(),
                                Options = f.Options?.OrderBy(o => o.DisplayOrder).Select(o => new GridFieldOptionDto { OptionId = o.Id, Value = o.Value }).ToList() ?? new List<GridFieldOptionDto>()
                            };
                            columns.Add(colMeta);
                            colMap[(f.Id, mc.Id)] = headerName;
                        }
                    }
                }
            }

            // 6. Build unified fields dictionary
            var allTemplateFields = template.Sections.SelectMany(s => s.Fields).ToList();
            var allSubFields = allTemplateFields.SelectMany(f => f.SubFields ?? new List<ExtractionField>()).ToList();
            var unifiedFieldsDict = allTemplateFields.Concat(allSubFields).DistinctBy(f => f.Id).ToDictionary(f => f.Id);

            // 7. Build rows
            var rows = new List<ExtractionGridRowDto>();

            foreach (var task in completedTasks)
            {
                var paperValues = valuesList.Where(v => v.PaperId == task.PaperId).ToList();

                var rowGroups = paperValues.GroupBy(v => v.MatrixRowIndex ?? 0).ToList();

                if (!rowGroups.Any())
                {
                    rowGroups = new List<IGrouping<int, ExtractedDataValue>>
                    {
                        new EnumerableQuery<ExtractedDataValue>(new List<ExtractedDataValue>()).GroupBy(x => 0).First()
                    };
                }

                string firstAuthor = "Unknown";
                if (!string.IsNullOrWhiteSpace(task.Paper.Authors))
                {
                    var authorParts = task.Paper.Authors.Split(',');
                    if (authorParts.Any() && !string.IsNullOrWhiteSpace(authorParts[0]))
                    {
                        firstAuthor = authorParts[0].Trim();
                    }
                }
                string yearStr = task.Paper.PublicationYearInt?.ToString() ?? "Unknown";
                string citationStr = $"{firstAuthor} et al., {yearStr}";

                foreach (var group in rowGroups)
                {
                    int rowIndex = group.Key;
                    var rowDto = new ExtractionGridRowDto
                    {
                        RowId = $"{task.PaperId}-{rowIndex}",
                        PaperTitle = task.Paper.Title ?? "",
                        Citation = citationStr,
                        Cells = new Dictionary<string, ExtractionGridCellDto>()
                    };

                    foreach (var headerKvp in colMap)
                    {
                        var fieldId = headerKvp.Key.FieldId;
                        var matrixColumnId = headerKvp.Key.MatrixColumnId;
                        var headerName = headerKvp.Value;

                        if (!unifiedFieldsDict.TryGetValue(fieldId, out var field))
                            continue;

                        var cellValues = group.Where(v => v.FieldId == fieldId && v.MatrixColumnId == matrixColumnId).ToList();

                        string valueStr = "";

                        if (cellValues.Any())
                        {
                            if (field.FieldType == FieldType.MultiSelect)
                            {
                                var optionIds = cellValues.Where(v => v.OptionId.HasValue).Select(v => v.OptionId!.Value).ToList();
                                var optionValues = field.Options?
                                    .Where(o => optionIds.Contains(o.Id))
                                    .Select(o => o.Value)
                                    .ToList() ?? new List<string>();

                                valueStr = string.Join(", ", optionValues);
                            }
                            else
                            {
                                var record = cellValues.First();
                                switch (field.FieldType)
                                {
                                    case FieldType.Text:
                                        valueStr = record.StringValue ?? "";
                                        break;
                                    case FieldType.Integer:
                                    case FieldType.Decimal:
                                        valueStr = record.NumericValue?.ToString() ?? "";
                                        break;
                                    case FieldType.Boolean:
                                        valueStr = record.BooleanValue.HasValue ? (record.BooleanValue.Value ? "Yes" : "No") : "";
                                        break;
                                    case FieldType.SingleSelect:
                                        if (record.OptionId.HasValue)
                                        {
                                            valueStr = field.Options?.FirstOrDefault(o => o.Id == record.OptionId.Value)?.Value ?? "";
                                        }
                                        break;
                                }
                            }
                        }

                        rowDto.Cells[headerName] = new ExtractionGridCellDto
                        {
                            PaperId = task.PaperId,
                            FieldId = fieldId,
                            MatrixColumnId = matrixColumnId,
                            MatrixRowIndex = matrixColumnId.HasValue ? (int?)rowIndex : null,
                            Value = valueStr,
                            FieldType = field.FieldType.ToString()
                        };
                    }
                    rows.Add(rowDto);
                }
            }

            return new ExtractionEditableGridDto
            {
                Columns = columns,
                Rows = rows
            };
        }

        public async Task UpdateGridCellAsync(Guid extractionProcessId, UpdateGridCellRequestDto request)
        {
            var currentUserIdStr = _currentUserService.GetUserId();
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
            {
                throw new UnauthorizedAccessException("Current user ID is invalid.");
            }

            var extractionProcess = await _unitOfWork.DataExtractionProcesses.FindFirstOrDefaultAsync(dp => dp.Id == extractionProcessId);

            if (extractionProcess == null)
            {
                throw new InvalidOperationException($"DataExtractionProcess {extractionProcessId} not found.");
            }

            var reviewProcess = await _unitOfWork.ReviewProcesses.FindFirstOrDefaultAsync(rp => rp.Id == extractionProcess.ReviewProcessId);

            if (reviewProcess == null)
            {
                throw new InvalidOperationException($"ReviewProcess {extractionProcess.ReviewProcessId} not found.");
            }

            var projectId = reviewProcess.ProjectId;

            var allMembers = (await _unitOfWork.SystematicReviewProjects.FindAllAsync(p => p.Id == projectId)).FirstOrDefault()?.ProjectMembers ?? new List<ProjectMember>();

            if (!allMembers.Any())
            {
                // We might need to load members. Let's just do a direct check using SystematicReviewProjects if possible,
                // or just rely on a query we know works. Let's use the explicit check:
            }

            // To be entirely safe and mimic other methods' auth checks:
            var project = await _unitOfWork.SystematicReviewProjects.GetQueryable()
                .Include(p => p.ProjectMembers)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            var currentUserProjectMember = project?.ProjectMembers.FirstOrDefault(pm => pm.UserId == currentUserId);

            if (currentUserProjectMember == null || currentUserProjectMember.Role != ProjectRole.Leader)
            {
                throw new UnauthorizedAccessException($"User is not authorized. Must be a Leader for project {projectId}.");
            }

            var templateList = await _unitOfWork.ExtractionTemplates.FindAllAsync(t => t.ProtocolId == reviewProcess.ProtocolId);
            var templateEntity = templateList.FirstOrDefault();

            if (templateEntity == null)
                throw new InvalidOperationException("Extraction template not found for this protocol.");

            var template = await _unitOfWork.ExtractionTemplates.GetByIdWithFieldsAsync(templateEntity.Id);

            if (template == null)
                throw new InvalidOperationException("Extraction template not found.");

            var allTemplateFields = template.Sections.SelectMany(s => s.Fields).ToList();
            var allSubFields = allTemplateFields.SelectMany(f => f.SubFields ?? new List<ExtractionField>()).ToList();
            var fieldEntity = allTemplateFields.Concat(allSubFields).FirstOrDefault(f => f.Id == request.FieldId);

            if (fieldEntity == null)
                throw new InvalidOperationException($"Field {request.FieldId} not found.");

            var existingRecords = (await _unitOfWork.ExtractedDataValues.FindAllAsync(e =>
                    e.PaperId == request.PaperId &&
                    e.FieldId == request.FieldId &&
                    e.MatrixColumnId == request.MatrixColumnId &&
                    e.MatrixRowIndex == request.MatrixRowIndex &&
                    e.IsConsensusFinal == true)).ToList();

            if (existingRecords.Any())
            {
                await _unitOfWork.ExtractedDataValues.RemoveMultipleAsync(existingRecords);
            }

            if (string.IsNullOrWhiteSpace(request.NewValue))
            {
                await _unitOfWork.SaveChangesAsync();
                return;
            }

            if (fieldEntity.FieldType == FieldType.MultiSelect)
            {
                var optionValues = request.NewValue.Split(',').Select(s => s.Trim());
                var allOptionsForField = fieldEntity.Options ?? new List<FieldOption>();
                var newOptions = new List<ExtractedDataValue>();

                foreach (var optionVal in optionValues)
                {
                    var opt = allOptionsForField.FirstOrDefault(o => o.Value.Equals(optionVal, StringComparison.OrdinalIgnoreCase));
                    if (opt != null)
                    {
                        var newVal = CreateExtractedValue(request, fieldEntity, currentUserId, opt.Id, optionVal, null, null);
                        newOptions.Add(newVal);
                    }
                }

                if (newOptions.Any())
                {
                    await _unitOfWork.ExtractedDataValues.AddRangeAsync(newOptions);
                }
            }
            else
            {
                var newVal = CreateExtractedValue(request, fieldEntity, currentUserId, null, null, null, null);

                switch (fieldEntity.FieldType)
                {
                    case FieldType.Text:
                        newVal.StringValue = request.NewValue;
                        break;
                    case FieldType.Integer:
                    case FieldType.Decimal:
                        if (decimal.TryParse(request.NewValue, out var nValue))
                        {
                            newVal.NumericValue = nValue;
                        }
                        break;
                    case FieldType.Boolean:
                        if (bool.TryParse(request.NewValue, out var bValue) || request.NewValue.Equals("Yes", StringComparison.OrdinalIgnoreCase))
                        {
                            newVal.BooleanValue = request.NewValue.Equals("Yes", StringComparison.OrdinalIgnoreCase) || bValue;
                        }
                        else if (request.NewValue.Equals("No", StringComparison.OrdinalIgnoreCase))
                        {
                            newVal.BooleanValue = false;
                        }
                        break;
                    case FieldType.SingleSelect:
                        var opt = fieldEntity.Options?.FirstOrDefault(o => o.Value.Equals(request.NewValue, StringComparison.OrdinalIgnoreCase));
                        if (opt != null)
                        {
                            newVal.OptionId = opt.Id;
                            newVal.StringValue = request.NewValue;
                        }
                        break;
                }

                await _unitOfWork.ExtractedDataValues.AddAsync(newVal);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        private ExtractedDataValue CreateExtractedValue(UpdateGridCellRequestDto req, ExtractionField field, Guid reviewerId, Guid? optionId, string? stringVal, decimal? numVal, bool? boolVal)
        {
            return new ExtractedDataValue
            {
                Id = Guid.NewGuid(),
                PaperId = req.PaperId,
                FieldId = field.Id,
                ReviewerId = reviewerId,
                OptionId = optionId,
                StringValue = stringVal,
                NumericValue = numVal,
                BooleanValue = boolVal,
                MatrixColumnId = req.MatrixColumnId,
                MatrixRowIndex = req.MatrixRowIndex,
                IsConsensusFinal = true,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };
        }

    }
}
