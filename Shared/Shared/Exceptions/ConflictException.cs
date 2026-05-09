using System.Net;

namespace Shared.Exceptions
{
    public class ConflictException : BaseDomainException
    {
        public ConflictException(string message, string errorCode = "CONFLICT")
            : base(message, HttpStatusCode.Conflict, errorCode)
        {
        }
    }
}
