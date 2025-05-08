using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelWebsite.Models;
using HotelWebsite.Models.ViewModels;
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

            // Get today's check-outs
            var todayCheckOuts = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .Where(b => b.CheckOutDate.Date == today && b.Status == "Checked In")
                .OrderBy(b => b.CheckOutDate)
                .ToListAsync();

            // Set ViewBag properties
            ViewBag.CheckOuts = todayCheckOuts;
            ViewBag.CheckOutsCount = todayCheckOuts.Count;
            ViewBag.AlreadyCheckedOut = await _context.Bookings
                .CountAsync(b => b.CheckOutDate.Date == today && b.Status == "Checked Out");

            // Get today's check-ins (both expected and already checked in)
            var todayCheckIns = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .Where(b => b.CheckInDate.Date == today)
                .OrderBy(b => b.CheckInDate)
                .ToListAsync();

            var expectedCheckIns = todayCheckIns.Where(b => b.Status == "Confirmed").ToList();
            var alreadyCheckedIn = todayCheckIns.Where(b => b.Status == "Checked In").ToList();

            // Get all guests currently staying at the hotel
            var currentGuests = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .Where(b => b.Status == "Checked In" && 
                        b.CheckInDate.Date <= today && 
                        b.CheckOutDate.Date >= today)
                .OrderBy(b => b.CheckOutDate)
                .ToListAsync();

            // Get pending bookings that need confirmation (cash payments)
            var pendingBookings = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .Include(b => b.Payments)
                .Where(b => b.Status == "Pending")
                .OrderBy(b => b.CheckInDate)
                .ToListAsync();

            // Get room availability summary
            var roomsByStatus = await _context.Rooms
                .GroupBy(r => r.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var vacantRooms = roomsByStatus.FirstOrDefault(r => r.Status == "Vacant")?.Count ?? 0;
            var occupiedRooms = roomsByStatus.FirstOrDefault(r => r.Status == "Occupied")?.Count ?? 0;
            var bookedRooms = roomsByStatus.FirstOrDefault(r => r.Status == "Booked")?.Count ?? 0;
            var maintenanceRooms = roomsByStatus.FirstOrDefault(r => r.Status == "Under Maintenance")?.Count ?? 0;
            var cleaningRooms = roomsByStatus.FirstOrDefault(r => r.Status == "Cleaning")?.Count ?? 0;

            // Get all available rooms
            var availableRooms = await _context.Rooms
                .Where(r => r.Status == "Vacant")
                .OrderBy(r => r.RoomNumber)
                .ToListAsync();

            // Set ViewBag properties
            ViewBag.ExpectedCheckIns = expectedCheckIns;
            ViewBag.AlreadyCheckedIn = alreadyCheckedIn;
            ViewBag.CheckInsCount = expectedCheckIns.Count;
            ViewBag.CheckedInCount = alreadyCheckedIn.Count;

            ViewBag.CurrentGuests = currentGuests;
            ViewBag.CurrentGuestsCount = currentGuests.Count;

            ViewBag.PendingBookings = pendingBookings;
            ViewBag.PendingBookingsCount = pendingBookings.Count;

            ViewBag.VacantRooms = vacantRooms;
            ViewBag.OccupiedRooms = occupiedRooms;
            ViewBag.BookedRooms = bookedRooms;
            ViewBag.MaintenanceRooms = maintenanceRooms;
            ViewBag.CleaningRooms = cleaningRooms;
            ViewBag.AvailableRooms = availableRooms;
            ViewBag.AvailableRoomsCount = availableRooms.Count;

            // Add this line to set ViewBag.CheckIns
            ViewBag.CheckIns = expectedCheckIns;

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

            // Get all guests with current or upcoming bookings
            var today = DateTime.Today;
            var guestsWithBookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room)
                .Where(b => b.CheckOutDate.Date >= today && 
                        (b.Status == "Confirmed" || b.Status == "Checked In"))
                .OrderBy(b => b.User.LastName)
                .ThenBy(b => b.User.FirstName)
                .ToListAsync();

            // Group by user to avoid duplicates
            var groupedGuests = guestsWithBookings
            .GroupBy(b => b.UserId)
            .Select(g => new GuestViewModel
            {
                User = g.First().User,
                CurrentBooking = g.Where(b => b.Status == "Checked In" && 
                                            b.CheckInDate.Date <= today && 
                                            b.CheckOutDate.Date >= today)
                                .FirstOrDefault(),
                UpcomingBooking = g.Where(b => b.Status == "Confirmed" && b.CheckInDate.Date > today)
                                .OrderBy(b => b.CheckInDate)
                                .FirstOrDefault(),
                CheckedInDate = g.Where(b => b.Status == "Checked In")
                                .Select(b => b.ActualCheckInDate)
                                .FirstOrDefault(),
                RoomNumber = g.Where(b => b.Status == "Checked In" && b.Room != null)
                            .Select(b => b.Room.RoomNumber)
                            .FirstOrDefault() ?? string.Empty
            })
            .ToList();

            return View(groupedGuests);
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

            // Get all rooms with their current status and occupant info
            var rooms = await _context.Rooms
                .OrderBy(r => r.RoomNumber)
                .ToListAsync();

            // Get current occupied rooms with guest info
            var occupiedRoomsWithGuests = await _context.Bookings
                .Include(b => b.User)
                .Where(b => b.Status == "Checked In")
                .ToDictionaryAsync(b => b.RoomId, b => new { 
                    GuestName = b.User.FullName,
                    CheckInDate = b.ActualCheckInDate,
                    CheckOutDate = b.CheckOutDate
                });

            // Get upcoming bookings for rooms
            var upcomingBookings = await _context.Bookings
                .Include(b => b.User)
                .Where(b => b.Status == "Confirmed" && b.CheckInDate.Date >= DateTime.Today)
                .OrderBy(b => b.CheckInDate)
                .GroupBy(b => b.RoomId)
                .ToDictionaryAsync(g => g.Key, g => g.First());

            var roomStatusList = rooms.Select(r => new RoomStatusViewModel
            {
                Room = r,
                CurrentGuest = occupiedRoomsWithGuests.ContainsKey(r.Id) 
                    ? occupiedRoomsWithGuests[r.Id].GuestName 
                    : null,
                CheckInDate = occupiedRoomsWithGuests.ContainsKey(r.Id) 
                    ? occupiedRoomsWithGuests[r.Id].CheckInDate 
                    : null,
                CheckOutDate = occupiedRoomsWithGuests.ContainsKey(r.Id) 
                    ? occupiedRoomsWithGuests[r.Id].CheckOutDate 
                    : null,
                NextBooking = upcomingBookings.ContainsKey(r.Id) 
                    ? upcomingBookings[r.Id] 
                    : null
            }).ToList();

            return View(roomStatusList);
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