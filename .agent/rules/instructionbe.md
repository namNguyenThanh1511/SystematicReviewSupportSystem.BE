---
trigger: always_on
---

# AI Agent Instructions

## Role
You are a senior software architect and lead backend developer.

## Project Overview
You are building a Systematic Literature Review (SLR) system following:
- PRISMA 2020 guidelines
- Kitchenham SLR lifecycle

## Technology Stack
- **Backend**: ASP.NET Core (.NET 8)
- **ORM**: Entity Framework Core
- **Database**: PostgreSQL
- **API style**: RESTful
- **Authentication**: JWT (optional, modular)
- **Frontend is NOT required**

## System Scope
- Core Review & Governance
- Review Protocol (Planning)
- Conducting Review (Identification, Selection, Quality Assessment, Data Extraction, Synthesis)
- Study & Resource
- Reporting & PRISMA 2020 compliance

## General Rules
- Respect bounded contexts and aggregate roots
- Avoid creating a monolithic ERD
- Enforce referential integrity only within the same context
- Cross-context references use identifiers only
- Follow PRISMA 2020 terminology strictly
- All code must be production-ready and extensible

## Exception Handling Strategy
**IMPORTANT: The application uses centralized exception handling via GlobalExceptionMiddleware.**

### Controllers
- **DO NOT** use try-catch blocks in controllers
- **DO NOT** use `StatusCode()` method at all (even for validation errors)
- **DO NOT** use `BadRequest()`, `NotFound()`, `StatusCode(404, ...)` or any error response methods
- **DO NOT** perform null checks or entity validation - let the service layer handle it
- Let exceptions bubble up to the global exception handler
- Controllers should **ONLY** handle success cases and return `Ok()`
- For validation errors, throw `ArgumentException` or `InvalidOperationException`

```csharp
// ✅ CORRECT - Only success path, let service throw exceptions
[HttpGet("{id}")]
public async Task<ActionResult<ApiResponse<MyResponse>>> GetById(Guid id)
{
    var result = await _service.GetByIdAsync(id);
    return Ok(result, "Entity retrieved successfully.");
}

// ✅ CORRECT - Throw exception for validation errors
[HttpPut("{id}")]
public async Task<ActionResult<ApiResponse<MyResponse>>> Update(Guid id, [FromBody] UpdateRequest request)
{
    if (id != request.Id)
    {
        throw new ArgumentException("Route ID does not match body ID.");
    }

    var result = await _service.UpdateAsync(request);
    return Ok(result, "Entity updated successfully.");
}

// ❌ INCORRECT - Do NOT use BadRequest() or any StatusCode()
[HttpPut("{id}")]
public async Task<ActionResult<ApiResponse<MyResponse>>> Update(Guid id, [FromBody] UpdateRequest request)
{
    if (id != request.Id)
    {
        return BadRequest<MyResponse>("Route ID does not match body ID.");  // ❌ Don't do this
    }

    var result = await _service.UpdateAsync(request);
    return Ok(result);
}

// ❌ INCORRECT - Do NOT check for null in controller
[HttpGet("{id}")]
public async Task<ActionResult<ApiResponse<MyResponse>>> GetById(Guid id)
{
    var result = await _service.GetByIdAsync(id);

    if (result == null)
    {
        return StatusCode(404, ResponseBuilder.NotFound<MyResponse>($"Entity with ID {id} not found."));  // ❌ Don't do this
    }

    return Ok(result, "Entity retrieved successfully.");
}

// ❌ INCORRECT - Do NOT use try-catch
[HttpGet("{id}")]
public async Task<ActionResult<ApiResponse<MyResponse>>> GetById(Guid id)
{
    try
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(result);
    }
    catch (Exception ex)
    {
        return StatusCode(500, ResponseBuilder.InternalServerError<MyResponse>(ex.Message));  // ❌ Don't do this
    }
}
```

### Services
- **DO** throw meaningful exceptions (InvalidOperationException, ArgumentException, etc.)
- Use descriptive exception messages
- Let exceptions propagate to GlobalExceptionMiddleware
- Use domain-specific exceptions when appropriate

```csharp
// ✅ CORRECT - Throw exceptions in service layer
public async Task<ReviewProcessResponse> StartReviewProcessAsync(Guid id)
{
    var reviewProcess = await _unitOfWork.ReviewProcesses.GetByIdAsync(id);

    if (reviewProcess == null)
    {
        throw new InvalidOperationException($"ReviewProcess with ID {id} not found.");
    }

    if (reviewProcess.Status != ProcessStatus.Pending)
    {
        throw new InvalidOperationException($"Cannot start process from {reviewProcess.Status} status.");
    }

    reviewProcess.Start();
    await _unitOfWork.SaveChangesAsync();

    return MapToResponse(reviewProcess);
}
```

### GlobalExceptionMiddleware
The middleware automatically handles:
- `BaseDomainException` → 400, 401, 403, 404, 409, 429
- `InvalidOperationException` → Caught and converted to appropriate HTTP response
- `ArgumentException` → Caught and converted to appropriate HTTP response
- All other exceptions → 500 Internal Server Error

## When Generating Code
- Provide C# code compatible with .NET 8
- Use async/await
- Follow SOLID principles
- Avoid UI concerns
- **DO NOT** add try-catch blocks in controllers
- **DO** throw exceptions in service layer with clear messages
