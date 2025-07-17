namespace QuickRentMyRide.Models
{
    public class Car
    {
        public Guid CarID { get; set; }  // Primary Key
        public string NumberPlate { get; set; }
        public string CarImage { get; set; }
        public bool IsAvailable { get; set; }
        public string CarBrand { get; set; }
        public string CarModel { get; set; }
        public double RentPerDay { get; set; }

        public int AdminID { get; set; }  // Foreign Key

    }
}
