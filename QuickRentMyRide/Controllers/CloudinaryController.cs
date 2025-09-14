using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using QuickRentMyRide.Models;

namespace QuickRentMyRide.Controllers
{
    public class CloudinaryController : Controller
    {
        private readonly CloudinarySettings _cloudinarySettings;

        public CloudinaryController(IOptions<CloudinarySettings> options)
        {
            _cloudinarySettings = options.Value;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
