using System;
using System.ComponentModel.DataAnnotations;

namespace HotelWebsite.Models
{
    public class BillItem
    {
        public int Id { get; set; }

        public int BookingId { get; set; }
        public virtual Booking Booking { get; set; }

        [Required]
        [Display(Name = "Item Description")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Date Added")]
        public DateTime DateAdded { get; set; } = DateTime.Now;

        [Display(Name = "Category")]
        public string Category { get; set; } = "Room Charge"; // Room Charge, Food & Beverage, Amenities, etc.

        [Display(Name = "Notes")]
        public string Notes { get; set; }
    }
}