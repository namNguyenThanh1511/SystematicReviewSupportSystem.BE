using Microsoft.AspNetCore.Http;

namespace Shared.Middlewares
{
    public class ListenToOnlyApiGateway
    {
        private readonly RequestDelegate _next;

        public ListenToOnlyApiGateway(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();
            if (path != null && path.Contains("swagger"))
            {
                await _next(context);
                return;
            }
            var signedHeader = context.Request.Headers["Api-Gateway"].FirstOrDefault();
            if (string.IsNullOrEmpty(signedHeader))
            {
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                await context.Response.WriteAsync("Sorry, service is not available");
                return;
            }

            await _next(context);
        }
    }
}
