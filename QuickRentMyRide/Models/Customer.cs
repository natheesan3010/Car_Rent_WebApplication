namespace QuickRentMyRide.Models
{
    public class Customer
    {
        public Guid CustomerID { get; set; }  // Primary Key
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string LicensePhoto { get; set; }
        public string ICNumber { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public DateTime DOB { get; set; }
    }
}
