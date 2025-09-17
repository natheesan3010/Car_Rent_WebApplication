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
using Microsoft.Extensions.Options;

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

        // ---------------- Car List ----------------
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

        // ---------------- Add Car ----------------
        [HttpGet]
        public IActionResult AddCar() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCar(Car model, IFormFile CarImageFile)
        {
            if (!ModelState.IsValid) return View(model);

            ImageUploadResult uploadResult = null;
            if (CarImageFile != null && CarImageFile.Length > 0)
            {
                uploadResult = await UploadImageToCloudinary(CarImageFile);
            }

            if (uploadResult != null && uploadResult.SecureUrl != null)
            {
                model.CarImage = uploadResult.SecureUrl.ToString();
                model.CarImagePublicId = uploadResult.PublicId;
            }

            _context.Cars.Add(model);
            await _context.SaveChangesAsync();

            var carFromDb = await _context.Cars.FindAsync(model.CarID);
            if (carFromDb != null && !string.IsNullOrEmpty(carFromDb.CarImage))
            {
                TempData["Success"] = "Car added successfully! Cloudinary URL saved: " + carFromDb.CarImage;
            }
            else
            {
                TempData["Error"] = "Car added, but Cloudinary URL not saved!";
            }

            return RedirectToAction("CarList");
        }


        // ---------------- Edit Car ----------------
        [HttpGet]
        public async Task<IActionResult> EditCar(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null) return NotFound();
            return View(car);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCar(Car updatedCar, IFormFile CarImageFile)
        {
            if (!ModelState.IsValid) return View(updatedCar);

            var car = await _context.Cars.FindAsync(updatedCar.CarID);
            if (car == null) return NotFound();

            car.NumberPlate = updatedCar.NumberPlate;
            car.CarBrand = updatedCar.CarBrand;
            car.CarModel = updatedCar.CarModel;
            car.RentPerDay = updatedCar.RentPerDay;
            car.IsAvailable = updatedCar.IsAvailable;

            if (CarImageFile != null && CarImageFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(car.CarImagePublicId))
                {
                    await _cloudinary.DestroyAsync(new DeletionParams(car.CarImagePublicId));
                }

                var uploadResult = await UploadImageToCloudinary(CarImageFile);
                if (uploadResult != null)
                {
                    car.CarImage = uploadResult.SecureUrl.ToString();
                    car.CarImagePublicId = uploadResult.PublicId;
                }
            }

            _context.Cars.Update(car);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Car updated successfully!";
            return RedirectToAction("CarList");
        }

        // ---------------- Delete Car ----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCar(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null)
            {
                TempData["Error"] = "Car not found!";
                return RedirectToAction("CarList");
            }

            if (!string.IsNullOrEmpty(car.CarImagePublicId))
            {
                await _cloudinary.DestroyAsync(new DeletionParams(car.CarImagePublicId));
            }

            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Car deleted successfully!";
            return RedirectToAction("CarList");
        }

        // ---------------- Available Cars ----------------
        public async Task<IActionResult> AvailableCars()
        {
            var cars = await _context.Cars
                .Where(c => c.IsAvailable)
                .OrderByDescending(c => c.CarID)
                .ToListAsync();
            return View(cars);
        }

        // ---------------- Cloudinary Helper ----------------
        private async Task<ImageUploadResult> UploadImageToCloudinary(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "CarImages"
            };
            return await _cloudinary.UploadAsync(uploadParams);
        }
    }
}