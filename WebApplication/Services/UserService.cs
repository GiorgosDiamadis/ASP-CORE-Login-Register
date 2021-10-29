using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using WebApplication.Filters;
using WebApplication.Models;
using WebApplication.Services.Interfaces;

namespace WebApplication.Services
{
    public static class UserService
    {
        public static void Authenticate(IConfiguration configuration, ITokenService tokenService,HttpContext context, User dbUser)
        {
            string token = tokenService.BuildToken(configuration["JWT:Key"],
                configuration["JWT:Issuer"], dbUser);

            if (token != null)
            {
                context.Session.SetString("Token", token);
            }
        }
        public static string GetName(string token, ITokenService tokenService)
        {
            return tokenService.DecodeToken(token).Claims.FirstOrDefault(cl => cl is {Type: ClaimTypes.Name})?.Value;
        }

        public static string GetRole(string token, ITokenService tokenService)
        {
            return tokenService.DecodeToken(token).Claims.FirstOrDefault(cl => cl is {Type: ClaimTypes.Role})?.Value;
        }

        public static string GetId(string token, ITokenService tokenService)
        {
            return tokenService.DecodeToken(token).Claims.FirstOrDefault(cl => cl is {Type: ClaimTypes.NameIdentifier})
                ?.Value;
        }
    }
}