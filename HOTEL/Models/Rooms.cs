namespace HotelWebsite.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; }
        public string Type { get; set; } // Deluxe, Suite, Standard
        public decimal Price { get; set; }
        public string Status { get; set; } // Occupied, Vacant, Under Maintenance
        public string Description { get; set; }
        public int Capacity { get; set; }
        public string ImageUrl { get; set; }
    }
}