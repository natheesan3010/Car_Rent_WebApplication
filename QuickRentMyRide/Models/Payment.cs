namespace QuickRentMyRide.Models
{
    public class Payment
    {
        public Guid PaymentID { get; set; }  // Primary Key
        public DateTime PaymentDate { get; set; }
        public double Amount { get; set; }
        public string Model { get; set; }

        public int CustomerID { get; set; }  // Foreign Key
        public int BookingID { get; set; }   // Foreign Key
    }
}
