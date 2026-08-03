using Microsoft.AspNetCore.Mvc;

namespace BloodDonation.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/404")]
        public IActionResult Error404()
        {
            return View("~/Views/Shared/Error404.cshtml");
        }

        [Route("Error/{statusCode}")]
        public IActionResult HandleErrorCode(int statusCode)
        {
            if (statusCode == 404)
            {
                return View("~/Views/Shared/Error404.cshtml");
            }

            return View("~/Views/Shared/Error.cshtml");
        }
    }
} 