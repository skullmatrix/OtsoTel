namespace HotelWebsite.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? MiddleName { get; set; } // Make MiddleName optional
        public string Email { get; set; }
        public string Password { get; set; }
        public string Address { get; set; }
        public string Photo { get; set; } // For Google Sign-In
    }
}