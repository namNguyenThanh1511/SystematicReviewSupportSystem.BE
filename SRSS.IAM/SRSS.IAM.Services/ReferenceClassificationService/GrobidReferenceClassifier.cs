using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.Services.ReferenceClassificationService
{
    public class GrobidReferenceResult
    {
        public string? XmlId { get; set; }
        public string? MainTitle { get; set; }
        public ReferenceType Type { get; set; }
    }

    public class GrobidReferenceClassifier
    {
        private static readonly XNamespace Tei = "http://www.tei-c.org/ns/1.0";
        private static readonly XNamespace Xml = "http://www.w3.org/XML/1998/namespace";

        public GrobidReferenceResult Classify(XElement biblStruct)
        {
            if (biblStruct == null)
            {
                return new GrobidReferenceResult { Type = ReferenceType.Unknown };
            }

            var xmlId = biblStruct.Attribute(Xml + "id")?.Value;
            var analytic = biblStruct.Element(Tei + "analytic");
            var monogr = biblStruct.Element(Tei + "monogr");
            
            var analyticTitle = analytic?.Element(Tei + "title")?.Value;
            var monogrTitle = monogr?.Element(Tei + "title")?.Value;
            
            var mainTitle = analyticTitle ?? monogrTitle;
            var type = DetermineType(biblStruct, analytic, monogr, mainTitle);

            return new GrobidReferenceResult
            {
                XmlId = xmlId,
                MainTitle = mainTitle,
                Type = type
            };
        }

        private ReferenceType DetermineType(XElement biblStruct, XElement? analytic, XElement? monogr, string? title)
        {
            // 1. Standard (Check title for standard identifiers)
            if (!string.IsNullOrWhiteSpace(title))
            {
                var upperTitle = title.ToUpperInvariant();
                if (upperTitle.Contains("ISO") || 
                    upperTitle.Contains("IEEE STD") || 
                    upperTitle.Contains("RFC") || 
                    upperTitle.Contains("ITU"))
                {
                    return ReferenceType.Standard;
                }
            }

            // 2. Report (note type="report_type" or keywords in title)
            var reportNote = biblStruct.Elements(Tei + "note")
                .FirstOrDefault(n => n.Attribute("type")?.Value == "report_type");
            
            if (reportNote != null || (!string.IsNullOrWhiteSpace(title) && 
                (title.Contains("Technical Report", StringComparison.OrdinalIgnoreCase) ||
                 title.Contains("White Paper", StringComparison.OrdinalIgnoreCase) ||
                 title.Contains("Memo", StringComparison.OrdinalIgnoreCase))))
            {
                return ReferenceType.Report;
            }

            // 3. Website (ptr target pointing to non-DOI URL or known media outlet)
            var ptr = biblStruct.Descendants(Tei + "ptr").FirstOrDefault();
            var target = ptr?.Attribute("target")?.Value;
            
            if (!string.IsNullOrWhiteSpace(target) && !target.Contains("doi.org"))
            {
                return ReferenceType.Website;
            }

            if (!string.IsNullOrWhiteSpace(title))
            {
                var lowerTitle = title.ToLowerInvariant();
                if (lowerTitle.Contains("forbes") || lowerTitle.Contains("new york times") || lowerTitle.Contains("nyt"))
                {
                    return ReferenceType.Website;
                }
            }

            // 4. Thesis (university name in publisher AND note contains "thesis" or "dissertation")
            var publisher = monogr?.Element(Tei + "imprint")?.Element(Tei + "publisher")?.Value;
            var notes = biblStruct.Elements(Tei + "note").Select(n => n.Value.ToLowerInvariant()).ToList();
            
            if (!string.IsNullOrWhiteSpace(publisher) && 
                (publisher.Contains("University", StringComparison.OrdinalIgnoreCase) || 
                 publisher.Contains("Institute", StringComparison.OrdinalIgnoreCase)) &&
                (notes.Any(n => n.Contains("thesis") || n.Contains("dissertation"))))
            {
                return ReferenceType.Thesis;
            }

            // 5. AcademicPaper (analytic exists AND monogr title level="j" or level="m")
            if (analytic != null && monogr != null)
            {
                var monogrTitleElement = monogr.Element(Tei + "title");
                var level = monogrTitleElement?.Attribute("level")?.Value;
                
                if (level == "j" || level == "m")
                {
                    return ReferenceType.AcademicPaper;
                }
            }

            // 6. Book (analytic MISSING AND monogr level="m" AND publisher exists)
            if (analytic == null && monogr != null)
            {
                var monogrTitleElement = monogr.Element(Tei + "title");
                var level = monogrTitleElement?.Attribute("level")?.Value;
                var hasPublisher = monogr.Element(Tei + "imprint")?.Element(Tei + "publisher") != null;

                if (level == "m" && hasPublisher)
                {
                    return ReferenceType.Book;
                }
            }

            return ReferenceType.Unknown;
        }
    }
}
