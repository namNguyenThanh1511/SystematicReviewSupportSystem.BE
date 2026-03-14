using System.Net;

namespace Shared.Exceptions
{
    /// <summary>
    /// Base class for domain-level exceptions with HTTP status mapping.
    /// </summary>
    public abstract class BaseDomainException : Exception
    {

        public HttpStatusCode StatusCode { get; }
        public string ErrorCode { get; set; } = string.Empty;

        protected BaseDomainException(string message, HttpStatusCode statusCode, string errorCode)
            : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }
    }
}
