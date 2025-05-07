using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelWebsite.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HotelWebsite.Controllers
{
    public class FrontDeskController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FrontDeskController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: FrontDesk
            public async Task<IActionResult> Index()
            {
                // Check if user is front desk or admin
                var userRole = HttpContext.Session.GetString("UserRole");
                if (userRole != "Administrator" && userRole != "FrontDesk")
                {
                    return RedirectToAction("Index", "Home");
                }

                var currentUser = HttpContext.Session.GetString("UserName");
                ViewBag.CurrentUser = currentUser;

                var today = DateTime.Today;

                // Get check-ins for today
                var checkIns = await _context.Bookings
                    .Include(b => b.Room)
                    .Include(b => b.User)
                    .Where(b => b.CheckInDate.Date == today && 
                        (b.Status == "Confirmed" || b.Status == "Pending"))
                    .ToListAsync();

                // Get already checked in count
                var alreadyCheckedIn = await _context.Bookings
                    .CountAsync(b => b.CheckInDate.Date == today && b.Status == "Checked In");

                // Get check-outs for today
                var checkOuts = await _context.Bookings
                    .Include(b => b.Room)
                    .Include(b => b.User)
                    .Where(b => b.CheckOutDate.Date == today && b.Status == "Checked In")
                    .ToListAsync();

                // Get already checked out count
                var alreadyCheckedOut = await _context.Bookings
                    .CountAsync(b => b.CheckOutDate.Date == today && b.Status == "Checked Out" && 
                            b.ActualCheckOutDate.HasValue && b.ActualCheckOutDate.Value.Date == today);

                // Get all room statuses for the chart
                var vacantRooms = await _context.Rooms.CountAsync(r => r.Status == "Vacant");
                var occupiedRooms = await _context.Rooms.CountAsync(r => r.Status == "Occupied" || r.Status == "Booked");
                var maintenanceRooms = await _context.Rooms.CountAsync(r => r.Status == "Under Maintenance");

                // Pass all data to view
                ViewBag.CheckIns = checkIns;
                ViewBag.CheckInsCount = checkIns.Count;
                ViewBag.AlreadyCheckedIn = alreadyCheckedIn;

                ViewBag.CheckOuts = checkOuts;
                ViewBag.CheckOutsCount = checkOuts.Count;
                ViewBag.AlreadyCheckedOut = alreadyCheckedOut;

                // For the room chart
                ViewBag.VacantRooms = vacantRooms;
                ViewBag.OccupiedRooms = occupiedRooms;
                ViewBag.MaintenanceRooms = maintenanceRooms;

                // Get recent bookings (last 10)
                var recentBookings = await _context.Bookings
                    .Include(b => b.Room)
                    .Include(b => b.User)
                    .OrderByDescending(b => b.BookingDate)
                    .Take(10)
                    .ToListAsync();
                
                ViewBag.RecentBookings = recentBookings;

                return View();
            }

        // GET: FrontDesk/CheckInOut
        public IActionResult CheckInOut()
        {
            return RedirectToAction("FrontDesk", "CheckInOut");
        }

        // GET: FrontDesk/Guests
        public async Task<IActionResult> Guests()
        {
            // Check if user is front desk or admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrator" && userRole != "FrontDesk")
            {
                return RedirectToAction("Index", "Home");
            }

            var guests = await _context.Users
                .Where(u => u.Role == "Guest")
                .ToListAsync();

            return View(guests);
        }

        // GET: FrontDesk/RoomStatus
        public async Task<IActionResult> RoomStatus()
        {
            // Check if user is front desk or admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrator" && userRole != "FrontDesk")
            {
                return RedirectToAction("Index", "Home");
            }

            var rooms = await _context.Rooms.ToListAsync();
            return View(rooms);
        }

        // GET: FrontDesk/ConfirmPayment/5
        public async Task<IActionResult> ConfirmPayment(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.Id == id);
                
            if (booking == null)
            {
                return NotFound();
            }
            
            return View(booking);
        }

        // POST: FrontDesk/ConfirmPayment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPayment(int id, string receiptNumber)
        {
            var booking = await _context.Bookings
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.Id == id);
                
            if (booking == null)
            {
                return NotFound();
            }
            
            // Update booking status
            booking.Status = "Confirmed";
            
            // Update payment status
            var cashPayment = booking.Payments
                .FirstOrDefault(p => p.PaymentMethod.Contains("Cash") && p.Status == "Pending");
                
            if (cashPayment != null)
            {
                cashPayment.Status = "Completed";
                cashPayment.TransactionReference = receiptNumber;
                cashPayment.Notes += $" | Confirmed by {HttpContext.Session.GetString("UserName")} on {DateTime.Now}";
            }
            
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        // GET: FrontDesk/PendingBookings
public async Task<IActionResult> PendingBookings()
{
    // Check if user is front desk or admin
    var userRole = HttpContext.Session.GetString("UserRole");
    if (userRole != "Administrator" && userRole != "FrontDesk")
    {
        return RedirectToAction("Index", "Home");
    }

    // Get all pending bookings
    var pendingBookings = await _context.Bookings
        .Include(b => b.Room)
        .Include(b => b.User)
        .Include(b => b.Payments)
        .Where(b => b.Status == "Pending")
        .OrderBy(b => b.CheckInDate)
        .ToListAsync();

    return View(pendingBookings);
}

// GET: FrontDesk/ConfirmBooking/5
public async Task<IActionResult> ConfirmBooking(int id)
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
        .Include(b => b.Payments)
        .FirstOrDefaultAsync(b => b.Id == id);

    if (booking == null)
    {
        return NotFound();
    }

    return View(booking);
}

// POST: FrontDesk/ConfirmBooking/5
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ConfirmBooking(int id, string confirmationNotes)
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
        .Include(b => b.Payments)
        .FirstOrDefaultAsync(b => b.Id == id);

    if (booking == null)
    {
        return NotFound();
    }

    // Update booking status to confirmed
    booking.Status = "Confirmed";
    
    // Update payment status for any pending payments
    var pendingPayment = booking.Payments.FirstOrDefault(p => p.Status == "Pending");
    if (pendingPayment != null)
    {
        pendingPayment.Status = "Completed";
        pendingPayment.Notes += $" | Confirmed by staff ID {userId} on {DateTime.Now}";
        
        if (!string.IsNullOrEmpty(confirmationNotes))
        {
            pendingPayment.Notes += $" | Notes: {confirmationNotes}";
        }
    }

    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "Booking has been confirmed successfully.";
    return RedirectToAction("PendingBookings");
}
    }
}