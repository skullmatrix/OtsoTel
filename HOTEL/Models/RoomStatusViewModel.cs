namespace HotelWebsite.Models.ViewModels
{
    public class RoomStatusViewModel
    {
        public Room Room { get; set; }
        public string CurrentGuest { get; set; } = string.Empty; // Initialize with empty string
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public Booking NextBooking { get; set; }
        
        // Helper properties for the view
        public bool IsOccupied => Room.Status == "Occupied";
        public bool IsBooked => Room.Status == "Booked";
        public bool IsVacant => Room.Status == "Vacant";
        public bool IsUnderMaintenance => Room.Status == "Under Maintenance";
        public bool IsCleaning => Room.Status == "Cleaning";
        
        public string StatusColor => 
            IsOccupied ? "success" :
            IsBooked ? "info" :
            IsVacant ? "primary" :
            IsUnderMaintenance ? "warning" :
            IsCleaning ? "danger" : "secondary";
            
        public string CheckInDateFormatted => CheckInDate.HasValue ? CheckInDate.Value.ToString("MMM dd, yyyy") : "";
        public string CheckOutDateFormatted => CheckOutDate.HasValue ? CheckOutDate.Value.ToString("MMM dd, yyyy") : "";
        
        public bool HasNextBooking => NextBooking != null;
        public string NextBookingDateFormatted => HasNextBooking ? NextBooking.CheckInDate.ToString("MMM dd, yyyy") : "";
        public string NextGuestName => HasNextBooking ? NextBooking.User?.FullName ?? "Unknown Guest" : "";
    }
}