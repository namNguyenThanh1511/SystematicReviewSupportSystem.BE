# Duplicate Papers API Documentation

**Version:** 3.0  
**Base URL:** `/api`  
**Last Updated:** 2025-02-27

---

## Overview

This endpoint retrieves **duplicate papers** detected within a specific Identification Process. In the PRISMA 2020 workflow, after papers are imported from multiple databases, a deduplication step identifies papers that appear more than once. Each duplicate record includes **deduplication metadata** — which original paper it duplicates, the detection method used, and a confidence score.

**Key concept:** Duplication is **process-scoped**, not global. A paper is a "duplicate" only in the context of a given Identification Process. The same paper can have different deduplication status across processes.

---

## Get Duplicate Papers by Identification Process

### 1. Basic Info

| Property | Value |
|----------|-------|
| **Method** | `GET` |
| **Route** | `/api/identification-processes/{identificationProcessId}/duplicates` |
| **Content-Type** | `application/json` |
| **Description** | Get paginated list of duplicate papers with deduplication metadata for a specific identification process |

### 2. Request Details

#### Headers

```
Content-Type: application/json
Authorization: Bearer {token}  (if authentication enabled)
```

#### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `identificationProcessId` | `string (UUID)` | ✅ Yes | The ID of the Identification Process |

#### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `search` | `string` | ❌ No | `null` | Search in Title, DOI, or Authors of the **duplicate paper** (case-insensitive, partial match) |
| `year` | `integer` | ❌ No | `null` | Filter by publication year of the duplicate paper (exact match) |
| `sortBy` | `string` | ❌ No | `null` | Sort field: `detectedAt` (default), `confidenceScore`, `title`, `method`, `reviewStatus` |
| `sortOrder` | `string` | ❌ No | `desc` | Sort direction: `asc` or `desc` |
| `reviewStatus` | `integer` | ❌ No | `null` | Filter by review status: `0`=Pending, `1`=Confirmed, `2`=Rejected |
| `pageNumber` | `integer` | ❌ No | `1` | Page number (minimum: 1) |
| `pageSize` | `integer` | ❌ No | `20` | Items per page (minimum: 1, maximum: 100) |

#### Validation Rules

- `identificationProcessId` must be a valid UUID of an existing Identification Process
- `pageNumber` is clamped to minimum `1`
- `pageSize` is clamped between `1` and `100`
- `search` matches against the **duplicate paper's** Title, DOI, or Authors
- `year` matches against the duplicate paper's parsed integer publication year

### 3. Response Details

#### Success Response (200 OK)

```json
{
  "isSuccess": true,
  "message": "Retrieved 20 of 53 duplicate papers.",
  "data": {
    "items": [
      {
        "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
        "title": "Machine Learning in Healthcare: A Systematic Review",
        "authors": "Smith, J.; Doe, A.; Johnson, M.",
        "abstract": "This paper reviews recent advances...",
        "doi": "10.1234/example.2024.001",
        "publicationType": "Journal Article",
        "publicationYear": "2024",
        "publicationYearInt": 2024,
        "publicationDate": "2024-03-15T00:00:00+00:00",
        "volume": "15",
        "issue": "3",
        "pages": "123-145",
        "publisher": "Springer",
        "language": "English",
        "keywords": "machine learning, healthcare",
        "url": "https://doi.org/10.1234/example.2024.001",
        "conferenceName": null,
        "conferenceLocation": null,
        "conferenceCountry": null,
        "conferenceYear": null,
        "journal": "Journal of Medical Informatics",
        "journalIssn": "1234-5678",
        "source": "RIS",
        "importedAt": "2024-06-01T10:30:00+00:00",
        "importedBy": "researcher@university.edu",
        "selectionStatus": null,
        "selectionStatusText": null,
        "pdfUrl": null,
        "fullTextAvailable": null,
        "accessType": null,
        "accessTypeText": null,
        "createdAt": "2024-06-01T10:30:00+00:00",
        "modifiedAt": "2024-06-01T10:30:00+00:00",

        "duplicateOfPaperId": "f7e6d5c4-b3a2-1098-7654-321fedcba098",
        "duplicateOfTitle": "Machine Learning in Healthcare: A Systematic Review",
        "duplicateOfAuthors": "Smith, J.; Doe, A.; Johnson, M.",
        "duplicateOfYear": "2024",
        "duplicateOfDoi": "10.1234/example.2024.001",
        "duplicateOfSource": "RIS",
        "duplicateOfAbstract": "This paper reviews recent advances...",
        "method": 0,
        "methodText": "DOI_MATCH",
        "confidenceScore": 1.0,
        "deduplicationNotes": "Duplicate detected by DOI: 10.1234/example.2024.001",
        "detectedAt": "2024-06-01T11:00:00+00:00",
        "reviewStatus": 0,
        "reviewStatusText": "Pending",
        "reviewedBy": null,
        "reviewedAt": null
      }
    ],
    "totalCount": 53,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 3,
    "hasPreviousPage": false,
    "hasNextPage": true
  },
  "errors": null
}
```

