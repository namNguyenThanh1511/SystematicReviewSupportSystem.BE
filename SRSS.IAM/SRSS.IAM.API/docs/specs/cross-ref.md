Request parameters
The request parameters available for each endpoint are given below for each endpoint. Here we provide more details about parameters with specific functionality:

Filters
Filters allow you to select items based on specific criteria and return a list. Multiple filters may be used in the same request. There are many filters available, especially for /works routes. They are described fully on our documentation website. For example:

/works?filter=from-created-date:2023-01-01T12:00:00,until-created-date:2023-01-01T13:00:00

Queries
Search queries can be made to look for the presence of one or more words in any field. For example,

/members?query=association+library

Some queries look only in one or several fields. For example,

/works?query.bibliographic=Richard+Feynmann

only incorporates fields used when referencing the content item (title, authors, volume, journal name, etc.).

Select response fields
For works endpoints, if you only need a few elements from the schema use the select parameter. For example:

/works?select=DOI,prefix,title

Retrieving large result sets
List requests return up to 1000 items in a single request. Paginating through multiple pages of results is possible using the cursor parameter. To retrieve multiple pages:

Add cursor=\* to a request (and rows > 0).
The response will include a next-cursor value.
Use cursor=[next-cursor] in your next request to obtain the following page of results.
Stop sending requests when the number of items in the response is less than the number of rows requested.
Cursors expire after 5 minutes if not used.

You can also use offset:n for an integer n up to 10,000 to return results starting at the the nth record of the result set. We recommend using cursors rather than offset, since there are no page limitations and results are returned faster.

Facets
For works endpoints, retrieve summary statistics by providing a facet parameter along with the type of information, and maximum number of returned values which can be up to 1000. Use \* to retrieve the maximum allowed number of values. The request format follows this example:

/works?facet=type-name:\*

Note that facet counts are approximate and may differ from exact counts obtained using filters. Note that records with the same relationships two or more times are counted multiple times (e.g., records with two published corrections).

Sorting
Results on the works endpoints can be sorted. sort sets the field by which results will be sorted. order sets the result ordering, either asc or desc (the default is desc). The following example sorts results by order of publication, beginning with the oldest:

/works?query=josiah+carberry&sort=published&order=asc

Response types
Responses are in JSON format with the mime-type application/vnd.crossref-api-message+json. If you access the API via a browser, we recommend using a JSON formatter plugin. Other formats can be retrieved for singleton requests using content negotiation.

There are three types of responses:

Singleton: The metadata record of a single object. Retrieving metadata for a specific identifier (e.g., DOI, ISSN, funder identifier) returns a singleton. For example:

https://api.crossref.org/works/10.5555%2F12345678

Headers only: Use an HTTP HEAD requests to quickly determine existence of a singleton without fetching any metadata. It returns headers and an HTTP status code (200=exists, 404=does not exist). For example (in a terminal):

curl --head "https://api.crossref.org/members/98"

List: Requests with queries or filters returns a list that can contain multiple content items. The maximum number of items returned is defined by the rows parameter, which can be set to 0 to retrieve only summary information. For example:

https://api.crossref.org/funders?rows=5

GET /works/{doi}
Returns metadata for the specified Crossref DOI, as an example use DOI 10.5555/12345678

Parameters

Name Description
doi \*
string
(path)
The DOI identifier of the content item.

doi
Responses
Response content type

application/json
Code Description
200
The work identified by {doi}.

