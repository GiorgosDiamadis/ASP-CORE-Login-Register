using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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

            Messenger result = await userDao.Search(name: userLoginData.Name);
            

            // If there is no record with this name
            if (result.IsError)
            {
                _flasher.Flash(Types.Danger, result.Message, true);
                return View();
            }

            // If there is a record with this name check for valid password
            if (result.GetData<User>() != null)
            {
                User dbUser = result.GetData<User>();
                if (PasswordIsValid(userLoginData.Password, dbUser.Salt, dbUser.Hash))
                {
                    UserService.Authenticate(_config, _tokenService, HttpContext, dbUser);
                    _flasher.Flash(Types.Success, $"Welcome {dbUser.Name}", true);
                    return RedirectToAction("Index", "Home");
                }
            }
            
            // If password is incorrect!
            _flasher.Flash(Types.Danger, "Password or username is incorrect!", true);
            return View();
        }

        private bool PasswordIsValid(string enteredPassword, string storedSalt, string storedHash)
        {
            return storedHash == HashPassword(enteredPassword, storedSalt)[0];
        }

        private static string[] HashPassword(string password, string salt = null)
        {
            string[] hashSalt = new string[2];

            string passwordSalt = "";

            RNGCryptoServiceProvider rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            if (salt == null)
            {
                byte[] buff = new byte[12];
                rngCryptoServiceProvider.GetBytes(buff);
                passwordSalt = Convert.ToBase64String(buff);
                hashSalt[1] = passwordSalt;
            }
            else
            {
                passwordSalt = salt;
            }


            string passwordWithSalt = password + passwordSalt;

            HashAlgorithm hashAlgorithm = new SHA256CryptoServiceProvider();
            byte[] bhash = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(passwordWithSalt));

            string passwordHashed = Convert.ToBase64String(bhash);
            hashSalt[0] = passwordHashed;
            return hashSalt;
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
            [Bind("PhoneNumber,Email,Role,Name,Password")]
            UserRegisterDto userRegisterDto)
        {
            UserDao userDao = new UserDao(_mySqlContext);
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters["name"] = userRegisterDto.Name;
            parameters["email"] = userRegisterDto.Email;
            parameters["phone"] = userRegisterDto.PhoneNumber;
            parameters["role"] = userRegisterDto.Role;

            string[] hashSalt = HashPassword(userRegisterDto.Password);

            parameters["hash"] = hashSalt[0];
            parameters["salt"] = hashSalt[1];
            
            Messenger result = await userDao.Register(parameters);

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