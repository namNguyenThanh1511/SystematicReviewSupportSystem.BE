using Microsoft.EntityFrameworkCore;
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

		// ==================== Extraction Templates ====================

		public async Task<ExtractionTemplateDto> UpsertTemplateAsync(ExtractionTemplateDto dto)
		{
			ExtractionTemplate template;

			if (dto.TemplateId.HasValue && dto.TemplateId.Value != Guid.Empty)
			{
				// UPDATE: Load existing template with all fields
				template = await _unitOfWork.ExtractionTemplates
					.GetByIdWithFieldsAsync(dto.TemplateId.Value)
					?? throw new KeyNotFoundException($"Template {dto.TemplateId.Value} không tồn tại");

				// Update basic properties
				dto.UpdateEntity(template);

				// Handle Fields: Remove old, add new (simplified approach)
				await DeleteFieldsRecursiveAsync(template.Id);

				// Add new fields from DTO
				var newFields = dto.Fields
					.SelectMany(f => f.ToEntitiesRecursive(template.Id, null))
					.ToList();

				foreach (var field in newFields)
				{
					await _unitOfWork.ExtractionFields.AddAsync(field);
				}
			}
			else
			{
				// CREATE: New template
				template = dto.ToEntity();
				await _unitOfWork.ExtractionTemplates.AddAsync(template);

				// Add fields
				foreach (var field in template.Fields)
				{
					await _unitOfWork.ExtractionFields.AddAsync(field);
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

			// Build tree structure for each template
			var dtos = new List<ExtractionTemplateDto>();
			foreach (var template in templates)
			{
				var dto = await BuildTemplateDtoWithTreeAsync(template);
				dtos.Add(dto);
			}

			return dtos;
		}

		public async Task<ExtractionTemplateDto> GetTemplateByIdAsync(Guid templateId)
		{
			var template = await _unitOfWork.ExtractionTemplates
				.FindSingleAsync(t => t.Id == templateId)
				?? throw new KeyNotFoundException($"Template {templateId} không tồn tại");

			return await BuildTemplateDtoWithTreeAsync(template);
		}

		public async Task DeleteTemplateAsync(Guid templateId)
		{
			var template = await _unitOfWork.ExtractionTemplates
				.FindSingleAsync(t => t.Id == templateId);

			if (template != null)
			{
				// Delete all fields first (cascade will handle options)
				await DeleteFieldsRecursiveAsync(templateId);

				// Delete template
				await _unitOfWork.ExtractionTemplates.RemoveAsync(template);
				await _unitOfWork.SaveChangesAsync();
			}
		}

		// ==================== PRIVATE HELPER METHODS ====================

		/// <summary>
		/// Builds Template DTO with full recursive tree structure
		/// </summary>
		private async Task<ExtractionTemplateDto> BuildTemplateDtoWithTreeAsync(ExtractionTemplate template)
		{
			// Get all fields for this template (flat list)
			var allFields = await _unitOfWork.ExtractionFields
				.GetByTemplateIdAsync(template.Id);

			// Get all options for all fields (optimize with single query)
			var allFieldIds = allFields.Select(f => f.Id).ToList();
			var allOptions = await _unitOfWork.FieldOptions
				.FindAllAsync(o => allFieldIds.Contains(o.FieldId));

			// Build lookup dictionary for performance
			var fieldLookup = allFields.ToDictionary(f => f.Id);
			var optionLookup = allOptions
				.GroupBy(o => o.FieldId)
				.ToDictionary(g => g.Key, g => g.ToList());

			// Build tree structure recursively
			var rootFields = allFields
				.Where(f => f.ParentFieldId == null)
				.OrderBy(f => f.OrderIndex)
				.Select(f => BuildFieldDtoRecursive(f, fieldLookup, optionLookup))
				.ToList();

			return new ExtractionTemplateDto
			{
				TemplateId = template.Id,
				ProtocolId = template.ProtocolId,
				Name = template.Name,
				Description = template.Description,
				Fields = rootFields
			};
		}

		/// <summary>
		/// Recursive method to build field DTO tree
		/// </summary>
		private ExtractionFieldDto BuildFieldDtoRecursive(
			ExtractionField field,
			Dictionary<Guid, ExtractionField> fieldLookup,
			Dictionary<Guid, List<FieldOption>> optionLookup)
		{
			// Get options for this field
			var options = optionLookup.ContainsKey(field.Id)
				? optionLookup[field.Id]
					.OrderBy(o => o.DisplayOrder)
					.Select(o => o.ToDto())
					.ToList()
				: new List<FieldOptionDto>();

			// Get sub-fields
			var subFields = fieldLookup.Values
				.Where(f => f.ParentFieldId == field.Id)
				.OrderBy(f => f.OrderIndex)
				.Select(f => BuildFieldDtoRecursive(f, fieldLookup, optionLookup))
				.ToList();

			return new ExtractionFieldDto
			{
				FieldId = field.Id,
				TemplateId = field.TemplateId,
				ParentFieldId = field.ParentFieldId,
				Name = field.Name,
				Instruction = field.Instruction,
				FieldType = Enum.Parse<FieldTypeEnum>(field.FieldType),
				IsRequired = field.IsRequired,
				OrderIndex = field.OrderIndex,
				Options = options,
				SubFields = subFields
			};
		}

		/// <summary>
		/// Delete all fields and their children recursively
		/// </summary>
		private async Task DeleteFieldsRecursiveAsync(Guid templateId)
		{
			var allFields = await _unitOfWork.ExtractionFields
				.GetByTemplateIdAsync(templateId);

			foreach (var field in allFields)
			{
				// Delete options first
				var options = await _unitOfWork.FieldOptions
					.GetByFieldIdAsync(field.Id);

				foreach (var option in options)
				{
					await _unitOfWork.FieldOptions.RemoveAsync(option);
				}

				// Delete field
				await _unitOfWork.ExtractionFields.RemoveAsync(field);
			}
		}
	}
}