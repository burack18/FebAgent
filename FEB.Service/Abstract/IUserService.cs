using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEB.Service.Abstract
{
    public record User(string Username, string Password /* Store HASHED password in reality */);

    public interface IUserService
    {
        Task<User?> GetUserAsync(string username);
    }
}
