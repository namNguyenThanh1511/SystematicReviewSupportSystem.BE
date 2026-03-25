namespace SRSS.IAM.Services.GrobidClient;

using System;

public class GrobidException : Exception
{
    public GrobidException(string message) : base(message) { }
    public GrobidException(string message, Exception innerException) : base(message, innerException) { }
}

public class GrobidTimeoutException : GrobidException
{
    public GrobidTimeoutException(string message) : base(message) { }
}

public class GrobidInvalidPdfException : GrobidException
{
    public GrobidInvalidPdfException(string message) : base(message) { }
}