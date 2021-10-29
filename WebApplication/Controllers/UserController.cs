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
        public async Task<IActionResult> LogIn([Bind("Name,Password")] User user)
        {
            UserDao userDao = new UserDao(_mySqlContext);
            User dbUser = await userDao.LogIn(user.Name, user.Password);
            if (dbUser != null)
            {
                UserService.Authenticate(_config, _tokenService, HttpContext, dbUser);
            }

            return RedirectToAction("Index","Home");
            // Response.Redirect(Request.Headers["Referer"].ToString());
        }


        public async Task<IActionResult> Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register([Bind("Role,Name,Password")] User user)
        {
            UserDao userDao = new UserDao(_mySqlContext);
            bool hasInserted = await userDao.Register(user);

            return View();
        }

        // GET: User/Delete
        [ServiceFilter(typeof(UserAuthorizationFilter))]
        public async Task<IActionResult> Delete()
        {
            // if (id == null)
            // {
            //     return Redirect("http://localhost:5000/Error/ShowError");
            // }

            return View();
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