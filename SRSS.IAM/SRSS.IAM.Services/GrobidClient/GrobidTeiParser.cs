using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using SRSS.IAM.Services.GrobidClient.DTOs;

namespace SRSS.IAM.Services.GrobidClient
{
    public static class GrobidTeiParser
    {
        public static GrobidHeaderExtractionDto Parse(string teiXml)
        {
            var dto = new GrobidHeaderExtractionDto { RawXml = teiXml };
            
            if (string.IsNullOrWhiteSpace(teiXml))
                return dto;

            try
            {
                var doc = XDocument.Parse(teiXml);
                XNamespace tei = "http://www.tei-c.org/ns/1.0";

                var header = doc.Descendants(tei + "teiHeader").FirstOrDefault();
                if (header == null) return dto;

                var fileDesc = header.Element(tei + "fileDesc");
                var sourceDesc = fileDesc?.Element(tei + "sourceDesc");
                var biblStruct = sourceDesc?.Element(tei + "biblStruct");
                var analytic = biblStruct?.Element(tei + "analytic");
                var monogr = biblStruct?.Element(tei + "monogr");

                // Title
                dto.Title = (string?)analytic?.Element(tei + "title")
                            ?? (string?)fileDesc?.Element(tei + "titleStmt")?.Element(tei + "title");

                // Authors
                var authors = analytic?.Elements(tei + "author")
                    .Select(a =>
                    {
                        var persName = a.Element(tei + "persName");
                        if (persName == null) return string.Empty;
                        
                        var forenames = persName.Elements(tei + "forename").Select(f => (string?)f).Where(f => !string.IsNullOrEmpty(f)).Select(f => f!);
                        var surname = (string?)persName.Element(tei + "surname");
                        
                        var nameParts = forenames.ToList();
                        if (!string.IsNullOrWhiteSpace(surname))
                        {
                            nameParts.Add(surname!);
                        }
                        
                        return string.Join(" ", nameParts);
                    })
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .ToList();

                if (authors != null && authors.Any())
                {
                    dto.Authors = string.Join("; ", authors);
                }

                // DOI
                var idnoDoi = (string?)analytic?.Elements(tei + "idno").FirstOrDefault(i => (string?)i.Attribute("type") == "DOI")
                              ?? (string?)biblStruct?.Elements(tei + "idno").FirstOrDefault(i => (string?)i.Attribute("type") == "DOI");
                dto.DOI = idnoDoi;

                // Abstract
                var profileDesc = header.Element(tei + "profileDesc");
                var abstractElem = profileDesc?.Element(tei + "abstract");
                if (abstractElem != null)
                {
                    var paras = abstractElem.Elements(tei + "p").Select(p => (string?)p).Where(p => !string.IsNullOrEmpty(p)).Select(p => p!);
                    if (paras.Any())
                    {
                        dto.Abstract = string.Join("\n\n", paras);
                    }
                    else
                    {
                        dto.Abstract = (string?)abstractElem;
                    }
                }

                // Journal / Monogr info
                if (monogr != null)
                {
                    dto.Journal = (string?)monogr.Elements(tei + "title").FirstOrDefault(t => (string?)t.Attribute("level") == "j")
                                  ?? (string?)monogr.Elements(tei + "title").FirstOrDefault();

                    var imprint = monogr.Element(tei + "imprint");
                    if (imprint != null)
                    {
                        var biblScopeVol = (string?)imprint.Elements(tei + "biblScope").FirstOrDefault(b => (string?)b.Attribute("unit") == "volume");
                        if (!string.IsNullOrWhiteSpace(biblScopeVol)) dto.Volume = biblScopeVol;

                        var biblScopeIssue = (string?)imprint.Elements(tei + "biblScope").FirstOrDefault(b => (string?)b.Attribute("unit") == "issue");
                        if (!string.IsNullOrWhiteSpace(biblScopeIssue)) dto.Issue = biblScopeIssue;

                        var biblScopePages = imprint.Elements(tei + "biblScope").FirstOrDefault(b => (string?)b.Attribute("unit") == "page");
                        if (biblScopePages != null)
                        {
                            var from = (string?)biblScopePages.Attribute("from");
                            var to = (string?)biblScopePages.Attribute("to");
                            if (!string.IsNullOrWhiteSpace(from) && !string.IsNullOrWhiteSpace(to))
                                dto.Pages = $"{from}-{to}";
                            else
                            {
                                var text = (string?)biblScopePages;
                                if (!string.IsNullOrWhiteSpace(text))
                                    dto.Pages = text;
                            }
                        }
                    }
                }

                // Publisher
                dto.Publisher = (string?)fileDesc?.Element(tei + "publicationStmt")?.Element(tei + "publisher");

                // PublishedDate
                var pubStmtDate = (string?)fileDesc?.Element(tei + "publicationStmt")?.Element(tei + "date")?.Attribute("when");
                var imprintDate = (string?)monogr?.Element(tei + "imprint")?.Element(tei + "date")?.Attribute("when");
                var dateStr = !string.IsNullOrWhiteSpace(pubStmtDate) ? pubStmtDate : imprintDate;

                if (!string.IsNullOrWhiteSpace(dateStr))
                {
                    if (DateTimeOffset.TryParse(dateStr, out var date))
                    {
                        dto.PublishedDate = date.ToUniversalTime();
                    }
                }

                // Year
                if (dto.PublishedDate.HasValue)
                {
                    if (int.TryParse(dto.PublishedDate.Value.ToString("yyyy"), out int year))
                    {
                        dto.Year = year;
                    }
                }

                // ISSN / EISSN
                if (monogr != null)
                {
                    var idnoIssn = monogr.Elements(tei + "idno").FirstOrDefault(i => (string?)i.Attribute("type") == "ISSN");
                    if (idnoIssn != null) dto.ISSN = (string?)idnoIssn;

                    var idnoEIssn = monogr.Elements(tei + "idno").FirstOrDefault(i => (string?)i.Attribute("type") == "eISSN");
                    if (idnoEIssn != null) dto.EISSN = (string?)idnoEIssn;
                }

                // Keywords
                var textClass = profileDesc?.Element(tei + "textClass");
                var keywordsElem = textClass?.Element(tei + "keywords");
                if (keywordsElem != null)
                {
                    var terms = keywordsElem.Elements(tei + "term").Select(t => (string?)t).Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t!);
                    if (terms.Any())
                    {
                        dto.Keywords = string.Join("; ", terms);
                    }
                }

                // Language
                dto.Language = (string?)header.Attribute(XNamespace.Xml + "lang");

                // Md5
                dto.Md5 = (string?)biblStruct?.Elements(tei + "idno").FirstOrDefault(i => (string?)i.Attribute("type") == "MD5");
            }
            catch
            {
                // Return dto with RawXml set
            }

