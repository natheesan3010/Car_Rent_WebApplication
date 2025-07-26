using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using QuickRentMyRide.Data;
using QuickRentMyRide.Models;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

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
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("Username")))
            {
                string role = HttpContext.Session.GetString("Role");
                if (role == "Admin") return RedirectToAction("Index", "Car");
                else return RedirectToAction("CustomerDashboard");
            }

            return View();
        }

        // ------------------- Register ------------------------
        [HttpPost]
        public IActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                if (_db.Users.Any(u => u.Username.ToLower() == user.Username.ToLower()))
                {
                    TempData["msg"] = "Username already taken!";
                    return RedirectToAction("LoginRegister");
                }

                user.Password = HashPassword(user.Password); // Password hashing
                _db.Users.Add(user);
                _db.SaveChanges();

                TempData["msg"] = "Registered successfully!";
            }

            return RedirectToAction("LoginRegister");
        }

        // -------------------- Login ------------------------
        [HttpPost]
        public IActionResult Login(User user)
        {
            string hashedInput = HashPassword(user.Password);

            var match = _db.Users.FirstOrDefault(u =>
                u.Username.ToLower() == user.Username.ToLower() &&
                u.Password == hashedInput);

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

        // ------------------- Forgot Password ------------------------
        [HttpPost]
        public IActionResult ResetPassword(string username, string newPassword)
        {
            var user = _db.Users.FirstOrDefault(u => u.Username.ToLower() == username.ToLower());
            if (user != null)
            {
                user.Password = HashPassword(newPassword);
                _db.SaveChanges();
                TempData["msg"] = "Password reset successful!";
            }
            else
            {
                TempData["msg"] = "Username not found!";
            }

            return RedirectToAction("LoginRegister");
        }

        // ------------------- Customer Dashboard ------------------------
        public IActionResult CustomerDashboard()
        {
            string role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || role != "Customer")
                return RedirectToAction("LoginRegister");

            ViewBag.Name = HttpContext.Session.GetString("Username");
            return View();
        }

        // ------------------- Logout ------------------------
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("LoginRegister");
        }

        // ------------------- Password Hashing Method ------------------------
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