#### Empty Result (200 OK)

```json
{
  "isSuccess": true,
  "message": "No duplicate papers found for this identification process.",
  "data": {
    "items": [],
    "totalCount": 0,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 0,
    "hasPreviousPage": false,
    "hasNextPage": false
  },
  "errors": null
}
```

#### Error Responses

| Status | Scenario | Example Response |
|--------|----------|-----------------|
| **500** | Identification Process not found | `{ "isSuccess": false, "message": "IdentificationProcess with ID {id} not found.", "errors": null }` |
| **500** | Server error | `{ "isSuccess": false, "message": "An error occurred", "errors": null }` |

#### HTTP Status Codes

| Code | Meaning |
|------|---------|
| 200 | Success (including empty results) |
| 500 | Identification Process not found or server error |

### 4. Business Logic Summary

#### What This API Does

1. **Validates** that the Identification Process exists
2. **Normalizes** pagination parameters (`pageNumber` ≥ 1, `pageSize` between 1–100)
3. **Queries** the `DeduplicationResult` table for records matching this Identification Process
4. **Includes** the duplicate paper metadata and the original paper's metadata (title, authors, year, DOI, source, abstract)
5. **Applies** optional search, year, and `reviewStatus` filters on the duplicate paper's metadata
6. **Sorts** by the requested field (`sortBy`/`sortOrder`), defaulting to `detectedAt` descending
7. **Returns** paginated results with review status for each deduplication result

#### Query Logic

```
DeduplicationResults WHERE:
  deduplicationResult.IdentificationProcessId == {identificationProcessId}
  
INCLUDE:
  deduplicationResult.Paper        → the paper flagged as duplicate
  deduplicationResult.DuplicateOfPaper → the original paper it duplicates
```

#### Deduplication Methods

Each duplicate record includes a `method` field indicating how the duplicate was detected:

| Value | Name | Description | Typical Confidence |
|-------|------|-------------|-------------------|
| `0` | `DOI_MATCH` | Exact DOI match (highest reliability) | `1.0` (100%) |
| `1` | `TITLE_FUZZY` | Fuzzy title matching | `0.7 – 0.95` |
| `2` | `TITLE_AUTHOR` | Title + Author similarity | `0.8 – 0.95` |
| `3` | `HYBRID` | Multiple criteria combined | `0.6 – 0.99` |
| `4` | `MANUAL` | Manual review by researcher | `1.0` (100%) |

#### Important Conditions for Frontend

| Condition | Detail |
|-----------|--------|
| **Each item = one deduplication result** | A paper can appear multiple times if it duplicates different originals |
| **`duplicateOfPaperId`** | The UUID of the original paper this is a duplicate of |
| **`duplicateOfTitle`** | The title of the original paper |
| **`duplicateOfAuthors`, `duplicateOfYear`, `duplicateOfDoi`, `duplicateOfSource`, `duplicateOfAbstract`** | Full original paper metadata for side-by-side comparison |
| **`reviewStatus` / `reviewStatusText`** | Review status: `Pending` (0), `Confirmed` (1), `Rejected` (2) |
| **`reviewedBy` / `reviewedAt`** | Who reviewed and when (null if pending) |
| **`confidenceScore`** | Range 0.0 to 1.0 — useful for displaying confidence badges/bars |
| **`method` is an integer enum** | Use `methodText` for display (e.g., `"DOI_MATCH"`) |
| **`selectionStatus` is always null** | Status lives in `ScreeningResolution`, not on the paper |
| **Process-scoped** | Duplicates are specific to one Identification Process |

#### Side Effects

- **None** — this is a read-only endpoint

---

## Resolve Duplicate

### Basic Info

