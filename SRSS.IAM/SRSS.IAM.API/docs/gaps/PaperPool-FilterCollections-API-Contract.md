# Paper Pool + Saved Filter Collections API Contract

## Scope
This contract is derived only from the new/updated Paper Pool frontend flow in this conversation.

Frontend capabilities covered:
- Project-scoped paper pool browsing
- Live filtering + search + pagination
- Review process rail with progress snapshots
- Manual add (selected papers)
- Bulk add using a pre-saved filter collection
- Saved filter collection CRUD
- Dedup-aware insert result reporting

## Core Response Models

### 1) PaperPoolItem
```json
{
  "id": "string",
  "title": "string",
  "authors": "string",
  "year": 2024,
  "doi": "string|null",
  "searchSourceId": "string",
  "searchSourceName":"string",
  "importBatchId": "string",
  "hasFullText": true,
  "abstract": "string",
  "keywords": ["string"]
}
```

### 2) PaperPoolFilters
```json
{
  "keyword": "string",
  "yearFrom": 2019,
  "yearTo": 2025,
  "searchSourceId": "all|string",
  "searchSourceName":"string":,
  "importBatchId": "all|string",
  "doiState": "all|has|missing",
  "fullTextState": "all|has|missing",
  "onlyUnused": true,
  "recentlyImported": false
}
```

### 3) PaperPoolFilterSetting ( create new Filter entity which have one Project have many Filters relationship )
(and 1 filter can be applied in many review process , and one review process can applied many filters)
```json response for API
{
  "id": "string",
  "name": "string",
  "searchText": "string",
  "filters": {
    "keyword": "string",
    "yearFrom": 2019,
    "yearTo": 2025,
    "searchSourceId": "all|string",
    "importBatchId": "all|string",
    "doiState": "all|has|missing",
    "fullTextState": "all|has|missing",
    "onlyUnused": true,
    "recentlyImported": false
  },
  "createdAt": "2026-04-21T10:00:00.000Z"
}
```

### 4) ReviewProcessSnapshot
```json
{
  "reviewProcessId": "string",
  "reviewProcessName": "string",
  "statusText": "NotStarted|InProgress|Completed|Cancelled",
  "startAt":DateTimeOffset
  "completedAt":DateTimeOffset
  "progressPercent": 42 ( calculate from completed state of all IdentificationProcess,StudySelectionProcess,QualityAssessmentProcess,DataExtractionProcessSynthesisProcess ) ,
  "totalPapersImported":int,
}
```
new API for AddPapersToReviewProcess
get identification from review process to start 
### 5) SelectionInsertResult
```json
{
  "inserted": 12,
  "skippedAsDuplicate": 3
}
```

## Endpoints

## A. Paper Pool listing
### GET /api/projects/{projectId}/papers
Returns filtered, paged papers for the main table.

Query parameters:
- `searchText: string` (free text over title/authors/doi)
- `keyword: string`
- `yearFrom: number|null`
- `yearTo: number|null`
- `searchSourceId: string` (`all` allowed)
- `importBatchId: string` (`all` allowed)
- `doiState: all|has|missing`
- `fullTextState: all|has|missing`
- `onlyUnused: boolean`
- `recentlyImported: boolean`
- `pageNumber: number`
- `pageSize: number`

Response:
```json
{
  "isSuccess": true,
  "message": "string",
  "data": {
    "items": [
      {
        "id": "string",
        "title": "string",
        "authors": "string",
        "year": 2024,
        "doi": "string|null",
        "source": "string",
        "searchSourceId": "string",
        "importBatchId": "string",
        "hasFullText": true,
        "abstract": "string",
        "keywords": ["string"]
      }
    ],
    "totalCount": 1250,
    "pageNumber": 1,
    "pageSize": 25,
    "totalPages": 50,
  }
}
```

Notes:
- create new api for getting import batches and search sources for filtering 
- `totalCount` is used by UI and process rail.

## B. Review process rail snapshots
### GET /api/projects/{projectId}/review-processes
Returns process cards shown in right rail.

