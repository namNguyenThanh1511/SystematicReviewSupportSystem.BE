namespace SRSS.IAM.Services.DTOs.Crossref;

/// <summary>
/// Parameters for the GET /works endpoint.
/// All fields are optional; supply at least one to get useful results.
/// See: https://api.crossref.org/swagger-ui/index.html#/Works/get_works
/// </summary>
public class CrossrefQueryParameters
{
    // ─── Field queries (query.xxx) ────────────────────────────────────────────

    /// <summary>General keyword search across all fields.</summary>
    public string? Query { get; set; }

    /// <summary>query.author — search in author given/family names.</summary>
    public string? QueryAuthor { get; set; }

    /// <summary>query.title — search in title fields.</summary>
    public string? QueryTitle { get; set; }

    /// <summary>query.bibliographic — search in bibliographic fields (title, authors, ISSN, volume, journal, year).</summary>
    public string? QueryBibliographic { get; set; }

    /// <summary>query.affiliation — search in contributor affiliations.</summary>
    public string? QueryAffiliation { get; set; }

    /// <summary>query.editor — search in editor names.</summary>
    public string? QueryEditor { get; set; }

    /// <summary>query.contributor — search in all contributor names (author, editor, chair, translator).</summary>
    public string? QueryContributor { get; set; }

    /// <summary>query.container-title — search in publication name (journal/book).</summary>
    public string? QueryContainerTitle { get; set; }

    /// <summary>query.event-name — search in event/conference name.</summary>
    public string? QueryEventName { get; set; }

    /// <summary>query.event-location — search in event location.</summary>
    public string? QueryEventLocation { get; set; }

    /// <summary>query.event-sponsor — search in event sponsor.</summary>
    public string? QueryEventSponsor { get; set; }

    /// <summary>query.publisher-name — search in publisher name.</summary>
    public string? QueryPublisherName { get; set; }

    /// <summary>query.publisher-location — search in publisher location.</summary>
    public string? QueryPublisherLocation { get; set; }

    /// <summary>query.funder-name — search in funder name.</summary>
    public string? QueryFunderName { get; set; }

    // ─── Pagination ───────────────────────────────────────────────────────────

    /// <summary>Number of results per page (max 1000).</summary>
    public int? Rows { get; set; }

    /// <summary>Number of results to skip (max 10 000; prefer cursor for large sets).</summary>
    public int? Offset { get; set; }

    /// <summary>Cursor value for deep pagination. Start with <c>*</c>.</summary>
    public string? Cursor { get; set; }

    // ─── Sorting ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Field to sort by. Supported: created, deposited, indexed, is-referenced-by-count,
    /// issued, published, published-online, published-print, references-count, relevance, score, updated.
    /// </summary>
    public string? Sort { get; set; }

    /// <summary>Sort direction: <c>asc</c> or <c>desc</c> (default).</summary>
    public string? Order { get; set; }

    // ─── Filtering ────────────────────────────────────────────────────────────

    /// <summary>
    /// Filter expression (e.g. <c>from-pub-date:2020-01-01,type:journal-article</c>).
    /// See the full filter list at https://api.crossref.org/swagger-ui.
    /// </summary>
    public string? Filter { get; set; }

    // ─── Field selection ─────────────────────────────────────────────────────

    /// <summary>
    /// Comma-separated list of top-level fields to return.
    /// E.g. <c>DOI,title,author,abstract,published</c>.
    /// </summary>
    public string? Select { get; set; }

    // ─── Facets ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Facet expression (e.g. <c>type-name:*</c>). Returns aggregate counts alongside items.
    /// </summary>
    public string? Facet { get; set; }

    // ─── Sampling ─────────────────────────────────────────────────────────────

    /// <summary>Return N randomly sampled items (cannot be combined with cursor).</summary>
    public int? Sample { get; set; }

    // ─── Polite pool ─────────────────────────────────────────────────────────

    /// <summary>
    /// Your contact e-mail. Supplying this identifies your traffic and grants
    /// access to Crossref's "polite" pool with higher rate limits.
    /// </summary>
    public string? Mailto { get; set; }

    // ─── Project Context ─────────────────────────────────────────────────────

    /// <summary>
    /// Optional project ID. If provided, the service will mark works that 
    /// have already been imported into this project.
    /// </summary>
    public Guid? ProjectId { get; set; }
}
