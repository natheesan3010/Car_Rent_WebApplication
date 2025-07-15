using Microsoft.EntityFrameworkCore;
using QuickRentMyRide.Models;
namespace QuickRentMyRide.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
