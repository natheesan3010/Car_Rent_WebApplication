namespace QuickRentMyRide.Models
{
    public class Maintenance
    {
        public Guid MaintenanceID { get; set; }  // Primary Key
        public string Description { get; set; }
        public double Cost { get; set; }
        public DateTime ServiceDate { get; set; }

        public int CarID { get; set; }  // Foreign Key
    }
}
