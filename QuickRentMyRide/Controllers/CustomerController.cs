using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickRentMyRide.Data;
using QuickRentMyRide.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace QuickRentMyRide.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Customer Dashboard
        public IActionResult C_Dashboard()
        {
            ViewData["ActivePage"] = "Dashboard"; // Active page set
            return View();
        }

        // GET: Profile
        public async Task<IActionResult> Profile()
        {
            ViewData["ActivePage"] = "Profile"; // Active page set

            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Username == username);
            if (customer == null) return NotFound();
            return View(customer);
        }

        // POST: Update Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(Customer model, IFormFile photo)
        {
            ViewData["ActivePage"] = "Profile"; // Active page set

            if (!ModelState.IsValid)
                return View("Profile", model);

            var customer = await _context.Customers.FindAsync(model.CustomerID);
            if (customer == null) return NotFound();

            customer.FirstName = model.FirstName;
            customer.LastName = model.LastName;
            customer.Email = model.Email;
            customer.PhoneNumber = model.PhoneNumber;
            customer.Address = model.Address;

            if (photo != null)
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                if (!Directory.Exists(imagePath))
                    Directory.CreateDirectory(imagePath);

                var fileName = Guid.NewGuid() + Path.GetExtension(photo.FileName);
                var filePath = Path.Combine(imagePath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }
                customer.LicensePhoto = "/images/" + fileName;
            }

            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }

        // --- Availability Search ---
        [HttpGet]
        public async Task<IActionResult> CheckAvailability(DateTime? startDate, DateTime? endDate)
        {
            ViewData["ActivePage"] = "Availability"; // Active page set

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            if (!startDate.HasValue || !endDate.HasValue || endDate <= startDate || startDate < DateTime.Today)
            {
                ViewBag.ShowResults = false;
                return View("AvailabilitySearch", new List<Car>());
            }

            var bookedCarIDs = await _context.Bookings
                .Where(b =>
                    (startDate >= b.StartDate && startDate <= b.EndDate) ||
                    (endDate >= b.StartDate && endDate <= b.EndDate) ||
                    (startDate <= b.StartDate && endDate >= b.EndDate))
                .Select(b => b.CarID)
                .ToListAsync();

            var availableCars = await _context.Cars
                .Where(c => !bookedCarIDs.Contains(c.CarID))
                .ToListAsync();

            ViewBag.ShowResults = true;
            return View("AvailabilitySearch", availableCars);
        }
    }
}
