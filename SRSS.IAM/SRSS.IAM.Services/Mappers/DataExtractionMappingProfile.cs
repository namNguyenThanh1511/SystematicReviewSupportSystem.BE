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
				Sections = entity.Sections != null
					? entity.Sections
						.OrderBy(s => s.OrderIndex)
						.Select(s => s.ToDto())
						.ToList()
					: new List<ExtractionSectionDto>()
			};
		}

		public static ExtractionTemplate ToEntity(this ExtractionTemplateDto dto)
		{
			return new ExtractionTemplate
			{
				Id = dto.TemplateId ?? Guid.NewGuid(),
				ProtocolId = dto.ProtocolId,
				Name = dto.Name,
				Description = dto.Description,
				CreatedAt = DateTimeOffset.UtcNow,
				ModifiedAt = DateTimeOffset.UtcNow,
				Sections = new List<ExtractionSection>()
			};
		}

		public static void UpdateEntity(this ExtractionTemplateDto dto, ExtractionTemplate entity)
		{
			entity.Name = dto.Name;
			entity.Description = dto.Description;
			entity.ModifiedAt = DateTimeOffset.UtcNow;
			// Sections/Fields will be handled separately in service logic
		}

		// ==================== ExtractionSection ====================

		public static ExtractionSectionDto ToDto(this ExtractionSection entity)
		{
			return new ExtractionSectionDto
			{
				SectionId = entity.Id,
				Name = entity.Name,
				Description = entity.Description,
				SectionType = (int)entity.SectionType,
				OrderIndex = entity.OrderIndex,
				Fields = entity.Fields != null
					? entity.Fields
						.Where(f => f.ParentFieldId == null) // Only root fields
						.OrderBy(f => f.OrderIndex)
						.Select(f => f.ToDto())
						.ToList()
					: new List<ExtractionFieldDto>(),
				MatrixColumns = entity.MatrixColumns != null
					? entity.MatrixColumns
						.OrderBy(c => c.OrderIndex)
						.Select(c => c.ToDto())
						.ToList()
					: new List<ExtractionMatrixColumnDto>()
			};
		}

		public static ExtractionSection ToEntity(this ExtractionSectionDto dto, Guid templateId)
		{
			return new ExtractionSection
			{
				Id = dto.SectionId ?? Guid.NewGuid(),
				TemplateId = templateId,
				Name = dto.Name,
				Description = dto.Description,
				SectionType = (SectionType)dto.SectionType,
				OrderIndex = dto.OrderIndex,
				CreatedAt = DateTimeOffset.UtcNow,
				ModifiedAt = DateTimeOffset.UtcNow
			};
		}

		// ==================== ExtractionMatrixColumn ====================

		public static ExtractionMatrixColumnDto ToDto(this ExtractionMatrixColumn entity)
		{
			return new ExtractionMatrixColumnDto
			{
				ColumnId = entity.Id,
				Name = entity.Name,
				Description = entity.Description,
				OrderIndex = entity.OrderIndex
			};
		}

		public static ExtractionMatrixColumn ToEntity(this ExtractionMatrixColumnDto dto, Guid sectionId)
		{
			return new ExtractionMatrixColumn
			{
				Id = dto.ColumnId ?? Guid.NewGuid(),
				SectionId = sectionId,
				Name = dto.Name,
				Description = dto.Description,
				OrderIndex = dto.OrderIndex,
				CreatedAt = DateTimeOffset.UtcNow,
				ModifiedAt = DateTimeOffset.UtcNow
			};
		}

		// ==================== ExtractionField (Recursive) ====================

		public static ExtractionFieldDto ToDto(this ExtractionField entity)
		{
			return new ExtractionFieldDto
			{
				FieldId = entity.Id,
				SectionId = entity.SectionId,
				ParentFieldId = entity.ParentFieldId,
				Name = entity.Name,
				Instruction = entity.Instruction,
				FieldType = (int)entity.FieldType,
				IsRequired = entity.IsRequired,
				OrderIndex = entity.OrderIndex,
				Options = entity.Options != null
					? entity.Options
						.OrderBy(o => o.DisplayOrder)
						.Select(o => o.ToDto())
						.ToList()
					: new List<FieldOptionDto>(),
				SubFields = entity.SubFields != null
					? entity.SubFields
						.OrderBy(sf => sf.OrderIndex)
						.Select(sf => sf.ToDto()) // Recursive call
						.ToList()
					: new List<ExtractionFieldDto>()
			};
		}

		/// <summary>
		/// Converts DTO to entities recursively (flattens tree structure)
		/// </summary>
		public static List<ExtractionField> ToEntitiesRecursive(
			this ExtractionFieldDto dto,
			Guid sectionId,
			Guid? parentFieldId)
		{
			var entities = new List<ExtractionField>();

			var fieldId = dto.FieldId ?? Guid.NewGuid();

			// Create current field
			var field = new ExtractionField
			{
				Id = fieldId,
				SectionId = sectionId,
				ParentFieldId = parentFieldId,
				Name = dto.Name,
				Instruction = dto.Instruction,
				FieldType = (FieldType)dto.FieldType,
				IsRequired = dto.IsRequired,
				OrderIndex = dto.OrderIndex,
				CreatedAt = DateTimeOffset.UtcNow,
				ModifiedAt = DateTimeOffset.UtcNow
			};

			// Add options
			if (dto.Options != null && dto.Options.Count > 0)
			{
				field.Options = dto.Options
					.Select(o => new FieldOption
					{
						Id = o.OptionId ?? Guid.NewGuid(),
						FieldId = fieldId,
						Value = o.Value,
						DisplayOrder = o.DisplayOrder > 0 ? o.DisplayOrder : 0,
						CreatedAt = DateTimeOffset.UtcNow,
						ModifiedAt = DateTimeOffset.UtcNow
					})
					.ToList();
			}
			else
			{
				field.Options = new List<FieldOption>();
			}

			entities.Add(field);

			// Recursively add sub-fields
			if (dto.SubFields != null && dto.SubFields.Count > 0)
			{
				foreach (var subFieldDto in dto.SubFields)
				{
					var subEntities = subFieldDto.ToEntitiesRecursive(sectionId, fieldId);
					entities.AddRange(subEntities);
				}
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

		public static FieldOption ToEntity(this FieldOptionDto dto, Guid fieldId)
		{
			return new FieldOption
			{
				Id = dto.OptionId ?? Guid.NewGuid(),
				FieldId = fieldId,
				Value = dto.Value,
				DisplayOrder = dto.DisplayOrder > 0 ? dto.DisplayOrder : 0,
				CreatedAt = DateTimeOffset.UtcNow,
				ModifiedAt = DateTimeOffset.UtcNow
			};
		}

		// ==================== Batch Operations ====================

		public static List<ExtractionTemplateDto> ToDtoList(this IEnumerable<ExtractionTemplate> entities)
		{
			return entities
				.Where(e => e != null)
				.Select(e => e.ToDto())
				.ToList();
		}

		public static List<ExtractionTemplate> ToEntityList(this IEnumerable<ExtractionTemplateDto> dtos)
		{
			return dtos
				.Where(d => d != null)
				.Select(d => d.ToEntity())
				.ToList();
		}
	}
}