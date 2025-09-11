using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // ---------------- Dashboard ----------------
        public async Task<IActionResult> Dashboard()
        {
            ViewBag.TotalCustomers = await _context.Customers.CountAsync();
            ViewBag.TotalCars = await _context.Cars.CountAsync();
            ViewBag.TotalBookings = await _context.Bookings.CountAsync();
            ViewBag.PendingBookings = await _context.Bookings.Where(b => b.Status == "Pending").CountAsync();

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
            if (LicensePhotoFile != null)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(LicensePhotoFile.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                using var stream = new FileStream(path, FileMode.Create);
                await LicensePhotoFile.CopyToAsync(stream);
                customer.LicensePhoto = "/images/" + fileName;
            }

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
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

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();

            if (!ModelState.IsValid) return View(model);

            customer.FirstName = model.FirstName;
            customer.LastName = model.LastName;
            customer.Email = model.Email;
            customer.PhoneNumber = model.PhoneNumber;
            customer.Address = model.Address;

            if (LicensePhotoFile != null)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(LicensePhotoFile.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                using var stream = new FileStream(path, FileMode.Create);
                await LicensePhotoFile.CopyToAsync(stream);
                customer.LicensePhoto = "/images/" + fileName;
            }

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
            if (CarImageFile != null)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(CarImageFile.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                using var stream = new FileStream(path, FileMode.Create);
                await CarImageFile.CopyToAsync(stream);
                car.CarImage = "/images/" + fileName;
            }

            car.IsAvailable = true;
            _context.Cars.Add(car);
            await _context.SaveChangesAsync();
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

            var car = await _context.Cars.FindAsync(id);
            if (car == null) return NotFound();

            if (!ModelState.IsValid) return View(model);

            car.CarID = model.CarID;
            car.CarModel = model.CarModel;
            car.NumberPlate = model.NumberPlate;
            car.RentPerDay = model.RentPerDay;
            car.IsAvailable = model.IsAvailable;

            if (CarImageFile != null)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(CarImageFile.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                using var stream = new FileStream(path, FileMode.Create);
                await CarImageFile.CopyToAsync(stream);
                car.CarImage = "/images/" + fileName;
            }

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
            var booking = await _context.Bookings.Include(b => b.Car).FirstOrDefaultAsync(b => b.BookingID == id);
            if (booking != null)
            {
                booking.Status = "Approved";
                if (booking.Car != null)
                    booking.Car.IsAvailable = false;

                _context.Bookings.Update(booking);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ManageBookings");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectBooking(int id)
        {
            var booking = await _context.Bookings.Include(b => b.Car).FirstOrDefaultAsync(b => b.BookingID == id);
            if (booking != null)
            {
                booking.Status = "Rejected";
                if (booking.Car != null)
                    booking.Car.IsAvailable = true;

                _context.Bookings.Update(booking);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ManageBookings");
        }
    }
}