Example Value
Model
{
"status": "string",
"message-type": "work",
"message-version": "string",
"message": {
"institution": [
{
"name": "string",
"place": [
"string"
],
"department": [
"string"
],
"acronym": [
"string"
],
"id": [
{
"id": "string",
"id-type": "string",
"asserted-by": "string"
}
]
}
],
"indexed": {
"date-parts": [
[
0
]
],
"version": "string"
},
"description": "string",
"posted": {
"date-parts": [
[
"string"
]
]
},
"publisher-location": "string",
"update-to": [
{
"label": "string",
"DOI": "string",
"type": "string",
"source": "string",
"updated": {
"date-parts": [
[
0
]
],
"date-time": "2026-05-02T08:23:36.358Z",
"timestamp": 0
},
"record-id": "string"
}
],
"standards-body": {
"name": "string",
"acronym": "string"
},
"edition-number": "string",
"group-title": "string",
"reference-count": 0,
"publisher": "string",
"issue": "string",
"isbn-type": [
{
"type": "string",
"value": "string"
}
],
"license": [
{
"URL": "string",
"start": {
"date-parts": [
[
0
]
],
"date-time": "2026-05-02T08:23:36.358Z",
"timestamp": 0
},
"delay-in-days": 0,
"content-version": "string"
}
],
"funder": [
{
"name": "string",
"DOI": "string",
"doi-asserted-by": "string",
"award": [
"string"
],
"id": [
{
"id": "string",
"id-type": "string",
"asserted-by": "string"
}
]
}
],
"content-domain": {
"domain": [
"string"
],
"crossmark-restriction": true
},
"chair": [
{
"ORCID": "string",
"suffix": "string",
"given": "string",
"family": "string",
"affiliation": [
{
"name": "string",
"place": [
"string"
],
"department": [
"string"
],
"acronym": [
"string"
],
"id": [
{
"id": "string",
"id-type": "string",
"asserted-by": "string"
}
]
}
],
"name": "string",
"authenticated-orcid": true,
"prefix": "string",
"sequence": "string"
}
],
"short-container-title": [
"string"
],
"accepted": {
"date-parts": [
[
"string"
]
]
},
"special-numbering": "string",
"content-updated": {
"date-parts": [
[
"string"
]
]
},
"published-print": {
"date-parts": [
[
"string"
]
]
},
"abstract": "string",
"DOI": "string",
"type": "string",
"created": {
"date-parts": [
[
0
]
],
"date-time": "2026-05-02T08:23:36.358Z",
"timestamp": 0
},
"approved": {
"date-parts": [
[
"string"
]
]
},
"page": "string",
"update-policy": "string",
"source": "string",
"is-referenced-by-count": 0,
"title": [
"string"
],
"prefix": "string",
"volume": "string",
"clinical-trial-number": [
{
"clinical-trial-number": "string",
"registry": "string",
"type": "string"
}
],
"author": [
{
"ORCID": "string",
"suffix": "string",
"given": "string",
"family": "string",
"affiliation": [
{
"name": "string",
"place": [
"string"
],
"department": [
"string"
],
"acronym": [
"string"
],
"id": [
{
"id": "string",
"id-type": "string",
"asserted-by": "string"
}
]
}
],
"name": "string",
"authenticated-orcid": true,
"prefix": "string",
"sequence": "string"
}
],
"member": "string",
"content-created": {
"date-parts": [
[
"string"
]
]
},
"published-online": {
"date-parts": [
[
"string"
]
]
},
"reference": [
{
"issn": "string",
"standards-body": "string",
"issue": "string",
"key": "string",
"series-title": "string",
"isbn-type": "string",
"doi-asserted-by": "string",
"first-page": "string",
"DOI": "string",
"type": "string",
"isbn": "string",
"component": "string",
"article-title": "string",
"volume-title": "string",
"volume": "string",
"author": "string",
"standard-designator": "string",
"year": "string",
"unstructured": "string",
"edition": "string",
"journal-title": "string",
"issn-type": "string"
}
],
"updated-by": [
{
"label": "string",
"DOI": "string",
"type": "string",
"source": "string",
"updated": {
"date-parts": [
[
0
]
],
"date-time": "2026-05-02T08:23:36.358Z",
"timestamp": 0
},
"record-id": "string"
}
],
"event": {
"name": "string",
"location": "string",
"start": {
"date-parts": [
[
"string"
]
]
},
"end": {
"date-parts": [
[
"string"
]
]
}
},
"container-title": [
"string"
],
"review": {
"type": "string",
"running-number": "string",
"revision-round": "string",
"stage": "string",
"competing-interest-statement": "string",
"recommendation": "string",
"language": "string"
},
"project": [
{
"award-end": [
{
"date-parts": [
[
"string"
]
]
}
],
"award-planned-start": [
{
"date-parts": [
[
"string"
]
]
}
],
"award-start": [
{
"date-parts": [
[
"string"
]
]
}
],
"lead-investigator": [
{
"ORCID": "string",
"suffix": "string",
"given": "string",
"family": "string",
"affiliation": [
{
"id": [
{
"id": "string",
"id-type": "string",
"asserted-by": "string"
}
],
"name": "string"
}
],
"name": "string",
"role-start": {
"date-parts": [
[
"string"
]
]
},
"authenticated-orcid": true,
"prefix": "string",
"alternate-name": "string",
"sequence": "string",
"role-end": {
"date-parts": [
[
"string"
]
]
}
}
],
"award-planned-end": [
{
"date-parts": [
[
"string"
]
]
}
],
"investigator": [
{
"ORCID": "string",
"suffix": "string",
"given": "string",
"family": "string",
"affiliation": [
{
"id": [
{
"id": "string",
"id-type": "string",
"asserted-by": "string"
}
],
"name": "string"
}
],
"name": "string",
"role-start": {
"date-parts": [
[
"string"
]
]
},
"authenticated-orcid": true,
"prefix": "string",
"alternate-name": "string",
"sequence": "string",
"role-end": {
"date-parts": [
[
"string"
]
]
}
}
],
"funding": [
{
"type": "string",
"scheme": "string",
"award-amount": {
"amount": 0,
"currency": "string",
"percentage": 0
},
"funder": {
"name": "string",
"DOI": "string",
"doi-asserted-by": "string",
"award": [
"string"
],
"id": [
{
"id": "string",
"id-type": "string",
"asserted-by": "string"
}
]
}
}
],
"project-title": [
{
"title": "string",
"language": "string"
}
],
"award-amount": {
"amount": 0,
"currency": "string",
"percentage": 0
},
"co-lead-investigator": [
{
"ORCID": "string",
"suffix": "string",
"given": "string",
"family": "string",
"affiliation": [
{
"id": [
{
"id": "string",
"id-type": "string",
"asserted-by": "string"
}
],
"name": "string"
}
],
"name": "string",
"role-start": {
"date-parts": [
[
"string"
]
]
},
"authenticated-orcid": true,
"prefix": "string",
"alternate-name": "string",
"sequence": "string",
"role-end": {
"date-parts": [
[
"string"
]
]
}
}
],
"project-description": [
{
"description": "string",
"language": "string"
}
]
}
],
"original-title": [
"string"
],
"status": {
"type": "string",
"update": {
"date-parts": [
[
"string"
]
]
},
"status-description": [
{
"language": "string",
"description": "string"
}
]
},
"language": "string",
"link": [
{
"URL": "string",
"content-type": "string",
"content-version": "string",
"intended-application": "string"
}
],
"deposited": {
"date-parts": [
[
0
]
],
"date-time": "2026-05-02T08:23:36.358Z",
"timestamp": 0
},
"score": 0,
"degree": [
"string"
],
"resource": {
"primary": {
"URL": "string"
},
"secondary": [
{
"URL": "string",
"label": "string"
}
]
},
"subtitle": [
"string"
],
"translator": [
{
"ORCID": "string",
"suffix": "string",
"given": "string",
"family": "string",
"affiliation": [
{
"name": "string",
"place": [
"string"
],
"department": [
"string"
],
"acronym": [
"string"
],
"id": [
{
"id": "string",
"id-type": "string",
"asserted-by": "string"
}
]
}
],
"name": "string",
"authenticated-orcid": true,
"prefix": "string",
"sequence": "string"
}
],
"free-to-read": {
"start-date": {
"date-parts": [
[
"string"
]
]
},
"end-date": {
"date-parts": [
[
"string"
]
]
}
},
"editor": [
{
"ORCID": "string",
"suffix": "string",
"given": "string",
"family": "string",
"affiliation": [
{
"name": "string",
"place": [
"string"
],
"department": [
"string"
],
"acronym": [
"string"
],
"id": [
{
"id": "string",
"id-type": "string",
"asserted-by": "string"
}
]
}
],
"name": "string",
"authenticated-orcid": true,
"prefix": "string",
"sequence": "string"
}
],
"proceedings-subject": "string",
"component-number": "string",
"short-title": [
"string"
],
"issued": {
"date-parts": [
[
"string"
]
]
},
"ISBN": [
"string"
],
"references-count": 0,
"part-number": "string",
"aliases": [
"string"
],
"issue-title": [
"string"
],
"journal-issue": {
"issue": "string",
"published-online": {
"date-parts": [
[
"string"
]
]
},
"published-print": {
"date-parts": [
[
"string"
]
]
}
},
"alternative-id": [
"string"
],
"version": {
"version": "string",
"language": "string",
"version-description": [
{
"language": "string",
"description": "string"
}
]
},
"URL": "string",
"archive": [
"string"
],
"relation": {
"additionalProp1": [
{
"id-type": "string",
"id": "string",
"asserted-by": "string"
}
],
"additionalProp2": [
{
"id-type": "string",
"id": "string",
"asserted-by": "string"
}
],
"additionalProp3": [
{
"id-type": "string",
"id": "string",
"asserted-by": "string"
}
]
},
"ISSN": [
"string"
],
"issn-type": [
{
"type": "string",
"value": "string"
}
],
"subject": [
"string"
],
"published-other": {
"date-parts": [
[
"string"
]
]
},
"published": {
"date-parts": [
[
"string"
]
]
},
"assertion": [
{
"group": {
"name": "string",
"label": "string"
},
"explanation": {
"URL": "string"
},
"name": "string",
"value": "string",
"URL": "string",
"order": 0,
"label": "string"
}
],
"subtype": "string",
"article-number": "string"
}
}

