namespace QuickRentMyRide.Models
{
    public class Admin
    {
        public Guid AdminID { get; set; }  // Primary Key
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string ICNumber { get; set; }
    }
}
