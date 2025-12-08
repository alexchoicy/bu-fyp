using Backend.Dtos.Auth;
using Backend.Models;
using Backend.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly UserManager<User> _userManager;

        public AuthController(IAuthService authService, UserManager<User> userManager)
        {
            _authService = authService;
            _userManager = userManager;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentUser()
        {
            User? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            IList<string> roles = await _userManager.GetRolesAsync(user);
            AuthResponseDto authResponse = new AuthResponseDto
            {
                ID = user.Id,
                Name = user.Name ?? "",
                Roles = roles
            };

            return Ok(authResponse);
        }
    }
}