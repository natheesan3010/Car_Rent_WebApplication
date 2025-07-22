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
    public class CarController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int DefaultPageSize = 5;

        public CarController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Car/CarList?search=abc&page=1
        public async Task<IActionResult> CarList(string search, int page = 1, int pageSize = DefaultPageSize)
        {
            var query = _context.Cars.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c =>
                    c.NumberPlate.Contains(search) ||
                    c.CarBrand.Contains(search) ||
                    c.CarModel.Contains(search));
            }

            int totalCars = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalCars / (double)pageSize);

            var cars = await query
                .OrderByDescending(c => c.CarID)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = search ?? "";
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;

            return View(cars);
        }

        [HttpGet]
        public async Task<IActionResult> EditCar(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null) return NotFound();
            return View(car);
        }

        [HttpPost]
        public async Task<IActionResult> EditCar(Car updatedCar, IFormFile CarImageFile)
        {
            if (!ModelState.IsValid)
                return View(updatedCar);

            var car = await _context.Cars.FindAsync(updatedCar.CarID);
            if (car == null) return NotFound();

            car.NumberPlate = updatedCar.NumberPlate;
            car.CarBrand = updatedCar.CarBrand;
            car.CarModel = updatedCar.CarModel;
            car.RentPerDay = updatedCar.RentPerDay;
            car.IsAvailable = updatedCar.IsAvailable;

            if (CarImageFile != null)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(CarImageFile.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await CarImageFile.CopyToAsync(stream);
                }
                car.CarImage = "/images/" + fileName;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Car updated successfully!";
            return RedirectToAction("CarList");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCar(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null)
            {
                TempData["Error"] = "Car not found.";
                return RedirectToAction("CarList");
            }

            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Car deleted successfully!";
            return RedirectToAction("CarList");
        }
    }
}
