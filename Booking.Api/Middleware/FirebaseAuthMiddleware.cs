using FirebaseAdmin.Auth;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace Booking.Api.Middleware
{
    public class FirebaseAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        public FirebaseAuthMiddleware(RequestDelegate next, IMemoryCache cache, IConfiguration configuration)
        {
            _next = next;
            _cache = cache;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (MiddlewareHelper.IsPublicRoute(context))
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
                var adminEmails = _configuration.GetSection("Admins").Get<List<string>>();
                var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, uid),
                            new Claim(ClaimTypes.Email, email ?? ""),
                        };
                if (adminEmails != null && adminEmails.Contains(email))
                {
                    claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                }
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
