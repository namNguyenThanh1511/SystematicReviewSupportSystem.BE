using System.Threading.Tasks;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Services.MetadataMergeService
{
    public class MetadataMergeService : IMetadataMergeService
    {
        public Task MergeSelectedFieldsAsync(Paper paper, PaperSourceMetadata sourceMetadata, List<string> fields)
        {
            if (paper == null || sourceMetadata == null || fields == null || !fields.Any())
            {
                return Task.CompletedTask;
            }

            var lowerFields = fields.Select(f => f.ToLowerInvariant()).ToList();

            if (lowerFields.Contains("title") && !string.IsNullOrWhiteSpace(sourceMetadata.Title))
                paper.Title = sourceMetadata.Title;

            if (lowerFields.Contains("authors") && !string.IsNullOrWhiteSpace(sourceMetadata.Authors))
                paper.Authors = sourceMetadata.Authors;

            if (lowerFields.Contains("abstract") && !string.IsNullOrWhiteSpace(sourceMetadata.Abstract))
                paper.Abstract = sourceMetadata.Abstract;

            if (lowerFields.Contains("doi") && !string.IsNullOrWhiteSpace(sourceMetadata.DOI))
                paper.DOI = sourceMetadata.DOI;

            if (lowerFields.Contains("journal") && !string.IsNullOrWhiteSpace(sourceMetadata.Journal))
                paper.Journal = sourceMetadata.Journal;

            if (lowerFields.Contains("volume") && !string.IsNullOrWhiteSpace(sourceMetadata.Volume))
                paper.Volume = sourceMetadata.Volume;

            if (lowerFields.Contains("issue") && !string.IsNullOrWhiteSpace(sourceMetadata.Issue))
                paper.Issue = sourceMetadata.Issue;

            if (lowerFields.Contains("pages") && !string.IsNullOrWhiteSpace(sourceMetadata.Pages))
                paper.Pages = sourceMetadata.Pages;

            if (lowerFields.Contains("publisher") && !string.IsNullOrWhiteSpace(sourceMetadata.Publisher))
                paper.Publisher = sourceMetadata.Publisher;

            if (lowerFields.Contains("year") && sourceMetadata.Year.HasValue)
            {
                paper.PublicationYearInt = sourceMetadata.Year.Value;
                paper.PublicationYear = sourceMetadata.Year.Value.ToString();
            }

            // Note: If you want to support keywords, language, etc., you can add them here
            if (lowerFields.Contains("keywords") && !string.IsNullOrWhiteSpace(sourceMetadata.Keywords))
                paper.Keywords = sourceMetadata.Keywords;

            if (lowerFields.Contains("language") && !string.IsNullOrWhiteSpace(sourceMetadata.Language))
                paper.Language = sourceMetadata.Language;

            if (lowerFields.Contains("issn") && !string.IsNullOrWhiteSpace(sourceMetadata.ISSN))
                paper.JournalIssn = sourceMetadata.ISSN;

            if (lowerFields.Contains("eissn") && !string.IsNullOrWhiteSpace(sourceMetadata.EISSN))
                paper.JournalEIssn = sourceMetadata.EISSN;
                
            if (lowerFields.Contains("md5") && !string.IsNullOrWhiteSpace(sourceMetadata.Md5))
                paper.Md5 = sourceMetadata.Md5;

            return Task.CompletedTask;
        }

        public Task MergeAsync(Paper paper, PaperSourceMetadata sourceMetadata)
        {
            if (paper == null || sourceMetadata == null)
            {
                return Task.CompletedTask;
            }

            // DOI: prefer RIS (only overwrite if RIS is empty)
            if (string.IsNullOrWhiteSpace(paper.DOI) && !string.IsNullOrWhiteSpace(sourceMetadata.DOI))
            {
                paper.DOI = sourceMetadata.DOI;
            }

            // Title: choose the longest title
            if (!string.IsNullOrWhiteSpace(sourceMetadata.Title))
            {
                if (string.IsNullOrWhiteSpace(paper.Title) || sourceMetadata.Title.Length > paper.Title.Length)
                {
                    paper.Title = sourceMetadata.Title;
                }
            }

            // Abstract: prefer GROBID if RIS abstract is empty
            if (string.IsNullOrWhiteSpace(paper.Abstract) && !string.IsNullOrWhiteSpace(sourceMetadata.Abstract))
            {
                paper.Abstract = sourceMetadata.Abstract;
            }

            // Authors: Prefer longer string
            if (!string.IsNullOrWhiteSpace(sourceMetadata.Authors))
            {
                if (string.IsNullOrWhiteSpace(paper.Authors) || sourceMetadata.Authors.Length > paper.Authors.Length)
                {
                    paper.Authors = sourceMetadata.Authors;
                }
            }

            // Journal: prefer RIS
            if (string.IsNullOrWhiteSpace(paper.Journal) && !string.IsNullOrWhiteSpace(sourceMetadata.Journal))
            {
                paper.Journal = sourceMetadata.Journal;
            }

            // Publisher: Prefer RIS.
            if (string.IsNullOrWhiteSpace(paper.Publisher) && !string.IsNullOrWhiteSpace(sourceMetadata.Publisher))
            {
                paper.Publisher = sourceMetadata.Publisher;
            }

            // PublishedDate: Prefer RIS if present, otherwise use GROBID
            if (paper.PublicationDate == null && !string.IsNullOrWhiteSpace(sourceMetadata.PublishedDate))
            {
                if (DateTimeOffset.TryParse(sourceMetadata.PublishedDate, out var parsedDate))
                {
                    paper.PublicationDate = parsedDate.ToUniversalTime();
                }
            }

            // Year: Derived from PublishedDate if missing
            if (paper.PublicationYearInt == null && sourceMetadata.Year.HasValue)
            {
                paper.PublicationYearInt = sourceMetadata.Year.Value;
                paper.PublicationYear = sourceMetadata.Year.Value.ToString();
            }

            // Volume / Issue / Pages: prefer GROBID if RIS missing
            if (string.IsNullOrWhiteSpace(paper.Volume) && !string.IsNullOrWhiteSpace(sourceMetadata.Volume))
            {
                paper.Volume = sourceMetadata.Volume;
            }

            if (string.IsNullOrWhiteSpace(paper.Issue) && !string.IsNullOrWhiteSpace(sourceMetadata.Issue))
            {
                paper.Issue = sourceMetadata.Issue;
            }

            if (string.IsNullOrWhiteSpace(paper.Pages) && !string.IsNullOrWhiteSpace(sourceMetadata.Pages))
            {
                paper.Pages = sourceMetadata.Pages;
            }
            
            // Keywords: Merge RIS and GROBID keywords. Avoid duplicates. Join using semicolon.
            if (!string.IsNullOrWhiteSpace(sourceMetadata.Keywords))
            {
                if (string.IsNullOrWhiteSpace(paper.Keywords))
                {
                    paper.Keywords = sourceMetadata.Keywords;
                }
                else
                {
                    var existingKeywords = paper.Keywords.Split(';', System.StringSplitOptions.RemoveEmptyEntries).Select(k => k.Trim()).ToList();
                    var newKeywords = sourceMetadata.Keywords.Split(';', System.StringSplitOptions.RemoveEmptyEntries).Select(k => k.Trim()).ToList();
                    
                    foreach (var nk in newKeywords)
                    {
                        if (!existingKeywords.Any(ek => ek.Equals(nk, System.StringComparison.OrdinalIgnoreCase)))
                        {
                            existingKeywords.Add(nk);
                        }
                    }
                    paper.Keywords = string.Join("; ", existingKeywords);
                }
            }

            // Language: Prefer GROBID.
            if (!string.IsNullOrWhiteSpace(sourceMetadata.Language))
            {
                paper.Language = sourceMetadata.Language;
            }

            // ISSN / EISSN: Prefer RIS if available.
            if (string.IsNullOrWhiteSpace(paper.JournalIssn) && !string.IsNullOrWhiteSpace(sourceMetadata.ISSN))
            {
                paper.JournalIssn = sourceMetadata.ISSN;
            }

            if (string.IsNullOrWhiteSpace(paper.JournalEIssn) && !string.IsNullOrWhiteSpace(sourceMetadata.EISSN))
            {
                paper.JournalEIssn = sourceMetadata.EISSN;
            }

            // Md5: Only populate if Paper.Md5 is empty.
            if (string.IsNullOrWhiteSpace(paper.Md5) && !string.IsNullOrWhiteSpace(sourceMetadata.Md5))
            {
                paper.Md5 = sourceMetadata.Md5;
            }

            return Task.CompletedTask;
        }
    }
}
