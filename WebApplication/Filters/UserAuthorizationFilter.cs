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
            string token = context.HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
            {
                RedirectToLoginPage(context);
                return;
            }

            if (!_tokenService.ValidateToken(_configuration["JWT:Key"], _configuration["JWT:Issuer"],
                _configuration["JWT:Audience"], token))
            {
                RedirectToLoginPage(context);
                return;
            }

            string id = context.HttpContext.Request.Query["id"];
            if (!string.IsNullOrEmpty(id))
            {
                if (id != UserService.GetId(token, _tokenService))
                {
                    // Diplay page for not authorized
                    Console.WriteLine("Not authorized");
                }
            }

            // Do the same
            Console.WriteLine(context.ModelState["Id"]);
        }

        private static void RedirectToLoginPage(AuthorizationFilterContext context)
        {
            if (context.HttpContext.Request.Path != "/User/Login" &&
                context.HttpContext.Request.Path != "/User/Register")
            {
                context.HttpContext.Response.Redirect("/User/Login");
            }
        }
    }
}