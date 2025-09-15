using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickRentMyRide.Data;
using QuickRentMyRide.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace QuickRentMyRide.Controllers
{
    public class CarController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Cloudinary _cloudinary;
        private const int DefaultPageSize = 5;

        public CarController(ApplicationDbContext context, Cloudinary cloudinary)
        {
            _context = context;
            _cloudinary = cloudinary;
        }

        // GET: CarList
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

        // GET: AddCar
        [HttpGet]
        public IActionResult AddCar()
        {
            return View();
        }

        // POST: AddCar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCar(Car model, IFormFile CarImageFile)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Cloudinary upload
            if (CarImageFile != null && CarImageFile.Length > 0)
            {
                using var stream = CarImageFile.OpenReadStream();
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(CarImageFile.FileName, stream),
                    Folder = "CarImages"
                };
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                model.CarImage = uploadResult.SecureUrl.ToString();
            }

            _context.Cars.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Car added successfully!";
            return RedirectToAction("CarList");
        }

        // GET: EditCar
        [HttpGet]
        public async Task<IActionResult> EditCar(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null) return NotFound();
            return View(car);
        }

        // POST: EditCar
        [HttpPost]
        [ValidateAntiForgeryToken]
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

            if (CarImageFile != null && CarImageFile.Length > 0)
            {
                using var stream = CarImageFile.OpenReadStream();
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(CarImageFile.FileName, stream),
                    Folder = "CarImages"
                };
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                car.CarImage = uploadResult.SecureUrl.ToString();
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Car updated successfully!";
            return RedirectToAction("CarList");
        }

        // POST: DeleteCar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCar(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null)
            {
                TempData["Error"] = "Car not found.";
                return RedirectToAction("CarList");
            }

            if (!string.IsNullOrEmpty(car.CarImage))
            {
                var publicId = GetCloudinaryPublicId(car.CarImage);
                if (!string.IsNullOrEmpty(publicId))
                    await _cloudinary.DestroyAsync(new DeletionParams(publicId));
            }

            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Car deleted successfully!";
            return RedirectToAction("CarList");
        }

        // Helper: Cloudinary public ID
        private string GetCloudinaryPublicId(string imageUrl)
        {
            try
            {
                var uri = new Uri(imageUrl);
                var fileName = uri.Segments.Last();
                return "CarImages/" + fileName.Split('.')[0];
            }
            catch
            {
                return null;
            }
        }
    }
}
