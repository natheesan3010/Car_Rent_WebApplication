using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickRentMyRide.Data;
using QuickRentMyRide.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QuickRentMyRide.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 10; // Items per page

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Customer/Index?search=abc&page=2
        public async Task<IActionResult> Index(string search, int page = 1)
        {
            var query = _context.Customers.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c =>
                    c.FirstName.Contains(search) ||
                    c.LastName.Contains(search) ||
                    c.Email.Contains(search));
            }

            int totalRecords = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalRecords / (double)PageSize);

            var customers = await query
                .OrderBy(c => c.FirstName)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.Search = search;

            return View(customers);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Customer customer, IFormFile photo)
        {
            if (ModelState.IsValid)
            {
                if (photo != null)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(photo.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await photo.CopyToAsync(stream);
                    }
                    customer.LicensePhoto = "/images/" + fileName;
                }

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Customer created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Customer customer, IFormFile photo)
        {
            if (ModelState.IsValid)
            {
                var existingCustomer = await _context.Customers.FindAsync(customer.CustomerID);
                if (existingCustomer == null) return NotFound();

                // Update properties
                existingCustomer.FirstName = customer.FirstName;
                existingCustomer.LastName = customer.LastName;
                existingCustomer.PhoneNumber = customer.PhoneNumber;
                existingCustomer.Email = customer.Email;
                existingCustomer.ICNumber = customer.ICNumber;
                existingCustomer.Gender = customer.Gender;
                existingCustomer.DOB = customer.DOB;
                existingCustomer.Address = customer.Address;

                if (photo != null)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(photo.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await photo.CopyToAsync(stream);
                    }
                    existingCustomer.LicensePhoto = "/images/" + fileName;
                }

                _context.Customers.Update(existingCustomer);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Customer updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        [HttpPost]  // Use POST for delete actions
        public async Task<IActionResult> Delete(int customerId)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                TempData["Error"] = "Customer not found.";
                return RedirectToAction(nameof(Index));
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Customer deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
