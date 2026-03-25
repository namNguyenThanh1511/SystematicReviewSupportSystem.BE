# Citation API ↔ UI Integration Report

This report evaluates the feasibility of integrating the new Citation APIs into the existing Title/Abstract Screening UI.

## 1) API ↔ UI Mapping

| UI Requirement | API Endpoint | Status |
| :--- | :--- | :--- |
| **References List** | `GET /api/papers/{id}/references` | ✅ **Functional** (author/doi missing) |
| **Citations List** | `GET /api/papers/{id}/citations` | ✅ **Functional** (author/doi missing) |
| **Citation Network Graph** | `GET /api/papers/{id}/graph` | ✅ **Functional** |
| **Top Cited Dashboard** | `GET /api/papers/top-cited` | ✅ **Functional** |
| **Main Paper Queue Counters** | `GET /api/papers/{id}/(citation|reference)-count` | ⚠️ **N+1 Risk** (see Gap 1) |
| **Discovery Suggestions** | ❌ **Missing API** | 🚨 **BLOCKER** (using placeholders) |

---

## 🚨 API Contract Gaps & Blockers

### 1. N+1 Performance (CRITICAL)
- **Endpoint**: `GET /api/papers/{id}/citation-count` and `GET /api/papers/{id}/reference-count`
- **Severity**: **BLOCKER**
- **Issue**: To show citation counts in the main paper list (Queue), the frontend must fire two separate requests *per paper* in a loop.
- **Impact**: 50 papers = 101 requests. High latency and server load.
- **Recommendation**: Add `citationCount: number` and `referenceCount: number` directly to the [AssignedPaperResponse] in the main screening list.

### 2. Missing Metadata in Node DTOs
- **Endpoint**: `/api/papers/{id}/references`, `/api/papers/{id}/citations`, `/api/papers/{id}/graph`
- **Severity**: **BLOCKER**
- **Issue**: `PaperNodeDto` lacks `authors` and `doi`. 
- **Impact**: SLR researchers identify papers primarily by authors. Displaying only titles is insufficient. DOI is required for manual snowballing.
- **Recommendation**: Update `PaperNodeDto` to include:
  ```ts
  authors: string | null;
  doi: string | null;
  ```

### 3. Missing Suggested/Recommended Papers API
- **Endpoint**: N/A
- **Severity**: **WARNING**
- **Issue**: The UI design includes a "Suggested Papers" section (recommended candidates that aren't in the queue).
- **Impact**: Currently requires manual frontend filtering of the entire citation graph, which is inefficient.
- **Recommendation**: Add `GET /api/papers/{id}/suggestions?limit=5`.

---

## 🛠️ TypeScript Infrastructure (Aligned with Current API)

```ts
/**
 * Synchronized with src/docs/citation-api-docs.md
 */

export interface PaperNodeDto {
  id: string;
  title: string;
  year: number | null;
  citationCount: number;
  /** @GAPS Suggest adding 'authors' and 'doi' here */
}

export interface CitationEdgeDto {
  sourcePaperId: string;
  targetPaperId: string;
  confidenceScore: number;
}

export interface CitationGraphDto {
  nodes: PaperNodeDto[];
  edges: CitationEdgeDto[];
}

export interface GetCitationGraphQuery {
  depth?: number;
  minConfidence?: number;
}

export interface GetTopCitedPapersQuery {
  topN?: number;
}
```

---

## ⚡ React Query Hook (Safe Integration Example)

```tsx
import { useQuery } from "@tanstack/react-query";
import { citationService } from "./citationService";

export function usePaperCitationGraph(paperId: string, depth = 2) {
  return useQuery({
    queryKey: ["papers", paperId, "graph", depth],
    queryFn: async () => {
      const resp = await citationService.getGraph(paperId, { depth });
      if (!resp.isSuccess) throw new Error(resp.message);
      
      // SAFE FALLBACK: If API returns empty data, return valid graph structure
      return resp.data ?? { nodes: [], edges: [] };
    },
    enabled: !!paperId,
    staleTime: 5 * 60 * 1000, // 5 min
  });
}
```

## ⚠️ Frontend Implementation Risks

1. **Disconnected Data**: Currently, `PaperNodeDto` is a specialized, lightweight DTO that differs from the main [ScreeningPaper](file:///d:/Capstone-project/fe/SystematicReviewSupportSystem.Web/src/pages/reviewProcess/studySelection/titleAbstractScreening/types.ts#36-82) type. This means you cannot directly "Screen" a paper from the References tab without a second fetch for details.
2. **Missing Loading States**: With large graphs (Depth 3), the backend BFS might take significant time. UX requires careful Skeleton loaders.
3. **Pagination**: As these are array-based responses, they might eventually cause memory pressure if a paper has 500+ citations.
