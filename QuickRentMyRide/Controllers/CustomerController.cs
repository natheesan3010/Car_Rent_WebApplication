using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickRentMyRide.Data;
using QuickRentMyRide.Models;
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

        // Gmail Login Callback
        public IActionResult GmailLoginCallback(string email)
        {
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("AvailableCars", "Car");

            var customer = _context.Customers.FirstOrDefault(c => c.Email == email);
            if (customer != null)
            {
                HttpContext.Session.SetInt32("CustomerID", customer.CustomerID);
                HttpContext.Session.SetString("Gmail_Address", customer.Email);
                HttpContext.Session.SetString("Role", "Customer");
                return RedirectToAction("Dashboard");
            }

            TempData["CustomerEmail"] = email;
            return RedirectToAction("AddDetails");
        }

        // Add Customer Details (GET)
        [HttpGet]
        public IActionResult AddDetails()
        {
            var email = TempData["CustomerEmail"]?.ToString();
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("AvailableCars", "Car");

            TempData.Keep("CustomerEmail");
            return View(new Customer { Email = email });
        }

        // Add Customer Details (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDetails(Customer model)
        {
            TempData.Keep("CustomerEmail");

            if (!ModelState.IsValid)
                return View(model);

            if (_context.Customers.Any(c => c.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email already exists.");
                return View(model);
            }

            if (string.IsNullOrEmpty(model.Username))
            {
                string baseUsername = model.Email.Split('@')[0];
                string username = baseUsername;
                int counter = 1;
                while (_context.Customers.Any(c => c.Username == username))
                {
                    username = baseUsername + counter;
                    counter++;
                }
                model.Username = username;
            }

            _context.Customers.Add(model);
            await _context.SaveChangesAsync();

            // ✅ Correct session
            HttpContext.Session.SetInt32("CustomerID", model.CustomerID);
            HttpContext.Session.SetString("Gmail_Address", model.Email);
            HttpContext.Session.SetString("Role", "Customer");

            TempData["SuccessMessage"] = "Details saved successfully!";
            return RedirectToAction("Dashboard");
        }

        // Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerID");
            var username = HttpContext.Session.GetString("Gmail_Address") ?? "Guest";

            if (!customerId.HasValue || username == "Guest")
                return RedirectToAction("Login", "Account");

            var bookings = await _context.Bookings
                .Include(b => b.Car)
                .Where(b => b.CustomerID == customerId)
                .OrderByDescending(b => b.StartDate)
                .ToListAsync();

            var availableCars = await _context.Cars.ToListAsync();

            ViewBag.MyBookings = bookings;
            ViewBag.Username = username;

            return View(availableCars);
        }

        // Profile
        public IActionResult Profile()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerID");
            if (!customerId.HasValue)
                return RedirectToAction("Login", "Account");

            var customer = _context.Customers.FirstOrDefault(c => c.CustomerID == customerId);
            if (customer == null)
                return RedirectToAction("Login", "Account");

            return View(customer);
        }
    }
}
