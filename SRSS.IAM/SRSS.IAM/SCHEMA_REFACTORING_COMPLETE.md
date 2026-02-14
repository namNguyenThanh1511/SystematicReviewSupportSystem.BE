# Schema Refactoring: Proper Systematic Review Methodology

## ✅ **REFACTORING COMPLETE - NEW ENTITIES CREATED**

### 🎯 **What Was Changed**

Refactored the database schema to follow proper systematic review methodology principles by separating concerns and supporting multi-reviewer workflows.

---

## 📊 **NEW ARCHITECTURE**

```
SystematicReviewProject (1)
  └── ReviewProcess (N)
        ├── IdentificationProcess (paper import)
        ├── StudySelectionProcess (screening phase)
        │      ├── ScreeningDecision (N) - multi-reviewer
        │      └── ScreeningResolution (1 per paper) - final decision
        ├── DataExtractionProcess (future)
        └── PrismaReport (N versions) - snapshot of results
```

---

## 🆕 **NEW ENTITIES CREATED**

### **1. StudySelectionProcess**
- Represents the screening/selection phase
- Links to ReviewProcess
- Has status: NotStarted, InProgress, Completed
- One-to-many with ScreeningDecisions and ScreeningResolutions

### **2. ScreeningDecision**
- Individual reviewer's decision on a paper
- **Supports multi-reviewer workflow**
- Fields: reviewer_id, paper_id, decision (Include/Exclude), reason, decided_at
- Multiple decisions per paper possible

### **3. ScreeningResolution**
- **Final resolved decision** for a paper in a selection process
- **Unique constraint**: (study_selection_process_id, paper_id)
- Fields: final_decision, resolved_by, resolved_at
- This is the authoritative decision

---

## 🔄 **MODIFIED ENTITIES**

### **Paper Entity**
❌ **Removed**: `IsIncludedFinal`  
✅ **Added**: Navigation properties for ScreeningDecisions and ScreeningResolutions  
✅ **Kept**: `CurrentSelectionStatus` (useful for quick filtering), `IsDuplicate`, `DuplicateOfId`

### **PrismaReport Entity**
❌ **Removed**: `ProjectId`  
✅ **Changed**: Now links to `ReviewProcessId` instead  
📝 **Rationale**: Reports are snapshots of a specific review process, not the entire project

### **ReviewProcess Entity**
✅ **Added**: Navigation properties:
- `StudySelectionProcesses` collection
- `PrismaReports` collection

---

## 🗄️ **NEW DATABASE TABLES**

### **study_selection_processes**
```sql
CREATE TABLE study_selection_processes (
    id UUID PRIMARY KEY,
    review_process_id UUID NOT NULL REFERENCES review_processes(id) ON DELETE CASCADE,
    notes TEXT,
    started_at TIMESTAMPTZ,
    completed_at TIMESTAMPTZ,
    status TEXT NOT NULL, -- NotStarted, InProgress, Completed
    created_at TIMESTAMPTZ NOT NULL,
    modified_at TIMESTAMPTZ NOT NULL
);

CREATE INDEX idx_study_selection_processes_review_process_id 
    ON study_selection_processes(review_process_id);
```

### **screening_decisions**
```sql
CREATE TABLE screening_decisions (
    id UUID PRIMARY KEY,
    study_selection_process_id UUID NOT NULL REFERENCES study_selection_processes(id) ON DELETE CASCADE,
    paper_id UUID NOT NULL REFERENCES papers(id) ON DELETE CASCADE,
    reviewer_id UUID NOT NULL,
    decision TEXT NOT NULL, -- Include, Exclude
    reason TEXT,
    decided_at TIMESTAMPTZ NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    modified_at TIMESTAMPTZ NOT NULL
);

CREATE INDEX idx_screening_decisions_process ON screening_decisions(study_selection_process_id);
CREATE INDEX idx_screening_decisions_paper ON screening_decisions(paper_id);
CREATE INDEX idx_screening_decisions_reviewer ON screening_decisions(reviewer_id);
CREATE INDEX idx_screening_decisions_composite 
    ON screening_decisions(study_selection_process_id, paper_id, reviewer_id);
```

