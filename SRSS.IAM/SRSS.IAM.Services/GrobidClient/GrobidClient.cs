namespace SRSS.IAM.Services.GrobidClient;

using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

public class GrobidClient : IGrobidClient
{
    private readonly HttpClient _httpClient;
    private readonly GrobidOptions _options;

    public GrobidClient(HttpClient httpClient, IOptions<GrobidOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? new GrobidOptions();

        if (!string.IsNullOrEmpty(_options.BaseUrl))
        {
            _httpClient.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/') + "/");
        }
    }

    public async Task<string> ProcessHeaderDocumentAsync(
        Stream pdfStream,
        int consolidateHeader = 0,
        bool includeRawAffiliations = false,
        bool includeRawCopyrights = false,
        int startPage = 1,
        int endPage = 2)
    {
        return await SendRequestWithRetryAsync(
            CreateHeaderContent(pdfStream, consolidateHeader, includeRawAffiliations, includeRawCopyrights, startPage, endPage),
            3, TimeSpan.FromSeconds(2));
    }

    public async Task<string> ProcessFulltextDocumentAsync(
        Stream pdfStream,
        int consolidateHeader = 0,
        int consolidateCitations = 0,
        int consolidateFunders = 0,
        bool segmentSentences = false)
    {
        return await SendRequestWithRetryAsync(
            CreateFulltextContent(pdfStream, consolidateHeader, consolidateCitations, consolidateFunders, segmentSentences),
            2, TimeSpan.FromSeconds(7));
    }

    public async Task<string> ProcessReferencesAsync(
        Stream pdfStream,
        int consolidateCitations = 0,
        bool includeRawCitations = false,
        bool returnBibtex = false)
    {
        return await SendRequestWithRetryAsync(
            CreateReferencesContent(pdfStream, consolidateCitations, includeRawCitations, returnBibtex),
            2, TimeSpan.FromSeconds(4));
    }

    private Func<HttpRequestMessage> CreateHeaderContent(
        Stream pdfStream, int consolidateHeader, bool includeRawAffiliations, bool includeRawCopyrights, int startPage, int endPage)
    {
        return () =>
        {
            var content = new MultipartFormDataContent();
            AddPdfContent(content, pdfStream);
            content.Add(new StringContent(consolidateHeader.ToString()), "consolidateHeader");
            content.Add(new StringContent(includeRawAffiliations ? "1" : "0"), "includeRawAffiliations");
            content.Add(new StringContent(includeRawCopyrights ? "1" : "0"), "includeRawCopyrights");
            content.Add(new StringContent(startPage.ToString()), "start");
            content.Add(new StringContent(endPage.ToString()), "end");

            var request = new HttpRequestMessage(HttpMethod.Post, "api/processHeaderDocument")
            {
                Content = content
            };
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            return request;
        };
    }

    private Func<HttpRequestMessage> CreateFulltextContent(
        Stream pdfStream, int consolidateHeader, int consolidateCitations, int consolidateFunders, bool segmentSentences)
    {
        return () =>
        {
            var content = new MultipartFormDataContent();
            AddPdfContent(content, pdfStream);
            content.Add(new StringContent(consolidateHeader.ToString()), "consolidateHeader");
            content.Add(new StringContent(consolidateCitations.ToString()), "consolidateCitations");
            content.Add(new StringContent(consolidateFunders.ToString()), "consolidateFunders");
            content.Add(new StringContent(segmentSentences ? "1" : "0"), "segmentSentences");

            var request = new HttpRequestMessage(HttpMethod.Post, "api/processFulltextDocument")
            {
                Content = content
            };
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            return request;
        };
    }

    private Func<HttpRequestMessage> CreateReferencesContent(
        Stream pdfStream, int consolidateCitations, bool includeRawCitations, bool returnBibtex)
    {
        return () =>
        {
            var content = new MultipartFormDataContent();
            AddPdfContent(content, pdfStream);
            content.Add(new StringContent(consolidateCitations.ToString()), "consolidateCitations");
            content.Add(new StringContent(includeRawCitations ? "1" : "0"), "includeRawCitations");

            var request = new HttpRequestMessage(HttpMethod.Post, "api/processReferences")
            {
                Content = content
            };

            request.Headers.Accept.Clear();
            if (returnBibtex)
            {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-bibtex"));
            }
            else
            {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            }

            return request;
        };
    }

    private void AddPdfContent(MultipartFormDataContent content, Stream pdfStream)
    {
        if (pdfStream.CanSeek)
        {
            pdfStream.Seek(0, SeekOrigin.Begin);
        }

        var streamContent = new StreamContent(pdfStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        content.Add(streamContent, "input", "document.pdf");
    }

    private async Task<string> SendRequestWithRetryAsync(Func<HttpRequestMessage> requestFactory, int maxRetries, TimeSpan retryDelay)
    {
        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            using var request = requestFactory();
            HttpResponseMessage response;
            
            try
            {
                response = await _httpClient.SendAsync(request);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                throw new GrobidTimeoutException("Request to GROBID API timed out.");
            }
            
            if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable && attempt < maxRetries)
            {
                await Task.Delay(retryDelay);
                continue;
            }

            await HandleErrorAsync(response);

            return await response.Content.ReadAsStringAsync();
        }

        throw new GrobidException("Exceeded maximum retries for GROBID API.");
    }

    private async Task HandleErrorAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var content = await response.Content.ReadAsStringAsync();
        var errorMessage = $"GROBID API Error ({(int)response.StatusCode}): {content}";

        if (content.Contains("BAD_INPUT_DATA") || 
            content.Contains("NO_BLOCKS") || 
            content.Contains("TOO_MANY_BLOCKS") || 
            content.Contains("TOO_MANY_TOKENS") || 
            content.Contains("PDFALTO_CONVERSION_FAILURE"))
        {
            throw new GrobidInvalidPdfException(errorMessage);
        }

        if (content.Contains("TIMEOUT"))
        {
            throw new GrobidTimeoutException(errorMessage);
        }

        throw new GrobidException(errorMessage);
    }
}