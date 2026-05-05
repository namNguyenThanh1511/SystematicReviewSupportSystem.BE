using System.Text;
using System.Text.RegularExpressions;
using SRSS.IAM.Services.DTOs.Identification;

namespace SRSS.IAM.Services.Utils
{
    public static partial class BibTexParser
    {
        [GeneratedRegex(@"@(\w+)\s*\{\s*([^,]+)\s*,\s*(.*?)\n\s*\}", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
        private static partial Regex EntryRegex();

        [GeneratedRegex(@"\b(\w+)\s*=\s*(.*)", RegexOptions.IgnoreCase)]
        private static partial Regex FieldRegex();

        public static List<RisPaperDto> Parse(Stream fileStream)
        {
            var papers = new List<RisPaperDto>();
            using var reader = new StreamReader(fileStream, Encoding.UTF8);
            var content = reader.ReadToEnd();

            // More robust entry splitting: Find @something{...}
            // BibTeX entries can be complex, this regex finds the main blocks
            var entries = Regex.Matches(content, @"@(\w+)\s*\{\s*([^,]+)\s*,\s*(.*?)\n\s*\}", RegexOptions.Singleline);

            foreach (Match entryMatch in entries)
            {
                var type = entryMatch.Groups[1].Value.Trim();
                var citeKey = entryMatch.Groups[2].Value.Trim();
                var fieldsContent = entryMatch.Groups[3].Value;

                var paper = new RisPaperDto
                {
                    PublicationType = type,
                    Id = citeKey,
                    RawReference = entryMatch.Value
                };

                // Parse fields
                ParseFields(fieldsContent, paper);
                
                if (!string.IsNullOrWhiteSpace(paper.Title))
                {
                    papers.Add(paper);
                }
            }

            return papers;
        }

        private static void ParseFields(string fieldsContent, RisPaperDto paper)
        {
            // Split fields by comma, but be careful with commas inside braces
            // This is a simplified approach for BibTeX parsing
            var lines = fieldsContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var line in lines)
            {
                var match = Regex.Match(line, @"\s*(\w+)\s*=\s*(.*)", RegexOptions.IgnoreCase);
                if (!match.Success) continue;

                var key = match.Groups[1].Value.Trim().ToLowerInvariant();
                var value = match.Groups[2].Value.Trim();

                // Remove trailing comma if present
                if (value.EndsWith(','))
                {
                    value = value.Substring(0, value.Length - 1).Trim();
                }

                // Clean the value (remove {} and "")
                value = CleanValue(value);

                switch (key)
                {
                    case "author":
                        var authors = Regex.Split(value, @"\s+and\s+", RegexOptions.IgnoreCase);
                        foreach (var auth in authors)
                        {
                            var cleanedAuth = auth.Trim();
                            if (!string.IsNullOrEmpty(cleanedAuth) && !paper.AuthorList.Contains(cleanedAuth))
                            {
                                paper.AuthorList.Add(cleanedAuth);
                            }
                        }
                        break;

                    case "title":
                        paper.Title = value;
                        break;

                    case "journal":
                    case "journaltitle":
                        paper.Journal = value;
                        break;

                    case "year":
                        paper.PublicationYear = value;
                        break;

                    case "booktitle":
                        paper.BookTitle = value;
                        paper.ConferenceName = value;
                        break;

                    case "pages":
                        paper.Pages = value.Replace("--", "-");
                        break;

                    case "abstract":
                        paper.Abstract = value;
                        break;

                    case "doi":
                        paper.DOI = value;
                        break;

                    case "keywords":
                        var keywords = value.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var kw in keywords)
                        {
                            var cleanedKw = kw.Trim();
                            if (!string.IsNullOrEmpty(cleanedKw) && !paper.KeywordList.Contains(cleanedKw))
                            {
                                paper.KeywordList.Add(cleanedKw);
                            }
                        }
                        break;

                    case "volume":
                        paper.Volume = value;
                        break;

                    case "number":
                    case "issue":
                        paper.Issue = value;
                        break;

                    case "publisher":
                        paper.Publisher = value;
                        break;

                    case "url":
                        paper.Url = value;
                        break;
                }
            }
        }

        private static string CleanValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;

            // Remove starting and ending braces or quotes
            if ((value.StartsWith('{') && value.EndsWith('}')) || (value.StartsWith('"') && value.EndsWith('"')))
            {
                value = value.Substring(1, value.Length - 2);
            }

            // Remove LaTeX-style curly braces inside (e.g. {T}he {A}rticle -> The Article)
            value = value.Replace("{", "").Replace("}", "");
            
            // Remove any remaining quotes at the ends (robustness)
            value = value.Trim('"');

            return value.Trim();
        }
    }
}
