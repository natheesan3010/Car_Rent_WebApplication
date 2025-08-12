using Microsoft.AspNetCore.Identity;  // For password hashing utilities
using QuickRentMyRide.Data;            // Your application's data context
using QuickRentMyRide.Models;          // Your User model
using Microsoft.AspNetCore.Http;       // For session management
using Microsoft.AspNetCore.Mvc;        // MVC controller base classes
using System.Linq;                     // For LINQ queries
using System;                         // For string and char utilities

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

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Helper method to check password complexity
        private bool IsPasswordComplex(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));
            bool hasMinLength = password.Length >= 8;

            return hasUpper && hasLower && hasDigit && hasSpecial && hasMinLength;
        }

        // POST: /Account/Register
        [HttpPost]
        public IActionResult Register(string username, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match!";
                return View();
            }

            if (!IsPasswordComplex(password))
            {
                ViewBag.Error = "Password must be at least 8 characters and include uppercase, lowercase, digit, and special character.";
                return View();
            }

            var existingUser = _context.Users.FirstOrDefault(u => u.Username == username);
            if (existingUser != null)
            {
                ViewBag.Error = "Username already taken!";
                return View();
            }

            var newUser = new User
            {
                Username = username,
                Role = "Customer"
            };

            // Hash password before saving
            newUser.Password = _passwordHasher.HashPassword(newUser, password);

            _context.Users.Add(newUser);
            _context.SaveChanges();

            ViewBag.Success = "Account created successfully! You can now login.";
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (user != null)
            {
                var result = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
                if (result == PasswordVerificationResult.Success)
                {
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("Role", user.Role);

                    if (user.Role == "Customer")
                    {
                        return RedirectToAction("C_Dashboard", "Customer");
                    }
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

        [HttpGet]
        public IActionResult Logout()
        {
            // Session-ஐ முழுவதும் clear பண்ணு
            HttpContext.Session.Clear();

            // Cache disable பண்ணு (browser back button prevent)
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            // Login page-க்கு redirect பண்ணு
            return RedirectToAction("Login", "Account");
        }

    }
}
