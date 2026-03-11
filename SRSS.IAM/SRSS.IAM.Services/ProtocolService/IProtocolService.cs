using SRSS.IAM.Services.DTOs.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Services.ProtocolService
{
	public interface IProtocolService
	{
		Task<ProtocolDetailResponse> CreateProtocolAsync(CreateProtocolRequest request);

		Task<ProtocolDetailResponse> UpdateProtocolAsync(UpdateProtocolRequest request);
		Task<ProtocolDetailResponse> GetProtocolByIdAsync(Guid protocolId);
		Task<ProtocolDetailResponse> GetProtocolDetailByIdAsync(Guid protocolId);
		Task<List<ProtocolDetailResponse>> GetProtocolsByProjectIdAsync(Guid projectId);
		Task ApproveProtocolAsync(Guid protocolId, Guid currentUserId);
		Task DeleteProtocolAsync(Guid protocolId);
		Task RejectProtocolAsync(Guid protocolId, Guid currentUserId, string? reason = null);
		Task SubmitForReviewAsync(Guid protocolId);
		Task RestoreProtocolAsync(Guid protocolId);

	}
}