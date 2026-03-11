using System.Text.Json;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.Protocol;
using SRSS.IAM.Services.Mappers;

namespace SRSS.IAM.Services.ProtocolService
{
	public class ProtocolService : IProtocolService
	{
		private readonly IUnitOfWork _unitOfWork;

		public ProtocolService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task ApproveProtocolAsync(Guid protocolId, Guid currentUserId)
		{
			var protocol = await _unitOfWork.Protocols.FindSingleAsync(p => p.Id == protocolId)
				?? throw new KeyNotFoundException($"Protocol {protocolId} không tồn tại");

			protocol.Approve(currentUserId);
			await _unitOfWork.Protocols.UpdateAsync(protocol);
			await _unitOfWork.SaveChangesAsync();
		}

		public async Task RejectProtocolAsync(Guid protocolId, Guid currentUserId, string? reason = null)
		{
			var protocol = await _unitOfWork.Protocols.FindSingleAsync(p => p.Id == protocolId)
				?? throw new KeyNotFoundException($"Protocol {protocolId} không tồn tại");

			await _unitOfWork.Protocols.UpdateAsync(protocol);

			protocol.Reject(currentUserId, reason);
			await _unitOfWork.SaveChangesAsync();
		}
		public async Task SubmitForReviewAsync(Guid protocolId)
		{
			var protocol = await _unitOfWork.Protocols.FindSingleAsync(p => p.Id == protocolId)
				?? throw new KeyNotFoundException($"Protocol {protocolId} không tồn tại");

			protocol.SubmitForReview();
			await _unitOfWork.Protocols.UpdateAsync(protocol);
			await _unitOfWork.SaveChangesAsync();
		}

		public async Task<ProtocolDetailResponse> CreateProtocolAsync(CreateProtocolRequest request)
		{
			var projectExists = await _unitOfWork.SystematicReviewProjects.AnyAsync(p => p.Id == request.ProjectId);
			if (!projectExists)
			{
				throw new KeyNotFoundException($"Project với ID {request.ProjectId} không tồn tại");
			}
			var protocol = new ReviewProtocol
			{
				ProjectId = request.ProjectId,
				ProtocolVersion = request.ProtocolVersion,
				Status = ProtocolStatus.Draft
			};

			await _unitOfWork.Protocols.AddAsync(protocol);
			await _unitOfWork.SaveChangesAsync();

			return await GetProtocolByIdAsync(protocol.Id);
		}

		public async Task DeleteProtocolAsync(Guid protocolId)
		{
			var protocol = await _unitOfWork.Protocols.FindSingleAsync(p => p.Id == protocolId)
		?? throw new KeyNotFoundException($"Protocol {protocolId} không tồn tại");

			protocol.SoftDelete();
			await _unitOfWork.Protocols.UpdateAsync(protocol);
			await _unitOfWork.SaveChangesAsync();
		}
		public async Task RestoreProtocolAsync(Guid protocolId)
		{
			// Use IgnoreQueryFilters to get deleted protocol
			var protocol = await _unitOfWork.Protocols.GetByIdIncludeDeletedAsync(protocolId)
				?? throw new KeyNotFoundException($"Protocol {protocolId} không tồn tại");

			protocol.Restore();
			await _unitOfWork.Protocols.UpdateAsync(protocol);
			await _unitOfWork.SaveChangesAsync();
		}

		public async Task<ProtocolDetailResponse> GetProtocolByIdAsync(Guid protocolId)
		{
			var protocol = await _unitOfWork.Protocols.GetByIdWithVersionsAsync(protocolId)
				?? throw new KeyNotFoundException($"Protocol {protocolId} không tồn tại");

			return protocol.ToDetailResponse();  
		}

		public async Task<ProtocolDetailResponse> GetProtocolDetailByIdAsync(Guid protocolId)
		{
			var protocol = await _unitOfWork.Protocols.GetProtocolDetailByIdAsync(protocolId)
				?? throw new KeyNotFoundException($"Protocol {protocolId} không tồn tại");

			return protocol.ToDetailResponse();
		}

		public async Task<List<ProtocolDetailResponse>> GetProtocolsByProjectIdAsync(Guid projectId)
		{
			var protocols = await _unitOfWork.Protocols.GetByProjectIdAsync(projectId);
			return protocols.Select(p => p.ToDetailResponse()).ToList();
		}

		public async Task<ProtocolDetailResponse> UpdateProtocolAsync(UpdateProtocolRequest request)
		{
			var protocol = await _unitOfWork.Protocols.GetByIdWithVersionsAsync(request.ProtocolId)
				?? throw new KeyNotFoundException($"Protocol {request.ProtocolId} không tồn tại");

			// If protocol is approved, create version history before updating
			if (protocol.Status == ProtocolStatus.Approved)
			{
				await CreateProtocolVersionAsync(protocol, request.ChangeSummary);

				// Increment version number
				var currentVersion = double.Parse(protocol.ProtocolVersion);
				protocol.ProtocolVersion = (currentVersion + 0.1).ToString("F1");
				protocol.Status = ProtocolStatus.Draft;
			}

			await _unitOfWork.Protocols.UpdateAsync(protocol);
			await _unitOfWork.SaveChangesAsync();

			return await GetProtocolByIdAsync(protocol.Id);
		}

		private async Task CreateProtocolVersionAsync(ReviewProtocol protocol, string? changeSummary)
		{
			var snapshotData = JsonSerializer.Serialize(new
			{
				protocol.Id,
				protocol.ProjectId,
				protocol.ProtocolVersion,
				protocol.Status,
				protocol.IsDeleted,
				protocol.CreatedAt,
				protocol.ApprovedAt,
				protocol.DeletedAt
			});

			var version = new ProtocolVersion
			{
				ProtocolId = protocol.Id,
				VersionNumber = protocol.ProtocolVersion,
				ChangeSummary = changeSummary ?? "Protocol updated after approval",
				SnapshotData = snapshotData
			};

			await _unitOfWork.ProtocolVersions.AddAsync(version);
		}
	}
}