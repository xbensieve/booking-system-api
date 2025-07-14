namespace Booking.Api.Middleware
{
    public static class MiddlewareHelper
    {
        public static bool IsPublicRoute(HttpContext context)
        {
            var method = context.Request.Method;
            var path = context.Request.Path.Value?.ToLower() ?? "";

            if (path == "/api/auth/login" || path == "/api/auth/logout")
                return true;

            if (HttpMethods.IsGet(method) && (
                path == "/api/hotels" ||
                path.StartsWith("/api/hotels/")
            ))
                return true;

            if (HttpMethods.IsGet(method) && (
                path.StartsWith("/api/rooms/")
            ))
                return true;

            if (HttpMethods.IsPost(method) && path == "/api/rooms/search")
                return true;

            return false;
        }
    }
}
