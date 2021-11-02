using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using WebApplication.Models;
using WebApplication.Services.Interfaces;

namespace WebApplication.Services
{
    public static class UserService
    {
        private static string currentToken;

        public static void Authenticate(IConfiguration configuration, ITokenService tokenService, HttpContext context,
            User dbUser)
        {
            currentToken = tokenService.BuildToken(configuration["JWT:Key"],
                configuration["JWT:Issuer"], dbUser);

            if (currentToken != null)
            {
                context.Session.SetString("Token", currentToken);
            }
        }

        public static void RemoveToken(HttpContext context)
        {
            if (context != null)
            {
                context.Session.Remove("Token");
            }
        }

        public static string GetName(ITokenService tokenService)
        {
            return tokenService.DecodeToken(currentToken)?.Claims.FirstOrDefault(cl => cl is {Type: ClaimTypes.Name})
                ?.Value;
        }

        public static string GetRole(ITokenService tokenService)
        {
            return tokenService.DecodeToken(currentToken).Claims.FirstOrDefault(cl => cl is {Type: ClaimTypes.Role})
                ?.Value;
        }

        public static string GetPhone(ITokenService tokenService)
        {
            return tokenService.DecodeToken(currentToken).Claims
                .FirstOrDefault(cl => cl is {Type: ClaimTypes.HomePhone})
                ?.Value;
        }

        public static string GetEmail(ITokenService tokenService)
        {
            return tokenService.DecodeToken(currentToken).Claims.FirstOrDefault(cl => cl is {Type: ClaimTypes.Email})
                ?.Value;
        }

        public static string GetId(ITokenService tokenService)
        {
            return tokenService.DecodeToken(currentToken).Claims
                .FirstOrDefault(cl => cl is {Type: ClaimTypes.NameIdentifier})
                ?.Value;
        }
    }
}