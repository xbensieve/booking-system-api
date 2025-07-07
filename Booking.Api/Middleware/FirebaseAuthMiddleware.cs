using FirebaseAdmin.Auth;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace Booking.Api.Middleware
{
    public class FirebaseAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;

        public FirebaseAuthMiddleware(RequestDelegate next, IMemoryCache cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();

            if (path != null && (
                path.StartsWith("/api/auth/login") ||
                path.StartsWith("/api/auth/register") ||
                path.StartsWith("/api/auth/refresh-token") ||
                path.StartsWith("/api/auth/logout")
            ))
            {
                await _next(context);
                return;
            }
            var idToken = context.Request.Cookies["idToken"];

            if (string.IsNullOrEmpty(idToken))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Missing idToken");
                return;
            }

            try
            {
                FirebaseToken? decodedToken;

                if (!_cache.TryGetValue(idToken, out decodedToken))
                {
                    decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
                    _cache.Set(idToken, decodedToken, TimeSpan.FromMinutes(55));
                }

                if (decodedToken == null)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Invalid Firebase token.");
                    return;
                }

                var uid = decodedToken.Uid ?? string.Empty;
                var email = decodedToken.Claims.TryGetValue("email", out var emailVal) ? emailVal?.ToString() : null;
                var name = decodedToken.Claims.TryGetValue("name", out var nameVal) ? nameVal?.ToString() : null;

                var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, uid),
                            new Claim(ClaimTypes.Email, email ?? ""),
                        };

                if (!string.IsNullOrEmpty(name))
                    claims.Add(new Claim("firebase:name", name));

                var identity = new ClaimsIdentity(claims, "Firebase");
                context.User = new ClaimsPrincipal(identity);
            }
            catch
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid Firebase token.");
                return;
            }

            await _next(context);
        }
    }
}