| Property | Value |
|----------|-------|
| **Method** | `POST` |
| **Route** | `/api/identification-processes/{identificationProcessId}/duplicates/{deduplicationResultId}/resolve` |
| **Content-Type** | `application/json` |
| **Description** | Confirm or reject a duplicate detection result |

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `identificationProcessId` | `string (UUID)` | ✅ Yes | The Identification Process ID |
| `deduplicationResultId` | `string (UUID)` | ✅ Yes | The Deduplication Result ID |

### Request Body

```json
{
  "resolution": 1,
  "reviewedBy": "researcher@university.edu",
  "notes": "Confirmed as duplicate — same DOI"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `resolution` | `integer` | ✅ Yes | `0`=Pending, `1`=Confirmed, `2`=Rejected |
| `reviewedBy` | `string` | ❌ No | User who performed the review |
| `notes` | `string` | ❌ No | Optional notes (updates existing notes if provided) |

### Success Response (200 OK)

Returns the updated `DuplicatePaperResponse` with `reviewStatus`, `reviewedBy`, and `reviewedAt` set.

```json
{
  "isSuccess": true,
  "message": "Duplicate resolution updated successfully.",
  "data": {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "title": "Machine Learning in Healthcare",
    "duplicateOfPaperId": "f7e6d5c4-...",
    "duplicateOfTitle": "Machine Learning in Healthcare",
    "duplicateOfAuthors": "Smith, J.",
    "reviewStatus": 1,
    "reviewStatusText": "Confirmed",
    "reviewedBy": "researcher@university.edu",
    "reviewedAt": "2025-02-27T14:30:00+00:00",
    "...": "(other paper + deduplication fields)"
  },
  "errors": null
}
```

### Error Responses

| Status | Scenario |
|--------|----------|
| **500** | Deduplication result not found for the given process |
| **500** | Server error |

#### Relationship to Unique Papers Endpoint

| Endpoint | Returns |
|----------|---------|
| `GET .../duplicates` | Papers **with** a `DeduplicationResult` for this process |
| `GET .../unique-papers` | Papers with **no** `DeduplicationResult` for this process |
| **Together** | They cover **all** papers in the Identification Process |

### 5. Frontend Integration Notes

#### When to Call This API

- After running deduplication on imported papers
- On the "Deduplication Results" or "PRISMA Flow" page
- When displaying statistics about how many duplicates were removed
- To allow researchers to review and verify detected duplicates
- To build duplicate comparison views (side-by-side with original paper)

#### Common Pitfalls

| ❌ Don't | ✅ Do |
|----------|------|
| Assume `totalCount` equals `items.length` | Use `totalCount` for total, `items.length` for current page |
| Ignore `confidenceScore` | Display it — low scores may need manual review |
| Treat `method` integer as meaningful to users | Use `methodText` string for display |
| Assume one duplicate per paper | A paper can appear in multiple deduplication results |
| Cache aggressively | Results change after re-running deduplication |
| Treat empty results as an error | No duplicates found (200 OK) is a valid state |

#### Confidence Score Display Suggestions

```typescript
function getConfidenceLabel(score: number | null): string {
  if (score === null) return 'Unknown';
  if (score >= 0.95) return 'Very High';
  if (score >= 0.80) return 'High';
  if (score >= 0.60) return 'Medium';
  return 'Low';
}

function getConfidenceColor(score: number | null): string {
  if (score === null) return '#9e9e9e'; // grey
  if (score >= 0.95) return '#4caf50'; // green
  if (score >= 0.80) return '#8bc34a'; // light green
  if (score >= 0.60) return '#ff9800'; // orange
  return '#f44336'; // red
}
```

#### Deduplication Method Display

```typescript
const METHOD_LABELS: Record<string, string> = {
  DOI_MATCH: 'DOI Match',
  TITLE_FUZZY: 'Fuzzy Title Match',
  TITLE_AUTHOR: 'Title + Author Match',
  HYBRID: 'Hybrid Detection',
  MANUAL: 'Manual Review'
};

const METHOD_ICONS: Record<string, string> = {
  DOI_MATCH: '🔗',
  TITLE_FUZZY: '📝',
  TITLE_AUTHOR: '👤',
  HYBRID: '🔀',
  MANUAL: '✋'
};
```

### 6. TypeScript Interfaces

```typescript
// ============================================
// Request
// ============================================
interface GetDuplicatePapersParams {
  identificationProcessId: string;     // UUID (path param)
  search?: string;                     // Optional search text
  year?: number;                       // Optional year filter
  sortBy?: 'detectedAt' | 'confidenceScore' | 'title' | 'method' | 'reviewStatus';  // Sort field
  sortOrder?: 'asc' | 'desc';         // Sort direction (default: desc)
  reviewStatus?: DeduplicationReviewStatus; // Filter by review status
  pageNumber?: number;                 // Default: 1
  pageSize?: number;                   // Default: 20, max: 100
}

