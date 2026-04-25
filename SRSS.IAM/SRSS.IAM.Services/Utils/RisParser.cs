using System.Text;
using System.Text.RegularExpressions;
using SRSS.IAM.Services.DTOs.Identification;

namespace SRSS.IAM.Services.Utils
{
    public static partial class RisParser
    {
        [GeneratedRegex(@"\b(19|20)\d{2}\b")]
        private static partial Regex YearRegex();

        public static List<RisPaperDto> Parse(Stream fileStream)
        {
            var papers = new List<RisPaperDto>();
            var currentPaper = new RisPaperDto();
            var currentRawReference = new StringBuilder();
            var isFirstRecord = true;

            // Local function to handle paper finalization and fallback logic
            void FinalizePaper()
            {
                if (string.IsNullOrEmpty(currentPaper.Title)) return;

                // --- Conference Name Priority Logic ---
                // RIS tags vary by publisher (IEEE, ACM, Scopus). 
                // Priority: C3 (Explicit) > T2 (Secondary Title) > BT (Book Title)
                if (string.IsNullOrWhiteSpace(currentPaper.ConferenceName))
                {
                    if (!string.IsNullOrWhiteSpace(currentPaper.SecondaryTitle))
                        currentPaper.ConferenceName = currentPaper.SecondaryTitle;
                    else if (!string.IsNullOrWhiteSpace(currentPaper.BookTitle))
                        currentPaper.ConferenceName = currentPaper.BookTitle;
                }

                // Final fallback for conference types: if still empty, use Journal field
                if (currentPaper.PublicationType?.ToUpper() == "CONF" && string.IsNullOrWhiteSpace(currentPaper.ConferenceName))
                {
                    currentPaper.ConferenceName = currentPaper.Journal;
                }

                currentPaper.RawReference = currentRawReference.ToString();
                papers.Add(currentPaper);

                // Reset for next record
                currentPaper = new RisPaperDto();
                currentRawReference.Clear();
            }

            using var reader = new StreamReader(fileStream, Encoding.UTF8);
            string? line;

            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                currentRawReference.AppendLine(line);

                // RIS format: TAG  - VALUE (2-letter tag, 2 spaces, dash, space)
                if (line.Length < 6 || line[2] != ' ' || line[3] != ' ' || line[4] != '-')
                    continue;

                var tag = line.Substring(0, 2).Trim().ToUpper();
                var value = line.Length > 6 ? line.Substring(6).Trim() : string.Empty;

                if (string.IsNullOrEmpty(value)) continue;

                switch (tag)
                {
                    case "TY": // Publication Type - Start of record
                        if (!isFirstRecord) FinalizePaper();
                        isFirstRecord = false;
                        currentPaper.PublicationType = value;
                        break;

                    case "TI": // Primary Title
                    case "T1":
                        if (string.IsNullOrEmpty(currentPaper.Title))
                            currentPaper.Title = value;
                        break;

                    case "AU": // Authors
                    case "A1":
                    case "A2":
                        if (!currentPaper.AuthorList.Contains(value))
                            currentPaper.AuthorList.Add(value);
                        break;

                    case "KW": // Keywords
                        if (!currentPaper.KeywordList.Contains(value))
                            currentPaper.KeywordList.Add(value);
                        break;

                    case "AB": // Abstract
                    case "N2":
                        currentPaper.Abstract = value;
                        break;

                    case "DO": // DOI
                        currentPaper.DOI = value;
                        break;

                    case "PY": // Publication Year
                    case "Y1":
                        currentPaper.PublicationYear = ExtractYear(value);
                        break;

                    case "DA": // Publication Date
                        if (DateTime.TryParse(value, out var date))
                            currentPaper.PublicationDate =
                                new DateTimeOffset(DateTime.SpecifyKind(date, DateTimeKind.Utc));
                        break;

                    // --- Publication Venue Tags ---
                    case "JO": // Journal
                    case "JF":
                        if (string.IsNullOrEmpty(currentPaper.Journal))
                            currentPaper.Journal = value;
                        break;

                    case "C3": // Conference Name (Primary for IEEE)
                        currentPaper.ConferenceName = value;
                        break;

                    case "T2": // Secondary Title / Conference Name Fallback
                        currentPaper.SecondaryTitle = value;
                        break;

                    case "BT": // Book Title / Proceedings Title
                        currentPaper.BookTitle = value;
                        // Some systems use BT for Journal if not specified elsewhere
                        if (string.IsNullOrEmpty(currentPaper.Journal))
                            currentPaper.Journal = value;
                        break;

                    case "CD": // Conference Date
                    case "Y2":
                        currentPaper.ConferenceDate = value;
                        break;

                    case "CY": // Conference Location
                        currentPaper.ConferenceLocation = value;
                        break;

                    case "SN": // ISSN/ISBN
                        currentPaper.JournalIssn = value;
                        break;

                    case "VL": // Volume
                        currentPaper.Volume = value;
                        break;

                    case "IS": // Issue
                    case "CP":
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

                    case "PB": // Publisher
                        currentPaper.Publisher = value;
                        break;

                    case "UR": // URL
                        currentPaper.Url = value;
                        break;

                    case "ID": // Record Identifier
                    case "AN": // Accession Number
                        currentPaper.Id = value;
                        break;

                    case "ER": // End of Record
                        FinalizePaper();
                        break;

                    default:
                        // Ignore unknown or unhandled tags safely
                        break;
                }
            }

            // Final check for trailing record if ER was missing
            FinalizePaper();

            return papers;
        }
        private static string? ExtractYear(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            var match = YearRegex().Match(input);
            return match.Success ? match.Value : null;
        }
    }
}
