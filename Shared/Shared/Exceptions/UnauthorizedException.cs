using System.Net;

namespace Shared.Exceptions
{
    public class UnauthorizedException : BaseDomainException
    {
        public UnauthorizedException(string message = "Không có quyền truy cập")
            : base(message, HttpStatusCode.Unauthorized, "UNAUTHORIZED") { }
    }
}
