using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelWebsite.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HotelWebsite.Controllers
{
    public class CheckInOutController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CheckInOutController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CheckInOut/FrontDesk
        public async Task<IActionResult> FrontDesk()
        {
            // Check if user is front desk or admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrator" && userRole != "FrontDesk")
            {
                return RedirectToAction("Index", "Home");
            }

            var todayDate = DateTime.Now.Date;

            // Get expected check-ins (booked for today but not checked in yet)
            var expectedCheckIns = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .Where(b => b.CheckInDate.Date == todayDate && b.Status == "Confirmed")
                .OrderBy(b => b.CheckInDate)
                .ToListAsync();

            // Get expected check-outs (checked in and due to check out today)
            var expectedCheckOuts = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .Where(b => b.CheckOutDate.Date == todayDate && b.Status == "Checked In")
                .OrderBy(b => b.CheckOutDate)
                .ToListAsync();

            // Get recently checked in guests (checked in today)
            var recentCheckIns = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .Where(b => b.ActualCheckInDate.HasValue && b.ActualCheckInDate.Value.Date == todayDate)
                .OrderByDescending(b => b.ActualCheckInDate)
                .ToListAsync();

            // Get recently checked out guests (checked out today)
            var recentCheckOuts = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .Where(b => b.ActualCheckOutDate.HasValue && b.ActualCheckOutDate.Value.Date == todayDate)
                .OrderByDescending(b => b.ActualCheckOutDate)
                .ToListAsync();

            ViewBag.ExpectedCheckIns = expectedCheckIns;
            ViewBag.ExpectedCheckOuts = expectedCheckOuts;
            ViewBag.RecentCheckIns = recentCheckIns;
            ViewBag.RecentCheckOuts = recentCheckOuts;

            return View();
        }

        // GET: CheckInOut/CheckIn/5
        public async Task<IActionResult> CheckIn(int id)
        {
            // Check if user is front desk or admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrator" && userRole != "FrontDesk")
            {
                return RedirectToAction("Index", "Home");
            }

            var booking = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: CheckInOut/CheckIn/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(int id, string idVerification)
        {
            // Check if user is front desk or admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrator" && userRole != "FrontDesk")
            {
                return RedirectToAction("Index", "Home");
            }

            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index", "Home");
            }

            var booking = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            // Update booking status
            booking.Status = "Checked In";
            booking.ActualCheckInDate = DateTime.Now;
            booking.CheckedInById = int.Parse(userId);
            booking.IdVerification = idVerification;

            // Update room status to Occupied
            booking.Room.Status = "Occupied";

            // Create bill item for the room charge if not already done
            var existingBillItem = await _context.BillItems
                .FirstOrDefaultAsync(b => b.BookingId == booking.Id && b.Category == "Room Charge");

            if (existingBillItem == null)
            {
                var roomCharge = new BillItem
                {
                    BookingId = booking.Id,
                    Description = $"{booking.Room.Type} Room - {booking.Room.RoomNumber}",
                    Amount = booking.TotalPrice,
                    Category = "Room Charge",
                    DateAdded = DateTime.Now
                };

                _context.BillItems.Add(roomCharge);
            }

            // Schedule housekeeping task for checkout
            var housekeepingTask = new HousekeepingTask
            {
                RoomId = booking.RoomId,
                TaskType = "Regular Cleaning",
                Status = "Pending",
                ScheduledDate = booking.CheckOutDate,
                Notes = $"Scheduled cleaning after checkout for room {booking.Room.RoomNumber}",
                Priority = "Normal"
            };

            _context.HousekeepingTasks.Add(housekeepingTask);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Guest {booking.User.FullName} has been checked in to Room {booking.Room.RoomNumber}.";
            return RedirectToAction("FrontDesk");
        }

        // POST: CheckInOut/CheckOut/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut(int id, string paymentMethod, string transactionReference, decimal paymentAmount)
        {
            // Check if user is front desk or admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrator" && userRole != "FrontDesk")
            {
                return RedirectToAction("Index", "Home");
            }

            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index", "Home");
            }

            var booking = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .Include(b => b.BillItems)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            // Check if there is an outstanding balance
            var totalBill = booking.BillItems.Sum(b => b.Amount);
            var totalPaid = booking.Payments.Where(p => p.Status == "Completed").Sum(p => p.Amount);
            var balance = totalBill - totalPaid;

            // If payment amount is provided, record the payment
            if (paymentAmount > 0)
            {
                var payment = new Payment
                {
                    BookingId = booking.Id,
                    Amount = paymentAmount,
                    PaymentDate = DateTime.Now,
                    PaymentMethod = paymentMethod,
                    TransactionReference = transactionReference,
                    Status = "Completed",
                    Notes = "Payment collected at checkout"
                };

                _context.Payments.Add(payment);

                // Update balance after new payment
                totalPaid += paymentAmount;
                balance = totalBill - totalPaid;
            }

            // Update payment status based on balance
            if (balance <= 0)
            {
                booking.PaymentStatus = "Paid";
            }
            else if (totalPaid > 0)
            {
                booking.PaymentStatus = "Partial";
            }

            // Update booking status
            booking.Status = "Checked Out";
            booking.ActualCheckOutDate = DateTime.Now;
            booking.CheckedOutById = int.Parse(userId);

            // Update room status to Cleaning (not directly to Vacant)
            booking.Room.Status = "Cleaning";

            // Create immediate housekeeping task
            var housekeepingTask = new HousekeepingTask
            {
                RoomId = booking.RoomId,
                TaskType = "Regular Cleaning",
                Status = "Pending",
                ScheduledDate = DateTime.Now,
                Notes = $"Room {booking.Room.RoomNumber} requires cleaning after checkout",
                Priority = "High"
            };

            _context.HousekeepingTasks.Add(housekeepingTask);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Guest {booking.User.FullName} has been checked out from Room {booking.Room.RoomNumber}.";
            return RedirectToAction("Invoice", "Billing", new { id = booking.Id });
        }

        // GET: CheckInOut/CheckOut/5
        public async Task<IActionResult> CheckOut(int id)
        {
            // Check if user is front desk or admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrator" && userRole != "FrontDesk")
            {
                return RedirectToAction("Index", "Home");
            }

            var booking = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .Include(b => b.BillItems)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

    }
}