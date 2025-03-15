namespace HotelWebsite.Models
{
    public class Admin
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty; // Initialize with default value
        public string Password { get; set; } = string.Empty; // Initialize with default value
    }
}