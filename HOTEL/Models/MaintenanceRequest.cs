using System;
using System.ComponentModel.DataAnnotations;

namespace HotelWebsite.Models
{
    public class MaintenanceRequest
    {
        public int Id { get; set; }

        public int RoomId { get; set; }
        public virtual Room Room { get; set; }

        [Required]
        [Display(Name = "Request Type")]
        public string RequestType { get; set; } // Plumbing, Electrical, Furniture, etc.

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending"; // Pending, In Progress, Completed, Verified

        [Required]
        [Display(Name = "Reported Date")]
        public DateTime ReportedDate { get; set; } = DateTime.Now;

        public DateTime? CompletedDate { get; set; }

        [Display(Name = "Reported By")]
        public int? ReportedByUserId { get; set; }
        public virtual User ReportedBy { get; set; }

        [Display(Name = "Assigned To")]
        public int? AssignedToUserId { get; set; }
        public virtual User AssignedTo { get; set; }

        [Display(Name = "Notes")]
        public string Notes { get; set; }

        [Display(Name = "Priority")]
        public string Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent
    }
}