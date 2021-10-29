using System.IdentityModel.Tokens.Jwt;
using WebApplication.Models;

namespace WebApplication.Services.Interfaces
{
    public interface ITokenService
    {
        string BuildToken(string key, string issuer, User user);
        bool ValidateToken(string key, string issuer, string audience, string token);
        JwtSecurityToken DecodeToken(string token);
    }
}