using System;
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
        private static string _currentToken;

        public static void Authenticate(IConfiguration configuration, ITokenService tokenService, HttpContext context,
            User dbUser)
        {
            _currentToken = tokenService.BuildToken(configuration["JWT:Key"],
                configuration["JWT:Issuer"], dbUser);

            if (_currentToken != null)
            {
                context.Response.Cookies.Append("jwtToken", Encryptor.Encrypt(_currentToken), new CookieOptions()
                {
                    HttpOnly = true, SameSite = SameSiteMode.Strict
                });
            }
        }

        public static void RemoveToken(HttpContext context)
        {
            if (context != null)
            {
                context.Response.Cookies.Delete("jwtToken");
            }
        }

        public static string GetName(ITokenService tokenService)
        {
            return tokenService.DecodeToken(_currentToken)?.Claims.FirstOrDefault(cl => cl is {Type: ClaimTypes.Name})
                ?.Value;
        }

        public static string GetRole(ITokenService tokenService)
        {
            return tokenService.DecodeToken(_currentToken).Claims.FirstOrDefault(cl => cl is {Type: ClaimTypes.Role})
                ?.Value;
        }

        public static string GetPhone(ITokenService tokenService)
        {
            return tokenService.DecodeToken(_currentToken).Claims
                .FirstOrDefault(cl => cl is {Type: ClaimTypes.HomePhone})
                ?.Value;
        }

        public static string GetEmail(ITokenService tokenService)
        {
            return tokenService.DecodeToken(_currentToken).Claims.FirstOrDefault(cl => cl is {Type: ClaimTypes.Email})
                ?.Value;
        }

        public static string GetId(ITokenService tokenService)
        {
            return tokenService.DecodeToken(_currentToken).Claims
                .FirstOrDefault(cl => cl is {Type: ClaimTypes.NameIdentifier})
                ?.Value;
        }
    }
}