Response:
```json
{
  "isSuccess": true,
  "message": "string",
  "data": [
    {

    "processId": "string",
    "processName": "string",
    "statusText": "NotStarted|InProgress|Completed|Cancelled",
    "startAt":DateTimeOffset
    "completedAt":DateTimeOffset
    "progressPercent": 42 ( calculate from completed state of  IdentificationProcess,StudySelectionProcess,QualityAssessmentProcess,DataExtractionProcess,SynthesisProcess ) ,
    "totalPapersImported":int,
    "total included papers":int,
    "total excluded papers":int,
    }
  ]
}
```

## C. Saved filter collections (settings objects)
### GET /api/projects/{projectId}/filter-settings
Response:
```json
{
  "isSuccess": true,
  "message": "string",
  "data": [
    {
      "id": "string",
      "name": "My filter",
      "searchText": "ai",
      "filters": {
        "keyword": "healthcare",
        "yearFrom": 2019,
        "yearTo": 2025,
        "searchSourceId": "all",
        "importBatchId": "all",
        "doiState": "has",
        "fullTextState": "all",
        "onlyUnused": false,
        "recentlyImported": false
      },
      "createdAt": "2026-04-21T10:00:00.000Z"
    }
  ]
}
```

### POST /api/projects/{projectId}/filter-settings
Request:
```json
{
  "name": "string",
  "searchText": "string",
  "filters": {
    "keyword": "string",
    "yearFrom": 2019,
    "yearTo": 2025,
    "searchSourceId": "all|string",
    "importBatchId": "all|string",
    "doiState": "all|has|missing",
    "fullTextState": "all|has|missing",
    "onlyUnused": true,
    "recentlyImported": false
  }
}
```
Response: created `PaperPoolFilterSetting`.

### PUT /api/projects/{projectId}/filter-settings/{filterSettingId}
Request shape same as POST.
Response: updated `PaperPoolFilterSetting`.

### DELETE /api/projects/{projectId}/filter-settings/{filterSettingId}
Response:
```json
{
  "isSuccess": true,
  "message": "Deleted",
  "data": null
}
```

## D. Add selected papers to process
### POST /api/review-processes/{reviewProcessId}/papers
Request:
```json
{
  "paperIds": ["string"]
}
```
Response:
```json
{
  "isSuccess": true,
  "message": "string",
  "data": {
    "inserted": 10,
    "reviewProcessSnapshot": {
      "reviewProcessId": "string",
      "reviewProcessName": "string",
      "statusText": "InProgress",
      "progressPercent": 44,

    }
  }
}
```

## E. Add all papers from saved filter collection
### POST /api/review-processes/{processId}/papers/add-from-filter-setting
Request:
```json
{
  "filterSettingId": "string"
}
```
Response:
```json
{
  "isSuccess": true,
  "message": "string",
  "data": {
    "inserted": 45,
    "skippedAsDuplicate": 6,
    "matchedTotal": 51,
    "processSnapshot": {
      "processId": "string",
      "processName": "string",
      "statusText": "InProgress",
      "progressPercent": 63,
      "existingPaperIds": ["string"]
    }
  }
}
```

## Validation + Behavior Requirements

- Dedup is required during insert.
  - Existing process papers must not be duplicated.
  - Result must return inserted and skipped duplicate counts.
- If saved filter has zero matched papers, API should return success=false or a clear business message.
- Filter setting names should be unique per project (recommended) to avoid confusion.
- `doiState` and `fullTextState` enums must be exactly: `all|has|missing`.
- `statusText` enum for process snapshot must be exactly:
  - `NotStarted`
  - `InProgress`
  - `Completed`
  - `Cancelled`



## Frontend Dependencies Summary

To fully support current frontend flow, backend must provide:
- Paged paper pool query with all filter params
- Saved filter settings CRUD
- Two insert actions:
  - by selected paper ids
  - by saved filter setting id
- Insert result metrics (`inserted`, `skippedAsDuplicate`) and updated process snapshot
