using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using QuickRentMyRide.Data;
using QuickRentMyRide.Models;
using System.Linq;
using System;

namespace QuickRentMyRide.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        // GET: Login
        [HttpGet]
        public IActionResult Login() => View();

        // GET: Register
        [HttpGet]
        public IActionResult Register() => View();

        // Password Complexity
        private bool IsPasswordComplex(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));
            bool hasMinLength = password.Length >= 8;

            return hasUpper && hasLower && hasDigit && hasSpecial && hasMinLength;
        }

        // POST: Register
        [HttpPost]
        public IActionResult Register(string Gmail_Address, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match!";
                return View();
            }

            if (!IsPasswordComplex(password))
            {
                ViewBag.Error = "Password must meet complexity requirements.";
                return View();
            }

            var existingUser = _context.Users.FirstOrDefault(u => u.Gmail_Address == Gmail_Address);
            if (existingUser != null)
            {
                ViewBag.Error = "Username already taken!";
                return View();
            }

            var newUser = new User
            {
                Gmail_Address = Gmail_Address,
                Role = "Customer",
                Password = _passwordHasher.HashPassword(null, password)
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            ViewBag.Success = "Account created successfully! You can now login.";

            // ✅ முக்கியம்: return statement
            return View();
        }



        // POST: Login
        [HttpPost]
        public IActionResult Login(string Gmail_Address, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Gmail_Address == Gmail_Address);

            if (user != null)
            {
                var result = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
                if (result == PasswordVerificationResult.Success)
                {
                    HttpContext.Session.SetString("Gmail_Address", user.Gmail_Address);
                    HttpContext.Session.SetString("Role", user.Role);

                    if (user.Role == "Customer")
                        return RedirectToAction("Dashboard", "Customer");
                    else
                    {
                        ViewBag.Error = "Only customers can log in here.";
                        HttpContext.Session.Clear();
                        return View();
                    }
                }
            }

            ViewBag.Error = "Invalid username or password.";
            return View();
        }

        // Logout
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            return RedirectToAction("Login", "Account");
        }
    }
}
