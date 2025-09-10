using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickRentMyRide.Models;
using QuickRentMyRide.Data;
using System;
using System.Linq;

namespace QuickRentMyRide.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔹 GET: Booking/Create
        [HttpGet]
        public IActionResult Create(int carID)
        {
            var car = _context.Cars.Find(carID);
            if (car == null) return NotFound();

            ViewBag.Car = car;
            return View();
        }

        // 🔹 POST: Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Booking booking)
        {
            if (ModelState.IsValid)
            {
                var car = _context.Cars.Find(booking.CarID);
                if (car == null) return NotFound();

                // Past date check
                if (booking.StartDate < DateTime.Today)
                {
                    ModelState.AddModelError("", "Booking start date cannot be in the past.");
                    ViewBag.Car = car;
                    return View(booking);
                }

                // Double booking full overlap check
                bool alreadyBooked = _context.Bookings.Any(b =>
                    b.CarID == booking.CarID &&
                    (
                        (booking.StartDate >= b.StartDate && booking.StartDate <= b.EndDate) ||  // start inside
                        (booking.EndDate >= b.StartDate && booking.EndDate <= b.EndDate) ||      // end inside
                        (booking.StartDate <= b.StartDate && booking.EndDate >= b.EndDate)       // full overlap
                    )
                );

                if (alreadyBooked)
                {
                    ModelState.AddModelError("", "This car is already booked for the selected dates.");
                    ViewBag.Car = car;
                    return View(booking);
                }

                // Date validation
                int days = (booking.EndDate - booking.StartDate).Days;
                if (days <= 0)
                {
                    ModelState.AddModelError("", "The end date must be later than the start date.");
                    ViewBag.Car = car;
                    return View(booking);
                }

                // Price calculation
                booking.TotalPrice = days * car.RentPerDay;

                _context.Bookings.Add(booking);
                _context.SaveChanges();

                return RedirectToAction("Details", new { id = booking.BookingID });
            }

            return View(booking);
        }

        // 🔹 GET: Booking/AvailabilitySearch
        [HttpGet]
        public IActionResult AvailabilitySearch()
        {
            return View(); // இது AvailabilitySearch.cshtml-ஐ load பண்ணும்
        }


        // 🔹 GET: Booking/Details/{id}
        public IActionResult Details(int id)
        {
            var booking = _context.Bookings
                .Where(b => b.BookingID == id)
                .Include(b => b.Car) // Car info சேர்க்க
                .FirstOrDefault();

            if (booking == null) return NotFound();

            return View(booking);
        }

        // 🔹 GET: Booking/CheckAvailability
        [HttpGet]
        public IActionResult CheckAvailability(DateTime startDate, DateTime endDate)
        {
            if (startDate < DateTime.Today || endDate <= startDate)
            {
                ModelState.AddModelError("", "Please enter valid dates.");
                return View("AvailabilityResult", Enumerable.Empty<Car>());
            }

            // ஏற்கனவே booked ஆன கார்களை கண்டுபிடிக்க
            var bookedCarIDs = _context.Bookings
                .Where(b =>
                    (startDate >= b.StartDate && startDate <= b.EndDate) ||   // start inside
                    (endDate >= b.StartDate && endDate <= b.EndDate) ||       // end inside
                    (startDate <= b.StartDate && endDate >= b.EndDate))       // full overlap
                .Select(b => b.CarID)
                .ToList();

            // Available cars மட்டும் எடுக்க
            var availableCars = _context.Cars
                .Where(c => !bookedCarIDs.Contains(c.CarID))
                .ToList();

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            return View("AvailabilityResult", availableCars);
        }

    }
}