GET /works
Returns a list of all works (journal articles, conference proceedings, books, components, etc), 20 per page by default.

In addition to the query parameter, this endpoint supports the following field queries:

query.affiliation - query contributor affiliations
query.author - query author given and family names
query.bibliographic - query bibliographic information, useful for citation look up, includes titles, authors, ISSNs and publication years
query.chair - query chair given and family names
query.container-title - query container title aka. publication name
query.contributor - query author, editor, chair and translator given and family names
query.degree - query degree
query.description - query description
query.editor - query editor given and family names
query.event-acronym - query acronym of the event
query.event-location - query location of the event
query.event-name - query name of the event
query.event-sponsor - query sponsor of the event
query.event-theme - query theme of the event
query.funder-name - query name of the funder
query.publisher-location - query location of the publisher
query.publisher-name - query publisher name
query.standards-body-acronym - query acronym of the standards body
query.standards-body-name - query standards body name
query.title - query title
query.translator - query translator given and family names
sort: Sorting by the following fields is supported:
created deposited indexed is-referenced-by-count issued published published-online published-print references-count relevance score updated

facet: This endpoint supports the following facets:

affiliation - author affiliation
archive - archive location
assertion - custom Crossmark assertion name
assertion-group - custom Crossmark assertion group name
category-name - category name of work
container-title - [max value 100], work container title, such as journal title, or book title
funder-doi - funder DOI
funder-name - funder name as deposited it appears in a metadata record
issn - [max value 100], journal ISSN (any - print, electronic, link)
journal-issue - journal issue number
journal-volume - journal volume
license - license URI of work
link-application - intended application of the full text link
orcid - [max value 100], contributor ORCID
published - earliest year of publication
publisher-name - publisher name of work
relation-type - relation type described by work or described by another work with work as object
ror-id - institution ROR ID
source - source of the DOI
type-name - work type name, such as journal-article or book-chapter
update-type - significant update type
filter: See our documentation website for a full list of available filters.

