using AutoMapper;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.SearchStrategy;

namespace SRSS.IAM.Services.Mappers
{
	public class SearchStrategyMappingProfile : Profile
	{
		public SearchStrategyMappingProfile()
		{
			CreateMap<SearchStrategy, SearchStrategyDto>()
				.ForMember(dest => dest.StrategyId, opt => opt.MapFrom(src => src.Id))
				.ReverseMap()
				.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.StrategyId ?? Guid.Empty));

			CreateMap<SearchString, SearchStringDto>()
				.ForMember(dest => dest.SearchStringId, opt => opt.MapFrom(src => src.Id))
				.ReverseMap()
				.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.SearchStringId ?? Guid.Empty));

			CreateMap<SearchTerm, SearchTermDto>()
				.ForMember(dest => dest.TermId, opt => opt.MapFrom(src => src.Id))
				.ReverseMap()
				.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.TermId ?? Guid.Empty));

			CreateMap<SearchStringTerm, SearchStringTermDto>().ReverseMap();

			CreateMap<SearchSource, SearchSourceDto>()
				.ForMember(dest => dest.SourceId, opt => opt.MapFrom(src => src.Id))
				.ReverseMap()
				.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.SourceId ?? Guid.Empty));
		}
	}
}