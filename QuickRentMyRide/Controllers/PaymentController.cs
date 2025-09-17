using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickRentMyRide.Data;
using QuickRentMyRide.Models;
using Stripe.Checkout;

namespace QuickRentMyRide.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public PaymentController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ---------------- CHECKOUT PAGE ----------------
        public IActionResult Checkout(int bookingId)
        {
            var booking = _context.Bookings
                .Include(b => b.Car)
                .FirstOrDefault(b => b.BookingID == bookingId);

            if (booking == null) return NotFound();

            ViewBag.Booking = booking;
            ViewBag.PublishableKey = _config["Stripe:PublishableKey"];

            return View();
        }

        // ---------------- STRIPE SESSION ----------------
        [HttpPost]
        public IActionResult CreateCheckoutSession(int bookingId)
        {
            var booking = _context.Bookings
                .Include(b => b.Car)
                .FirstOrDefault(b => b.BookingID == bookingId);

            if (booking == null) return NotFound();

            var domain = $"{Request.Scheme}://{Request.Host}";

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(booking.TotalPrice * 100), // convert to cents
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Car Booking - {booking.Car.CarModel}"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = domain + "/Payment/Success?bookingId=" + booking.BookingID,
                CancelUrl = domain + "/Payment/Cancel?bookingId=" + booking.BookingID
            };

            var service = new SessionService();
            Session session = service.Create(options);

            return Json(new { id = session.Id });
        }

        // ---------------- SUCCESS ----------------
        public IActionResult Success(int bookingId)
        {
            var booking = _context.Bookings.Find(bookingId);
            if (booking != null)
            {
                booking.PaymentStatus = "Paid";
                booking.Status = "Pending"; // Waiting for admin approval
                _context.SaveChanges();
            }

            TempData["SuccessMessage"] = "Payment successful! Booking confirmed.";
            return RedirectToAction("MyBookings", "Booking");
        }

        // ---------------- CANCEL ----------------
        public IActionResult Cancel(int bookingId)
        {
            TempData["ErrorMessage"] = "Payment cancelled.";
            return RedirectToAction("MyBookings", "Booking");
        }
    }
}
