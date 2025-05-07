namespace HotelWebsite.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? MiddleName { get; set; } // Make middle name optional
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Photo { get; set; } = string.Empty;
        public bool IsAdmin { get; set; } = false;
        public string Role { get; set; } = "Guest"; // Guest, Administrator, FrontDesk, Housekeeping, Maintenance
        
        // Additional guest information fields
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Nationality { get; set; }
        public string? IdType { get; set; } // Passport, National ID, Driver's License
        public string? IdNumber { get; set; }
        public string? PreferredLanguage { get; set; }
        public string? LoyaltyMembershipId { get; set; }
        public int? LoyaltyPoints { get; set; }
        public string? Notes { get; set; }
        public string? PreferredPaymentMethod { get; set; }
        
        // Calculated full name property
        public string FullName => $"{FirstName} {(string.IsNullOrEmpty(MiddleName) ? "" : MiddleName + " ")}{LastName}";
    }
}