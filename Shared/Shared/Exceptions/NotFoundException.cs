using System.Net;

namespace Shared.Exceptions
{
    public class NotFoundException : BaseDomainException
    {
        public NotFoundException(string message = "Không tìm thấy dữ liệu")
            : base(message, HttpStatusCode.NotFound, "NOT_FOUND") { }
    }
}