// ============================================
// Response Wrapper
// ============================================
interface ApiResponse<T> {
  isSuccess: boolean;
  message: string;
  data: T | null;
  errors: ApiError[] | null;
}

interface ApiError {
  code: string;
  message: string;
}

// ============================================
// Paginated Response
// ============================================
interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

// ============================================
// Duplicate Paper Response
// (extends PaperResponse with deduplication metadata)
// ============================================
interface DuplicatePaperResponse extends PaperResponse {
  // Deduplication metadata
  duplicateOfPaperId: string;            // UUID of the original paper
  duplicateOfTitle?: string | null;      // Title of the original paper
  duplicateOfAuthors?: string | null;    // Authors of the original paper
  duplicateOfYear?: string | null;       // Publication year of the original paper
  duplicateOfDoi?: string | null;        // DOI of the original paper
  duplicateOfSource?: string | null;     // Source of the original paper
  duplicateOfAbstract?: string | null;   // Abstract of the original paper
  method: DeduplicationMethod;           // Enum integer value
  methodText: string;                    // Enum string (e.g., "DOI_MATCH")
  confidenceScore?: number | null;       // 0.0 to 1.0
  deduplicationNotes?: string | null;    // Why it was flagged
  detectedAt: string;                    // ISO 8601 — when duplicate was detected
  reviewStatus: DeduplicationReviewStatus; // Review status enum value
  reviewStatusText: string;              // Review status display text
  reviewedBy?: string | null;            // Who reviewed
  reviewedAt?: string | null;            // When reviewed (ISO 8601)
}

// ============================================
// Deduplication Review Status Enum
// ============================================
enum DeduplicationReviewStatus {
  Pending = 0,
  Confirmed = 1,
  Rejected = 2
}

// ============================================
// Resolve Duplicate Request
// ============================================
interface ResolveDuplicateRequest {
  resolution: DeduplicationReviewStatus; // 0=Pending, 1=Confirmed, 2=Rejected
  reviewedBy?: string | null;            // User who reviewed
  notes?: string | null;                 // Optional review notes
}

// ============================================
// Paper Response (base)
// ============================================
interface PaperResponse {
  id: string;                            // UUID

  // Core Metadata
  title: string;
  authors?: string | null;
  abstract?: string | null;
  doi?: string | null;
  publicationType?: string | null;
  publicationYear?: string | null;
  publicationYearInt?: number | null;
  publicationDate?: string | null;       // ISO 8601
  volume?: string | null;
  issue?: string | null;
  pages?: string | null;
  publisher?: string | null;
  language?: string | null;
  keywords?: string | null;
  url?: string | null;

  // Conference Metadata
  conferenceName?: string | null;
  conferenceLocation?: string | null;
  conferenceCountry?: string | null;
  conferenceYear?: number | null;

  // Journal Metadata
  journal?: string | null;
  journalIssn?: string | null;

  // Source Tracking
  source?: string | null;
  importedAt?: string | null;            // ISO 8601
  importedBy?: string | null;

  // Selection Status (always null from this endpoint)
  selectionStatus?: string | null;
  selectionStatusText?: string | null;

  // Access
  pdfUrl?: string | null;
  fullTextAvailable?: boolean | null;
  accessType?: string | null;
  accessTypeText?: string | null;

  // Audit
  createdAt: string;                     // ISO 8601
  modifiedAt: string;                    // ISO 8601
}

