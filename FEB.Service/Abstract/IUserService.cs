using FEB.Infrastructure.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEB.Service.Abstract
{

    public interface IUserService
    {
        Task<UserDto?> GetUserAsync(string username);
        Task<bool> CheckPassword(string username, string password);
        Task AddUser(Service.Dto.SignUpRequest signUpRequest);
    }
}
