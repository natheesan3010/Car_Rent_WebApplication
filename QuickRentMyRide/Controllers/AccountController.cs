using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using QuickRentMyRide.Data;
using QuickRentMyRide.Models;
using System.Linq;
using System;
using Microsoft.AspNetCore.Http.HttpResults;

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

        // 🔐 Password Complexity
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
                ViewBag.Error = "Password must contain Uppercase, Lowercase, Digit, Special char and min 8 length.";
                return View();
            }

            var existingUser = _context.Users.FirstOrDefault(u => u.Gmail_Address == Gmail_Address);
            if (existingUser != null)
            {
                ViewBag.Error = "Email already registered!";
                return View();
            }

            var newUser = new User
            {
                Gmail_Address = Gmail_Address,
                Role = "Customer"
            };

            // ✅ Correct password hash
            newUser.Password = _passwordHasher.HashPassword(newUser, password);

            _context.Users.Add(newUser);
            _context.SaveChanges();

            TempData["Success"] = "Account created successfully! Please login.";
            return RedirectToAction("Login");
        }

        // POST: Login
        [HttpPost]
        public IActionResult Login(string Gmail_Address, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Gmail_Address == Gmail_Address);

            if (user != null)
            {
                // 🔑 Verify password hash
                var result = _passwordHasher.VerifyHashedPassword(user, user.Password, password);

                if (result == PasswordVerificationResult.Success)
                {
                    // ✅ Save session (Guid to string)
                    HttpContext.Session.SetString("CustomerID", user.UserID.ToString());
                    HttpContext.Session.SetString("Gmail_Address", user.Gmail_Address);
                    HttpContext.Session.SetString("Role", user.Role);

                    // ✅ Redirect by role
                    if (user.Role.Equals("Customer", StringComparison.OrdinalIgnoreCase))
                        return RedirectToAction("Dashboard", "Customer");
                    else if (user.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                        return RedirectToAction("Index", "Admin");
                    else if (user.Role.Equals("Staff", StringComparison.OrdinalIgnoreCase))
                        return RedirectToAction("Index", "Staff");

                    return RedirectToAction("Login");
                }
            }

            ViewBag.Error = "Invalid Gmail or Password.";
            return View();
        }


        // GET: Logout
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}
