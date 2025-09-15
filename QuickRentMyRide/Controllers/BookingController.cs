using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickRentMyRide.Models;
using QuickRentMyRide.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace QuickRentMyRide.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---------------- CREATE BOOKING (GET) ----------------
        [HttpGet]
        public IActionResult Create(int carID)
        {
            var car = _context.Cars.Find(carID);
            if (car == null) return NotFound();

            ViewBag.Car = car;

            var booking = new Booking
            {
                CarID = car.CarID,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1)
            };

            return View(booking);
        }

        // ---------------- CREATE BOOKING (POST) ----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            var car = await _context.Cars.FindAsync(booking.CarID);
            if (car == null) return NotFound();

            ViewBag.Car = car;

            // Check login session
            var customerId = HttpContext.Session.GetInt32("CustomerID");
            if (!customerId.HasValue)
            {
                TempData["ErrorMessage"] = "Please login first.";
                return RedirectToAction("Login", "Account");
            }

            var customer = await _context.Customers.FindAsync(customerId.Value);
            if (customer == null)
                return RedirectToAction("AddDetails", "Customer");

            booking.CustomerID = customer.CustomerID;

            if (!ModelState.IsValid) return View(booking);

            // Validate dates
            if (booking.StartDate < DateTime.Today)
                booking.StartDate = DateTime.Today;
            if (booking.EndDate <= booking.StartDate)
                booking.EndDate = booking.StartDate.AddDays(1);

            // Check double booking
            bool alreadyBooked = _context.Bookings.Any(b =>
                b.CarID == booking.CarID &&
                (b.Status == "Pending" || b.Status == "Approved") &&
                ((booking.StartDate >= b.StartDate && booking.StartDate <= b.EndDate) ||
                 (booking.EndDate >= b.StartDate && booking.EndDate <= b.EndDate) ||
                 (booking.StartDate <= b.StartDate && booking.EndDate >= b.EndDate))
            );

            if (alreadyBooked)
            {
                ModelState.AddModelError("", "This car is already booked for the selected dates.");
                return View(booking);
            }

            // Total price
            int days = (booking.EndDate - booking.StartDate).Days;
            booking.TotalPrice = (days <= 0 ? 1 : days) * car.RentPerDay;

            // Save booking directly as Pending
            booking.Status = "Pending";
            booking.PaymentStatus = "Pending";

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Booking created successfully! Waiting for admin approval.";
            return RedirectToAction("MyBookings");
        }

        // ---------------- My Bookings ----------------
        public async Task<IActionResult> MyBookings()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerID");
            if (!customerId.HasValue)
                return RedirectToAction("Login", "Account");

            var bookings = await _context.Bookings
                .Include(b => b.Car)
                .Where(b => b.CustomerID == customerId.Value)
                .OrderByDescending(b => b.StartDate)
                .ToListAsync();

            return View(bookings);
        }

        // ---------------- Cancel Booking ----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            if (booking.StartDate <= DateTime.Today)
            {
                TempData["ErrorMessage"] = "Cannot cancel a booking that has already started.";
                return RedirectToAction("MyBookings");
            }

            booking.Status = "Cancelled";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Booking cancelled successfully!";
            return RedirectToAction("MyBookings");
        }

        // ---------------- CHECK AVAILABILITY ----------------
        [HttpGet]
        public async Task<IActionResult> CheckAvailability(DateTime? startDate, DateTime? endDate)
        {
            var cars = await _context.Cars.ToListAsync();

            if (startDate.HasValue && endDate.HasValue)
            {
                cars = cars.Where(c =>
                    !_context.Bookings.Any(b =>
                        b.CarID == c.CarID &&
                        (b.Status == "Pending" || b.Status == "Approved") &&
                        ((startDate.Value >= b.StartDate && startDate.Value <= b.EndDate) ||
                         (endDate.Value >= b.StartDate && endDate.Value <= b.EndDate) ||
                         (startDate.Value <= b.StartDate && endDate.Value >= b.EndDate))
                    )
                ).ToList();
            }

            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd") ?? "";
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd") ?? "";

            return View(cars);
        }
    }
}
