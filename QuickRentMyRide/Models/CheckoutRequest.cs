namespace QuickRentMyRide.Models
{
    public class CheckoutRequest
    {
        public int CarID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
