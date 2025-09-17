using System.ComponentModel.DataAnnotations;

namespace QuickRentMyRide.Models
{
    public class Car
    {
        public int CarID { get; set; }

        [Required]
        public string NumberPlate { get; set; }

        public string? CarImage { get; set; }

        [Required]
        public bool IsAvailable { get; set; }

        [Required]
        public string CarBrand { get; set; }

        [Required]
        public string CarModel { get; set; }

        [Required]
        public decimal RentPerDay { get; set; }
        public string? CarImagePublicId { get; set; } // <-- ADD THIS PROPERTY


    }

}
