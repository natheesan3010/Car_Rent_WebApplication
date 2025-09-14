using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickRentMyRide.Models;
using QuickRentMyRide.Data;
using QuickRentMyRide.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;

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
            if (car == null)
                return NotFound();

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
            var car = _context.Cars.Find(booking.CarID);
            if (car == null) return NotFound();
            ViewBag.Car = car;

            // First-time customer check
            var email = User.Identity.Name;
            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Please login first.";
                return RedirectToAction("AvailableCars", "Car");
            }

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);

            // If customer not found, redirect to AddDetails page
            if (customer == null)
            {
                TempData["CustomerEmail"] = email;
                return RedirectToAction("AddDetails", "Customer");
            }

            booking.CustomerID = customer.CustomerID;

            if (!ModelState.IsValid)
                return View(booking);

            // Validate dates
            if (booking.StartDate < DateTime.Today)
                booking.StartDate = DateTime.Today;
            if (booking.EndDate <= booking.StartDate)
                booking.EndDate = booking.StartDate.AddDays(1);

            // Double booking check
            bool alreadyBooked = _context.Bookings.Any(b =>
                b.CarID == booking.CarID &&
                (b.Status == "OTPVerified" || b.Status == "Approved") &&
                ((booking.StartDate >= b.StartDate && booking.StartDate <= b.EndDate) ||
                 (booking.EndDate >= b.StartDate && booking.EndDate <= b.EndDate) ||
                 (booking.StartDate <= b.StartDate && booking.EndDate >= b.EndDate))
            );

            if (alreadyBooked)
            {
                ModelState.AddModelError("", "This car is already booked for the selected dates.");
                return View(booking);
            }

            // Calculate total price
            int days = (booking.EndDate - booking.StartDate).Days;
            booking.TotalPrice = (days <= 0 ? 1 : days) * car.RentPerDay;
            booking.Status = "Pending";
            booking.PaymentStatus = "Pending";

            // Generate OTP
            booking.OTP = OTPHelper.GenerateOTP();
            booking.OTPGeneratedAt = DateTime.Now;

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // Send OTP
            EmailHelper.SendOTP(customer.Email, booking.OTP);

            TempData["BookingID"] = booking.BookingID;
            TempData["SuccessMessage"] = $"Booking created! OTP sent to {customer.Email}";

            return RedirectToAction("VerifyOTP");
        }

        // ---------------- VERIFY OTP ----------------
        [HttpGet]
        public async Task<IActionResult> VerifyOTP()
        {
            if (TempData["BookingID"] == null)
                return RedirectToAction("AvailableCars", "Car");

            int bookingId = (int)TempData["BookingID"];
            var booking = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Car)
                .FirstOrDefaultAsync(b => b.BookingID == bookingId);

            if (booking == null) return NotFound();

            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOTP(string inputOTP)
        {
            if (TempData["BookingID"] == null)
                return RedirectToAction("AvailableCars", "Car");

            int bookingId = (int)TempData["BookingID"];
            var booking = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Car)
                .FirstOrDefaultAsync(b => b.BookingID == bookingId);

            if (booking == null) return NotFound();

            // OTP expire check
            if (!booking.OTPGeneratedAt.HasValue || DateTime.Now > booking.OTPGeneratedAt.Value.AddMinutes(5))
            {
                booking.Status = "Cancelled";
                await _context.SaveChangesAsync();
                TempData["ErrorMessage"] = "OTP expired! Booking cancelled.";
                return RedirectToAction("Create", new { carID = booking.CarID });
            }

            if (inputOTP == booking.OTP)
            {
                booking.Status = "OTPVerified";
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "OTP verified! Waiting for admin approval.";
                return RedirectToAction("AwaitAdminApproval");
            }

            ModelState.AddModelError("", "Invalid OTP. Try again.");
            return View(booking);
        }

        // ---------------- Await Admin Approval ----------------
        public IActionResult AwaitAdminApproval()
        {
            return View();
        }

        // ---------------- My Bookings ----------------
        public async Task<IActionResult> MyBookings()
        {
            var email = User.Identity.Name;
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("AvailableCars", "Car");

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
            if (customer == null) return RedirectToAction("AvailableCars", "Car");

            var bookings = await _context.Bookings
                .Include(b => b.Car)
                .Where(b => b.CustomerID == customer.CustomerID)
                .OrderByDescending(b => b.StartDate)
                .ToListAsync();

            return View(bookings);
        }

        // ---------------- Cancel Booking ----------------
        [HttpPost]
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
    }
}
