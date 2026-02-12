using AutoMapper;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.Synthesis;

namespace SRSS.IAM.Services.Mappers
{
	public class SynthesisMappingProfile : Profile
	{
		public SynthesisMappingProfile()
		{
			CreateMap<DataSynthesisStrategy, DataSynthesisStrategyDto>()
				.ForMember(dest => dest.SynthesisStrategyId, opt => opt.MapFrom(src => src.Id))
				.ReverseMap()
				.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.SynthesisStrategyId ?? Guid.Empty));

			CreateMap<DisseminationStrategy, DisseminationStrategyDto>()
				.ForMember(dest => dest.DisseminationId, opt => opt.MapFrom(src => src.Id))
				.ReverseMap()
				.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.DisseminationId ?? Guid.Empty));

			CreateMap<ProjectTimetable, ProjectTimetableDto>()
				.ForMember(dest => dest.TimetableId, opt => opt.MapFrom(src => src.Id))
				.ReverseMap()
				.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.TimetableId ?? Guid.Empty));
		}
	}
}