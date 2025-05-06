using System;
using System.ComponentModel.DataAnnotations;

namespace HotelWebsite.Models
{
    public class HousekeepingTask
    {
        public int Id { get; set; }

        public int RoomId { get; set; }
        public virtual Room Room { get; set; }

        [Required]
        [Display(Name = "Task Type")]
        public string TaskType { get; set; } // Regular Cleaning, Deep Cleaning, Turndown Service, etc.

        [Required]
        [Display(Name = "Task Status")]
        public string Status { get; set; } = "Pending"; // Pending, In Progress, Completed, Verified

        [Required]
        [Display(Name = "Scheduled For")]
        public DateTime ScheduledDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        [Display(Name = "Assigned To")]
        public int? AssignedToUserId { get; set; }
        public virtual User AssignedTo { get; set; }

        [Display(Name = "Notes")]
        public string Notes { get; set; }

        [Display(Name = "Priority")]
        public string Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent
    }
}