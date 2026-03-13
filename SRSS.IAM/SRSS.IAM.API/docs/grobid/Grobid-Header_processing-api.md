The GROBID API provides two primary endpoints for processing header information: **`/api/processHeaderDocument`** for extracting headers from PDF files and **`/api/processHeaderNames`** for parsing raw strings of names from a header section.

### 1. PDF Header Extraction: `/api/processHeaderDocument`
This service extracts the header from an input PDF document, normalizes the data, and converts it into **TEI XML** or **BibTeX** format.

*   **Methods:** `POST`, `PUT`.
*   **Request Type:** `multipart/form-data`.
*   **Response Type:** `application/xml` (default) or `application/x-bibtex` (requested via the `Accept` header).

#### Request Parameters
| Parameter | Requirement | Description |
| :--- | :--- | :--- |
| **`input`** | **Required** | The PDF file to be processed. |
| **`consolidateHeader`** | Optional | A string value: **0** (no consolidation), **1** (consolidate and inject all extra metadata; default), **2** (inject DOI only), or **3** (consolidate using only extracted DOI). |
| **`includeRawAffiliations`** | Optional | Boolean (**0** for default, **1** to include raw affiliation strings in the result). |
| **`includeRawCopyrights`** | Optional | Boolean (**0** for default, **1** to include raw copyrights/license strings). |
| **`start`** | Optional | Start page number (default is **-1**, starting from the first page). |
| **`end`** | Optional | End page number (default is **2**). |

#### Example cURL Commands
*   **For TEI XML result:**
    `curl -v -H "Accept: application/xml" --form input=@./thefile.pdf localhost:8070/api/processHeaderDocument`
*   **For BibTeX result:**
    `curl -v -H "Accept: application/x-bibtex" --form input=@./thefile.pdf localhost:8070/api/processHeaderDocument`

---

### 2. Raw Text Header Name Parsing: `/api/processHeaderNames`
This service parses a raw string corresponding to a name or a sequence of names typically found in a header section and returns normalized authors in TEI format.

*   **Methods:** `POST`, `PUT`.
*   **Request Type:** `application/x-www-form-urlencoded`.
*   **Response Type:** `application/xml`.

#### Request Parameters
| Parameter | Requirement | Description |
| :--- | :--- | :--- |
| **`names`** | **Required** | The sequence of names to be parsed as a raw string. |

#### Example cURL Command
`curl -X POST -d "names=John Doe and Jane Smith" localhost:8070/api/processHeaderNames`

---

### Response Status Codes
Both services share a common set of HTTP status codes:
*   **200:** Successful operation.
*   **204:** Process completed, but no content could be extracted.
*   **400:** Wrong request (missing parameters or headers).
*   **500:** Internal service error.
*   **503:** Service unavailable (all threads are currently in use); a wait time of **2 seconds** is suggested before retrying for header processing.