using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace QuickRentMyRide.Models
{
    public class User
    {
        public Guid UserID { get; set; }  // Primary Key
        [Required]
        public string Gmail_Address { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; }
    }
}