select: You can select any of the following fields:
DOI ISBN ISSN URL abstract accepted alternative-id approved archive article-number assertion author chair clinical-trial-number container-title content-created content-domain created degree deposited editor event funder group-title indexed is-referenced-by-count issn-type issue issued license link member original-title page posted prefix published published-online published-print publisher publisher-location reference references-count relation resource score short-container-title short-title standards-body subject subtitle title translator type update-policy update-to updated-by volume

Parameters

Name Description
rows
integer($int64)
(query)
The number of rows per page of results

rows
order
string
(query)
Specify the order of sorted results, e.g. asc or desc (default).

order
facet
string
(query)
Retrieve counts for pre-defined facets e.g. type-name:\* returns counts of all works by type. See Facets.

facet
sample
integer($int64)
(query)
Retrieve N randomly sampled items

sample
sort
string
(query)
Sort results by a certain field. See Sorting.

sort
offset
integer($int64)
(query)
The number of rows to skip before returning. See Retrieving large results sets.

offset
mailto
string
(query)
The email address to identify yourself and access the 'polite pool'

mailto
select
string
(query)
Select certain fields, supports a comma separated list of fields. See Select response fields

select
query
string
(query)
Query certain fields. See Queries.

query
filter
string
(query)
Filter by certain fields. See filters

