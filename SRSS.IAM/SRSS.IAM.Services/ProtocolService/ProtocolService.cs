using AutoMapper;
using System.Text.Json;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.Protocol;

namespace SRSS.IAM.Services.ProtocolService
{
	public class ProtocolService : IProtocolService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public ProtocolService(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task ApproveProtocolAsync(Guid protocolId, Guid reviewerId)
		{
			var protocol = await _unitOfWork.Protocols.FindSingleAsync(p => p.Id == protocolId)
				?? throw new KeyNotFoundException($"Protocol {protocolId} không tồn tại");

			if (protocol.Status == "Approved")
				throw new InvalidOperationException("Protocol đã được phê duyệt trước đó");

			protocol.Status = "Approved";
			protocol.ApprovedAt = DateTimeOffset.UtcNow;

			await _unitOfWork.Protocols.UpdateAsync(protocol);

			// Create evaluation record
			var evaluation = new ProtocolEvaluation
			{
				ProtocolId = protocolId,
				ReviewerId = reviewerId,
				EvaluationResult = "Approved",
				EvaluatedAt = DateTimeOffset.UtcNow
			};

			await _unitOfWork.ProtocolEvaluations.AddAsync(evaluation);
			await _unitOfWork.SaveChangesAsync();
		}

		public async Task<ProtocolDetailResponse> CreateProtocolAsync(CreateProtocolRequest request)
		{
			var protocol = new ReviewProtocol
			{
				ProjectId = request.ProjectId,
				ProtocolVersion = request.ProtocolVersion,
				Status = "Draft"
			};

			await _unitOfWork.Protocols.AddAsync(protocol);
			await _unitOfWork.SaveChangesAsync();

			return await GetProtocolByIdAsync(protocol.Id);
		}

		public async Task<ProtocolDetailResponse> GetProtocolByIdAsync(Guid protocolId)
		{
			var protocol = await _unitOfWork.Protocols.GetByIdWithVersionsAsync(protocolId)
				?? throw new KeyNotFoundException($"Protocol {protocolId} không tồn tại");

			return new ProtocolDetailResponse
			{
				ProtocolId = protocol.Id,
				ProjectId = protocol.ProjectId,
				ProtocolVersion = protocol.ProtocolVersion,
				Status = protocol.Status,
				CreatedAt = protocol.CreatedAt,
				ApprovedAt = protocol.ApprovedAt,
				Versions = protocol.Versions.Select(v => new VersionHistoryDto
				{
					VersionId = v.Id,
					VersionNumber = v.VersionNumber,
					ChangeSummary = v.ChangeSummary,
					CreatedAt = v.CreatedAt
				}).ToList()
			};
		}

		public async Task<ProtocolDetailResponse> UpdateProtocolAsync(UpdateProtocolRequest request)
		{
			var protocol = await _unitOfWork.Protocols.GetByIdWithVersionsAsync(request.ProtocolId)
				?? throw new KeyNotFoundException($"Protocol {request.ProtocolId} không tồn tại");

			// If protocol is approved, create version history before updating
			if (protocol.Status == "Approved")
			{
				await CreateProtocolVersionAsync(protocol, request.ChangeSummary);

				// Increment version number
				var currentVersion = double.Parse(protocol.ProtocolVersion);
				protocol.ProtocolVersion = (currentVersion + 0.1).ToString("F1");
				protocol.Status = "Draft"; // Reset to draft after major change
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
				protocol.CreatedAt,
				protocol.ApprovedAt
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