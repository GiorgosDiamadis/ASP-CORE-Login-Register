using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebApplication.Database;
using WebApplication.Database.DatabaseAccessObjects;
using WebApplication.Filters;
using WebApplication.Models;
using WebApplication.Models.DataTransferObjects;
using WebApplication.Services;
using WebApplication.Services.Interfaces;

namespace WebApplication.Controllers
{
    public class UserController : Controller
    {
        private readonly WebApplicationContext _context;
        private readonly IConfiguration _config;
        private readonly ITokenService _tokenService;
        private readonly MySqlContext _mySqlContext;

        public UserController(WebApplicationContext context, IConfiguration config, ITokenService tokenService,
            MySqlContext mySqlContext)
        {
            _context = context;
            _config = config;
            _tokenService = tokenService;
            _mySqlContext = mySqlContext;
        }


        public async Task<IActionResult> LogIn()
        {

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LogIn([Bind("Name,Password")] UserLoginDTO userLoginDto)
        {
            UserDao userDao = new UserDao(_mySqlContext);
            User dbUser = await userDao.LogIn(userLoginDto.Name, userLoginDto.Password);
            if (dbUser != null)
            {
                UserService.Authenticate(_config, _tokenService, HttpContext, dbUser);

                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View();
            }
        }


        // [Route("/malakas")]
        public async Task<IActionResult> Register()
        {
            return View();
        }


        [Route("/confirmEmail")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
        {
            UserDao userDao = new UserDao(_mySqlContext);
            bool hasConfirmed = await userDao.ConfirmEmail(token);


            return RedirectToAction("LogIn");
        }

        [HttpPost]
        public async Task<IActionResult> Register([Bind("PhoneNumber,Email,Role,Name,Password")] User user)
        {
            UserDao userDao = new UserDao(_mySqlContext);
            bool hasInserted = await userDao.Register(user);

            if (hasInserted)
            {
                return RedirectToAction("LogIn");
            }
            else
            {
                return View();
            }
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(UserAuthorizationFilter))]
        public async void DeleteConfirmed([Bind("Id")] User user)
        {
            Response.Redirect(Request.Headers["Referer"].ToString());
        }

        private bool UserExists(string id)
        {
            return _context.User.Any(e => e.Id == id);
        }
    }
}