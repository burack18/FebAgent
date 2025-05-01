using FEB.API.Dto;
using FEB.Service.Abstract;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
            if (user == null)
            {
                return Unauthorized("Invalid username or password.");
            }

            bool isCorrectPassword = await _userService.CheckPassword(loginRequest.Username, loginRequest.Password);

            if (!isCorrectPassword)
            {
                return Unauthorized("Invalid username or password.");
            }

            List<Claim> claims = [
        new Claim("UserID", user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Name, user.UserName)
        ];
            var (tokenString, expiration) = _tokenService.GenerateToken(user.UserName,claims);

            return Ok(new LoginResponse
            {
                Token = tokenString,
                Expiration = expiration,
                Username = user.UserName,
                UserID = user.Id,
            });
        }


        [HttpPost("signup")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest signUpRequest)
        {
            throw new Exception("Not In Use");

            var user = await _userService.GetUserAsync(signUpRequest.UserName);
            if (user != null)
            {
                return Unauthorized("Username is already in use");
            }

            await _userService.AddUser(new Service.Dto.SignUpRequest()
            {
                UserName = signUpRequest.UserName,
                Password = signUpRequest.Password,
                Email = signUpRequest.Email,
                FirstName = signUpRequest.FirstName,
                LastName = signUpRequest.LastName,
                UserID = signUpRequest.UserID,
            });


            return Ok();
        }
    }
}
