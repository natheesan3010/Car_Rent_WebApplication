using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickRentMyRide.Data;
using QuickRentMyRide.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuickRentMyRide.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---------------- Dashboard ----------------
        public async Task<IActionResult> C_Dashboard()
        {
            var username = HttpContext.Session.GetString("Username");
            var customerId = HttpContext.Session.GetInt32("CustomerID");

            if (string.IsNullOrEmpty(username))
            {
                TempData["ErrorMessage"] = "Session expired, please login again!";
                return RedirectToAction("Login", "Account");
            }

            // Booking History
            var bookings = await _context.Bookings
                .Include(b => b.Car)
                .Where(b => b.CustomerID == customerId)
                .OrderByDescending(b => b.StartDate)
                .ToListAsync();

            ViewBag.MyBookings = bookings;
            ViewData["ActivePage"] = "Dashboard";
            return View();
        }

        // ---------------- Profile (GET) ----------------
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Username == username);
            if (customer == null) return NotFound();

            ViewData["ActivePage"] = "Profile";
            return View(customer);
        }

        // ---------------- Profile (POST Update) ----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(Customer model, IFormFile photo)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var customer = await _context.Customers.FindAsync(model.CustomerID);
            if (customer == null) return NotFound();

            // Email duplicate check
            var emailExists = await _context.Customers
                .AnyAsync(c => c.Email == model.Email && c.CustomerID != model.CustomerID);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "This email is already in use!");
                return View("Profile", model);
            }

            if (!ModelState.IsValid)
                return View("Profile", model);

            customer.FirstName = model.FirstName;
            customer.LastName = model.LastName;
            customer.Email = model.Email;
            customer.PhoneNumber = model.PhoneNumber;
            customer.Address = model.Address;

            // Photo upload + old delete
            if (photo != null)
            {
                if (!string.IsNullOrEmpty(customer.LicensePhoto))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", customer.LicensePhoto.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                var fileName = Guid.NewGuid() + Path.GetExtension(photo.FileName);
                var filePath = Path.Combine(path, fileName);
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

        // ---------------- Booking (GET) ----------------
        [HttpGet]
        public IActionResult Booking()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            ViewBag.AvailableCars = _context.Cars.Where(c => c.IsAvailable).ToList();
            return View();
        }

        // ---------------- Booking (POST) ----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Booking(Customer model, IFormFile LicensePhotoFile, int CarID, DateTime StartDate, DateTime EndDate)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                ViewBag.AvailableCars = _context.Cars.Where(c => c.IsAvailable).ToList();
                return View(model);
            }

            // Duplicate booking check
            var existingBooking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.CarID == CarID &&
                                          ((StartDate >= b.StartDate && StartDate <= b.EndDate) ||
                                           (EndDate >= b.StartDate && EndDate <= b.EndDate)));

            if (existingBooking != null)
            {
                TempData["ErrorMessage"] = "This car is already booked in the selected period!";
                return RedirectToAction("Booking");
            }

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == model.Email);
            if (customer == null)
            {
                customer = model;

                if (LicensePhotoFile != null)
                {
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                    var fileName = Guid.NewGuid() + Path.GetExtension(LicensePhotoFile.FileName);
                    var filePath = Path.Combine(path, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await LicensePhotoFile.CopyToAsync(stream);
                    }
                    customer.LicensePhoto = "/images/" + fileName;
                }

                if (string.IsNullOrEmpty(customer.Username))
                    customer.Username = customer.Email.Split('@')[0];

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
            }

            var booking = new Booking
            {
                CustomerID = customer.CustomerID,
                CarID = CarID,
                StartDate = StartDate,
                EndDate = EndDate
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // Car availability update
            var car = await _context.Cars.FindAsync(CarID);
            if (car != null)
            {
                car.IsAvailable = false;
                _context.Cars.Update(car);
                await _context.SaveChangesAsync();
            }

            HttpContext.Session.SetInt32("CustomerID", customer.CustomerID);
            HttpContext.Session.SetString("Username", customer.Username);

            TempData["SuccessMessage"] = "Booking confirmed!";
            return RedirectToAction("BookingConfirmation", new { id = booking.BookingID });
        }

        // ---------------- Availability Search ----------------
        [HttpGet]
        public async Task<IActionResult> CheckAvailability(DateTime? startDate, DateTime? endDate)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            ViewData["ActivePage"] = "Availability";

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

        // ---------------- Booking Confirmation ----------------
        public async Task<IActionResult> BookingConfirmation(int id)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var booking = await _context.Bookings
                .Include(b => b.Car)
                .Include(b => b.Customer)
                .FirstOrDefaultAsync(b => b.BookingID == id);

            if (booking == null) return NotFound();
            return View(booking);
        }
    }
}
