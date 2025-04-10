using FEB.Service.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEB.Service.Concrete
{
    public class InMemoryUserService : IUserService
    {
        // !!! WARNING: Hardcoded user and PLAIN TEXT password - FOR DEMO ONLY !!!
        // !!! Replace with database lookup and hashed password comparison !!!
        private readonly List<User> _users =
        [
            new("testuser", "password123")
        ];

        public Task<User?> GetUserAsync(string username)
        {
            var user = _users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(user);
        }
    }
}
