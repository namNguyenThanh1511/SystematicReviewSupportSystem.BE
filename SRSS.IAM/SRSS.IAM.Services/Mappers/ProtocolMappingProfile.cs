using AutoMapper;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.Protocol;

namespace SRSS.IAM.Services.Mappers
{
	public class ProtocolMappingProfile : Profile
	{
		public ProtocolMappingProfile()
		{
			CreateMap<ReviewProtocol, ProtocolDetailResponse>()
				.ForMember(dest => dest.ProtocolId, opt => opt.MapFrom(src => src.Id));

			CreateMap<ProtocolVersion, VersionHistoryDto>()
				.ForMember(dest => dest.VersionId, opt => opt.MapFrom(src => src.Id));
		}
	}
}