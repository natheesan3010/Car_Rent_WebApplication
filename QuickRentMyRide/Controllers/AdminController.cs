using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickRentMyRide.Data;
using QuickRentMyRide.Models;
using QuickRentMyRide.Helpers; // OTP + Email helpers
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace QuickRentMyRide.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---------------- Dashboard ----------------
        public async Task<IActionResult> Dashboard()
        {
            ViewBag.TotalCustomers = await _context.Customers.CountAsync();
            ViewBag.TotalCars = await _context.Cars.CountAsync();
            ViewBag.TotalBookings = await _context.Bookings.CountAsync();
            ViewBag.PendingBookings = await _context.Bookings.Where(b => b.Status == "OTPVerified").CountAsync();
            ViewBag.TotalRevenue = await _context.Bookings
                .Where(b => b.Status == "Approved")
                .SumAsync(b => b.TotalPrice);

            return View();
        }

        // ---------------- Customer Management ----------------
        public async Task<IActionResult> ManageCustomers()
        {
            var customers = await _context.Customers.ToListAsync();
            return View(customers);
        }

        [HttpGet]
        public IActionResult AddCustomer() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCustomer(Customer customer, IFormFile LicensePhotoFile)
        {
            if (!ModelState.IsValid) return View(customer);

            if (LicensePhotoFile != null)
                customer.LicensePhoto = await UploadFileAsync(LicensePhotoFile, "images");

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Customer added successfully!";
            return RedirectToAction("ManageCustomers");
        }

        [HttpGet]
        public async Task<IActionResult> EditCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCustomer(int id, Customer model, IFormFile LicensePhotoFile)
        {
            if (id != model.CustomerID) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();

            customer.FirstName = model.FirstName;
            customer.LastName = model.LastName;
            customer.Email = model.Email;
            customer.PhoneNumber = model.PhoneNumber;
            customer.Address = model.Address;

            if (LicensePhotoFile != null)
                customer.LicensePhoto = await UploadFileAsync(LicensePhotoFile, "images");

            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Customer updated successfully!";
            return RedirectToAction("ManageCustomers");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ManageCustomers");
        }

        // ---------------- Car Management ----------------
        public async Task<IActionResult> ManageCars()
        {
            var cars = await _context.Cars.ToListAsync();
            return View(cars);
        }

        [HttpGet]
        public IActionResult AddCar() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCar(Car car, IFormFile CarImageFile)
        {
            if (!ModelState.IsValid) return View(car);

            if (CarImageFile != null)
                car.CarImage = await UploadFileAsync(CarImageFile, "images");

            car.IsAvailable = true;

            _context.Cars.Add(car);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Car added successfully!";
            return RedirectToAction("ManageCars");
        }

        [HttpGet]
        public async Task<IActionResult> EditCar(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null) return NotFound();
            return View(car);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCar(int id, Car model, IFormFile CarImageFile)
        {
            if (id != model.CarID) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var car = await _context.Cars.FindAsync(id);
            if (car == null) return NotFound();

            car.CarModel = model.CarModel;
            car.NumberPlate = model.NumberPlate;
            car.RentPerDay = model.RentPerDay;
            car.IsAvailable = model.IsAvailable;

            if (CarImageFile != null)
                car.CarImage = await UploadFileAsync(CarImageFile, "images");

            _context.Cars.Update(car);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Car updated successfully!";
            return RedirectToAction("ManageCars");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCar(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car != null)
            {
                _context.Cars.Remove(car);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ManageCars");
        }

        // ---------------- Booking Management ----------------
        public async Task<IActionResult> ManageBookings()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Car)
                .OrderByDescending(b => b.StartDate)
                .ToListAsync();

            return View(bookings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveBooking(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Car)
                .FirstOrDefaultAsync(b => b.BookingID == id);

            if (booking != null && booking.Status == "OTPVerified")
            {
                booking.Status = "Approved";           // Admin approved
                booking.PaymentStatus = "Paid";        // Assume payment done
                if (booking.Car != null)
                    booking.Car.IsAvailable = false;   // Mark car as unavailable

                await _context.SaveChangesAsync();

                // Send email confirmation to customer
                EmailHelper.SendBookingApproved(booking.Customer.Email, booking.BookingID);
            }

            return RedirectToAction("ManageBookings");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectBooking(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Car)
                .FirstOrDefaultAsync(b => b.BookingID == id);

            if (booking != null && booking.Status == "OTPVerified")
            {
                booking.Status = "Rejected";
                booking.PaymentStatus = "Failed";
                if (booking.Car != null)
                    booking.Car.IsAvailable = true;

                await _context.SaveChangesAsync();

                // Send email rejection
                EmailHelper.SendBookingRejected(booking.Customer.Email, booking.BookingID);
            }

            return RedirectToAction("ManageBookings");
        }

        // ---------------- Helper: File Upload ----------------
        private async Task<string> UploadFileAsync(IFormFile file, string folder = "images")
        {
            if (file == null) return null;

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folder, fileName);

            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            return "/" + folder + "/" + fileName;
        }
    }
}
