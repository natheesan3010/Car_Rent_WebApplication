using System.ComponentModel.DataAnnotations;

namespace QuickRentMyRide.Models
{
    public class User
    {
        public int UserID { get; set; }

        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Gmail_Address { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public string Role { get; set; } = "Customer";
    }
}