// ============================================
// Deduplication Method Enum
// ============================================
enum DeduplicationMethod {
  DOI_MATCH = 0,
  TITLE_FUZZY = 1,
  TITLE_AUTHOR = 2,
  HYBRID = 3,
  MANUAL = 4
}
```

### 7. Example API Calls

#### Fetch Function

```typescript
async function getDuplicatePapers(
  params: GetDuplicatePapersParams
): Promise<PaginatedResponse<DuplicatePaperResponse>> {
  const queryParams = new URLSearchParams();

  if (params.search) queryParams.set('search', params.search);
  if (params.year) queryParams.set('year', params.year.toString());
  if (params.pageNumber) queryParams.set('pageNumber', params.pageNumber.toString());
  if (params.pageSize) queryParams.set('pageSize', params.pageSize.toString());

  const url = `/api/identification-processes/${params.identificationProcessId}/duplicates?${queryParams}`;

  const response = await fetch(url, {
    method: 'GET',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${getToken()}`
    }
  });

  const result: ApiResponse<PaginatedResponse<DuplicatePaperResponse>> = await response.json();

  if (!result.isSuccess || !result.data) {
    throw new Error(result.message);
  }

  return result.data;
}
```

#### Usage Examples

```typescript
// Basic: Get first page of duplicates
const page1 = await getDuplicatePapers({
  identificationProcessId: 'abc-123-def'
});
console.log(`${page1.totalCount} duplicates found`);

// With search
const filtered = await getDuplicatePapers({
  identificationProcessId: 'abc-123-def',
  search: 'machine learning'
});

// With year filter
const duplicates2024 = await getDuplicatePapers({
  identificationProcessId: 'abc-123-def',
  year: 2024,
  pageSize: 50
});

// Pagination: next page
const page2 = await getDuplicatePapers({
  identificationProcessId: 'abc-123-def',
  pageNumber: 2,
  pageSize: 20
});
```

#### React Hook Example

```typescript
import { useState, useEffect, useCallback } from 'react';

interface UseDuplicatePapersOptions {
  identificationProcessId: string;
  pageSize?: number;
}

export function useDuplicatePapers({ identificationProcessId, pageSize = 20 }: UseDuplicatePapersOptions) {
  const [data, setData] = useState<PaginatedResponse<DuplicatePaperResponse> | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState('');
  const [year, setYear] = useState<number | undefined>();
  const [pageNumber, setPageNumber] = useState(1);

  const fetchDuplicates = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      const result = await getDuplicatePapers({
        identificationProcessId,
        search: search || undefined,
        year,
        pageNumber,
        pageSize
      });
      setData(result);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load duplicates');
    } finally {
      setLoading(false);
    }
  }, [identificationProcessId, search, year, pageNumber, pageSize]);

  useEffect(() => {
    fetchDuplicates();
  }, [fetchDuplicates]);

  // Reset to page 1 when filters change
  const updateSearch = (value: string) => {
    setSearch(value);
    setPageNumber(1);
  };

  const updateYear = (value: number | undefined) => {
    setYear(value);
    setPageNumber(1);
  };

  return {
    duplicates: data?.items ?? [],
    totalCount: data?.totalCount ?? 0,
    totalPages: data?.totalPages ?? 0,
    hasNextPage: data?.hasNextPage ?? false,
    hasPreviousPage: data?.hasPreviousPage ?? false,
    pageNumber,
    loading,
    error,
    setPageNumber,
    setSearch: updateSearch,
    setYear: updateYear,
    refetch: fetchDuplicates
  };
}
```

#### React Component Example

```tsx
import React from 'react';
import { useDuplicatePapers } from './useDuplicatePapers';

interface Props {
  identificationProcessId: string;
}

export const DuplicatePapersTable: React.FC<Props> = ({ identificationProcessId }) => {
  const {
    duplicates,
    totalCount,
    totalPages,
    hasNextPage,
    hasPreviousPage,
    pageNumber,
    loading,
    error,
    setPageNumber,
    setSearch,
    setYear
  } = useDuplicatePapers({ identificationProcessId, pageSize: 20 });

  if (error) return <div className="error">Error: {error}</div>;

  return (
    <div>
      <h2>Duplicate Papers ({totalCount})</h2>

      {/* Filters */}
      <div className="filters">
        <input
          type="text"
          placeholder="Search title, DOI, authors..."
          onChange={(e) => setSearch(e.target.value)}
        />
        <input
          type="number"
          placeholder="Year"
          onChange={(e) => setYear(e.target.value ? Number(e.target.value) : undefined)}
        />
      </div>

      {/* Table */}
      {loading ? (
        <div>Loading...</div>
      ) : duplicates.length === 0 ? (
        <div>No duplicate papers found.</div>
      ) : (
        <table>
          <thead>
            <tr>
              <th>Title</th>
              <th>Authors</th>
              <th>Year</th>
              <th>DOI</th>
              <th>Duplicate Of</th>
              <th>Method</th>
              <th>Confidence</th>
            </tr>
          </thead>
          <tbody>
            {duplicates.map((dup) => (
              <tr key={dup.id}>
                <td>{dup.title}</td>
                <td>{dup.authors ?? 'N/A'}</td>
                <td>{dup.publicationYear ?? 'N/A'}</td>
                <td>
                  {dup.doi ? (
                    <a href={`https://doi.org/${dup.doi}`} target="_blank" rel="noreferrer">
                      {dup.doi}
                    </a>
                  ) : 'N/A'}
                </td>
                <td title={dup.duplicateOfPaperId}>
                  {dup.duplicateOfTitle ?? dup.duplicateOfPaperId}
                </td>
                <td>
                  <span className={`method-badge method-${dup.methodText.toLowerCase()}`}>
                    {dup.methodText.replace('_', ' ')}
                  </span>
                </td>
                <td>
                  <span
                    className="confidence-badge"
                    style={{ color: getConfidenceColor(dup.confidenceScore ?? null) }}
                  >
                    {dup.confidenceScore != null
                      ? `${(dup.confidenceScore * 100).toFixed(0)}%`
                      : 'N/A'}
                  </span>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      {/* Pagination */}
      <div className="pagination">
        <button disabled={!hasPreviousPage} onClick={() => setPageNumber(pageNumber - 1)}>
          Previous
        </button>
        <span>Page {pageNumber} of {totalPages}</span>
        <button disabled={!hasNextPage} onClick={() => setPageNumber(pageNumber + 1)}>
          Next
        </button>
      </div>
    </div>
  );
};

