using System.Net;

namespace Shared.Exceptions
{
    public class BadRequestException : BaseDomainException
    {
        public BadRequestException(string message = "Yêu cầu không hợp lệ")
            : base(message, HttpStatusCode.BadRequest, "BAD_REQUEST") { }
    }
}
