using Microsoft.AspNetCore.Mvc;
using QuickRentMyRide.Data;
using QuickRentMyRide.Models;

namespace QuickRentMyRide.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        // === CUSTOMER ===
        [HttpGet]
        public IActionResult AddCustomer() => View();

        [HttpPost]
        public async Task<IActionResult> AddCustomer(Customer customer, IFormFile LicensePhotoFile)
        {
            if (LicensePhotoFile != null)
            {
                var fileName = Path.GetFileName(LicensePhotoFile.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                using var stream = new FileStream(path, FileMode.Create);
                await LicensePhotoFile.CopyToAsync(stream);
                customer.LicensePhoto = "/images/" + fileName;
            }

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return RedirectToAction("Dashboard");
        }

        // === CAR ===
        [HttpGet]
        public IActionResult AddCar() => View();

        [HttpPost]
        public async Task<IActionResult> AddCar(Car car, IFormFile CarImageFile)
        {
            if (CarImageFile != null)
            {
                var fileName = Path.GetFileName(CarImageFile.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                using var stream = new FileStream(path, FileMode.Create);
                await CarImageFile.CopyToAsync(stream);
                car.CarImage = "/images/" + fileName;
            }

            _context.Cars.Add(car);
            await _context.SaveChangesAsync();
            return RedirectToAction("Dashboard");
        }
    }

}
