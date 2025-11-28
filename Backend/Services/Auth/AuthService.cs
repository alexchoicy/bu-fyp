using Backend.Dtos.Auth;
using Backend.Models;
using Microsoft.AspNetCore.Identity;

namespace Backend.Services.Auth
{
    public interface IAuthService
    {
        public Task<(AuthResponseDto?, string?)> Login(LoginRequestDto loginRequest);
    }
    public class AuthService : IAuthService
    {
        private readonly ITokenService _tokenService;
        private readonly UserManager<User> _userManager;
        public AuthService(ITokenService tokenService, UserManager<User> userManager)
        {
            _tokenService = tokenService;
            _userManager = userManager;
        }

        public async Task<(AuthResponseDto?, string?)> Login(LoginRequestDto loginRequest)
        {
            User? user = await _userManager.FindByNameAsync(loginRequest.Username);
            if (user == null)
            {
                return (null, null);
            }

            bool isPasswordValid = await _userManager.CheckPasswordAsync(user, loginRequest.Password);
            if (!isPasswordValid)
            {
                return (null, null);
            }

            IList<string> roles = await _userManager.GetRolesAsync(user);

            string token = _tokenService.CreateToken(user, roles.ToList());

            AuthResponseDto authResponse = new AuthResponseDto
            {
                ID = user.Id,
                Name = user.Name ?? "",
                Roles = roles
            };

            return (authResponse, token);
        }
    }
}