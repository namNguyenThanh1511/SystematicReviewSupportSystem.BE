# Title–Abstract Screening — Implementation Plan

## Summary

Implement the complete Title–Abstract Screening workflow (GAP report items G-01 through G-12), using **Option A (Separate Child Entities)** for phase separation. [StudySelectionProcess](file:///d:/Capstone-project/be/SRSS/SRSS.IAM/SRSS.IAM.Repositories/Entities/StudySelectionProcess.cs#8-56) remains the aggregate root; `TitleAbstractScreening` and `FullTextScreening` become 1:1 child entities, each owning their own screening decisions and resolutions.

---

## Proposed Changes

### Entity Layer

#### [NEW] [TitleAbstractScreening.cs](file:///d:/Capstone-project/be/SRSS/SRSS.IAM/SRSS.IAM.Repositories/Entities/TitleAbstractScreening.cs)
New entity as child of [StudySelectionProcess](file:///d:/Capstone-project/be/SRSS/SRSS.IAM/SRSS.IAM.Repositories/Entities/StudySelectionProcess.cs#8-56):
- `Guid StudySelectionProcessId`
- `ScreeningPhaseStatus Status` (NotStarted, InProgress, Completed)
- `DateTimeOffset? StartedAt`, `CompletedAt`
- `int MinReviewersPerPaper = 2`, `int MaxReviewersPerPaper = 3`
- Navigation: `ICollection<ScreeningDecision>`, `ICollection<ScreeningResolution>`
- Domain methods: [Start()](file:///d:/Capstone-project/be/SRSS/SRSS.IAM/SRSS.IAM.Repositories/Entities/StudySelectionProcess.cs#21-43), [Complete()](file:///d:/Capstone-project/be/SRSS/SRSS.IAM/SRSS.IAM.Repositories/Entities/ReviewProcess.cs#65-78), `ValidatePaperMetadata(Paper)`

#### [NEW] [FullTextScreening.cs](file:///d:/Capstone-project/be/SRSS/SRSS.IAM/SRSS.IAM.Repositories/Entities/FullTextScreening.cs)
Similar structure to `TitleAbstractScreening`, placeholder for Phase 2.

#### [NEW] [ExclusionReasonCode enum](file:///d:/Capstone-project/be/SRSS/SRSS.IAM/SRSS.IAM.Repositories/Entities/ExclusionReasonCode.cs)
Standardized codes from TA.md §5:
```
NotRelevantToTopic, NotRelevantPopulation, NotRelevantIntervention,
NotEmpiricalStudy, NotResearchPaper, OutsideTimeRange,
UnsupportedLanguage, DuplicateStudy, Other
```

#### [MODIFY] [ScreeningDecision.cs](file:///d:/Capstone-project/be/SRSS/SRSS.IAM/SRSS.IAM.Repositories/Entities/ScreeningDecision.cs)
- Add `ScreeningPhase Phase` enum (`TitleAbstract`, `FullText`)
- Add `ExclusionReasonCode? ExclusionReasonCode` (structured, replaces free-text for analytics)
- Add `string? ReviewerNotes` (separate from `Reason` per spec §9)
- Replace FK from `StudySelectionProcessId` → `TitleAbstractScreeningId` (nullable) + `FullTextScreeningId` (nullable)
  - *Or* keep `StudySelectionProcessId` and add `ScreeningPhase` discriminator — this is simpler and avoids breaking existing queries
- **Chosen approach**: Keep `StudySelectionProcessId` FK + add `ScreeningPhase` discriminator + new structured fields. This minimizes disruption to existing repositories.

#### [MODIFY] [ScreeningResolution.cs](file:///d:/Capstone-project/be/SRSS/SRSS.IAM/SRSS.IAM.Repositories/Entities/ScreeningResolution.cs)
- Add `ScreeningPhase Phase`

#### [MODIFY] [StudySelectionProcess.cs](file:///d:/Capstone-project/be/SRSS/SRSS.IAM/SRSS.IAM.Repositories/Entities/StudySelectionProcess.cs)
- Add navigation properties: `TitleAbstractScreening?`, `FullTextScreening?`
- Update [Start()](file:///d:/Capstone-project/be/SRSS/SRSS.IAM/SRSS.IAM.Repositories/Entities/StudySelectionProcess.cs#21-43) to also check protocol status
- Add `EnsureProtocolApproved()` domain guard (G-04, G-12)

#### [MODIFY] [ReviewProtocol.cs](file:///d:/Capstone-project/be/SRSS/SRSS.IAM/SRSS.IAM.Repositories/Entities/ReviewProtocol.cs)
- Add `ProtocolStatus.Locked` to enum
- Add `Lock()` domain method
- Modify [CanEdit()](file:///d:/Capstone-project/be/SRSS/SRSS.IAM/SRSS.IAM.Repositories/Entities/ReviewProtocol.cs#148-152) to return false when `Locked`

#### [MODIFY] [Paper.cs](file:///d:/Capstone-project/be/SRSS/SRSS.IAM/SRSS.IAM.Repositories/Entities/Paper.cs)
- No structural changes. Metadata validation will be in the service/entity domain methods.

---

### EF Core Configuration Layer

#### [NEW] [TitleAbstractScreeningConfiguration.cs](file:///d:/Capstone-project/be/SRSS/SRSS.IAM/SRSS.IAM.Repositories/Configurations/TitleAbstractScreeningConfiguration.cs)
Table `title_abstract_screenings`, 1:1 with [StudySelectionProcess](file:///d:/Capstone-project/be/SRSS/SRSS.IAM/SRSS.IAM.Repositories/Entities/StudySelectionProcess.cs#8-56).

#### [NEW] [FullTextScreeningConfiguration.cs](file:///d:/Capstone-project/be/SRSS/SRSS.IAM/SRSS.IAM.Repositories/Configurations/FullTextScreeningConfiguration.cs)
Table `full_text_screenings`, 1:1 with [StudySelectionProcess](file:///d:/Capstone-project/be/SRSS/SRSS.IAM/SRSS.IAM.Repositories/Entities/StudySelectionProcess.cs#8-56).

#### [MODIFY] [ScreeningDecisionConfiguration.cs](file:///d:/Capstone-project/be/SRSS/SRSS.IAM/SRSS.IAM.Repositories/Configurations/ScreeningDecisionConfiguration.cs)
Add columns: `screening_phase`, `exclusion_reason_code`, `reviewer_notes`.

#### [MODIFY] [ScreeningResolutionConfiguration.cs](file:///d:/Capstone-project/be/SRSS/SRSS.IAM/SRSS.IAM.Repositories/Configurations/ScreeningResolutionConfiguration.cs)
Add column: `screening_phase`.

#### [MODIFY] [StudySelectionProcessConfiguration.cs](file:///d:/Capstone-project/be/SRSS/SRSS.IAM/SRSS.IAM.Repositories/Configurations/StudySelectionProcessConfiguration.cs)
Add 1:1 navigations to `TitleAbstractScreening` and `FullTextScreening`.

#### [MODIFY] [AppDbContext.cs](file:///d:/Capstone-project/be/SRSS/SRSS.IAM/SRSS.IAM.Repositories/AppDbContext.cs)
Register `DbSet<TitleAbstractScreening>` and `DbSet<FullTextScreening>`.

---

### Repository Layer

#### [NEW] TitleAbstractScreeningRepo/
- `ITitleAbstractScreeningRepository.cs` — extends `IGenericRepository`, adds `GetByProcessIdAsync`
- `TitleAbstractScreeningRepository.cs` — implementation

#### [MODIFY] [IUnitOfWork.cs](file:///d:/Capstone-project/be/SRSS/SRSS.IAM/SRSS.IAM.Repositories/UnitOfWork/IUnitOfWork.cs)
Add `ITitleAbstractScreeningRepository TitleAbstractScreenings { get; }`

#### [MODIFY] [UnitOfWork.cs](file:///d:/Capstone-project/be/SRSS/SRSS.IAM/SRSS.IAM.Repositories/UnitOfWork/UnitOfWork.cs)
Wire up the new repository.

---

### Service & DTO Layer

#### [MODIFY] [StudySelectionDto.cs](file:///d:/Capstone-project/be/SRSS/SRSS.IAM/SRSS.IAM.Services/DTOs/StudySelection/StudySelectionDto.cs)
- Add `ScreeningPhase` enum
- Update [SubmitScreeningDecisionRequest](file:///d:/Capstone-project/be/SRSS/SRSS.IAM/SRSS.IAM.Services/DTOs/StudySelection/StudySelectionDto.cs#15-21): add `ExclusionReasonCode?`, `ReviewerNotes?`
- Update [ScreeningDecisionResponse](file:///d:/Capstone-project/be/SRSS/SRSS.IAM/SRSS.IAM.Services/DTOs/StudySelection/StudySelectionDto.cs#57-70): add `ExclusionReasonCode`, `ReviewerNotes`, `Phase`
- Update [ScreeningResolutionResponse](file:///d:/Capstone-project/be/SRSS/SRSS.IAM/SRSS.IAM.Services/DTOs/StudySelection/StudySelectionDto.cs#71-84): add `Phase`
- Add `ExclusionReasonBreakdownItem` to [SelectionStatisticsResponse](file:///d:/Capstone-project/be/SRSS/SRSS.IAM/SRSS.IAM.Services/DTOs/StudySelection/StudySelectionDto.cs#121-131)
- Add `TitleAbstractScreeningResponse` DTO

#### [MODIFY] [StudySelectionService.cs](file:///d:/Capstone-project/be/SRSS/SRSS.IAM/SRSS.IAM.Services/StudySelectionService/StudySelectionService.cs)
- Add `CreateTitleAbstractScreeningAsync` (creates TA phase child)
- Add `StartTitleAbstractScreeningAsync` (with protocol lock + metadata validation)
- Refactor [SubmitScreeningDecisionAsync](file:///d:/Capstone-project/be/SRSS/SRSS.IAM/SRSS.IAM.Services/StudySelectionService/StudySelectionService.cs#185-245) to be phase-aware (pass `ScreeningPhase`)
- Add majority-decision auto-resolution logic in decision submission (G-05, G-06)
- Update [GetSelectionStatisticsAsync](file:///d:/Capstone-project/be/SRSS/SRSS.IAM/SRSS.IAM.Services/StudySelectionService/StudySelectionService.cs#400-451) to include exclusion reason breakdown (G-10)
- Add `ValidatePaperMetadata` helper (G-07)

#### [MODIFY] [IStudySelectionService.cs](file:///d:/Capstone-project/be/SRSS/SRSS.IAM/SRSS.IAM.Services/StudySelectionService/IStudySelectionService.cs)
Add new method signatures for TA screening lifecycle.

---

### Controller Layer

#### [MODIFY] [StudySelectionController.cs](file:///d:/Capstone-project/be/SRSS/SRSS.IAM/SRSS.IAM.API/Controllers/StudySelectionController.cs)
- Add endpoints for TA screening lifecycle:
  - `POST study-selection/{id}/title-abstract` — create
  - `POST study-selection/{id}/title-abstract/start` — start
  - `POST study-selection/{id}/title-abstract/complete` — complete
  - `GET study-selection/{id}/title-abstract` — get status
- Add `ScreeningPhase` query param to existing decision/conflict endpoints

---

## Verification Plan

### Automated Tests
No existing test project found. Verification will be:

```powershell
# Build the solution to verify no compilation errors
dotnet build d:\Capstone-project\be\SRSS\SRSS.sln
```

### Manual Verification
After building successfully, the user can:
1. Run `dotnet ef migrations add TitleAbstractScreening` to verify EF model is valid
2. Inspect the generated migration SQL to verify table/column structure
3. Test the API endpoints via Swagger/Postman