### **screening_resolutions**
```sql
CREATE TABLE screening_resolutions (
    id UUID PRIMARY KEY,
    study_selection_process_id UUID NOT NULL REFERENCES study_selection_processes(id) ON DELETE CASCADE,
    paper_id UUID NOT NULL REFERENCES papers(id) ON DELETE CASCADE,
    final_decision TEXT NOT NULL, -- Include, Exclude
    resolution_notes TEXT,
    resolved_by UUID NOT NULL,
    resolved_at TIMESTAMPTZ NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    modified_at TIMESTAMPTZ NOT NULL,
    
    -- UNIQUE CONSTRAINT: One resolution per paper per process
    CONSTRAINT uq_screening_resolution_process_paper 
        UNIQUE (study_selection_process_id, paper_id)
);

CREATE INDEX idx_screening_resolutions_paper ON screening_resolutions(paper_id);
CREATE INDEX idx_screening_resolutions_resolved_by ON screening_resolutions(resolved_by);
```

---

## 🔄 **MODIFIED TABLES**

### **prisma_reports**
```sql
-- BEFORE:
project_id UUID NOT NULL REFERENCES systematic_review_projects(id) ON DELETE CASCADE

-- AFTER:
review_process_id UUID NOT NULL REFERENCES review_processes(id) ON DELETE CASCADE
```

### **papers**
```sql
-- REMOVED:
is_included_final BOOLEAN NOT NULL DEFAULT false

-- KEPT:
current_selection_status TEXT NOT NULL
is_duplicate BOOLEAN NOT NULL DEFAULT false
duplicate_of_id UUID REFERENCES papers(id) ON DELETE SET NULL
```

---

## 🎯 **KEY DESIGN PRINCIPLES**

### **1. Separation of Concerns**
- ✅ Papers store metadata only
- ✅ Decisions stored separately in screening tables
- ✅ Final decisions in resolution table
- ✅ PRISMA reports snapshot the process

### **2. Multi-Reviewer Support**
```
Paper 1:
  ├─ Reviewer A: Include (reason: "Meets criteria")
  ├─ Reviewer B: Exclude (reason: "Wrong methodology")
  └─ Resolution: Include (resolved_by: Lead Researcher)
```

### **3. Reproducibility**
- ✅ All decisions tracked with timestamp and reviewer
- ✅ Final resolutions documented
- ✅ Multiple selection processes per review possible
- ✅ Historical data preserved

### **4. Cascade Delete Integrity**
```
ReviewProcess deleted
  ↓
StudySelectionProcess deleted
  ↓
All ScreeningDecisions deleted
All ScreeningResolutions deleted
PrismaReports deleted
```

---

## 📊 **PRISMA Report Calculation** (Updated Logic)

### **Before** (INCORRECT):
```csharp
StudiesIncluded = papers.Count(p => p.IsIncludedFinal && !p.IsDuplicate)
```

### **After** (CORRECT):
```csharp
// Get the StudySelectionProcess for this ReviewProcess
var selectionProcess = await GetSelectionProcessForReview(reviewProcessId);

// Calculate from resolutions, not from Paper entity
StudiesIncluded = await _context.ScreeningResolutions
    .Where(sr => sr.StudySelectionProcessId == selectionProcess.Id)
    .Where(sr => sr.FinalDecision == ScreeningDecisionType.Include)
    .Join(papers, sr => sr.PaperId, p => p.Id, (sr, p) => p)
    .Where(p => !p.IsDuplicate)
    .CountAsync();
```

---

## 🔄 **WORKFLOW EXAMPLE**

### **Phase 1: Identification**
```
1. Import 100 papers from Scopus → IdentificationProcess
2. Import 80 papers from IEEE (20 duplicates) → IdentificationProcess
3. System marks duplicates automatically
   Result: 180 papers, 20 duplicates
```

### **Phase 2: Screening (NEW)**
```
4. Create StudySelectionProcess
5. Assign papers to reviewers
6. Reviewer A screens 90 papers
   - 60 Include decisions
   - 30 Exclude decisions
7. Reviewer B screens same 90 papers (double screening)
   - 58 Include decisions
   - 32 Exclude decisions
8. Resolve conflicts → Create ScreeningResolutions
   - Final: 59 Include, 31 Exclude
```

