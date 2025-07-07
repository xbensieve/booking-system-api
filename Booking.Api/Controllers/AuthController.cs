using Booking.Service.Interfaces;
using Booking.Service.Models;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Mvc;
using TokenRequest = Booking.Service.Models.TokenRequest;

namespace Booking.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] TokenRequest request)
        {
            if (string.IsNullOrEmpty(request.IdToken)) return Unauthorized("Missing idToken");

            FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.IdToken);
            bool emailVerified = decodedToken.Claims.TryGetValue("email_verified", out var verifiedVal) && (bool)verifiedVal;

            if (!emailVerified)
                return Unauthorized("Email not verified.");

            string uid = decodedToken.Uid;

            var userResponse = await _authService.GetUserByIdAsync(uid);

            if (userResponse.Data == null)
            {
                var newUser = new RegisterModel
                {
                    Uid = uid,
                    Email = decodedToken.Claims.TryGetValue("email", out var emailVal) ? emailVal?.ToString() : null,
                    FullName = decodedToken.Claims.TryGetValue("name", out var nameVal) ? nameVal?.ToString() : null,
                    AvatarUrl = decodedToken.Claims.TryGetValue("picture", out var pictureVal) ? pictureVal?.ToString() : null,
                };

                var registerResponse = await _authService.RegisterAsync(newUser);

                if (!registerResponse.Success)
                {
                    return BadRequest(registerResponse.Message);
                }
            }

            var expires = DateTime.UtcNow.AddMinutes(55);

            Response.Cookies.Append("idToken", request.IdToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = expires
            });

            var csrfToken = Guid.NewGuid().ToString();

            Response.Cookies.Append("X-CSRF-TOKEN", csrfToken, new CookieOptions
            {
                HttpOnly = false,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = expires
            });

            return Ok(new { message = "Login success" });
        }


        [HttpPost("logout")]
        public IActionResult Logout()
        {
            var expired = DateTime.UnixEpoch;

            Response.Cookies.Append("idToken", "", new CookieOptions
            {
                Expires = expired,
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/"
            });

            Response.Cookies.Append("X-CSRF-TOKEN", "", new CookieOptions
            {
                Expires = expired,
                HttpOnly = false,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/"
            });

            return Ok(new { message = "Logged out successfully" });
        }
        [HttpGet("protected")]
        public async Task<IActionResult> ProtectedEndpoint()
        {
            var token = Request.Cookies["idToken"];

            try
            {
                FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);
                string uid = decodedToken.Uid;
                return Ok(new { Message = "Access granted", Uid = uid });
            }
            catch (FirebaseAuthException)
            {
                return Unauthorized("Invalid or expired token");
            }
        }
    }
}
