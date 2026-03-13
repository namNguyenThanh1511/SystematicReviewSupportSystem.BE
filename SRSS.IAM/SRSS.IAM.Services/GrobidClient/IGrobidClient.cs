namespace SRSS.IAM.Services.GrobidClient;

using System.IO;
using System.Threading.Tasks;

public interface IGrobidClient
{
    Task<string> ProcessHeaderDocumentAsync(
        Stream pdfStream,
        int consolidateHeader = 0,
        bool includeRawAffiliations = false,
        bool includeRawCopyrights = false,
        int startPage = 1,
        int endPage = 2);

    Task<string> ProcessFulltextDocumentAsync(
        Stream pdfStream,
        int consolidateHeader = 0,
        int consolidateCitations = 0,
        int consolidateFunders = 0,
        bool segmentSentences = false);

    Task<string> ProcessReferencesAsync(
        Stream pdfStream,
        int consolidateCitations = 0,
        bool includeRawCitations = false,
        bool returnBibtex = false);
}