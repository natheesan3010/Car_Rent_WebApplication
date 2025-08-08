using Microsoft.AspNetCore.Mvc;
using QuickRentMyRide.Data;
using QuickRentMyRide.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace QuickRentMyRide.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user != null)
            {
                // Store session data
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Role", user.Role);

                // Check role
                if (user.Role == "Customer")
                {
                    return RedirectToAction("C_Dashboard", "Customer");
                }
                else
                {
                    ViewBag.Error = "Only customers can log in here.";
                    HttpContext.Session.Clear(); // remove any partial login
                    return View();
                }
            }

            // Invalid login
            ViewBag.Error = "Invalid username or password.";
            return View();
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string username, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match!";
                return View();
            }

            // Check if username exists
            var existingUser = _context.Users.FirstOrDefault(u => u.Username == username);
            if (existingUser != null)
            {
                ViewBag.Error = "Username already taken!";
                return View();
            }

            // Save to database
            var newUser = new User
            {
                Username = username,
                Password = password, // ⚠️ Later we should hash this
                Role = "Customer"
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            ViewBag.Success = "Account created successfully! You can now login.";
            return View();
        }

    }
}
