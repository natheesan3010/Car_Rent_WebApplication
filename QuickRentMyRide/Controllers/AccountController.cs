using Microsoft.AspNetCore.Mvc;
using QuickRentMyRide.Models;
using QuickRentMyRide.Data;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace QuickRentMyRide.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AccountController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: Login & Register Page
        public IActionResult LoginRegister()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                if (_db.Users.Any(u => u.Username == user.Username))
                {
                    TempData["msg"] = "Username already taken!";
                    return RedirectToAction("LoginRegister");
                }

                _db.Users.Add(user);
                _db.SaveChanges();
                TempData["msg"] = "Registered successfully!";
            }

            return RedirectToAction("LoginRegister");
        }

        [HttpPost]
        public IActionResult Login(User user)
        {
            var match = _db.Users.FirstOrDefault(u =>
                u.Username == user.Username && u.Password == user.Password);

            if (match != null)
            {
                HttpContext.Session.SetString("Username", match.Username);
                HttpContext.Session.SetString("Role", match.Role);

                if (match.Role == "Admin")
                    return RedirectToAction("Index", "Car");
                else
                    return RedirectToAction("CustomerDashboard");
            }

            TempData["msg"] = "Invalid username or password!";
            return RedirectToAction("LoginRegister");
        }

        public IActionResult CustomerDashboard()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("Username")))
                return RedirectToAction("LoginRegister");

            ViewBag.Name = HttpContext.Session.GetString("Username");
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("LoginRegister");
        }
    }
}
