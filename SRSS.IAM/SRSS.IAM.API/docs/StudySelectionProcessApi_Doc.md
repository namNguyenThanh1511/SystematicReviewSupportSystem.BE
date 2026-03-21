# Study Selection Process API Documentation

This document describes the API for retrieving the details of a Study Selection Process, including independent statistics for both Title/Abstract and Full-Text screening phases.

## 1. Get Study Selection Process

Retrieves the current state, metadata, and progress statistics for a study selection process.

- **Method**: `GET`
- **Route**: `api/study-selection/{id}`
- **Auth**: Required (JWT Bearer Token)

### Request Details

#### Path Parameters
- `id` (Guid, Required): The unique identifier of the Study Selection Process.

### Response Details

Returns a `StudySelectionProcessResponse` containing process metadata and phase-based statistics.

#### Response Schema (TypeScript)
```typescript
interface StudySelectionProcessResponse {
    id: string;
    reviewProcessId: string;
    notes?: string;
    startedAt?: string; // ISO DateTime
    completedAt?: string; // ISO DateTime
    status: number; // 0: Pending, 1: InProgress, 2: Completed, 3: Canceled
    statusText: string;
    createdAt: string;
    modifiedAt: string;
    
    /** @deprecated Use phaseStatistics for phase-specific counts. Mirrors TitleAbstract stats. */
    selectionStatistics: SelectionStatisticsResponse;
    
    /** Statistics separated by screening phase */
    phaseStatistics: {
        titleAbstract: SelectionStatisticsResponse;
        fullText: SelectionStatisticsResponse;
    };
}

interface SelectionStatisticsResponse {
    studySelectionProcessId: string;
    totalPapers: number;
    includedCount: number;
    excludedCount: number;
    conflictCount: number;
    pendingCount: number;
    completionPercentage: number;
    exclusionReasonBreakdown: ExclusionReasonBreakdownItem[];
}

interface ExclusionReasonBreakdownItem {
    reasonCode: number;
    reasonText: string;
    count: number;
}
```

### Business Logic Summary

1. **Parallel Phases**: The system treats Title/Abstract and Full-Text screening as a pipeline.
2. **Title/Abstract (Phase 0)**: Includes all papers imported during the Identification stage.
3. **Full-Text (Phase 1)**: Only includes papers that received an "Include" resolution in Phase 0.
4. **Statistics Progress**: 
   - `totalPapers` in `fullText` stats will grow dynamically as papers are included in Phase 0.
   - `completionPercentage` reflects how many eligible papers in that specific phase have reached a final decision.

### Example Response
```json
{
  "success": true,
  "message": "Study Selection Process retrieved successfully.",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "reviewProcessId": "4ba96g75-6828-5673-c4gd-3d074g77bgb7",
    "status": 1,
    "statusText": "InProgress",
    "phaseStatistics": {
      "titleAbstract": {
        "totalPapers": 100,
        "includedCount": 20,
        "excludedCount": 30,
        "conflictCount": 5,
        "pendingCount": 45,
        "completionPercentage": 55.0,
        "exclusionReasonBreakdown": [
          { "reasonCode": 0, "reasonText": "NotRelevantToTopic", "count": 15 },
          { "reasonCode": 7, "reasonText": "DuplicateStudy", "count": 10 }
        ]
      },
      "fullText": {
        "totalPapers": 20,
        "includedCount": 5,
        "excludedCount": 2,
        "conflictCount": 3,
        "pendingCount": 10,
        "completionPercentage": 50.0,
        "exclusionReasonBreakdown": []
      }
    }
  }
}
```
