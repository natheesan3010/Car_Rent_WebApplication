using Microsoft.AspNetCore.Mvc;
using QuickRentMyRide.Data;
using QuickRentMyRide.Models;

namespace QuickRentMyRide.Controllers
{
    public class CarController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CarController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult CarList(string search, int page = 1, int pageSize = 5)
        {
            var query = _context.Cars.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.NumberPlate.Contains(search) || c.CarBrand.Contains(search) || c.CarModel.Contains(search));
            }

            var totalCars = query.Count();
            var cars = query
                        .OrderByDescending(c => c.CarID)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();

            ViewBag.Search = search;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCars / pageSize);

            return View(cars);
        }

        [HttpGet]
        public IActionResult EditCar(int id)
        {
            var car = _context.Cars.Find(id);
            return View(car);
        }

        [HttpPost]
        public IActionResult EditCar(Car updatedCar, IFormFile CarImageFile)
        {
            var car = _context.Cars.Find(updatedCar.CarID);

            if (car != null)
            {
                car.NumberPlate = updatedCar.NumberPlate;
                car.CarBrand = updatedCar.CarBrand;
                car.CarModel = updatedCar.CarModel;
                car.RentPerDay = updatedCar.RentPerDay;
                car.IsAvailable = updatedCar.IsAvailable;

                if (CarImageFile != null)
                {
                    string fileName = Path.GetFileName(CarImageFile.FileName);
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        CarImageFile.CopyTo(stream);
                    }
                    car.CarImage = fileName;
                }

                _context.SaveChanges();
            }

            return RedirectToAction("CarList");
        }

        public IActionResult DeleteCar(int id)
        {
            var car = _context.Cars.Find(id);
            if (car != null)
            {
                _context.Cars.Remove(car);
                _context.SaveChanges();
            }

            return RedirectToAction("CarList");
        }

    }

}
