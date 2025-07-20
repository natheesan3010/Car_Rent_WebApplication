using System.ComponentModel.DataAnnotations;

namespace QuickRentMyRide.Models
{
    public class Customer
    {
        public int CustomerID { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string LicensePhoto { get; set; }

        [Required]
        public string ICNumber { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public DateTime DOB { get; set; }
    }

}
