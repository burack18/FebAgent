using FEB.Infrastructure.Dto;
using FEBAgent.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEB.Infrastructure.Repositories.Abstract
{
    public interface IUserRepository
    {
        Task<UserDto?> GetUserAsync(string username);
        Task<UserDto?> GetUserByIdAsync(string userId);
        Task<List<UserDto>> GetAllUsersAsync();
        Task AddUserAsync(User user);
        Task UpdateUserAsync(UserDto user);
        Task DeleteUserAsync(string userId);
        Task<bool> CheckPassword(string username, string password);
        Task<bool> Verify(string storedPassword, string password);
        Task<string> BycrptPassword(string password);
    }
}
