using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Builder;
using Shared.Cache;
using Shared.Exceptions;
using Shared.Models;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.Identification;
using SRSS.IAM.Services.DTOs.Paper;
using SRSS.IAM.Services.DTOs.SemanticScholar;
using SRSS.IAM.Services.IdentificationService;
using SRSS.IAM.Services.UserService;

namespace SRSS.IAM.API.Controllers
{
    [ApiController]
    [Route("api/semantic-scholar/import-workflow")]
    public class SemanticScholarImportWorkflowController : BaseController
    {
        private readonly IIdentificationService _identificationService;
        private readonly IRedisCacheService _redisCacheService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;

        public SemanticScholarImportWorkflowController(
            IIdentificationService identificationService,
            IRedisCacheService redisCacheService,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork)
        {
            _identificationService = identificationService;
            _redisCacheService = redisCacheService;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// API #1 — CREATE IMPORT SESSION (NEW)
        /// Initialize a new import workflow session before RIS upload.
        /// </summary>
        [HttpPost("start")]
        public async Task<ActionResult<ApiResponse<ImportSessionResponse>>> StartImportSession(
            [FromBody] StartImportSessionRequest request)
        {
            var userId = _currentUserService.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException("User not authenticated.");
            }

            var sessionId = Guid.NewGuid();
            var sessionKey = $"import_session:{sessionId}";

            var sessionModel = new ImportSessionRedisModel
            {
                UserId = userId,
                ProjectId = request.ProjectId,
                ValidPaperIds = new List<Guid>(),
                DuplicatePaperIds = new List<Guid>(),
                State = "created",
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _redisCacheService.SetAsync(sessionKey, System.Text.Json.JsonSerializer.Serialize(sessionModel), TimeSpan.FromHours(24));

            return Ok(new ImportSessionResponse
            {
                SessionId = sessionId,
                Status = "Created"
            }, "Import session initialized successfully.");
        }

        /// <summary>
        /// API #2 — UPLOAD RIS (NEW WRAPPER)
        /// Wrap existing RIS import logic + store session result
        /// </summary>
        [HttpPost("ris")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponse<ImportRisWorkflowResponse>>> UploadRis(
            [FromForm] UploadRisWorkflowRequest request,
            CancellationToken ct = default)
        {
            // Step 1: Validate session exists in Redis
            var sessionKey = $"import_session:{request.SessionId}";
            var sessionJson = await _redisCacheService.GetStringAsync(sessionKey);
            if (string.IsNullOrEmpty(sessionJson))
            {
                throw new NotFoundException($"Import session with ID {request.SessionId} not found.");
            }

            var session = System.Text.Json.JsonSerializer.Deserialize<ImportSessionRedisModel>(sessionJson);
            if (session == null)
            {
                throw new InvalidOperationException("Failed to deserialize import session.");
            }

            // Lock per session
            var lockKey = $"import_lock:{request.SessionId}";
            var lockAcquired = await _redisCacheService.SetIfNotExistsAsync(lockKey, "locked", TimeSpan.FromMinutes(30));
            if (!lockAcquired)
            {
                throw new ConflictException("RIS import is already in progress for this session.");
            }

            try
            {
                // Step 2: Call EXISTING service
                if (request.File == null || request.File.Length == 0)
                {
                    throw new ArgumentException("No file uploaded.");
                }

                var extension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
                if (extension != ".ris")
                {
                    throw new ArgumentException("Invalid file format. Only .ris files are accepted.");
                }

                using var stream = request.File.OpenReadStream();
                var result = await _identificationService.ImportRisFileAsync(
                    stream,
                    request.File.FileName,
                    request.SearchSourceId,
                    session.ProjectId,
                    ct);

                // Step 3: Extract result
                // validPaperIds = ImportedPaperIds (all papers added in this batch)
                // duplicatePaperIds = subset of ImportedPaperIds where IsDuplicated is true
                var importedPaperIds = result.ImportedPaperIds;

                var duplicatePaperIds = await _unitOfWork.Papers.GetQueryable()
                    .Where(p => importedPaperIds.Contains(p.Id) && p.IsDuplicated)
                    .Select(p => p.Id)
                    .ToListAsync(ct);

                // Step 4: Update Redis session
                var paperMappings = await _unitOfWork.Papers.GetQueryable()
                    .Where(p => importedPaperIds.Contains(p.Id))
                    .Select(p => new PaperImportMappingDto
                    {
                        InternalId = p.Id,
                        SourceId = p.SourceRecordId
                    })
                    .ToListAsync(ct);

                session.ValidPaperIds = importedPaperIds;
                session.DuplicatePaperIds = duplicatePaperIds;
                session.PaperMappings = paperMappings;
                session.State = "completed";

                await _redisCacheService.SetAsync(sessionKey, System.Text.Json.JsonSerializer.Serialize(session), TimeSpan.FromHours(24));

                // Step 5: Return
                return Ok(new ImportRisWorkflowResponse
                {
                    SessionId = request.SessionId,
                    ValidPaperIds = session.ValidPaperIds,
                    DuplicatePaperIds = session.DuplicatePaperIds,
                    PaperMappings = session.PaperMappings
                }, "RIS file imported and session updated.");
            }
            finally
            {
                await _redisCacheService.RemoveAsync(lockKey);
            }
        }

        /// <summary>
        /// API #3 — GET SESSION STATUS
        /// </summary>
        [HttpGet("{sessionId}")]
        public async Task<ActionResult<ApiResponse<ImportSessionStatusResponse>>> GetSessionStatus(Guid sessionId)
        {
            var sessionKey = $"import_session:{sessionId}";
            var sessionJson = await _redisCacheService.GetStringAsync(sessionKey);
            if (string.IsNullOrEmpty(sessionJson))
            {
                throw new NotFoundException($"Import session with ID {sessionId} not found.");
            }

            var session = System.Text.Json.JsonSerializer.Deserialize<ImportSessionRedisModel>(sessionJson);
            if (session == null)
            {
                throw new InvalidOperationException("Failed to deserialize import session.");
            }

            return Ok(new ImportSessionStatusResponse
            {
                ValidPaperIds = session.ValidPaperIds,
                DuplicatePaperIds = session.DuplicatePaperIds,
                PaperMappings = session.PaperMappings,
                State = session.State
            }, "Session status retrieved successfully.");
        }

    }
}
