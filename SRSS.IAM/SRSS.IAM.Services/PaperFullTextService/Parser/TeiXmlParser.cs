using SRSS.IAM.Services.DTOs.PaperFullText;
using System.Xml.Linq;
using System.Linq;
using System;

namespace SRSS.IAM.Services.PaperFullTextService.Parser
{
    public interface ITeiXmlParser
    {
        ParsedPaperFullTextDto Parse(string rawXml);
    }

    public class TeiXmlParser : ITeiXmlParser
    {
        public ParsedPaperFullTextDto Parse(string rawXml)
        {
            var result = new ParsedPaperFullTextDto();
            if (string.IsNullOrWhiteSpace(rawXml)) return result;

            try
            {
                var doc = XDocument.Parse(rawXml);
                XNamespace ns = "http://www.tei-c.org/ns/1.0";

                int globalSectionOrder = 1;

                // 1. Parse Abstract from teiHeader/profileDesc/abstract
                var abstractNode = doc.Descendants(ns + "profileDesc")
                                      .Descendants(ns + "abstract")
                                      .FirstOrDefault();
                
                if (abstractNode != null)
                {
                    var abstractSection = new ParsedSectionDto
                    {
                        Order = globalSectionOrder,
                        SectionTitle = "Abstract",
                        SectionType = "Abstract"
                    };

                    int pOrder = 1;
                    foreach (var p in abstractNode.Descendants(ns + "p"))
                    {
                        var text = p.Value?.Trim();
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            abstractSection.Paragraphs.Add(new ParsedParagraphDto
                            {
                                Order = pOrder++,
                                Text = text,
                                Coordinates = p.Attribute("coords")?.Value
                            });
                        }
                    }

                    if (abstractSection.Paragraphs.Any())
                    {
                        result.Sections.Add(abstractSection);
                        globalSectionOrder++;
                    }
                }

                // 2. Parse Body from text/body/div
                var body = doc.Descendants(ns + "body").FirstOrDefault();
                if (body != null)
                {
                    foreach (var div in body.Elements(ns + "div"))
                    {
                        var headNode = div.Element(ns + "head");
                        var title = headNode?.Value?.Trim() ?? "Untitled";
                        
                        var section = new ParsedSectionDto
                        {
                            Order = globalSectionOrder,
                            SectionTitle = title,
                            SectionType = DetermineSectionType(title),
                            Coordinates = headNode?.Attribute("coords")?.Value
                        };

                        int pOrder = 1;
                        foreach (var p in div.Elements(ns + "p"))
                        {
                            var text = p.Value?.Trim();
                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                section.Paragraphs.Add(new ParsedParagraphDto
                                {
                                    Order = pOrder++,
                                    Text = text,
                                    Coordinates = p.Attribute("coords")?.Value
                                });
                            }
                        }

                        if (section.Paragraphs.Any())
                        {
                            result.Sections.Add(section);
                            globalSectionOrder++;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // In a real scenario, we might want to log this specifically
                // But as per instructions, we follow the simplified structure for now
                throw;
            }

            return result;
        }

        private string DetermineSectionType(string? head)
        {
            if (string.IsNullOrWhiteSpace(head)) return "Other";

            var h = head.ToLowerInvariant();
            if (h.Contains("abstract")) return "Abstract";
            if (h.Contains("introduction") || h.Contains("background")) return "Introduction";
            if (h.Contains("method") || h.Contains("material")) return "Methods";
            if (h.Contains("result")) return "Results";
            if (h.Contains("discussion")) return "Discussion";
            if (h.Contains("conclusion")) return "Conclusion";

            return "Other";
        }
    }
}
