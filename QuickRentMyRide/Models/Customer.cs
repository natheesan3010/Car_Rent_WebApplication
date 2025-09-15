using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickRentMyRide.Models
{
    public class Customer
    {
        public int CustomerID { get; set; }

        // 🔗 Foreign key to User
        public int UserID { get; set; }

        [ForeignKey("UserID")]
        

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string Username { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public string Email { get; set; }

       

        [Required]
        public string ICNumber { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public DateTime DOB { get; set; }

        // Cloudinary URL save செய்ய இந்த property சேர்க்கவும்
        public string? LicensePhotoPath { get; set; }

        public User User { get; set; }
    }

}