            return dto;
        }

        public static List<GrobidReferenceDto> ParseReferences(string teiXml)
        {
            var dtos = new List<GrobidReferenceDto>();
            if (string.IsNullOrWhiteSpace(teiXml))
                return dtos;

            try
            {
                var doc = XDocument.Parse(teiXml);
                XNamespace tei = "http://www.tei-c.org/ns/1.0";

                var listBibl = doc.Descendants(tei + "listBibl").FirstOrDefault();
                if (listBibl == null) return dtos;

                var biblStructs = listBibl.Elements(tei + "biblStruct");
                foreach (var bibl in biblStructs)
                {
                    var dto = new GrobidReferenceDto();
                    var analytic = bibl.Element(tei + "analytic");
                    var monogr = bibl.Element(tei + "monogr");

                    // Title
                    dto.Title = (string?)analytic?.Element(tei + "title")
                                ?? (string?)monogr?.Element(tei + "title") 
                                ?? string.Empty;

                    // Authors
                    var authorsElem = analytic?.Elements(tei + "author");
                    if (authorsElem == null || !authorsElem.Any())
                    {
                        authorsElem = monogr?.Elements(tei + "author");
                    }
                    
                    if (authorsElem != null)
                    {
                        var authorNames = authorsElem.Select(a =>
                        {
                            var persName = a.Element(tei + "persName");
                            if (persName == null) return string.Empty;
                            
                            var forenames = persName.Elements(tei + "forename").Select(f => (string?)f).Where(f => !string.IsNullOrEmpty(f)).Select(f => f!);
                            var surname = (string?)persName.Element(tei + "surname");
                            
                            var nameParts = forenames.ToList();
                            if (!string.IsNullOrWhiteSpace(surname))
                                nameParts.Add(surname!);
                            
                            return string.Join(" ", nameParts);
                        }).Where(n => !string.IsNullOrWhiteSpace(n)).ToList();

                        if (authorNames.Any())
                        {
                            dto.Authors = string.Join("; ", authorNames);
                        }
                    }

                    // DOI
                    var idnoDoi = (string?)analytic?.Elements(tei + "idno").FirstOrDefault(i => (string?)i.Attribute("type") == "DOI")
                                  ?? (string?)bibl.Elements(tei + "idno").FirstOrDefault(i => (string?)i.Attribute("type") == "DOI");
                    dto.DOI = idnoDoi;

                    // PublishedDate / Year
                    var imprintDate = (string?)monogr?.Element(tei + "imprint")?.Element(tei + "date")?.Attribute("when");
                    if (!string.IsNullOrWhiteSpace(imprintDate))
                    {
                        if (imprintDate.Length >= 4)
                        {
                            dto.PublishedYear = imprintDate.Substring(0, 4);
                        }
                    }

                    // Note / Raw Reference
                    var note = bibl.Element(tei + "note");
                    if (note != null && (string?)note.Attribute("type") == "raw_reference")
                    {
                        dto.RawReference = (string?)note;
                    }

                    if (!string.IsNullOrWhiteSpace(dto.Title) || !string.IsNullOrWhiteSpace(dto.RawReference))
                    {
                        dtos.Add(dto);
                    }
                }
            }
            catch
            {
                // Return whatever we managed to parse
            }

            return dtos;
        }
    }
}
