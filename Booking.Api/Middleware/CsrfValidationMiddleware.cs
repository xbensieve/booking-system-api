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
            var path = context.Request.Path.Value?.ToLower();

            if (HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method) || MiddlewareHelper.IsPublicRoute(context))
            {
                await _next(context);
                return;
            }

            var csrfCookie = context.Request.Cookies["X-CSRF-TOKEN"];
            var expiresCookie = context.Request.Cookies["X-CSRF-EXPIRES"];

            if (string.IsNullOrEmpty(csrfCookie))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("CSRF token mismatch");
                return;
            }

            if (string.IsNullOrEmpty(expiresCookie) || !DateTime.TryParse(expiresCookie, out var expires) || expires < DateTime.UtcNow)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("CSRF token expired");
                return;
            }

            await _next(context);

        }
    }
}
