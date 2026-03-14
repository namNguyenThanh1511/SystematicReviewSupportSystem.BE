using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.DataExtraction;
using SRSS.IAM.Services.Mappers;

namespace SRSS.IAM.Services.DataExtractionService
{
	public class DataExtractionService : IDataExtractionService
	{
		private readonly IUnitOfWork _unitOfWork;

		public DataExtractionService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		// ==================== EXTRACTION TEMPLATES ====================

		public async Task<ExtractionTemplateDto> UpsertTemplateAsync(ExtractionTemplateDto dto)
		{
			// Validate first
			var validationResult = await ValidateTemplateAsync(dto);
			if (!validationResult.IsValid)
			{
				throw new InvalidOperationException(
					$"Template validation failed: {string.Join(", ", validationResult.Errors.Select(e => e.Message))}");
			}

			ExtractionTemplate template;

			if (dto.TemplateId.HasValue && dto.TemplateId.Value != Guid.Empty)
			{
				// UPDATE: Load existing template
				template = await _unitOfWork.ExtractionTemplates
					.FindSingleAsync(t => t.Id == dto.TemplateId.Value)
					?? throw new KeyNotFoundException($"Template {dto.TemplateId.Value} không tồn tại");

				// Update basic properties
				template.Name = dto.Name;
				template.Description = dto.Description;
				template.ModifiedAt = DateTimeOffset.UtcNow;

				// Delete old sections (cascade will delete fields and options)
				await DeleteSectionsAsync(template.Id);
			}
			else
			{
				// CREATE: New template
				template = new ExtractionTemplate
				{
					Id = Guid.NewGuid(),
					ProtocolId = dto.ProtocolId,
					Name = dto.Name,
					Description = dto.Description,
					CreatedAt = DateTimeOffset.UtcNow,
					ModifiedAt = DateTimeOffset.UtcNow
				};

				await _unitOfWork.ExtractionTemplates.AddAsync(template);
			}

			// Add sections with their fields
			foreach (var sectionDto in dto.Sections)
			{
				var section = sectionDto.ToEntity(template.Id);
				await _unitOfWork.ExtractionSections.AddAsync(section);

				// Add fields for this section
				if (sectionDto.Fields != null && sectionDto.Fields.Count > 0)
				{
					foreach (var fieldDto in sectionDto.Fields)
					{
						var fieldEntities = fieldDto.ToEntitiesRecursive(section.Id, null);
						foreach (var field in fieldEntities)
						{
							await _unitOfWork.ExtractionFields.AddAsync(field);
						}
					}
				}
			}

			await _unitOfWork.SaveChangesAsync();

			// Reload to get full tree structure
			var result = await GetTemplateByIdAsync(template.Id);
			return result;
		}

		public async Task<List<ExtractionTemplateDto>> GetTemplatesByProtocolIdAsync(Guid protocolId)
		{
			var templates = await _unitOfWork.ExtractionTemplates
				.GetByProtocolIdAsync(protocolId);

			return templates.Select(t => t.ToDto()).ToList();
		}

		public async Task<ExtractionTemplateDto> GetTemplateByIdAsync(Guid templateId)
		{
			var template = await _unitOfWork.ExtractionTemplates
				.GetByIdWithFieldsAsync(templateId)
				?? throw new KeyNotFoundException($"Template {templateId} không tồn tại");

			return template.ToDto();
		}

		public async Task DeleteTemplateAsync(Guid templateId)
		{
			var template = await _unitOfWork.ExtractionTemplates
				.FindSingleAsync(t => t.Id == templateId);

			if (template != null)
			{
				// Delete sections first (cascade will handle fields and options)
				await DeleteSectionsAsync(templateId);

				// Delete template
				await _unitOfWork.ExtractionTemplates.RemoveAsync(template);
				await _unitOfWork.SaveChangesAsync();
			}
		}

