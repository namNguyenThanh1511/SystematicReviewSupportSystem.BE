using System.Text;
using SRSS.IAM.Services.DTOs.Identification;

namespace SRSS.IAM.Services.Utils
{
    public static class RisParser
    {
        public static List<RisPaperDto> Parse(Stream fileStream)
        {
            var papers = new List<RisPaperDto>();
            var currentPaper = new RisPaperDto();
            var currentRawReference = new StringBuilder();
            var authors = new List<string>();
            var isFirstRecord = true;

            using var reader = new StreamReader(fileStream, Encoding.UTF8);
            string? line;

            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                currentRawReference.AppendLine(line);

                // RIS format: TAG  - VALUE
                if (line.Length < 6 || line[2] != ' ' || line[3] != ' ' || line[4] != '-')
                    continue;

                var tag = line.Substring(0, 2).Trim();
                var value = line.Length > 6 ? line.Substring(6).Trim() : string.Empty;

                switch (tag)
                {
                    case "TY": // Publication Type
                        if (!isFirstRecord && !string.IsNullOrEmpty(currentPaper.Title))
                        {
                            // Save previous paper before starting a new one
                            currentPaper.Authors = string.Join("; ", authors);
                            papers.Add(currentPaper);
                            currentPaper = new RisPaperDto();
                            currentRawReference.Clear();
                            authors.Clear();
                        }
                        isFirstRecord = false;
                        currentPaper.PublicationType = value;
                        break;

                    case "TI": // Title
                    case "T1": // Title (alternative)
                        if (string.IsNullOrEmpty(currentPaper.Title))
                            currentPaper.Title = value;
                        break;

                    case "AB": // Abstract
                        currentPaper.Abstract = value;
                        break;

                    case "DO": // DOI
                        currentPaper.DOI = value;
                        break;

                    case "PY": // Publication Year
                        currentPaper.PublicationYear = value;
                        break;

                    case "DA": // Publication Date
                        if (DateTime.TryParse(value, out var date))
                            currentPaper.PublicationDate = new DateTimeOffset(date);
                        break;

                    case "JO": // Journal Name
                    case "JF": // Journal Name (alternative)
                        if (string.IsNullOrEmpty(currentPaper.Journal))
                            currentPaper.Journal = value;
                        break;

                    case "SN": // ISSN
                        currentPaper.JournalIssn = value;
                        break;

                    case "VL": // Volume
                        currentPaper.Volume = value;
                        break;

                    case "IS": // Issue
                        currentPaper.Issue = value;
                        break;

                    case "SP": // Start Page
                        currentPaper.Pages = value;
                        break;

                    case "EP": // End Page
                        if (!string.IsNullOrEmpty(currentPaper.Pages))
                            currentPaper.Pages += "-" + value;
                        else
                            currentPaper.Pages = value;
                        break;

                    case "AU": // Author
                    case "A1": // Author (alternative)
                        if (!string.IsNullOrEmpty(value))
                            authors.Add(value);
                        break;

                    case "PB": // Publisher
                        currentPaper.Publisher = value;
                        break;

                    case "CY": // Conference Location
                        currentPaper.ConferenceLocation = value;
                        break;

                    case "T2": // Conference Name or Secondary Title
                        currentPaper.ConferenceName = value;
                        break;

                    case "KW": // Keywords
                        if (string.IsNullOrEmpty(currentPaper.Keywords))
                            currentPaper.Keywords = value;
                        else
                            currentPaper.Keywords += "; " + value;
                        break;

                    case "UR": // URL
                        currentPaper.Url = value;
                        break;

                    case "ER": // End of Record
                        currentPaper.RawReference = currentRawReference.ToString();
                        if (!string.IsNullOrEmpty(currentPaper.Title))
                        {
                            currentPaper.Authors = string.Join("; ", authors);
                            papers.Add(currentPaper);
                        }
                        currentPaper = new RisPaperDto();
                        currentRawReference.Clear();
                        authors.Clear();
                        break;

                    default:
                        // Ignore unknown tags safely
                        break;
                }
            }

            // Handle case where file doesn't end with ER tag
            if (!string.IsNullOrEmpty(currentPaper.Title))
            {
                currentPaper.RawReference = currentRawReference.ToString();
                currentPaper.Authors = string.Join("; ", authors);
                papers.Add(currentPaper);
            }

            return papers;
        }
    }
}
