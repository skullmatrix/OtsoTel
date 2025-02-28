using Microsoft.AspNetCore.Mvc;

namespace HotelWebsite.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
