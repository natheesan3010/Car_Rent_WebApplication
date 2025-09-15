using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using QuickRentMyRide.Data;
using QuickRentMyRide.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.Linq;
using System.Threading.Tasks;

namespace QuickRentMyRide.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly Cloudinary _cloudinary;

        public AccountController(ApplicationDbContext context, IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();

            var cloudSettings = cloudinaryConfig.Value;
            var account = new Account(cloudSettings.CloudName, cloudSettings.ApiKey, cloudSettings.ApiSecret);
            _cloudinary = new Cloudinary(account);
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpGet]
        public IActionResult Login() => View();

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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Customer customer, string password, string confirmPassword, IFormFile LicensePhotoPath)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                ViewBag.Error = "ModelState invalid: " + errors;
                return View(customer);
            }

            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match!";
                return View(customer);
            }

            if (!IsPasswordComplex(password))
            {
                ViewBag.Error = "Password must be at least 8 characters long, contain upper, lower, digit, and special character.";
                return View(customer);
            }

            if (_context.Users.Any(u => u.Gmail_Address == customer.Email))
            {
                ViewBag.Error = "Email already registered!";
                return View(customer);
            }

            if (_context.Users.Any(u => u.Username == customer.Username))
            {
                ViewBag.Error = "Username already exists!";
                return View(customer);
            }

            // Cloudinary upload
            if (LicensePhotoPath != null)
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(LicensePhotoPath.FileName, LicensePhotoPath.OpenReadStream()),
                    Folder = "licenses"
                };
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                customer.LicensePhotoPath = uploadResult.SecureUrl.AbsoluteUri;
            }

            // Save User
            var newUser = new User
            {
                Gmail_Address = customer.Email,
                Username = customer.Username,
                Role = "Customer"
            };
            newUser.PasswordHash = _passwordHasher.HashPassword(newUser, password);
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Save Customer
            customer.UserID = newUser.UserID;
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Account created successfully! Please login.";
            return RedirectToAction("Login");
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string Gmail_Address, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Gmail_Address == Gmail_Address);

            if (user != null)
            {
                var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
                if (result == PasswordVerificationResult.Success)
                {
                    // Set UserID session
                    HttpContext.Session.SetInt32("UserID", user.UserID);
                    HttpContext.Session.SetString("Gmail_Address", user.Gmail_Address);
                    HttpContext.Session.SetString("Role", user.Role);

                    // ✅ Set CustomerID session if user is a Customer
                    if (user.Role.ToLower() == "customer")
                    {
                        var customer = _context.Customers.FirstOrDefault(c => c.UserID == user.UserID);
                        if (customer != null)
                            HttpContext.Session.SetInt32("CustomerID", customer.CustomerID);
                    }

                    return user.Role.ToLower() switch
                    {
                        "customer" => RedirectToAction("Dashboard", "Customer"),
                        "admin" => RedirectToAction("Index", "Admin"),
                        "staff" => RedirectToAction("Index", "Staff"),
                        _ => RedirectToAction("Login")
                    };
                }
            }

            ViewBag.Error = "Invalid Gmail or Password.";
            return View();
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}
