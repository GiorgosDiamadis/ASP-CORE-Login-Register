using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using WebApplication.Services;
using WebApplication.Services.Interfaces;

namespace WebApplication.Filters
{
    public class UserAuthorizationFilter : Attribute, IAuthorizationFilter
    {
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        public UserAuthorizationFilter(IConfiguration configuration, ITokenService tokenService)
        {
            _configuration = configuration;
            _tokenService = tokenService;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            string token = context.HttpContext.Request.Cookies["jwtToken"];
            if (string.IsNullOrEmpty(token))
            {
                RedirectToLoginPage(context);
                return;
            }

            if (!_tokenService.ValidateToken(_configuration["JWT:Key"], _configuration["JWT:Issuer"],
                _configuration["JWT:Audience"], Encryptor.Decrypt(token)))
            {
                RedirectToLoginPage(context);
            }
        }

        private static void RedirectToLoginPage(AuthorizationFilterContext context)
        {
            if (context.HttpContext.Request.Path != "/login" &&
                context.HttpContext.Request.Path != "/register")
            {
                context.HttpContext.Response.Redirect("/login");
            }
        }
    }
}