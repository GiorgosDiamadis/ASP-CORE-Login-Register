using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Flash;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IConfiguration _config;
        private readonly ITokenService _tokenService;
        private readonly MySqlContext _mySqlContext;
        private readonly IFlasher _flasher;
        private readonly IMailer _mailer;

        public UserController(IConfiguration config, ITokenService tokenService,
            MySqlContext mySqlContext, IFlasher flasher, IMailer mailer)
        {
            _config = config;
            _tokenService = tokenService;
            _mySqlContext = mySqlContext;
            _flasher = flasher;
            _mailer = mailer;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
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
            args["token"] = Encryptor.Hash(passwordRecoverytoken);
            args["user_name"] = forgotPasswordData.Name;
            Messenger result = await passwordRecoveryDbAccess.Insert(args);

            if (result.IsError)
            {
                _flasher.Flash(Types.Danger, result.Message);
                return RedirectToAction("LogIn");
            }


            string tokenEncrypted = Encryptor.Encrypt(passwordRecoverytoken);
            string nameEcrypted = Encryptor.Encrypt(dbUser.Name);
            HttpContext.Session.SetString("Recover", nameEcrypted);
            _mailer.ForgotPassWordEmail(dbUser.Email, tokenEncrypted);
            _flasher.Flash(Types.Success, result.Message);
            return RedirectToAction("LogIn");
        }

        [Route("/passwordRecovery")]
        public async Task<IActionResult> PasswordRecovery([FromQuery] string token)
        {
            string userName = HttpContext.Session.GetString("Recover");
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userName))
            {
                _flasher.Flash(Types.Danger, "Something went wrong during the password recovery process!");
                return RedirectToAction("LogIn");
            }

            token = Encryptor.Decrypt(token);
            string userNameDecrypt = Encryptor.Decrypt(userName);

            PasswordRecoveryDbAccess passwordRecoveryDbAccess = new PasswordRecoveryDbAccess(_mySqlContext);
            Messenger result = await passwordRecoveryDbAccess.Get(id: Encryptor.Hash(token));
            if (result.IsError)
            {
                _flasher.Flash(Types.Danger, result.Message);
                return RedirectToAction("LogIn");
            }

            PasswordRecoveryEntry passwordRecoveryEntry = result.GetData<PasswordRecoveryEntry>();

            if (DateTime.Now >= DateTime.Parse(passwordRecoveryEntry.ExpirationDate))
            {
                await passwordRecoveryDbAccess.Remove(userNameDecrypt);
                _flasher.Flash(Types.Danger, "Token has expired!");
                return RedirectToAction("LogIn");
            }

            UserDbAccess userDbAccess = new UserDbAccess(_mySqlContext);
            result = await userDbAccess.Get(name: userNameDecrypt);

            if (result.IsError)
            {
                _flasher.Flash(Types.Danger, "Something went wrong during password recovery process!");
                return RedirectToAction("LogIn");
            }

            User dbUser = result.GetData<User>();

            if (passwordRecoveryEntry.Username == dbUser.Name)
            {
                _flasher.Flash(Types.Danger, "Something went wrong during password recovery process!");
                return RedirectToAction("LogIn");
            }

            ViewBag.username = userName;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/passwordRecovery")]
        public async Task<IActionResult> PasswordRecovery(
            [Bind("Password,ConfirmPassword,Username,Key")]
            NewPasswordData newPasswordData)
        {
            if (newPasswordData.Password != newPasswordData.ConfirmPassword)
            {
                _flasher.Flash(Types.Danger, "Passwords don't match!");
                return RedirectToAction("PasswordRecovery");
            }

            newPasswordData.Username = Encryptor.Decrypt(newPasswordData.Username);

            UserDbAccess userDbAccess = new UserDbAccess(_mySqlContext);
            PasswordRecoveryDbAccess passwordRecoveryDbAccess = new PasswordRecoveryDbAccess(_mySqlContext);
            await passwordRecoveryDbAccess.Remove(newPasswordData.Username);
            HttpContext.Session.Remove("Recover");

            // Retrieve user, check is key is correct
            Messenger messenger = await userDbAccess.Get(name: newPasswordData.Username);
            if (messenger.IsError)
            {
                _flasher.Flash(Types.Danger,
                    "Something went wrong during password recovery process! Request a new recovery link!");
                return RedirectToAction("LogIn");
            }

            User dbUser = messenger.GetData<User>();

            if (dbUser.EncryptionKey != Encryptor.Hash(newPasswordData.Key))
            {
                _flasher.Flash(Types.Danger, "Security key is invalid! Request a new recovery link!");
                return RedirectToAction("LogIn");
            }

            string[] hashSalt = Encryptor.HashPassword(newPasswordData.Password);
            Console.WriteLine(newPasswordData.Password);
            Console.WriteLine(hashSalt[0] + " " + hashSalt[1]);


            Dictionary<string, KeyValuePair<object, Type>> edit = new Dictionary<string, KeyValuePair<object, Type>>();
            Dictionary<string, KeyValuePair<object, Type>> where = new Dictionary<string, KeyValuePair<object, Type>>();

            edit["user_hash"] = new KeyValuePair<object, Type>(new string(hashSalt[0]), typeof(string));
            edit["user_salt"] = new KeyValuePair<object, Type>(new string(hashSalt[1]), typeof(string));

            where["user_name"] = new KeyValuePair<object, Type>(new string(newPasswordData.Username), typeof(string));

            Messenger result = await userDbAccess.Edit(edit, where);

            if (result.IsError)
            {
                _flasher.Flash(Types.Danger,
                    "Something went wrong during password recovery process! Request a new recovery link!");
            }
            else
            {
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
        [ValidateAntiForgeryToken]
        [Route("/login")]
        public async Task<IActionResult> LogIn([Bind("Name,Password")] UserLoginData userLoginData)
        {
            UserDbAccess userDbAccess = new UserDbAccess(_mySqlContext);

            Messenger result = await userDbAccess.Get(name: userLoginData.Name);

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

                if (dbUser.HasValidated == 0)
                {
                    _flasher.Flash(Types.Danger, $"Your email is not confirmed!", true);
                    return RedirectToAction("LogIn");
                }

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
            string tokenDecrypt = Encryptor.Decrypt(token);
            UserDbAccess userDbAccess = new UserDbAccess(_mySqlContext);
            Messenger messenger = await userDbAccess.ConfirmEmail(tokenDecrypt);


            if (messenger.IsError)
                _flasher.Flash(Types.Danger, messenger.Message, true);
            else
                _flasher.Flash(Types.Success, messenger.Message, true);
            return RedirectToAction("LogIn");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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

            parameters["key"] = Encryptor.GenerateRandomKey(16);


            string[] hashSalt = Encryptor.HashPassword(userRegisterData.Password);

            parameters["hash"] = hashSalt[0];
            parameters["salt"] = hashSalt[1];

            Messenger result = await userDbAccess.Insert(parameters);

            if (result.IsError)
            {
                _flasher.Flash(Types.Danger, result.Message);
                return RedirectToAction("Register");
            }

            User newUser = result.GetData<User>();


            _mailer.ConfirmEmail(newUser.Email, Encryptor.Encrypt(newUser.ConfirmationToken));

            _flasher.Flash(Types.Success, result.Message);
            return RedirectToAction("LogIn");
        }

        [HttpPost, ActionName("Delete")]
        [ServiceFilter(typeof(UserAuthorizationFilter))]
        public async void DeleteConfirmed([Bind("Id")] User user)
        {
            Response.Redirect(Request.Headers["Referer"].ToString());
        }


        private bool PasswordIsValid(string enteredPassword, string storedSalt, string storedHash)
        {
            return storedHash == Encryptor.HashPassword(enteredPassword, storedSalt)[0];
        }
    }
}