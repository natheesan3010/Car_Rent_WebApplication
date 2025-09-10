using Microsoft.AspNetCore.Mvc;
using QuickRentMyRide.Models;
using QuickRentMyRide.Data;
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

                // Double booking check
                bool alreadyBooked = _context.Bookings.Any(b =>
                    b.CarID == booking.CarID &&
                    ((booking.StartDate >= b.StartDate && booking.StartDate <= b.EndDate) ||
                     (booking.EndDate >= b.StartDate && booking.EndDate <= b.EndDate)));

                if (alreadyBooked)
                {
                    ModelState.AddModelError("", "This car is already booked for the selected dates.");
                    ViewBag.Car = car;
                    return View(booking);
                }

                // Price calculation
                int days = (booking.EndDate - booking.StartDate).Days;
                if (days <= 0)
                {
                    ModelState.AddModelError("", "The end date must be later than the start date.");
                    ViewBag.Car = car;
                    return View(booking);
                }

                booking.TotalPrice = days * car.PricePerDay;

                _context.Bookings.Add(booking);
                _context.SaveChanges();

                return RedirectToAction("Details", new { id = booking.BookingID });
            }

            return View(booking);
        }

        // 🔹 GET: Booking/Details/{id}
        public IActionResult Details(int id)
        {
            var booking = _context.Bookings
                .Where(b => b.BookingID == id)
                .FirstOrDefault();

            if (booking == null) return NotFound();

            return View(booking);
        }
    }
}
