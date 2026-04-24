using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.DataExtraction;
using SRSS.IAM.Services.Mappers;
using SRSS.IAM.Services.UserService;
using SRSS.IAM.Services.OpenRouter;
using Shared.Exceptions;

namespace SRSS.IAM.Services.DataExtractionService
{
    public class DataExtractionService : IDataExtractionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IOpenRouterService _openRouterService;

        public DataExtractionService(
            IUnitOfWork unitOfWork, 
            ICurrentUserService currentUserService,
            IOpenRouterService openRouterService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _openRouterService = openRouterService;
        }

        private async Task EnsureLeaderAsync(Guid processId)
        {
            var userIdString = _currentUserService.GetUserId();
            if (string.IsNullOrEmpty(userIdString))
            {
                throw new UnauthorizedException("User is not authenticated.");
            }

            var userId = Guid.Parse(userIdString);
            
            var process = await _unitOfWork.DataExtractionProcesses
                .FindSingleAsync(x => x.Id == processId)
                ?? throw new KeyNotFoundException($"Process {processId} không tồn tại");

            var reviewProcess = await _unitOfWork.ReviewProcesses.FindSingleAsync(x => x.Id == process.ReviewProcessId)
                ?? throw new KeyNotFoundException("ReviewProcess không tồn tại.");

            var isLeader = await _unitOfWork.SystematicReviewProjects.IsProjectLeaderAsync(reviewProcess.ProjectId, userId);
            if (!isLeader)
            {
                throw new ForbiddenException("Only project leader can perform this action.");
            }
        }

        // ==================== EXTRACTION TEMPLATES ====================