		public async Task<TemplateValidationResultDto> ValidateTemplateAsync(ExtractionTemplateDto dto)
		{
			var errors = new List<ValidationErrorDetail>();

			// 1. Validate template name
			if (string.IsNullOrWhiteSpace(dto.Name))
			{
				errors.Add(new ValidationErrorDetail
				{
					Code = "TEMPLATE_NAME_REQUIRED",
					Message = "Template name is required"
				});
			}

			// 2. Validate sections exist
			if (dto.Sections == null || dto.Sections.Count == 0)
			{
				errors.Add(new ValidationErrorDetail
				{
					Code = "NO_SECTIONS",
					Message = "Template must have at least one section"
				});
				return new TemplateValidationResultDto { IsValid = false, Errors = errors };
			}

			// 3. Validate each section
			foreach (var section in dto.Sections)
			{
				// Validate section name
				if (string.IsNullOrWhiteSpace(section.Name))
				{
					errors.Add(new ValidationErrorDetail
					{
						Code = "SECTION_NAME_REQUIRED",
						Message = "Section name is required"
					});
				}

				// Validate section type
				if (section.SectionType < 0 || section.SectionType > 1)
				{
					errors.Add(new ValidationErrorDetail
					{
						Code = "INVALID_SECTION_TYPE",
						Message = $"Section '{section.Name}' has invalid type {section.SectionType}"
					});
				}

				// Validate fields within section
				if (section.Fields == null || section.Fields.Count == 0)
				{
					errors.Add(new ValidationErrorDetail
					{
						Code = "NO_FIELDS_IN_SECTION",
						Message = $"Section '{section.Name}' must have at least one field"
					});
					continue;
				}

				// Validate duplicate field names within section
				var fieldNames = section.Fields.Select(f => f.Name).ToList();
				var duplicates = fieldNames
					.GroupBy(x => x)
					.Where(g => g.Count() > 1)
					.Select(g => g.Key)
					.ToList();

				foreach (var duplicate in duplicates)
				{
					var indices = section.Fields
						.Select((f, idx) => (f.Name == duplicate ? idx : -1))
						.Where(idx => idx >= 0)
						.ToList();

					errors.Add(new ValidationErrorDetail
					{
						Code = "DUPLICATE_FIELD_NAME",
						Message = $"Field name '{duplicate}' is duplicated in section '{section.Name}' at indices {string.Join(", ", indices)}"
					});
				}

				// Validate field options and types
				foreach (var field in section.Fields)
				{
					ValidateFieldOptions(field, errors);

					// Recursively validate sub-fields
					if (field.SubFields != null && field.SubFields.Count > 0)
					{
						foreach (var subField in field.SubFields)
						{
							ValidateFieldOptions(subField, errors, field.Name);
						}
					}
				}
			}

			// 4. Validate duplicate section names
			var sectionNames = dto.Sections.Select(s => s.Name).ToList();
			var sectionDuplicates = sectionNames
				.GroupBy(x => x)
				.Where(g => g.Count() > 1)
				.Select(g => g.Key)
				.ToList();

			foreach (var duplicate in sectionDuplicates)
			{
				errors.Add(new ValidationErrorDetail
				{
					Code = "DUPLICATE_SECTION_NAME",
					Message = $"Section name '{duplicate}' is duplicated"
				});
			}

			return new TemplateValidationResultDto
			{
				IsValid = errors.Count == 0,
				Errors = errors
			};
		}

		// ==================== PRIVATE HELPER METHODS ====================

		private void ValidateFieldOptions(
			ExtractionFieldDto field,
			List<ValidationErrorDetail> errors,
			string? parentName = null)
		{
			const int MinSelectOptions = 2;

			// SingleSelect (4) and MultiSelect (5) must have at least 2 options
			if ((field.FieldType == 4 || field.FieldType == 5) &&
				(field.Options == null || field.Options.Count < MinSelectOptions))
			{
				var fieldDisplayName = string.IsNullOrEmpty(parentName)
					? field.Name
					: $"{parentName} > {field.Name}";

				errors.Add(new ValidationErrorDetail
				{
					Code = "INVALID_OPTION_COUNT",
					Message = $"Field '{fieldDisplayName}' with type {(field.FieldType == 4 ? "SingleSelect" : "MultiSelect")} " +
						$"must have at least {MinSelectOptions} options, found {field.Options?.Count ?? 0}"
				});
			}

			// Validate option values are not empty
			if (field.Options != null)
			{
				foreach (var option in field.Options.Where(o => string.IsNullOrWhiteSpace(o.Value)))
				{
					errors.Add(new ValidationErrorDetail
					{
						Code = "EMPTY_OPTION_VALUE",
						Message = $"Field '{field.Name}' has option with empty value"
					});
				}
			}

			// Validate field type range
			if (field.FieldType < 0 || field.FieldType > 5)
			{
				errors.Add(new ValidationErrorDetail
				{
					Code = "INVALID_FIELD_TYPE",
					Message = $"Field '{field.Name}' has invalid type {field.FieldType}"
				});
			}
		}

		private async Task DeleteSectionsAsync(Guid templateId)
		{
			var allSections = await _unitOfWork.ExtractionSections
				.FindAllAsync(s => s.TemplateId == templateId);

			if (allSections == null || !allSections.Any())
			{
				return;
			}

			foreach (var section in allSections)
			{
				// Delete fields in this section
				var fields = await _unitOfWork.ExtractionFields
					.FindAllAsync(f => f.SectionId == section.Id);

				if (fields != null)
				{
					foreach (var field in fields)
					{
						// Delete options for this field
						var options = await _unitOfWork.FieldOptions
							.FindAllAsync(o => o.FieldId == field.Id);

						if (options != null)
						{
							foreach (var option in options)
							{
								await _unitOfWork.FieldOptions.RemoveAsync(option);
							}
						}

						await _unitOfWork.ExtractionFields.RemoveAsync(field);
					}
				}

				// Delete section
				await _unitOfWork.ExtractionSections.RemoveAsync(section);
			}
		}
	}
}