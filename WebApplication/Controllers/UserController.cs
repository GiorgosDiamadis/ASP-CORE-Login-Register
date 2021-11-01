using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Core.Flash;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RestSharp;
using RestSharp.Authenticators;
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
        private readonly IFlasher _flasher;

        public UserController(WebApplicationContext context, IConfiguration config, ITokenService tokenService,
            MySqlContext mySqlContext, IFlasher flasher)
        {
            _context = context;
            _config = config;
            _tokenService = tokenService;
            _mySqlContext = mySqlContext;
            _flasher = flasher;
        }


        public async Task<IActionResult> LogIn()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LogIn([Bind("Name,Password")] UserLoginData userLoginData)
        {
            UserDao userDao = new UserDao(_mySqlContext);
            Messenger message = await userDao.LogIn(userLoginData);
            if (message.IsError)
            {
                _flasher.Flash(Types.Danger, message.Message, true);
                return View();
            }
            else
            {
                User dbUser = message.GetData<User>();
                UserService.Authenticate(_config, _tokenService, HttpContext, dbUser);
                _flasher.Flash(Types.Success, message.Message, true);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Register()
        {
            return View();
        }


        [Route("/confirmEmail")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
        {
            UserDao userDao = new UserDao(_mySqlContext);
            Messenger messenger = await userDao.ConfirmEmail(token);

            if (messenger.IsError)
                _flasher.Flash(Types.Danger, messenger.Message, true);
            else
                _flasher.Flash(Types.Success, messenger.Message, true);
            return RedirectToAction("LogIn");
        }

        [HttpPost]
        public async Task<IActionResult> Register(
            [Bind("PhoneNumber,Email,Role,Name,Password")] UserRegisterDto userRegisterDto)
        {
            UserDao userDao = new UserDao(_mySqlContext);
            Messenger result = await userDao.Register(userRegisterDto);

            if (result.IsError)
            {
                _flasher.Flash(Types.Danger, result.Message);
                return RedirectToAction("Register");
            }
            else
            {
                _flasher.Flash(Types.Success, result.Message);
                return RedirectToAction("LogIn");
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