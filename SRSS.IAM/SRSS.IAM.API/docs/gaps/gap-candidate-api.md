# API Integration Analysis & Gap Report

## 1. UI ↔ API Mapping

| UI Level | Component | Primary API Endpoint | Key Fields Required |
| :--- | :--- | :--- | :--- |
| **Level 1** | [SnowballingPaperList](file:///d:/Capstone-project/fe/SystematicReviewSupportSystem.Web/src/components/reviewProcess/leader/snowballing/SnowballingPaperList.tsx#20-158) | `GET .../papers-with-candidates` | `title`, `authors`, `totalRefs`, `highConfCount`, `dupCount` |
| **Level 2** | [SnowballingPaperWorkspace](file:///d:/Capstone-project/fe/SystematicReviewSupportSystem.Web/src/components/reviewProcess/leader/snowballing/SnowballingPaperWorkspace.tsx#26-226) | `GET .../papers/{id}/candidates` | `abstract`, `totalRefs`, `highConfCount`, list of candidates |
| **Level 3** | [CandidateDetailPanel](file:///d:/Capstone-project/fe/SystematicReviewSupportSystem.Web/src/components/reviewProcess/leader/snowballing/CandidateDetailPanel.tsx#15-203) | — (From list item) | `title`, `doi`, `status`, `rawReference`, `confidenceScore` |

---

## 2. 🚨 Gap Report

### 🚩 Issue 1: Missing Statistics in Paper Overview
- **Endpoint**: `GET /api/review-processes/{processId}/papers-with-candidates`
- **Severity**: **BLOCKER**
- **What UI Needs**: `highConfidenceCount`, `duplicateCount`.
- **Current API Provides**: Only `candidateCount`.
- **Problem**: The Level 1 UI (Table List) specifically summarizes the quality of extraction per paper. Without these counts, the user cannot prioritize which paper to review first.
- **Recommendation**: Update `PaperWithCandidateDto` to include `highConfidenceCount` and `duplicateCount`.

### 🚩 Issue 2: Missing Abstract for Workspace Context
- **Endpoint**: `GET /api/review-processes/{processId}/papers-with-candidates`
- **Severity**: **WARNING**
- **What UI Needs**: `abstract`.
- **Current API Provides**: `title`, `authors`, but no `abstract`.
- **Problem**: Level 2 (Workspace) shows the origin paper's abstract in the left panel to help the reviewer decide if the references are relevant.
- **Recommendation**: Add `abstract: string` to `PaperWithCandidateDto`.

### 🚩 Issue 3: Missing Raw Reference Text
- **Endpoint**: All Candidate List endpoints.
- **Severity**: **BLOCKER**
- **What UI Needs**: `rawReference`.
- **Current API Provides**: Not listed in [CandidatePaperDto](file:///d:/Capstone-project/fe/SystematicReviewSupportSystem.Web/src/types/paper.ts#406-421) (Section 5 Tip vs Model description).
- **Problem**: Level 3 (Detail) relies on the "Raw Extracted Text" to show the user exactly where GROBID found the reference. Without this, the UI is significantly less useful for verification.
- **Recommendation**: Explicitly include `rawReference: string` in [CandidatePaperDto](file:///d:/Capstone-project/fe/SystematicReviewSupportSystem.Web/src/types/paper.ts#406-421).

---

## 3. Updated TypeScript Types (v4 Aligned)

```typescript
export enum CandidateStatus {
  Detected = 0,
  Rejected = 1,
  Imported = 2,
  Duplicate = 3,
  Processed = 4,
  Added = 5,
}

export interface CandidatePaperDto {
  candidateId: string;
  originPaperId: string;
  originPaperTitle: string;
  title: string;
  authors: string;
  publicationYear: string;
  doi: string;
  status: CandidateStatus;
  statusText: string;
  confidenceScore: number;
  rawReference?: string; // GAP: Critical for Detail UI
}

export interface PaperWithCandidateDto {
  id: string;
  title: string;
  authors: string;
  publicationYear: string;
  doi: string;
  candidateCount: number;
  // RECOMMENDATIONS:
  highConfidenceCount?: number; 
  duplicateCount?: number;
  abstract?: string;
}
```

---

## 4. Suggested Integration (React Query)

```typescript
// useSnowballing.ts
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { api } from "./api-client";

export const useSourcePapers = (processId: string) => {
  return useQuery({
    queryKey: ["snowballing", processId, "papers"],
    queryFn: () => api.get(`/review-processes/${processId}/papers-with-candidates`).then(res => res.data),
  });
};

export const usePaperCandidates = (processId: string, paperId: string) => {
  return useQuery({
    queryKey: ["snowballing", processId, "paper", paperId, "candidates"],
    queryFn: () => api.get(`/review-processes/${processId}/papers/${paperId}/candidates`).then(res => res.data),
    enabled: !!paperId,
  });
};
```

---

## 5. Frontend Risk Notes
1. **Extraction Latency**: The `extract-references` endpoint starts GROBID. The UI MUST poll or wait for a background status update before the paper appears in Level 1 with its counts.
2. **"Processed" Status**: As per integration tips, candidates in `Detected` status cannot be promoted. The frontend should disable "Promote" buttons and show a "Processing..." tooltip if `status === 0`.
3. **Pagination**: The UI needs to correctly pass `pageNumber` and `pageSize` to all list endpoints to handle large reference pools (often 50+ per paper).
