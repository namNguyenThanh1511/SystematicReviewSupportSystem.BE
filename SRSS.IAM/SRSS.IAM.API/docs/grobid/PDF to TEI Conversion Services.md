The **PDF to TEI conversion services** in GROBID are designed to extract structured data from PDF documents and convert them into TEI XML or BibTeX formats. These services use the `POST` or `PUT` methods and require requests to be sent as `multipart/form-data`.

### **Metadata Consolidation**
A key feature across these services is the use of **consolidation parameters** (`consolidateHeader`, `consolidateCitations`, and `consolidateFunders`). These parameters determine if GROBID should supplement extracted data with external metadata from CrossRef or biblio-glutton:
*   **0**: No consolidation; all metadata comes from the source PDF.
*   **1**: Full consolidation; publisher metadata is combined with and may correct PDF-extracted data.
*   **2**: Consolidation is performed only to add a DOI if a match is found.
*   **3**: (Header only) Consolidation is restricted to using only a DOI already extracted from the header.

---

### **1. Header Extraction: `/api/processHeaderDocument`**
This service extracts and normalizes the document header (e.g., title, authors, abstract).
*   **Required Parameter:** `input` (the PDF file).
*   **Optional Parameters:**
    *   `consolidateHeader`: Values 0, 1, 2, or 3.
    *   `includeRawAffiliations`: Boolean (0 or 1) to include the original affiliation strings.
    *   `includeRawCopyrights`: Boolean (0 or 1) to include original license/copyright strings.
    *   `start` / `end`: Page range to process (default is starting from page 1 and ending at page 2).
*   **Response:** Returns TEI XML by default, but can return **BibTeX** if the `Accept: application/x-bibtex` header is used.
*   **Error Handling:** If a **503 error** occurs, a wait time of **2 seconds** is suggested before retrying.

---

### **2. Fulltext Conversion: `/api/processFulltextDocument`**
This endpoint converts the entire document, including the header, body text, and bibliographical section, into TEI XML.
*   **Required Parameter:** `input`.
*   **Key Optional Parameters:**
    *   `consolidateHeader`, `consolidateCitations`, `consolidateFunders`: Consolidation settings for different sections.
    *   `teiCoordinates`: A list of elements (like `figure`, `persName`, `formula`) for which to include physical coordinates from the PDF.
    *   `segmentSentences`: If set to 1, paragraphs are further broken down into sentence elements (`<s>`) using algorithms like OpenNLP.
    *   `flavor`: Specifies a document structure "flavor" if the default structuring fails for specific document types.
*   **Error Handling:** For **503 errors**, the recommended wait time is **5-10 seconds**.

---

### **3. Reference Extraction: `/api/processReferences`**
This service focuses specifically on extracting and converting the bibliographical references found at the end of a document.
*   **Required Parameter:** `input`.
*   **Optional Parameters:**
    *   `consolidateCitations`: Values 0, 1, or 2.
    *   `includeRawCitations`: Boolean (0 or 1) to include the original raw reference string in the output.
*   **Response:** Supports both TEI XML and **BibTeX** formats.
*   **Error Handling:** For **503 errors**, a wait time of **3-6 seconds** is suggested.

### **General Error Codes for Conversion**
When using these services, you may encounter specific 500-level errors indicating processing issues:
*   **`BAD_INPUT_DATA`**: The PDF is unreadable or missing.
*   **`NO_BLOCKS`**: The PDF contains no text (likely a scan without OCR).
*   **`TOO_MANY_BLOCKS` / `TOO_MANY_TOKENS`**: The document is too large for safety limits.
*   **`TIMEOUT`**: Processing took too long and was aborted.
*   **`PDFALTO_CONVERSION_FAILURE`**: The internal tool (pdfalto) failed to convert the file, often due to file damage.