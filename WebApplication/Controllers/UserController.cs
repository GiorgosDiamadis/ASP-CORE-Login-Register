using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Core.Flash;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;
using WebApplication.Database;
using WebApplication.Database.DatabaseAccessObjects;
using WebApplication.Filters;
using WebApplication.Models;
using WebApplication.Models.DataTransferObjects;
using WebApplication.Models.Interfaces;
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
        private readonly IMailer _mailer;

        public UserController(WebApplicationContext context, IConfiguration config, ITokenService tokenService,
            MySqlContext mySqlContext, IFlasher flasher, IMailer mailer)
        {
            _context = context;
            _config = config;
            _tokenService = tokenService;
            _mySqlContext = mySqlContext;
            _flasher = flasher;
            _mailer = mailer;
        }


        [HttpPost]
        [Route("/logout")]
        public async Task<IActionResult> LogOut()
        {
            UserService.RemoveToken(HttpContext);
            return RedirectToAction("LogIn");
        }


        [Route("/forgotPassword")]
        public async Task<IActionResult> ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [Route("/forgotPassword")]
        public async Task<IActionResult> ForgotPassword([Bind("Name")] ForgotPasswordData forgotPasswordData)
        {
            UserDbAccess userDbAccess = new UserDbAccess(_mySqlContext);
            Messenger userResult = await userDbAccess.Get(name: forgotPasswordData.Name);
            if (userResult.IsError)
            {
                _flasher.Flash(Types.Danger, "Something went wrong during the password recovery process!", true);
                return RedirectToAction("LogIn");
            }

            User dbUser = userResult.GetData<User>();

            string passwordRecoverytoken = Guid.NewGuid().ToString();

            PasswordRecoveryDbAccess passwordRecoveryDbAccess = new PasswordRecoveryDbAccess(_mySqlContext);
            Dictionary<string, object> args = new Dictionary<string, object>();
            args["token"] = passwordRecoverytoken;
            args["user_name"] = forgotPasswordData.Name;
            Messenger result = await passwordRecoveryDbAccess.Insert(args);

            if (result.IsError)
            {
                _flasher.Flash(Types.Danger, result.Message);
                return RedirectToAction("LogIn");
            }


            _mailer.ForgotPassWordEmail(dbUser.Email, passwordRecoverytoken, dbUser.Name);
            _flasher.Flash(Types.Success, result.Message);
            return RedirectToAction("LogIn");
        }

        [Route("/passwordRecovery")]
        public async Task<IActionResult> PasswordRecovery([FromQuery] string token, [FromQuery] string userName)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userName))
            {
                _flasher.Flash(Types.Danger, "You are not authorized for this action!");
                return RedirectToAction("LogIn");
            }


            PasswordRecoveryDbAccess passwordRecoveryDbAccess = new PasswordRecoveryDbAccess(_mySqlContext);
            Messenger result = await passwordRecoveryDbAccess.Get(id: token);
            if (result.IsError)
            {
                _flasher.Flash(Types.Danger, result.Message);
                return RedirectToAction("LogIn");
            }

            PasswordRecoveryEntry passwordRecoveryEntry = result.GetData<PasswordRecoveryEntry>();

            if (DateTime.Now >= DateTime.Parse(passwordRecoveryEntry.ExpirationDate))
            {
                await passwordRecoveryDbAccess.Remove(passwordRecoveryEntry.Username);
                _flasher.Flash(Types.Danger, "Token has expired!");
                return RedirectToAction("LogIn");
            }

            UserDbAccess userDbAccess = new UserDbAccess(_mySqlContext);
            result = await userDbAccess.Get(name: passwordRecoveryEntry.Username);

            if (result.IsError)
            {
                _flasher.Flash(Types.Danger, "Something went wrong during password recovery process!");
                return RedirectToAction("LogIn");
            }

            ViewBag.username = userName;
            return View();
        }

        [HttpPost]
        [Route("/passwordRecovery")]
        public async Task<IActionResult> PasswordRecovery(
            [Bind("Password,ConfirmPassword,Username")]
            NewPasswordData newPasswordData)
        {
            if (newPasswordData.Password != newPasswordData.ConfirmPassword)
            {
                _flasher.Flash(Types.Danger, "Passwords don't match!");
                return RedirectToAction("PasswordRecovery");
            }

            HttpContext.Session.Remove("Recover");

            string[] hashSalt = HashPassword(newPasswordData.Password);

            UserDbAccess userDbAccess = new UserDbAccess(_mySqlContext);

            Dictionary<string, KeyValuePair<object, Type>> edit = new Dictionary<string, KeyValuePair<object, Type>>();
            Dictionary<string, KeyValuePair<object, Type>> where = new Dictionary<string, KeyValuePair<object, Type>>();

            edit["user_hash"] = new KeyValuePair<object, Type>(new string(hashSalt[0]), typeof(string));
            edit["user_salt"] = new KeyValuePair<object, Type>(new string(hashSalt[1]), typeof(string));

            where["user_name"] = new KeyValuePair<object, Type>(new string(newPasswordData.Username), typeof(string));

            Messenger result = await userDbAccess.Edit(edit, where);

            if (result.IsError)
            {
                _flasher.Flash(Types.Danger, "Something went wrong during password recovery process!");
            }
            else
            {
                PasswordRecoveryDbAccess passwordRecoveryDbAccess = new PasswordRecoveryDbAccess(_mySqlContext);
                await passwordRecoveryDbAccess.Remove(newPasswordData.Username);
                _flasher.Flash(Types.Success, "Your password has been changed successfully!");
            }

            return RedirectToAction("LogIn");
        }

        [Route("/login")]
        public async Task<IActionResult> LogIn()
        {
            return View();
        }

        [HttpPost]
        [Route("/login")]
        public async Task<IActionResult> LogIn([Bind("Name,Password")] UserLoginData userLoginData)
        {
            UserDbAccess userDbAccess = new UserDbAccess(_mySqlContext);

            Messenger result = await userDbAccess.Get(name: userLoginData.Name);

            Console.WriteLine(result.Message);

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


        [Route("/register")]
        public async Task<IActionResult> Register()
        {
            return View();
        }


        [Route("/confirmEmail")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
        {
            Console.WriteLine(token);
            UserDbAccess userDbAccess = new UserDbAccess(_mySqlContext);
            Messenger messenger = await userDbAccess.ConfirmEmail(token);
            Console.WriteLine(token);

            if (messenger.IsError)
                _flasher.Flash(Types.Danger, messenger.Message, true);
            else
                _flasher.Flash(Types.Success, messenger.Message, true);
            return RedirectToAction("LogIn");
        }

        [HttpPost]
        [Route("/register")]
        public async Task<IActionResult> Register(
            [Bind("PhoneNumber,Email,Role,Name,Password")]
            UserRegisterData userRegisterData)
        {
            UserDbAccess userDbAccess = new UserDbAccess(_mySqlContext);
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters["name"] = userRegisterData.Name;
            parameters["email"] = userRegisterData.Email;
            parameters["phone"] = userRegisterData.PhoneNumber;
            parameters["role"] = userRegisterData.Role;

            string[] hashSalt = HashPassword(userRegisterData.Password);

            parameters["hash"] = hashSalt[0];
            parameters["salt"] = hashSalt[1];

            Messenger result = await userDbAccess.Insert(parameters);

            if (result.IsError)
            {
                _flasher.Flash(Types.Danger, result.Message);
                return RedirectToAction("Register");
            }
            else
            {
                User newUser = result.GetData<User>();


                _mailer.ConfirmEmail(newUser.Email, newUser.ConfirmationToken);

                _flasher.Flash(Types.Success, result.Message);
                return RedirectToAction("LogIn");
            }
        }


        [HttpPost, ActionName("Delete")]
        [ServiceFilter(typeof(UserAuthorizationFilter))]
        public async void DeleteConfirmed([Bind("Id")] User user)
        {
            Response.Redirect(Request.Headers["Referer"].ToString());
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
    }
}