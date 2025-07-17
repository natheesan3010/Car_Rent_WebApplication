namespace QuickRentMyRide.Models
{
    public class Booking
    {
        public Guid BookingID { get; set; }  // Primary Key
        public DateTime PickupDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public double TotalCost { get; set; }

        public int CustomerID { get; set; }  // Foreign Key
        public int CarID { get; set; }       // Foreign Key

    }
}
