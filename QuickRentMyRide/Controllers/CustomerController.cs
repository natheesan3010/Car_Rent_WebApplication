using Microsoft.AspNetCore.Mvc;

namespace QuickRentMyRide.Controllers
{
    public class CustomerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