### **Phase 3: PRISMA Report**
```
9. Generate PrismaReport (linked to ReviewProcess)
   - Records identified: 180
   - Duplicates removed: 20
   - Records screened: 160
   - Records excluded: 101
   - Studies included: 59
```

---

## ✅ **BENEFITS OF NEW ARCHITECTURE**

### **1. Methodological Correctness**
- ✅ Follows PRISMA 2020 guidelines
- ✅ Supports proper screening workflow
- ✅ Tracks all decisions transparently

### **2. Multi-Reviewer Support**
- ✅ Multiple reviewers can screen same paper
- ✅ Conflicts can be identified
- ✅ Resolution process documented

### **3. Reproducibility**
- ✅ Complete audit trail
- ✅ Can regenerate PRISMA diagram anytime
- ✅ Historical decisions preserved

### **4. Flexibility**
- ✅ Multiple selection processes per review
- ✅ Can restart screening if needed
- ✅ Different screening strategies supported

---

## 🚀 **NEXT STEPS**

### **1. Update PrismaReportService**
```csharp
// Change calculation logic to use ScreeningResolution
var selectionProcess = await GetLatestSelectionProcess(reviewProcessId);
var includedCount = await GetResolutionCount(selectionProcess.Id, Include);
```

### **2. Create ScreeningService**
```csharp
// New service for managing screening workflow
- CreateDecisionAsync(reviewerId, paperId, decision)
- ResolveConflictAsync(paperId, finalDecision, resolvedBy)
- GetConflictsAsync(selectionProcessId)
```

### **3. Update Controllers**
```csharp
// Change endpoint
// Before: POST /api/projects/{projectId}/prisma-report
// After:  POST /api/review-processes/{reviewProcessId}/prisma-report
```

### **4. Update Repository Methods**
```csharp
// Change from projectId to reviewProcessId
GetReportsByReviewProcessAsync(reviewProcessId)
GetLatestReportByReviewProcessAsync(reviewProcessId)
```

---

## 📚 **FILES CREATED/MODIFIED**

### Created (7 files):
1. ✅ `StudySelectionProcess.cs` - Entity
2. ✅ `ScreeningDecision.cs` - Entity
3. ✅ `ScreeningResolution.cs` - Entity
4. ✅ `StudySelectionProcessConfiguration.cs` - EF Config
5. ✅ `ScreeningDecisionConfiguration.cs` - EF Config
6. ✅ `ScreeningResolutionConfiguration.cs` - EF Config

### Modified (8 files):
7. ✅ `Paper.cs` - Removed IsIncludedFinal, added navigations
8. ✅ `PrismaReport.cs` - Changed ProjectId to ReviewProcessId
9. ✅ `ReviewProcess.cs` - Added navigation properties
10. ✅ `PrismaReportConfiguration.cs` - Updated FK
11. ✅ `PaperConfiguration.cs` - Removed IsIncludedFinal column
12. ✅ `AppDbContext.cs` - Added new DbSets
13. ✅ `PrismaReportDto.cs` - Updated to use ReviewProcessId
14. ✅ `IPrismaReportService.cs` - Changed method signatures

---

## ⚠️ **MIGRATION REQUIRED**

```bash
cd SRSS.IAM.Repositories
dotnet ef migrations add RefactorScreeningWorkflow --startup-project ../SRSS.IAM.API
```

**Migration will**:
- Create 3 new tables
- Modify prisma_reports (project_id → review_process_id)
- Remove is_included_final from papers
- Create all foreign keys and indexes

---

## 🎓 **ACADEMIC COMPLIANCE**

✅ **PRISMA 2020** - Proper flow tracking  
✅ **Cochrane Guidelines** - Multi-reviewer support  
✅ **Transparency** - Complete decision trail  
✅ **Reproducibility** - All data preserved  
✅ **Quality** - Conflict resolution documented  

---

## 🎉 **SUMMARY**

**Before**: Decisions stored in Paper entity, reports linked to Project  
**After**: Proper screening workflow, decisions in separate tables, reports linked to ReviewProcess

**Result**: Scientifically correct, reproducible, multi-reviewer systematic review system! 🚀
