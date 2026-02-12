using AutoMapper;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.QualityAssessment;

namespace SRSS.IAM.Services.Mappers
{
	public class QualityAssessmentMappingProfile : Profile
	{
		public QualityAssessmentMappingProfile()
		{
			CreateMap<QualityAssessmentStrategy, QualityAssessmentStrategyDto>()
				.ForMember(dest => dest.QaStrategyId, opt => opt.MapFrom(src => src.Id))
				.ReverseMap()
				.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.QaStrategyId ?? Guid.Empty));

			CreateMap<QualityChecklist, QualityChecklistDto>()
				.ForMember(dest => dest.ChecklistId, opt => opt.MapFrom(src => src.Id))
				.ReverseMap()
				.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ChecklistId ?? Guid.Empty));

			CreateMap<QualityCriterion, QualityCriterionDto>()
				.ForMember(dest => dest.QualityCriterionId, opt => opt.MapFrom(src => src.Id))
				.ReverseMap()
				.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.QualityCriterionId ?? Guid.Empty));
		}
	}
}