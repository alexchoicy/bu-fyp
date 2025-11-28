using Backend.Dtos.Auth;
using Backend.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            (AuthResponseDto? authResponse, string? token) = await _authService.Login(loginRequest);

            if (authResponse == null || token == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            Response.Cookies.Append("fypToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return Ok(authResponse);
        }
    }
}