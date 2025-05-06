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
                .Where(b => b.CheckInDate.Date == today && b.Status == "Confirmed")
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
                .CountAsync(b => b.CheckOutDate.Date == today && b.Status == "Checked Out" && b.ActualCheckOutDate.HasValue && b.ActualCheckOutDate.Value.Date == today);

            // Get available rooms
            var availableRooms = await _context.Rooms
                .Where(r => r.Status == "Vacant")
                .ToListAsync();

            // Count premium and standard rooms
            var premiumCount = availableRooms.Count(r => r.Type == "Suite" || r.Type == "Deluxe");
            var standardCount = availableRooms.Count(r => r.Type == "Standard");

            // Pass data to view
            ViewBag.CheckIns = checkIns;
            ViewBag.CheckInsCount = checkIns.Count;
            ViewBag.AlreadyCheckedIn = alreadyCheckedIn;

            ViewBag.CheckOuts = checkOuts;
            ViewBag.CheckOutsCount = checkOuts.Count;
            ViewBag.AlreadyCheckedOut = alreadyCheckedOut;

            ViewBag.AvailableRooms = availableRooms;
            ViewBag.AvailableRoomsCount = availableRooms.Count;
            ViewBag.PremiumCount = premiumCount;
            ViewBag.StandardCount = standardCount;

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
    }
}