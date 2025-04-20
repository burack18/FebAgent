using FEB.Infrastructure.Dto;
using FEB.Infrastructure.Repositories.Abstract;
using FEB.Service.Abstract;
using FEBAgent.Domain;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FEB.Service.Concrete
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(IUserRepository userRepository, IHttpContextAccessor httpContextAccessor = null)
        {
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task AddUser(Service.Dto.SignUpRequest signUpRequest)
        {
            await _userRepository.AddUserAsync(new User
            {
                UserName = signUpRequest.UserName,
                Password = signUpRequest.Password,
                Email = signUpRequest.Email,
                FirstName = signUpRequest.FirstName,
                LastName = signUpRequest.LastName,
                UserID = signUpRequest.UserID,
                Id = Guid.NewGuid().ToString(),
                CreatedOn = DateTime.UtcNow,
            });
        }

        public async Task<bool> CheckPassword(string username, string password)
        {
            return await _userRepository.CheckPassword(username, password);
        }

        public async Task<UserDto?> GetCurrentUser()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var userName = user?.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(userName)) return null;
            return await this.GetUserAsync(userName);
        }

        public async Task<UserDto?> GetUserAsync(string username)
        {
            return await _userRepository.GetUserAsync(username);
        }

    }
}
