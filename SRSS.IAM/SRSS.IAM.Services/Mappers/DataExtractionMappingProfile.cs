using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.DataExtraction;

namespace SRSS.IAM.Services.Mappers
{
	public static class DataExtractionMappingProfile
	{
		// ==================== ExtractionTemplate ====================
		public static ExtractionTemplateDto ToDto(this ExtractionTemplate entity)
		{
			return new ExtractionTemplateDto
			{
				TemplateId = entity.Id,
				ProtocolId = entity.ProtocolId,
				Name = entity.Name,
				Description = entity.Description,
				Fields = entity.Fields
					.Where(f => f.ParentFieldId == null) // Only root fields
					.OrderBy(f => f.OrderIndex)
					.Select(f => f.ToDto())
					.ToList()
			};
		}

		public static ExtractionTemplate ToEntity(this ExtractionTemplateDto dto)
		{
			var entity = new ExtractionTemplate
			{
				Id = dto.TemplateId ?? Guid.NewGuid(),
				ProtocolId = dto.ProtocolId,
				Name = dto.Name,
				Description = dto.Description
			};

			// Recursively convert fields
			entity.Fields = dto.Fields
				.SelectMany(fieldDto => fieldDto.ToEntitiesRecursive(entity.Id, null))
				.ToList();

			return entity;
		}

		public static void UpdateEntity(this ExtractionTemplateDto dto, ExtractionTemplate entity)
		{
			entity.Name = dto.Name;
			entity.Description = dto.Description;
			// Fields will be handled separately in service logic
		}

		// ==================== ExtractionField (Recursive) ====================
		public static ExtractionFieldDto ToDto(this ExtractionField entity)
		{
			return new ExtractionFieldDto
			{
				FieldId = entity.Id,
				TemplateId = entity.TemplateId,
				ParentFieldId = entity.ParentFieldId,
				Name = entity.Name,
				Instruction = entity.Instruction,
				FieldType = Enum.Parse<FieldTypeEnum>(entity.FieldType),
				IsRequired = entity.IsRequired,
				OrderIndex = entity.OrderIndex,
				Options = entity.Options
					.OrderBy(o => o.DisplayOrder)
					.Select(o => o.ToDto())
					.ToList(),
				SubFields = entity.SubFields
					.OrderBy(sf => sf.OrderIndex)
					.Select(sf => sf.ToDto()) // Recursive call
					.ToList()
			};
		}

		/// <summary>
		/// Converts DTO to entities recursively (flattens tree structure)
		/// </summary>
		public static List<ExtractionField> ToEntitiesRecursive(
			this ExtractionFieldDto dto,
			Guid templateId,
			Guid? parentFieldId)
		{
			var entities = new List<ExtractionField>();

			var fieldId = dto.FieldId ?? Guid.NewGuid();

			// Create current field
			var field = new ExtractionField
			{
				Id = fieldId,
				TemplateId = templateId,
				ParentFieldId = parentFieldId,
				Name = dto.Name,
				Instruction = dto.Instruction,
				FieldType = dto.FieldType.ToString(),
				IsRequired = dto.IsRequired,
				OrderIndex = dto.OrderIndex
			};

			// Add options
			field.Options = dto.Options.Select(o => new FieldOption
			{
				Id = o.OptionId ?? Guid.NewGuid(),
				FieldId = fieldId,
				Value = o.Value,
				DisplayOrder = o.DisplayOrder
			}).ToList();

			entities.Add(field);

			// Recursively add sub-fields
			foreach (var subFieldDto in dto.SubFields)
			{
				entities.AddRange(subFieldDto.ToEntitiesRecursive(templateId, fieldId));
			}

			return entities;
		}

		// ==================== FieldOption ====================
		public static FieldOptionDto ToDto(this FieldOption entity)
		{
			return new FieldOptionDto
			{
				OptionId = entity.Id,
				FieldId = entity.FieldId,
				Value = entity.Value,
				DisplayOrder = entity.DisplayOrder
			};
		}

		// ==================== List Extensions ====================
		public static List<ExtractionTemplateDto> ToDtoList(this IEnumerable<ExtractionTemplate> entities)
		{
			return entities.Select(e => e.ToDto()).ToList();
		}
	}
}