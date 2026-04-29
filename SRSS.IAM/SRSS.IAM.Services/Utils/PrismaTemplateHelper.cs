using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text.RegularExpressions;

namespace SRSS.IAM.Services.Utils
{
    public static class PrismaTemplateFixer
    {
        private static readonly Dictionary<string, string> KeywordMap = new()
        {
            { "Records identified", "{{RecordsIdentified}}" },
            { "Duplicate records removed", "{{DuplicateRecordsRemoved}}" },
            { "Records screened", "{{RecordsScreened}}" },
            { "Records excluded", "{{RecordsExcluded}}" },
            { "Reports sought for retrieval", "{{ReportsSoughtForRetrieval}}" },
            { "Reports not retrieved", "{{ReportsNotRetrieved}}" },
            { "Reports assessed for eligibility", "{{ReportsAssessed}}" },
            { "Reports excluded", "{{ReportsExcludedFT}}" },
            { "Studies included in review", "{{StudiesIncluded}}" }
        };

        public static void PrepareTemplate(string filePath)
        {
            using (WordprocessingDocument doc = WordprocessingDocument.Open(filePath, true))
            {
                FixPrismaPlaceholders(doc);
                doc.MainDocumentPart?.Document.Save();
            }
        }

        public static void FixPrismaPlaceholders(WordprocessingDocument doc)
        {
            var allParagraphs = GetAllParagraphs(doc);

            foreach (var para in allParagraphs)
            {
                string text = para.InnerText;
                bool modified = false;

                // 1. Handle Bulk Sections (Breakdown/Reasons)
                // These typically occupy their own lines or sections within a box
                if (text.Contains("Database") || text.Contains("Register"))
                {
                    text = "{{IdentifiedBreakdown}}";
                    modified = true;
                }
                else if (text.Contains("Reason 1") || text.Contains("Reason 2"))
                {
                    // Context check to distinguish between TA and FT exclusion reasons
                    // This assumes keywords appear in nearby boxes or the same box
                    if (IsFullTextContext(para))
                        text = "{{ExclusionReasonsFT}}";
                    else
                        text = "{{ExclusionReasonsTA}}";
                    modified = true;
                }
                // 2. Handle standard (n = ) replacements
                else
                {
                    foreach (var entry in KeywordMap)
                    {
                        if (text.Contains(entry.Key, StringComparison.OrdinalIgnoreCase) && text.Contains("(n = )"))
                        {
                            // Replace "(n = )" with the specific placeholder
                            text = text.Replace("(n = )", $"(n = {entry.Value})");
                            modified = true;
                            break; 
                        }
                    }
                }

                if (modified)
                {
                    UpdateParagraphText(para, text);
                }
            }
        }

        private static bool IsFullTextContext(Paragraph para)
        {
            // Simple heuristic: search upwards or in the same container for "eligibility" or "full-text"
            // In PRISMA, FT exclusion reasons are usually in the bottom half of the diagram
            var container = para.Ancestors<TextBoxContent>().FirstOrDefault();
            return container?.InnerText.Contains("eligibility", StringComparison.OrdinalIgnoreCase) ?? false;
        }

        private static void UpdateParagraphText(Paragraph para, string newText)
        {
            // Capture original formatting from the first run
            var firstRun = para.GetFirstChild<Run>();
            var runProps = firstRun?.RunProperties?.CloneNode(true) as RunProperties;

            para.RemoveAllChildren<Run>();

            var newRun = new Run(new Text(newText));
            if (runProps != null)
            {
                newRun.PrependChild(runProps);
            }
            para.AppendChild(newRun);
        }

        private static IEnumerable<Paragraph> GetAllParagraphs(WordprocessingDocument doc)
        {
            var parts = new List<OpenXmlPart>();
            if (doc.MainDocumentPart != null)
            {
                parts.Add(doc.MainDocumentPart);
                parts.AddRange(doc.MainDocumentPart.HeaderParts);
                parts.AddRange(doc.MainDocumentPart.FooterParts);
            }

            foreach (var part in parts)
            {
                // Traverse Body/Header/Footer
                foreach (var para in part.RootElement!.Descendants<Paragraph>())
                {
                    yield return para;
                }

                // Specifically traverse TextBoxContent (covers DrawingML and VML)
                // OpenXML stores TextBox content separately from the main flow
                foreach (var txbx in part.RootElement!.Descendants<TextBoxContent>())
                {
                    foreach (var para in txbx.Descendants<Paragraph>())
                    {
                        yield return para;
                    }
                }
            }
        }
    }
}
