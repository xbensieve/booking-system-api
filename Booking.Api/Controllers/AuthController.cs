using Booking.Application.DTOs.Auth;
using Booking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Booking.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        public AuthController(IAuthService authService, IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
        }
        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="model">Registration model containing email, password, phone, etc.</param>
        /// <returns>
        /// 201 Created with the created user location when userId is available, 
        /// or 200 OK with response payload; 400 Bad Request on validation/error.
        /// </returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _authService.RegisterAsync(model);

            if (!response.Success)
                return BadRequest(new { response.Message });

            try
            {
                var userObj = response.Data?.GetType().GetProperty("User")?.GetValue(response.Data);
                var idObj = userObj?.GetType().GetProperty("UserId")?.GetValue(userObj);

                if (idObj != null && Guid.TryParse(idObj.ToString(), out var userId))
                {
                    return CreatedAtAction(nameof(GetUser), new { userId }, response);
                }
            }
            catch
            {
                // ignore extraction errors, fall back to Ok
            }

            return Ok(response);
        }
        /// <summary>
        /// Gets a user by id.
        /// </summary>
        /// <param name="userId">The user id (GUID).</param>
        /// <returns>200 OK with user data or 404 NotFound.</returns>
        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> GetUser(Guid userId)
        {
            var response = await _authService.GetUserByIdAsync(userId);

            if (!response.Success)
                return NotFound(new { response.Message });

            return Ok(response);
        }
        /// <summary>
        /// Login user with email and password
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _authService.LoginAsync(model);

            if (!response.Success)
                return Unauthorized(new { response.Message });

            var accessToken = response.Data?.GetType().GetProperty("AccessToken")?.GetValue(response.Data)?.ToString();
            var refreshToken = response.Data?.GetType().GetProperty("RefreshToken")?.GetValue(response.Data)?.ToString();

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                return StatusCode(500, "Failed to generate tokens.");

            Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7"))
            });

            return Ok(new
            {
                Message = "Login successful",
                AccessToken = accessToken,
                User = response.Data?.GetType().GetProperty("User")?.GetValue(response.Data)
            });
        }
        /// <summary>
        /// Refreshes the access token when the current one is expired.  
        /// The refresh token is retrieved from an HttpOnly cookie.
        /// </summary>
        /// <param name="request">
        /// The request containing the expired access token.
        /// </param>
        /// <returns>
        /// Returns 200 OK with a new access token (and sets a new refresh token in an HttpOnly cookie),  
        /// or 401 Unauthorized if the refresh token is invalid or expired.
        /// </returns>
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var refreshTokenRaw = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshTokenRaw))
                return Unauthorized("Missing refresh token cookie.");

            var result = await _authService.RefreshTokenAsync(request.AccessToken, refreshTokenRaw);

            if (!result.Success)
                return Unauthorized(result.Message);

            var newRefreshToken = result.Data?.GetType().GetProperty("RefreshToken")?.GetValue(result.Data)?.ToString();
            if (!string.IsNullOrEmpty(newRefreshToken))
            {
                Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(7)
                });
            }

            return Ok(new
            {
                Message = result.Message,
                AccessToken = result.Data?.GetType().GetProperty("AccessToken")?.GetValue(result.Data),
                User = result.Data?.GetType().GetProperty("User")?.GetValue(result.Data)
            });
        }
        /// <summary>
        /// Logs out the current user by removing the refresh token from both
        /// the database and the HttpOnly cookie.
        /// </summary>
        /// <returns>
        /// Returns 200 OK if logout was successful, otherwise 400 BadRequest.
        /// </returns>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest("No refresh token found.");

            var result = await _authService.LogoutAsync(refreshToken);

            Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None
            });

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(new { message = "Logout successful" });
        }

        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpByEmailRequest request)
        {
            var result = await _authService.ResendOtpByEmailAsync(request.Email);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailByOtpRequest request)
        {
            var result = await _authService.VerifyEmailByOtpAsync(request.Email, request.OtpCode);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var result = await _authService.ForgotPasswordAsync(request.Email);
            return Ok(result);
        }
        [HttpPost("verify-reset-otp")]
        public async Task<IActionResult> VerifyResetOtp([FromBody] VerifyResetOtpRequest request)
        {
            var result = await _authService.VerifyResetOtpAsync(request.Email, request.OtpCode);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
        [Authorize]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idStr) || !Guid.TryParse(idStr, out var userId))
                return Unauthorized("Invalid user ID in token.");

            var result = await _authService.ResetPasswordAsync(userId, request.NewPassword);

            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
        public record RefreshTokenRequest(string AccessToken);
        public record ResendOtpByEmailRequest(string Email);
        public record VerifyEmailByOtpRequest(string Email, string OtpCode);
        public record ForgotPasswordRequest(string Email);
        public record VerifyResetOtpRequest(string Email, string OtpCode);
        public record ResetPasswordRequest(string NewPassword);
    }
}
