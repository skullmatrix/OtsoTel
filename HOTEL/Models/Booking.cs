using System;
using System.ComponentModel.DataAnnotations;

namespace HotelWebsite.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required]
        public int RoomId { get; set; }
        public Room? Room { get; set; }

        [Required]
        public string GuestName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string GuestEmail { get; set; } = string.Empty;

        [Required, Phone]
        public string GuestPhone { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Check-In Date")]
        public DateTime CheckInDate { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Check-Out Date")]
        public DateTime CheckOutDate { get; set; } = DateTime.Today.AddDays(1);

        [Required, Range(1, 10)]
        [Display(Name = "Number of Guests")]
        public int NumberOfGuests { get; set; } = 1;

        [Display(Name = "Special Requests")]
        public string? SpecialRequests { get; set; }

        public DateTime BookingDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Confirmed";
    }
}