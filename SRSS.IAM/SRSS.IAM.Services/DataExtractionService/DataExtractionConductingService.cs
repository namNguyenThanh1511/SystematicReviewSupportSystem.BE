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

        public DataExtractionConductingService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IGrobidService grobidService,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _grobidService = grobidService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        private async Task SyncEligiblePapersAsync(Guid extractionProcessId)
        {
            // 1. Lấy thông tin ReviewProcess để biết ID của StudySelectionProcess
            var extractionProcess = await _unitOfWork.DataExtractionProcesses.GetQueryable()
                .Include(dp => dp.ReviewProcess)
                    .ThenInclude(rp => rp.StudySelectionProcess)
                .FirstOrDefaultAsync(dp => dp.Id == extractionProcessId);

            if (extractionProcess?.ReviewProcess?.StudySelectionProcess == null) return;

            var selectionProcessId = extractionProcess.ReviewProcess.StudySelectionProcess.Id;

            // 2. Lấy danh sách PaperId đã PASS vòng Full-Text Screening            
            var eligiblePapers = await _unitOfWork.StudySelectionProcessPapers.FindAllAsync(sr => sr.StudySelectionProcessId == selectionProcessId);

            var eligiblePaperIds = eligiblePapers.Select(sr => sr.PaperId).ToList();

            // 3. Lấy danh sách Task đã tồn tại trong Data Extraction để tránh tạo trùng
            var existingTaskPaperIds = await _unitOfWork.ExtractionPaperTasks.FindAllAsync(t => t.DataExtractionProcessId == extractionProcessId);

            // 4. Tìm ra những Paper mới được pass Screening nhưng chưa có Task ở Extraction
            var newPaperIds = eligiblePaperIds.Where(paperId => !existingTaskPaperIds.Select(t => t.PaperId).Contains(paperId)).ToList();

            if (newPaperIds.Any())
            {
                var newTasks = newPaperIds.Select(paperId => new ExtractionPaperTask
                {
                    DataExtractionProcessId = extractionProcessId,
                    PaperId = paperId,
                    Status = PaperExtractionStatus.NotStarted,
                    Reviewer1Status = ReviewerTaskStatus.NotStarted,
                    Reviewer2Status = ReviewerTaskStatus.NotStarted,
                    CreatedAt = DateTimeOffset.UtcNow
                }).ToList();

                // Lưu hàng loạt xuống DB
                // Lưu ý: Nếu Repo của bạn không có AddRangeAsync, có thể dùng foreach
                await _unitOfWork.ExtractionPaperTasks.AddRangeAsync(newTasks);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task<ExtractionDashboardResponseDto> GetDashboardAsync(Guid extractionProcessId, ExtractionDashboardFilterDto filter)
        {
            await SyncEligiblePapersAsync(extractionProcessId);

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

            if (project == null || !project.ProjectMembers.Any(pm => pm.UserId == reviewerId.Value))
            {
                throw new ArgumentException($"User {reviewerId} is not a member of project {projectId}.");
            }
        }

        public async Task<DataExtractionProcessResponse> StartAsync(Guid extractionProcessId)
        {
            var entity = await _unitOfWork.DataExtractionProcesses.GetQueryable()
                .Include(dp => dp.ReviewProcess)
                .FirstOrDefaultAsync(dp => dp.Id == extractionProcessId);

            if (entity == null)
            {
                throw new InvalidOperationException($"Data extraction process {extractionProcessId} not found.");
            }

            if (entity.Status == ExtractionProcessStatus.Completed)
            {
                throw new InvalidOperationException("Data extraction process already completed.");
            }

            if (entity.Status == ExtractionProcessStatus.NotStarted)
            {
                entity.Status = ExtractionProcessStatus.InProgress;
                entity.StartedAt ??= DateTimeOffset.UtcNow;
                entity.ModifiedAt = DateTimeOffset.UtcNow;
            }

            await _unitOfWork.SaveChangesAsync();

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
            {
                throw new UnauthorizedAccessException("User is not authorized to submit extraction for this paper.");
            }

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

            if (!task.Reviewer1Id.HasValue || !task.Reviewer2Id.HasValue)
                throw new InvalidOperationException("Consensus requires two reviewers.");

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

            // Fetch extracted answers
            var extractedValues = await _unitOfWork.ExtractedDataValues.FindAllAsync(e =>
                e.PaperId == paperId &&
                (e.ReviewerId == task.Reviewer1Id.Value || e.ReviewerId == task.Reviewer2Id.Value || e.IsConsensusFinal == true));

            var valuesList = extractedValues.ToList();

            var dto = new ConsensusWorkspaceDto
            {
                PaperId = paperId,
                TemplateId = template.Id,
                Reviewer1Id = task.Reviewer1Id.Value,
                Reviewer2Id = task.Reviewer2Id.Value,
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
                        .Select(f => MapConsensusField(f, valuesList, task.Reviewer1Id.Value, task.Reviewer2Id.Value))
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
                    dto.DisplayValue = record.NumericValue?.ToString();
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

            if (task.Status != PaperExtractionStatus.AwaitingConsensus && task.Status != PaperExtractionStatus.Completed)
            {
                throw new InvalidOperationException($"Cannot reopen extraction from status '{task.Status}'. Task must be in AwaitingConsensus or Completed status.");
            }

            // 3. State Reversal
            if (request.Target == TargetReviewer.Reviewer1 || request.Target == TargetReviewer.Both)
            {
                task.Reviewer1Status = ReviewerTaskStatus.InProgress;
            }

            if (request.Target == TargetReviewer.Reviewer2 || request.Target == TargetReviewer.Both)
            {
                task.Reviewer2Status = ReviewerTaskStatus.InProgress;
            }

            task.Status = PaperExtractionStatus.InProgress;

            // 4. Data Cleanup: Remove any existing consensus data (IsConsensusFinal == true)
            var consensusValues = await _unitOfWork.ExtractedDataValues.FindAllAsync(e =>
                e.PaperId == paperId && e.IsConsensusFinal == true);

            if (consensusValues != null && consensusValues.Any())
            {
                foreach (var val in consensusValues)
                {
                    await _unitOfWork.ExtractedDataValues.RemoveAsync(val);
                }
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
            var authorNodes = doc.Descendants().Where(x => x.Name.LocalName == "author");
            var authorsList = new List<string>();
            foreach (var author in authorNodes)
            {
                var names = author.Descendants()
                                .Where(x => x.Name.LocalName == "forename" || x.Name.LocalName == "surname")
                                .Select(x => x.Value);
                if (names.Any()) authorsList.Add(string.Join(" ", names));
            }
            if (authorsList.Any()) sb.AppendLine($"AUTHORS: {string.Join(", ", authorsList)}\n");

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
    }
}
