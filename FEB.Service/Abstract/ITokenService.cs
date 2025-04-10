using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FEB.Service.Abstract
{
    public interface ITokenService
    {
        (string token, DateTime expiration) GenerateToken(string username, IEnumerable<Claim>? additionalClaims = null);
    }
}