        public async Task<ExtractionTemplateDto> UpsertTemplateAsync(ExtractionTemplateDto dto)
        {
            await EnsureLeaderAsync(dto.DataExtractionProcessId);

            var validationResult = await ValidateTemplateAsync(dto);
            if (!validationResult.IsValid)
            {
                throw new InvalidOperationException(
                    $"Template validation failed: {string.Join(", ", validationResult.Errors.Select(e => e.Message))}");
            }

            ExtractionTemplate template;

            if (dto.TemplateId.HasValue && dto.TemplateId.Value != Guid.Empty)
            {
                template = await _unitOfWork.ExtractionTemplates
                    .GetByIdWithFieldsAsync(dto.TemplateId.Value)
                    ?? throw new KeyNotFoundException($"Template {dto.TemplateId.Value} không tồn tại");

                dto.UpdateEntity(template);
                
                // Delta Update (Merge) Sections
                await MergeSectionsAsync(template, dto.Sections);
            }
            else
            {
                template = new ExtractionTemplate
                {
                    Id = Guid.NewGuid(),
                    DataExtractionProcessId = dto.DataExtractionProcessId,
                    Name = dto.Name,
                    Description = dto.Description,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                };

                await _unitOfWork.ExtractionTemplates.AddAsync(template);

                foreach (var sectionDto in dto.Sections)
                {
                    var section = sectionDto.ToEntity(template.Id);
                    await _unitOfWork.ExtractionSections.AddAsync(section);

                    if (sectionDto.Fields != null)
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

                    if (sectionDto.MatrixColumns != null)
                    {
                        foreach (var columnDto in sectionDto.MatrixColumns)
                        {
                            var columnEntity = columnDto.ToEntity(section.Id);
                            await _unitOfWork.ExtractionMatrixColumns.AddAsync(columnEntity);
                        }
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return await GetTemplateByIdAsync(template.Id);
        }

        private async Task MergeSectionsAsync(ExtractionTemplate template, List<ExtractionSectionDto> incomingSections)
        {
            var existingSections = template.Sections.ToList();

            // 1. Update or Add
            foreach (var sectionDto in incomingSections)
            {
                var existingSection = existingSections.FirstOrDefault(s => s.Id == sectionDto.SectionId);
                if (existingSection != null)
                {
                    sectionDto.UpdateEntity(existingSection);
                    
                    // Merge Fields
                    await MergeFieldsAsync(existingSection.Id, existingSection.Fields.ToList(), sectionDto.Fields, null);
                    
                    // Merge Matrix Columns
                    await MergeMatrixColumnsAsync(existingSection, sectionDto.MatrixColumns);
                }
                else
                {
                    var newSection = sectionDto.ToEntity(template.Id);
                    await _unitOfWork.ExtractionSections.AddAsync(newSection);

                    if (sectionDto.Fields != null)
                    {
                        foreach (var fieldDto in sectionDto.Fields)
                        {
                            var fieldEntities = fieldDto.ToEntitiesRecursive(newSection.Id, null);
                            foreach (var field in fieldEntities)
                            {
                                await _unitOfWork.ExtractionFields.AddAsync(field);
                            }
                        }
                    }

                    if (sectionDto.MatrixColumns != null)
                    {
                        foreach (var columnDto in sectionDto.MatrixColumns)
                        {
                            var columnEntity = columnDto.ToEntity(newSection.Id);
                            await _unitOfWork.ExtractionMatrixColumns.AddAsync(columnEntity);
                        }
                    }
                }
            }

            // 2. Delete missing
            var sectionsToDelete = existingSections.Where(s => !incomingSections.Any(dto => dto.SectionId == s.Id)).ToList();
            foreach (var section in sectionsToDelete)
            {
                await _unitOfWork.ExtractionSections.RemoveAsync(section);
            }
        }

        private async Task MergeFieldsAsync(Guid sectionId, List<ExtractionField> existingFields, List<ExtractionFieldDto> incomingFields, Guid? parentFieldId)
        {
            incomingFields ??= new List<ExtractionFieldDto>();

            // 1. Update or Add
            foreach (var fieldDto in incomingFields)
            {
                var existingField = existingFields.FirstOrDefault(f => f.Id == fieldDto.FieldId);
                if (existingField != null)
                {
                    fieldDto.UpdateEntity(existingField);
                    
                    // Merge Options
                    await MergeOptionsAsync(existingField, fieldDto.Options);
                    
                    // Merge SubFields (Recursive)
                    await MergeFieldsAsync(sectionId, existingField.SubFields.ToList(), fieldDto.SubFields, existingField.Id);
                }
                else
                {
                    var fieldEntities = fieldDto.ToEntitiesRecursive(sectionId, parentFieldId);
                    foreach (var field in fieldEntities)
                    {
                        await _unitOfWork.ExtractionFields.AddAsync(field);
                    }
                }
            }

            // 2. Delete missing
            var fieldsToDelete = existingFields.Where(f => !incomingFields.Any(dto => dto.FieldId == f.Id)).ToList();
            foreach (var field in fieldsToDelete)
            {
                await _unitOfWork.ExtractionFields.RemoveAsync(field);
            }
        }

        private async Task MergeOptionsAsync(ExtractionField field, List<FieldOptionDto> incomingOptions)
        {
            incomingOptions ??= new List<FieldOptionDto>();
            var existingOptions = field.Options.ToList();

            // 1. Update or Add
            foreach (var optionDto in incomingOptions)
            {
                var existingOption = existingOptions.FirstOrDefault(o => o.Id == optionDto.OptionId);
                if (existingOption != null)
                {
                    existingOption.Value = optionDto.Value;
                    existingOption.DisplayOrder = optionDto.DisplayOrder;
                    existingOption.ModifiedAt = DateTimeOffset.UtcNow;
                }
                else
                {
                    var newOption = optionDto.ToEntity(field.Id);
                    await _unitOfWork.FieldOptions.AddAsync(newOption);
                }
            }

            // 2. Delete missing
            var optionsToDelete = existingOptions.Where(o => !incomingOptions.Any(dto => dto.OptionId == o.Id)).ToList();
            foreach (var option in optionsToDelete)
            {
                await _unitOfWork.FieldOptions.RemoveAsync(option);
            }
        }

        private async Task MergeMatrixColumnsAsync(ExtractionSection section, List<ExtractionMatrixColumnDto> incomingColumns)
        {
            incomingColumns ??= new List<ExtractionMatrixColumnDto>();
            var existingColumns = section.MatrixColumns.ToList();

            // 1. Update or Add
            foreach (var columnDto in incomingColumns)
            {
                var existingColumn = existingColumns.FirstOrDefault(c => c.Id == columnDto.ColumnId);
                if (existingColumn != null)
                {
                    columnDto.UpdateEntity(existingColumn);
                }
                else
                {
                    var newColumn = columnDto.ToEntity(section.Id);
                    await _unitOfWork.ExtractionMatrixColumns.AddAsync(newColumn);
                }
            }

            // 2. Delete missing
            var columnsToDelete = existingColumns.Where(c => !incomingColumns.Any(dto => dto.ColumnId == c.Id)).ToList();
            foreach (var column in columnsToDelete)
            {
                await _unitOfWork.ExtractionMatrixColumns.RemoveAsync(column);
            }
        }

        public async Task<List<ExtractionFieldDto>> SuggestFieldsForSectionAsync(string sectionName, string projectContext = "")
        {
            var prompt = $@"You are a methodology expert in Systematic Literature Reviews (Software Engineering). 
I need to extract data from primary studies to answer the following Research Question (or Context): '{sectionName}'. 
{(string.IsNullOrEmpty(projectContext) ? "" : $"Additional context: {projectContext}")}
Suggest a JSON array of 3 to 5 highly relevant data extraction fields. 
Use this strict JSON schema: [{{ ""Name"": ""string"", ""Instruction"": ""string"", ""FieldType"": int, ""Options"": [{{ ""Value"": ""string"" }}] }}]. 
FieldType mapping: 0=Text, 1=Integer, 2=Decimal, 3=Boolean, 4=SingleSelect, 5=MultiSelect. 
For select types (4 or 5), provide 2-4 standard Options.";

            try
            {
                var response = await _openRouterService.GenerateStructuredContentAsync<List<ExtractionFieldDto>>(prompt);
                return response;
            }
            catch (Exception ex)
            {
                // Fallback or rethrow as a domain exception if preferred
                throw new InvalidOperationException($"Failed to suggest fields: {ex.Message}");
            }
        }

        public async Task<List<ExtractionTemplateDto>> GetTemplatesByProcessIdAsync(Guid processId)
        {
            var templates = await _unitOfWork.ExtractionTemplates
                .GetByProcessIdAsync(processId);

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
                .FindSingleAsync(t => t.Id == templateId)
                ?? throw new KeyNotFoundException($"Template {templateId} not found.");

            await EnsureLeaderAsync(template.DataExtractionProcessId);

            // Delete sections first (cascade will handle fields and options)
            await DeleteSectionsAsync(templateId);

            await _unitOfWork.ExtractionTemplates.RemoveAsync(template);
            await _unitOfWork.SaveChangesAsync();
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

                // Validate duplicate field names within section (only when fields are present)
                if (section.Fields != null && section.Fields.Count > 0)
                {
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
                        // Delete extracted data values referencing this field (prevents FK constraint violation)
                        var extractedValues = await _unitOfWork.ExtractedDataValues
                            .FindAllAsync(e => e.FieldId == field.Id);

                        if (extractedValues != null && extractedValues.Any())
                        {
                            foreach (var ev in extractedValues)
                            {
                                await _unitOfWork.ExtractedDataValues.RemoveAsync(ev);
                            }
                        }

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

                // Delete matrix columns in this section
                var matrixColumns = await _unitOfWork.ExtractionMatrixColumns
                    .FindAllAsync(c => c.SectionId == section.Id);

                if (matrixColumns != null)
                {
                    foreach (var column in matrixColumns)
                    {
                        await _unitOfWork.ExtractionMatrixColumns.RemoveAsync(column);
                    }
                }

                // Delete section
                await _unitOfWork.ExtractionSections.RemoveAsync(section);
            }
        }
    }
}