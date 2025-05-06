using System;
using System.ComponentModel.DataAnnotations;

namespace HotelWebsite.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public int BookingId { get; set; }
        public virtual Booking Booking { get; set; }

        [Required]
        [Display(Name = "Payment Amount")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Payment Date")]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } // Credit Card, Debit Card, Cash, Bank Transfer, etc.

        [Display(Name = "Transaction Reference")]
        public string TransactionReference { get; set; }

        [Display(Name = "Payment Status")]
        public string Status { get; set; } = "Pending"; // Pending, Completed, Failed, Refunded

        [Display(Name = "Notes")]
        public string Notes { get; set; }
    }
}