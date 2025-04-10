using FEB.API.Dto;
using FEB.Service.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace FEB.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;

        public AuthController(IUserService userService, ITokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            if (string.IsNullOrWhiteSpace(loginRequest.Username) || string.IsNullOrWhiteSpace(loginRequest.Password))
            {
                return Unauthorized("Username and password required.");
            }

            
            var user = await _userService.GetUserAsync(loginRequest.Username);

            if (user == null || user.Password != loginRequest.Password)
            {
                return Unauthorized("Invalid username or password.");
            }


            var (tokenString, expiration) = _tokenService.GenerateToken(user.Username);

            return Ok(new LoginResponse
            {
                Token = tokenString,
                Expiration = expiration,
                Username = user.Username
            });
        }
    }
}
