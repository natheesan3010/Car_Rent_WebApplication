using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickRentMyRide.Models
{
    public class Booking
    {
        [Key]
        public int BookingID { get; set; }

        // ---------------- CAR ----------------
        [Required]
        public int CarID { get; set; }

        [ForeignKey("CarID")]
        public Car Car { get; set; }

        // ---------------- CUSTOMER ----------------
        [Required]
        public int CustomerID { get; set; }

        [ForeignKey("CustomerID")]
        public Customer Customer { get; set; }

        // Optional: CustomerName for quick display
        [Required]
        public string CustomerName { get; set; }

        // ---------------- DATES ----------------
        [DataType(DataType.Date)]
        [Required]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [Required]
        public DateTime EndDate { get; set; }

        // ---------------- PAYMENT ----------------
        public decimal TotalPrice { get; set; }

        public string PaymentStatus { get; set; } = "Pending"; // Pending / Paid / Failed

        // ---------------- BOOKING STATUS ----------------
        public string Status { get; set; } = "Pending"; // Pending / OTPVerified / Approved / Cancelled

        // ---------------- OTP ----------------
        public string OTP { get; set; } // 6-digit OTP for verification

        public DateTime? OTPGeneratedAt { get; set; } // OTP timestamp
    }
}
