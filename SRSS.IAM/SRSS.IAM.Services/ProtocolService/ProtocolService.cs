using Microsoft.EntityFrameworkCore;
using SRSS.IAM.Repositories;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SRSS.IAM.Services.ProtocolService
{
	public class ProtocolService : IProtocolService
	{
		private readonly AppDbContext _context;

		public ProtocolService(AppDbContext context)
		{
			_context = context;
		}
		public async Task ApproveProtocolAsync(Guid protocolId, Guid reviewerId)
		{
			var protocol = await _context.ReviewProtocols
				.FirstOrDefaultAsync(p => p.Id == protocolId)
				?? throw new KeyNotFoundException($"Protocol {protocolId} không tồn tại");

			if (protocol.Status == "Approved")
				throw new InvalidOperationException("Protocol đã được phê duyệt trước đó");

			protocol.Status = "Approved";
			protocol.ApprovedAt = DateTimeOffset.UtcNow;
			protocol.ModifiedAt = DateTimeOffset.UtcNow;

			// Create evaluation record
			var evaluation = new ProtocolEvaluation
			{
				ProtocolId = protocolId,
				ReviewerId = reviewerId,
				EvaluationResult = "Approved",
				EvaluatedAt = DateTimeOffset.UtcNow
			};

			_context.ProtocolEvaluations.Add(evaluation);
			await _context.SaveChangesAsync();
		}

		public async Task<ProtocolDetailResponse> CreateProtocolAsync(CreateProtocolRequest request)
		{
			var protocol = new ReviewProtocol
			{
				ProjectId = request.ProjectId,
				ProtocolVersion = request.ProtocolVersion,
				Status = "Draft",
				CreatedAt = DateTimeOffset.UtcNow,
				ModifiedAt = DateTimeOffset.UtcNow
			};

			_context.ReviewProtocols.Add(protocol);
			await _context.SaveChangesAsync();

			return await GetProtocolByIdAsync(protocol.Id);
		}

		public async Task<ProtocolDetailResponse> GetProtocolByIdAsync(Guid protocolId)
		{
			var protocol = await _context.ReviewProtocols
				.Include(p => p.Versions)
				.FirstOrDefaultAsync(p => p.Id == protocolId)
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
			var protocol = await _context.ReviewProtocols
				.Include(p => p.Versions)
				.FirstOrDefaultAsync(p => p.Id == request.ProtocolId)
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

			protocol.ModifiedAt = DateTimeOffset.UtcNow;
			await _context.SaveChangesAsync();

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
				SnapshotData = snapshotData,
				CreatedAt = DateTimeOffset.UtcNow
			};

			_context.ProtocolVersions.Add(version);
		}
	}
}
