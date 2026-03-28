using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Services.ReferenceMatchingService.DTOs;
using System;

namespace SRSS.IAM.Services.ReferenceClassificationService
{
    public class ReferenceClassificationService : IReferenceClassificationService
    {
        public ReferenceType Classify(ExtractedReference reference)
        {
            if (reference == null) return ReferenceType.Unknown;

            // 1. Academic Paper (DOI is the strongest indicator)
            if (!string.IsNullOrWhiteSpace(reference.DOI))
                return ReferenceType.AcademicPaper;

            // 2. Academic Paper (Journal name presence)
            if (!string.IsNullOrWhiteSpace(reference.Journal))
                return ReferenceType.AcademicPaper;

            var raw = reference.RawReference?.ToUpperInvariant() ?? string.Empty;

            // 3. Website (URL or "http" in raw reference)
            if (raw.Contains("HTTP://") || raw.Contains("HTTPS://") || raw.Contains("WWW."))
                return ReferenceType.Website;

            // 4. Book (ISBN presence)
            if (raw.Contains("ISBN"))
                return ReferenceType.Book;

            // 5. Standard (Specific keywords)
            if (raw.Contains("ISO ") || raw.Contains("IEEE STD") || raw.Contains("RFC "))
                return ReferenceType.Standard;

            // 6. Thesis (Keywords)
            if (raw.Contains("THESIS") || raw.Contains("DISSERTATION"))
                return ReferenceType.Thesis;

            // 7. Report (Keywords)
            if (raw.Contains("TECHNICAL REPORT") || raw.Contains("TECH REPORT"))
                return ReferenceType.Report;

            // Default to AcademicPaper if basic metadata (Title, Authors, Year) is present
            // as GROBID is primarily for academic references.
            if (!string.IsNullOrWhiteSpace(reference.Title) && 
                !string.IsNullOrWhiteSpace(reference.PublishedYear))
                return ReferenceType.AcademicPaper;

            return ReferenceType.Unknown;
        }
    }
}
