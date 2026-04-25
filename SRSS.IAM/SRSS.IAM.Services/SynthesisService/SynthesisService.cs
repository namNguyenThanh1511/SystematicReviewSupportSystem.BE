using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.Synthesis;
using SRSS.IAM.Services.Mappers;
using SRSS.IAM.Services.UserService;
using Shared.Exceptions;

namespace SRSS.IAM.Services.SynthesisService
{
    public class SynthesisService : ISynthesisService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public SynthesisService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        private async Task EnsureLeaderAsync(Guid projectId)
        {
            var userIdString = _currentUserService.GetUserId();
            if (string.IsNullOrEmpty(userIdString))
            {
                throw new UnauthorizedException("User is not authenticated.");
            }

            var userId = Guid.Parse(userIdString);
            var isLeader = await _unitOfWork.SystematicReviewProjects.IsProjectLeaderAsync(projectId, userId);
            if (!isLeader)
            {
                throw new ForbiddenException("Only project leader can perform this action.");
            }
        }

        // ==================== Data Synthesis Strategies ====================
        public async Task<DataSynthesisStrategyDto> UpsertSynthesisStrategyAsync(DataSynthesisStrategyDto dto)
        {
            var process = await _unitOfWork.SynthesisProcesses.FindSingleAsync(p => p.Id == dto.SynthesisProcessId)
                ?? throw new KeyNotFoundException($"SynthesisProcess {dto.SynthesisProcessId} không tồn tại");

            var reviewProcess = await _unitOfWork.ReviewProcesses.FindSingleAsync(rp => rp.Id == process.ReviewProcessId)
                ?? throw new KeyNotFoundException($"ReviewProcess {process.ReviewProcessId} không tồn tại");

            await EnsureLeaderAsync(reviewProcess.ProjectId);

            DataSynthesisStrategy entity;

            if (dto.SynthesisStrategyId.HasValue && dto.SynthesisStrategyId.Value != Guid.Empty)
            {
                entity = await _unitOfWork.SynthesisStrategies.FindSingleAsync(s => s.Id == dto.SynthesisStrategyId.Value)
                    ?? throw new KeyNotFoundException($"Strategy {dto.SynthesisStrategyId.Value} không tồn tại");

                dto.UpdateEntity(entity);
                await _unitOfWork.SynthesisStrategies.UpdateAsync(entity);
            }
            else
            {
                entity = dto.ToEntity();
                await _unitOfWork.SynthesisStrategies.AddAsync(entity);
            }

            await _unitOfWork.SaveChangesAsync();
            return entity.ToDto();
        }

        public async Task<List<DataSynthesisStrategyDto>> GetSynthesisStrategiesByProcessIdAsync(Guid processId)
        {
            var entities = await _unitOfWork.SynthesisStrategies.GetByProcessIdAsync(processId);
            return entities.ToDtoList();
        }

        public async Task DeleteSynthesisStrategyAsync(Guid strategyId)
        {
            var entity = await _unitOfWork.SynthesisStrategies.FindSingleAsync(s => s.Id == strategyId);
            if (entity != null)
            {
                var process = await _unitOfWork.SynthesisProcesses.FindSingleAsync(p => p.Id == entity.SynthesisProcessId)
                    ?? throw new KeyNotFoundException($"SynthesisProcess {entity.SynthesisProcessId} không tồn tại");

                var reviewProcess = await _unitOfWork.ReviewProcesses.FindSingleAsync(rp => rp.Id == process.ReviewProcessId)
                    ?? throw new KeyNotFoundException($"ReviewProcess {process.ReviewProcessId} không tồn tại");

                await EnsureLeaderAsync(reviewProcess.ProjectId);
                await _unitOfWork.SynthesisStrategies.RemoveAsync(entity);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}