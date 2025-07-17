using System.ComponentModel.DataAnnotations;

namespace QuickRentMyRide.Models
{
    public class User
    {
        public Guid UserID { get; set; }  // Primary Key
        public string Role { get; set; }  // admin / staff / customer
        public string Username { get; set; }
        public string Password { get; set; }

    }
}
