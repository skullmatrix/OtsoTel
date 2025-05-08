namespace HotelWebsite.Models.ViewModels
{
    public class GuestViewModel
    {
        public User User { get; set; }
        public Booking CurrentBooking { get; set; }
        public Booking UpcomingBooking { get; set; }
        public DateTime? CheckedInDate { get; set; }
        public string RoomNumber { get; set; } = string.Empty; // Initialize with empty string to avoid null references
        
        // Helper properties for the view
        public bool IsCurrentlyStaying => CurrentBooking != null;
        public bool HasUpcomingBooking => UpcomingBooking != null;
        public string CheckInDateFormatted => CheckedInDate.HasValue ? CheckedInDate.Value.ToString("MMM dd, yyyy h:mm tt") : "Not checked in";
        public string Status => 
            IsCurrentlyStaying ? "Checked In" :
            HasUpcomingBooking ? "Confirmed" : "No Active Booking";
    }
}