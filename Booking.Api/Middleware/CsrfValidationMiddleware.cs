namespace Booking.Api.Middleware
{
    public class CsrfValidationMiddleware
    {
        private readonly RequestDelegate _next;
        public CsrfValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var method = context.Request.Method;
            var path = context.Request.Path.Value?.ToLower();

            if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method) ||
            (path != null && path.StartsWith("/api/auth")))
            {
                await _next(context);
                return;
            }

            var csrfCookie = context.Request.Cookies["X-CSRF-TOKEN"];
            var csrfHeader = context.Request.Headers["X-CSRF-TOKEN"].FirstOrDefault();

            if (string.IsNullOrEmpty(csrfCookie) || csrfCookie != csrfHeader)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("CSRF token mismatch");
                return;
            }

            await _next(context);

        }
    }
}
