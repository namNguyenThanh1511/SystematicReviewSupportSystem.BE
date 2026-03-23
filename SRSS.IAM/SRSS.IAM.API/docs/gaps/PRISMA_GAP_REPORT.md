# 🚨 PRISMA 2020 API Contract Gap Report

This report identifies critical discrepancies between the Systematic Review Support System's Backend API and the PRISMA 2020 UI requirements.

## 📊 Summary of Findings

| Severity | Count | issues |
| :--- | :--- | :--- |
| **BLOCKER** | 3 | Data Hallucination, Missing Stages, Contract Mismatch |
| **WARNING** | 2 | Placeholder Logic (0-counts), Inconsistent Docs |
| **NICE_TO_HAVE** | 1 | Notes Field usage |

---

## 🚨 BLOCKER ISSUES

### 1. Frontend Data Hallucination
*   **Location**: [src/pages/reviewProcess/prismaReport/sections/PrismaFlowDiagram.tsx](file:///d:/Capstone-project/fe/SystematicReviewSupportSystem.Web/src/pages/reviewProcess/prismaReport/sections/PrismaFlowDiagram.tsx) (Lines 351–360)
*   **Issue**: The UI is faking source breakdowns (IEEE, PubMed, OpenAlex) using hardcoded `Math.floor(total * 0.X)` percentages.
*   **Why this is a problem**: It presents false data to the user. A systematic review tool must provide audit-trailed, accurate counts from the database.
*   **Backend Recommendation**: Add `breakdown: { label: string; count: number }[]` to [PrismaFlowRecordResponse](file:///d:/Capstone-project/fe/SystematicReviewSupportSystem.Web/src/types/prismaReport.ts#55-70). The backend must query `ImportBatch.SourceName` to populate this for the Identification stage.

### 2. Missing PRISMA Stages (5–8)
*   **Endpoint**: `GET /api/review-processes/{id}/prisma-report/latest`
*   **Issue**: The API only return records for stages 0-4 and 9. Stages 5 (`ReportsSoughtForRetrieval`), 6 (`ReportsNotRetrieved`), 7 (`ReportsAssessed`), 8 (`ReportsExcluded`) are absent.
*   **Why this is a problem**: The PRISMA 2020 standard requires the Eligibility and Retrieval phases. The UI currently shows `0` for these, which is misleading as the stages aren't even being calculated.
*   **Backend Recommendation**: Implement calculation logic for these stages. Even if 0, these records MUST be returned so the UI can distinguish "computed-as-zero" from "unimplemented".

### 3. Contract Mismatch: Flat vs. Hierarchical
*   **API Model**: `PrismaFlowRecordResponse[]` (Flat list)
*   **UI Model**: [PrismaFlowNode](file:///d:/Capstone-project/fe/SystematicReviewSupportSystem.Web/src/types/prismaReport.ts#131-139) (Recursive/Side-box structure)
*   **Issue**: The UI has to "manually stitch" the flat list into a PRISMA tree. This logic is brittle and currently involves hardcoding which stage is a "main" box vs. a "side" box.
*   **Backend Recommendation**: Adopt the structure proposed in [PRISMA_API_DOC.md](file:///d:/Capstone-project/fe/SystematicReviewSupportSystem.Web/src/docs/PRISMA_API_DOC.md) which uses a `nodes` array where each node can optionally contain a `sideBox`. This aligns the API with the visual hierarchy of the PRISMA diagram.

---

## ⚠️ WARNINGS

### 4. Placeholder Counts (Always 0)
*   **Fields**: `RecordsExcluded` (Stage 4) and `StudiesIncluded` (Stage 9).
*   **Issue**: Backend documentation admits these are tracked but currently return `0` as a placeholder.
*   **Backend Recommendation**: Complete the TODOs in the backend to query `ScreeningResolution` and `ScreeningDecision` entities.

### 5. Documentation Inconsistency
*   **Discrepancy**: [src/docs/PRISMA_API_DOC.md](file:///d:/Capstone-project/fe/SystematicReviewSupportSystem.Web/src/docs/PRISMA_API_DOC.md) (Node-based) vs [src/docs/PrismaReportApiDoc.md](file:///d:/Capstone-project/fe/SystematicReviewSupportSystem.Web/src/docs/PrismaReportApiDoc.md) (Record-based).
*   **Why this is a problem**: Developer confusion. New developers may implement against the wrong spec.
*   **Recommendation**: Deprecate [PrismaReportApiDoc.md](file:///d:/Capstone-project/fe/SystematicReviewSupportSystem.Web/src/docs/PrismaReportApiDoc.md) and move towards the Node-based structure in [PRISMA_API_DOC.md](file:///d:/Capstone-project/fe/SystematicReviewSupportSystem.Web/src/docs/PRISMA_API_DOC.md).

---

## 🛠️ Proposed Integration Action Plan

1.  **Frontend**: Remove all `Math.floor` hallucinations immediately. Replace with "Not available" or [(n=?)](file:///d:/Capstone-project/fe/SystematicReviewSupportSystem.Web/src/hooks/usePrismaReport.ts#97-105) if data is missing from API.
2.  **API**: Update [PrismaFlowRecordResponse](file:///d:/Capstone-project/fe/SystematicReviewSupportSystem.Web/src/types/prismaReport.ts#55-70) to include `breakdown` and `reasons` arrays.
3.  **API**: Ensure all 10 stages are returned consistently.