function getConfidenceColor(score: number | null): string {
  if (score === null) return '#9e9e9e';
  if (score >= 0.95) return '#4caf50';
  if (score >= 0.80) return '#8bc34a';
  if (score >= 0.60) return '#ff9800';
  return '#f44336';
}
```

---

## cURL Examples

```bash
# Basic call
curl -X GET "https://your-api.com/api/identification-processes/abc-123-def/duplicates" \
  -H "Authorization: Bearer {token}"

# With search and pagination
curl -X GET "https://your-api.com/api/identification-processes/abc-123-def/duplicates?search=deep+learning&pageNumber=1&pageSize=50" \
  -H "Authorization: Bearer {token}"

# With year filter
curl -X GET "https://your-api.com/api/identification-processes/abc-123-def/duplicates?year=2024" \
  -H "Authorization: Bearer {token}"
```

---

## Summary

| Item | Detail |
|------|--------|
| **Endpoint** | `GET /api/identification-processes/{id}/duplicates` |
| **Purpose** | Retrieve papers flagged as duplicates with deduplication metadata |
| **Pagination** | ✅ Supported (`pageNumber`, `pageSize`, max 100) |
| **Search** | ✅ By title, DOI, or authors of the duplicate paper (case-insensitive) |
| **Year Filter** | ✅ Exact match on publication year |
| **Sort Order** | Configurable via `sortBy`/`sortOrder` (default: `detectedAt` descending) |
| **Side Effects** | None (read-only) |
| **Companion** | `GET .../unique-papers` returns the inverse set (non-duplicates) |
| **Key Extra Fields** | `duplicateOfPaperId`, `duplicateOfTitle`, `duplicateOfAuthors`, `duplicateOfYear`, `duplicateOfDoi`, `duplicateOfSource`, `duplicateOfAbstract`, `method`, `methodText`, `confidenceScore`, `deduplicationNotes`, `detectedAt`, `reviewStatus`, `reviewStatusText`, `reviewedBy`, `reviewedAt` |
| **Resolve Endpoint** | `POST .../duplicates/{id}/resolve` — confirm or reject a detection |
| **Paired Endpoint** | `GET .../duplicate-pairs` — side-by-side comparison with both papers' full metadata |
| **Pair Resolve Endpoint** | `PATCH .../duplicate-pairs/{pairId}/resolve` — resolve with decision: `keep-original` / `keep-duplicate` / `keep-both` |

---

**Backend Implementation:**
- Controller: `SRSS.IAM.API.Controllers.PapersController`
- Service: `SRSS.IAM.Services.PaperService.IPaperService`
- Repository (flat): `SRSS.IAM.Repositories.PaperRepo.IPaperRepository.GetDuplicatePapersByIdentificationProcessAsync`
- Repository (pairs): `SRSS.IAM.Repositories.DeduplicationResultRepo.IDeduplicationResultRepository.GetDuplicatePairsAsync`

**Document Version:** 3.0  
**Maintained by:** Backend Team

---

## Changelog

### v2.0 (2025-02-27) — Gap Report Resolution

All changes are **backward compatible** — existing consumers continue to work unchanged.

| Change | Gap Issue | Type | Breaking |
|--------|-----------|------|----------|
| Added `duplicateOfAuthors`, `duplicateOfYear`, `duplicateOfDoi`, `duplicateOfSource`, `duplicateOfAbstract` to response | Issue 1 | Additive | ❌ No |
| Added `POST .../duplicates/{id}/resolve` endpoint | Issue 2 | New endpoint | ❌ No |
| Added `reviewStatus`, `reviewStatusText`, `reviewedBy`, `reviewedAt` to response | Issue 3 | Additive | ❌ No |
| Added `sortBy`, `sortOrder` query params | Issue 4 | Additive | ❌ No |
| Added `reviewStatus` query param for filtering | Issue 3 | Additive | ❌ No |
| DB migration: `AddDeduplicationReviewFields` (3 new columns + index) | Issues 2,3 | Schema | ❌ No (defaults to `Pending`) |

**Migration notes:**
- Run `dotnet ef database update` to apply the migration
- Existing rows get `review_status = 'Pending'`, `reviewed_by = NULL`, `reviewed_at = NULL`
- No data loss, no column renames, no type changes

### v3.0 (2025-02-27) — Duplicate Pairs Feature

New paired endpoint for side-by-side researcher review workflow. All changes are **backward compatible** — existing `/duplicates` endpoint is unchanged.

| Change | Type | Breaking |
|--------|------|----------|
| Added `GET .../duplicate-pairs` endpoint — returns paired `originalPaper` + `duplicatePaper` with lightweight DTO | New endpoint | ❌ No |
| Added `PATCH .../duplicate-pairs/{pairId}/resolve` endpoint — resolve with `keep-original` / `keep-duplicate` / `keep-both` | New endpoint | ❌ No |
| Added `ResolvedDecision` field to `DeduplicationResult` entity | Additive | ❌ No |
| Added `DuplicatePairPaperDto` — lightweight paper DTO (13 fields vs 30+ in full `PaperResponse`) | New DTO | ❌ No |
| Added `DuplicatePairResponse`, `DuplicatePairsRequest`, `ResolveDuplicatePairRequest`, `ResolveDuplicatePairResponse` DTOs | New DTOs | ❌ No |
| Added `GetDuplicatePairsAsync` to `IDeduplicationResultRepository` — search across both papers, filter by status/confidence/method, configurable sort | New method | ❌ No |
| DB migration: `AddResolvedDecisionField` (`resolved_decision` varchar(50), nullable) | Schema | ❌ No |

**New query parameters for `GET .../duplicate-pairs`:**

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `search` | `string` | `null` | Search title/DOI/authors of **either** paper in the pair |
| `status` | `integer` | `null` | Filter by review status: `0`=Pending, `1`=Confirmed, `2`=Rejected |
| `minConfidence` | `decimal` | `null` | Filter pairs with confidence ≥ value (0.0–1.0) |
| `method` | `integer` | `null` | Filter by detection method enum |
| `sortBy` | `string` | `confidenceDesc` | Sort: `confidenceDesc`, `confidenceAsc`, `detectedAtDesc` |
| `pageNumber` | `integer` | `1` | Page number |
| `pageSize` | `integer` | `20` | Items per page (max: 100) |

**Resolve decisions:**

| Decision | Effect | Maps to ReviewStatus |
|----------|--------|---------------------|
| `keep-original` | Duplicate paper excluded | `Confirmed` |
| `keep-duplicate` | Original paper excluded | `Confirmed` |
| `keep-both` | Not a duplicate, keep both | `Rejected` |

**Migration notes:**
- Run `dotnet ef database update` to apply the migration
- Existing rows get `resolved_decision = NULL`
- No data loss, no column renames, no type changes

**Endpoint relationship:**

| Endpoint | Purpose |
|----------|---------|
| `GET .../duplicates` | Flat reporting, statistics, export |
| `GET .../duplicate-pairs` | Review workflow, side-by-side comparison |
| `PATCH .../duplicate-pairs/{id}/resolve` | Record researcher decision with audit trail |
| `GET .../unique-papers` | Non-duplicate papers |
