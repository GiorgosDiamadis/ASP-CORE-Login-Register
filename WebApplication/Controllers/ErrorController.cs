using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace WebApplication.Controllers
{
    public class ErrorController : Controller
    {
        public ErrorController()
        {
        }

        [Route("{*url}", Order = 1000)]
        public IActionResult PageNotFound()
        {
            Response.StatusCode = 404;
            return View();
        }

        public IActionResult BadRequestPage()
        {
            Response.StatusCode = 400;
            return View();
        }
    }
}