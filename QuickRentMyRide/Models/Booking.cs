using QuickRentMyRide.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace QuickRentMyRide.Models
{
    public class Booking
    {
        public int BookingID { get; set; }
        public int CustomerID { get; set; }
        public Customer Customer { get; set; }
        [Required]
        public int CarID { get; set; }

        [Required]
        public string UserID { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public decimal TotalPrice { get; set; }

        public virtual Car Car { get; set; }
    }
}
