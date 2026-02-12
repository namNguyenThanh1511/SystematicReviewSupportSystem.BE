using AutoMapper;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.SelectionCriteria;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SRSS.IAM.Services.Mappers
{
	public class SelectionCriteriaMappingProfile : Profile
	{
		public SelectionCriteriaMappingProfile()
		{
			CreateMap<StudySelectionCriteria, StudySelectionCriteriaDto>()
				.ForMember(dest => dest.CriteriaId, opt => opt.MapFrom(src => src.Id))
				.ReverseMap()
				.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CriteriaId ?? Guid.Empty));

			CreateMap<InclusionCriterion, InclusionCriterionDto>()
				.ForMember(dest => dest.InclusionId, opt => opt.MapFrom(src => src.Id))
				.ReverseMap()
				.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.InclusionId ?? Guid.Empty));

			CreateMap<ExclusionCriterion, ExclusionCriterionDto>()
				.ForMember(dest => dest.ExclusionId, opt => opt.MapFrom(src => src.Id))
				.ReverseMap()
				.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ExclusionId ?? Guid.Empty));

			CreateMap<StudySelectionProcedure, StudySelectionProcedureDto>()
				.ForMember(dest => dest.ProcedureId, opt => opt.MapFrom(src => src.Id))
				.ReverseMap()
				.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ProcedureId ?? Guid.Empty));

			CreateMap<StudySelectionProcedure, StudySelectionProcedureDto>()
				.ForMember(dest => dest.ProcedureId, opt => opt.MapFrom(src => src.Id))
				.ReverseMap()
				.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ProcedureId ?? Guid.Empty));
		}
	}
}