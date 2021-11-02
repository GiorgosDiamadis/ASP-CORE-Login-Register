using System.Diagnostics;
using Core.Flash;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebApplication.Filters;
using WebApplication.Models;
using WebApplication.Services.Interfaces;

namespace WebApplication.Controllers
{
    [ServiceFilter(typeof(UserAuthorizationFilter))]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _config;
        private readonly ITokenService _tokenService;
        private readonly IFlasher _flasher;

        public HomeController(ILogger<HomeController> logger, IConfiguration config, ITokenService tokenService,
            IFlasher flasher)
        {
            _logger = logger;
            _config = config;
            _tokenService = tokenService;
            _flasher = flasher;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}