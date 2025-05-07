using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace HotelWebsite.Models
{
    public class Booking
    {
        public int Id { get; set; }

        public int RoomId { get; set; }
        public virtual Room? Room { get; set; }

        public int UserId { get; set; }
        public virtual User? User { get; set; }

        [Required]
        [Display(Name = "Check-in Date")]
        [DataType(DataType.Date)]
        public DateTime CheckInDate { get; set; }

        [Required]
        [Display(Name = "Check-out Date")]
        [DataType(DataType.Date)]
        public DateTime CheckOutDate { get; set; }

        [Required]
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Checked In, Checked Out, Cancelled

        public int NumberOfGuests { get; set; }

        [Display(Name = "Special Requests")]
        public string SpecialRequests { get; set; } = "";

        public DateTime BookingDate { get; set; } = DateTime.Now;

        // Total price calculation
        [Display(Name = "Total Price")]
        public decimal TotalPrice { get; set; }

        // Additional fields for check-in/out processing
        public DateTime? ActualCheckInDate { get; set; }
        public DateTime? ActualCheckOutDate { get; set; }

        [Display(Name = "Checked In By")]
        public int? CheckedInById { get; set; } // Changed from CheckedInByUserId to match database
        
        [ForeignKey("CheckedInById")]
        public virtual User? CheckedInBy { get; set; }

        [Display(Name = "Checked Out By")]
        public int? CheckedOutById { get; set; } // Changed from CheckedOutByUserId to match database
        
        [ForeignKey("CheckedOutById")]
        public virtual User? CheckedOutBy { get; set; }

        [Display(Name = "ID Verification")]
        public string IdVerification { get; set; } = "Pending Verification";

        [Display(Name = "Payment Status")]
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Partial, Paid, Refunded

        // Navigation properties
        public virtual ICollection<BillItem> BillItems { get; set; } = new List<BillItem>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

        // Calculated properties
        [Display(Name = "Total Bill")]
        public decimal TotalBill => BillItems?.Sum(b => b.Amount) ?? 0;

        [Display(Name = "Total Paid")]
        public decimal TotalPaid => Payments?.Where(p => p.Status == "Completed").Sum(p => p.Amount) ?? 0;

        [Display(Name = "Balance")]
        public decimal Balance => TotalBill - TotalPaid;

        public DateTime CreatedAt { get; set; }
    }
}