filter
cursor
string
(query)
Page through large result sets. See Retrieving large results sets.

cursor
Responses
Response content type

application/json
Code Description
200
A list of works

Example Value
Model
{
"status": "string",
"message-type": "work-list",
"message-version": "string",
"message": {
"items-per-page": 0,
"query": {
"start-index": 0,
"search-terms": "string"
},
"total-results": 0,
"next-cursor": "string",
"facets": "string",
"items": [
{
"institution": [
{
"name": "string",
"place": [
"string"
],
"department": [
"string"
],
"acronym": [
"string"
],
"id": [
{
"id": "string",
"id-type": "string",
"asserted-by": "string"
}
]
}
],
"indexed": {
"date-parts": [
[
0
]
],
"version": "string"
},
"description": "string",
"posted": {
"date-parts": [
[
"string"
]
]
},
"publisher-location": "string",
"update-to": [
{
"label": "string",
"DOI": "string",
"type": "string",
"source": "string",
"updated": {
"date-parts": [
[
0
]
],
"date-time": "2026-05-02T08:14:45.150Z",
"timestamp": 0
},
"record-id": "string"
}
],
"standards-body": {
"name": "string",
"acronym": "string"
},
"edition-number": "string",
"group-title": "string",
"reference-count": 0,
"publisher": "string",
"issue": "string",
"isbn-type": [
{
"type": "string",
"value": "string"
}
],
"license": [
{
"URL": "string",
"start": {
"date-parts": [
[
0
]
],
"date-time": "2026-05-02T08:14:45.150Z",
"timestamp": 0
},
"delay-in-days": 0,
"content-version": "string"
}
],
"funder": [
{
"name": "string",
"DOI": "string",
"doi-asserted-by": "string",
"award": [
"string"
],
"id": [
{
"id": "string",
"id-type": "string",
"asserted-by": "string"
}
]
}
],
"content-domain": {
"domain": [
"string"
],
"crossmark-restriction": true
},
"chair": [
{
"ORCID": "string",
"suffix": "string",
"given": "string",
"family": "string",
"affiliation": [
{
"name": "string",
"place": [
"string"
],
"department": [
"string"
],
"acronym": [
"string"
],
"id": [
{
"id": "string",
"id-type": "string",
"asserted-by": "string"
}
]
}
],
"name": "string",
"authenticated-orcid": true,
"prefix": "string",
"sequence": "string"
}
],
"short-container-title": [
"string"
],
"accepted": {
"date-parts": [
[
"string"
]
]
},
"special-numbering": "string",
"content-updated": {
"date-parts": [
[
"string"
]
]
},
"published-print": {
"date-parts": [
[
"string"
]
]
},
"abstract": "string",
"DOI": "string",
"type": "string",
"created": {
"date-parts": [
[
0
]
],
"date-time": "2026-05-02T08:14:45.150Z",
"timestamp": 0
},
"approved": {
"date-parts": [
[
"string"
]
]
},
"page": "string",
"update-policy": "string",
"source": "string",
"is-referenced-by-count": 0,
"title": [
"string"
],
"prefix": "string",
"volume": "string",
"clinical-trial-number": [
{
"clinical-trial-number": "string",
"registry": "string",
"type": "string"
}
],
"author": [
{
"ORCID": "string",
"suffix": "string",
"given": "string",
"family": "string",
"affiliation": [
{
"name": "string",
"place": [
"string"
],
"department": [
"string"
],
"acronym": [
"string"
],
"id": [
{
"id": "string",
"id-type": "string",
"asserted-by": "string"
}
]
}
],
"name": "string",
"authenticated-orcid": true,
"prefix": "string",
"sequence": "string"
}
],
"member": "string",
"content-created": {
"date-parts": [
[
"string"
]
]
},
"published-online": {
"date-parts": [
[
"string"
]
]
},
"reference": [
{
"issn": "string",
"standards-body": "string",
"issue": "string",
"key": "string",
"series-title": "string",
"isbn-type": "string",
"doi-asserted-by": "string",
"first-page": "string",
"DOI": "string",
"type": "string",
"isbn": "string",
"component": "string",
"article-title": "string",
"volume-title": "string",
"volume": "string",
"author": "string",
"standard-designator": "string",
"year": "string",
"unstructured": "string",
"edition": "string",
"journal-title": "string",
"issn-type": "string"
}
],
"updated-by": [
{
"label": "string",
"DOI": "string",
"type": "string",
"source": "string",
"updated": {
"date-parts": [
[
0
]
],
"date-time": "2026-05-02T08:14:45.150Z",
"timestamp": 0
},
"record-id": "string"
}
],
"event": {
"name": "string",
"location": "string",
"start": {
"date-parts": [
[
"string"
]
]
},
"end": {
"date-parts": [
[
"string"
]
]
}
},
"container-title": [
"string"
],
"review": {
"type": "string",
"running-number": "string",
"revision-round": "string",
"stage": "string",
"competing-interest-statement": "string",
"recommendation": "string",
"language": "string"
},
"project": [
{
"award-end": [
{
"date-parts": [
[
"string"
]
]
}
],
"award-planned-start": [
{
"date-parts": [
[
"string"
]
]
}
],
"award-start": [
{
"date-parts": [
[
"string"
]
]
}
],
"lead-investigator": [
{
"ORCID": "string",
"suffix": "string",
"given": "string",
"family": "string",
"affiliation": [
{
"id": [
{
"id": "string",
"id-type": "string",
"asserted-by": "string"
}
],
"name": "string"
}
],
"name": "string",
"role-start": {
"date-parts": [
[
"string"
]
]
},
"authenticated-orcid": true,
"prefix": "string",
"alternate-name": "string",
"sequence": "string",
"role-end": {
"date-parts": [
[
"string"
]
]
}
}
],
"award-planned-end": [
{
"date-parts": [
[
"string"
]
]
}
],
"investigator": [
{
"ORCID": "string",
"suffix": "string",
"given": "string",
"family": "string",
"affiliation": [
{
"id": [
{
"id": "string",
"id-type": "string",
"asserted-by": "string"
}
],
"name": "string"
}
],
"name": "string",
"role-start": {
"date-parts": [
[
"string"
]
]
},
"authenticated-orcid": true,
"prefix": "string",
"alternate-name": "string",
"sequence": "string",
"role-end": {
"date-parts": [
[
"string"
]
]
}
}
],
"funding": [
{
"type": "string",
"scheme": "string",
"award-amount": {
"amount": 0,
"currency": "string",
"percentage": 0
},
"funder": {
"name": "string",
"DOI": "string",
"doi-asserted-by": "string",
"award": [
"string"
],
"id": [
{
"id": "string",
"id-type": "string",
"asserted-by": "string"
}
]
}
}
],
"project-title": [
{
"title": "string",
"language": "string"
}
],
"award-amount": {
"amount": 0,
"currency": "string",
"percentage": 0
},
"co-lead-investigator": [
{
"ORCID": "string",
"suffix": "string",
"given": "string",
"family": "string",
"affiliation": [
{
"id": [
{
"id": "string",
"id-type": "string",
"asserted-by": "string"
}
],
"name": "string"
}
],
"name": "string",
"role-start": {
"date-parts": [
[
"string"
]
]
},
"authenticated-orcid": true,
"prefix": "string",
"alternate-name": "string",
"sequence": "string",
"role-end": {
"date-parts": [
[
"string"
]
]
}
}
],
"project-description": [
{
"description": "string",
"language": "string"
}
]
}
],
"original-title": [
"string"
],
"status": {
"type": "string",
"update": {
"date-parts": [
[
"string"
]
]
},
"status-description": [
{
"language": "string",
"description": "string"
}
]
},
"language": "string",
"link": [
{
"URL": "string",
"content-type": "string",
"content-version": "string",
"intended-application": "string"
}
],
"deposited": {
"date-parts": [
[
0
]
],
"date-time": "2026-05-02T08:14:45.150Z",
"timestamp": 0
},
"score": 0,
"degree": [
"string"
],
"resource": {
"primary": {
"URL": "string"
},
"secondary": [
{
"URL": "string",
"label": "string"
}
]
},
"subtitle": [
"string"
],
"translator": [
{
"ORCID": "string",
"suffix": "string",
"given": "string",
"family": "string",
"affiliation": [
{
"name": "string",
"place": [
"string"
],
"department": [
"string"
],
"acronym": [
"string"
],
"id": [
{
"id": "string",
"id-type": "string",
"asserted-by": "string"
}
]
}
],
"name": "string",
"authenticated-orcid": true,
"prefix": "string",
"sequence": "string"
}
],
"free-to-read": {
"start-date": {
"date-parts": [
[
"string"
]
]
},
"end-date": {
"date-parts": [
[
"string"
]
]
}
},
"editor": [
{
"ORCID": "string",
"suffix": "string",
"given": "string",
"family": "string",
"affiliation": [
{
"name": "string",
"place": [
"string"
],
"department": [
"string"
],
"acronym": [
"string"
],
"id": [
{
"id": "string",
"id-type": "string",
"asserted-by": "string"
}
]
}
],
"name": "string",
"authenticated-orcid": true,
"prefix": "string",
"sequence": "string"
}
],
"proceedings-subject": "string",
"component-number": "string",
"short-title": [
"string"
],
"issued": {
"date-parts": [
[
"string"
]
]
},
"ISBN": [
"string"
],
"references-count": 0,
"part-number": "string",
"aliases": [
"string"
],
"issue-title": [
"string"
],
"journal-issue": {
"issue": "string",
"published-online": {
"date-parts": [
[
"string"
]
]
},
"published-print": {
"date-parts": [
[
"string"
]
]
}
},
"alternative-id": [
"string"
],
"version": {
"version": "string",
"language": "string",
"version-description": [
{
"language": "string",
"description": "string"
}
]
},
"URL": "string",
"archive": [
"string"
],
"relation": {
"additionalProp1": [
{
"id-type": "string",
"id": "string",
"asserted-by": "string"
}
],
"additionalProp2": [
{
"id-type": "string",
"id": "string",
"asserted-by": "string"
}
],
"additionalProp3": [
{
"id-type": "string",
"id": "string",
"asserted-by": "string"
}
]
},
"ISSN": [
"string"
],
"issn-type": [
{
"type": "string",
"value": "string"
}
],
"subject": [
"string"
],
"published-other": {
"date-parts": [
[
"string"
]
]
},
"published": {
"date-parts": [
[
"string"
]
]
},
"assertion": [
{
"group": {
"name": "string",
"label": "string"
},
"explanation": {
"URL": "string"
},
"name": "string",
"value": "string",
"URL": "string",
"order": 0,
"label": "string"
}
],
"subtype": "string",
"article-number": "string"
}
]
}
}
