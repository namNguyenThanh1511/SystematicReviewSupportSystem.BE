using AutoMapper;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.DataExtraction;

namespace SRSS.IAM.Services.Mappers
{
	public class DataExtractionMappingProfile : Profile
	{
		public DataExtractionMappingProfile()
		{
			CreateMap<DataExtractionStrategy, DataExtractionStrategyDto>()
				.ForMember(dest => dest.ExtractionStrategyId, opt => opt.MapFrom(src => src.Id))
				.ReverseMap()
				.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ExtractionStrategyId ?? Guid.Empty));

			CreateMap<DataExtractionForm, DataExtractionFormDto>()
				.ForMember(dest => dest.FormId, opt => opt.MapFrom(src => src.Id))
				.ReverseMap()
				.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.FormId ?? Guid.Empty));

			CreateMap<DataItemDefinition, DataItemDefinitionDto>()
				.ForMember(dest => dest.DataItemId, opt => opt.MapFrom(src => src.Id))
				.ReverseMap()
				.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.DataItemId ?? Guid.Empty));
		}
	}
}