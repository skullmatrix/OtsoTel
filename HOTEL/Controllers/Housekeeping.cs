using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelWebsite.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HotelWebsite.Controllers
{
    public class HousekeepingDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HousekeepingDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: HousekeepingDashboard
        // GET: /Admin/
        public async Task<IActionResult> Index()
        {
            // Check if user is admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrator" && userRole != "FrontDesk")
            {
                return RedirectToAction("Index", "Home");
            }

            // Get real-time dashboard statistics
            // 1. Room occupancy statistics
            int totalRooms = await _context.Rooms.CountAsync();
            int occupiedRooms = await _context.Rooms.CountAsync(r => r.Status == "Occupied");
            int vacantRooms = await _context.Rooms.CountAsync(r => r.Status == "Vacant");
            int maintenanceRooms = await _context.Rooms.CountAsync(r => r.Status == "Under Maintenance");
            double occupancyRate = totalRooms > 0 ? (double)occupiedRooms / totalRooms * 100 : 0;

            // 2. Staff on duty (simplified)
            int activeStaff = 12; // In a real implementation, this would come from a staff scheduling system

            // 3. Today's revenue
            var today = DateTime.Now.Date;
            decimal todayRevenue = await _context.Payments
                .Where(p => p.Status == "Completed" && p.PaymentDate.Date == today)
                .SumAsync(p => p.Amount);

            // 4. Yesterday's revenue for comparison
            var yesterday = today.AddDays(-1);
            decimal yesterdayRevenue = await _context.Payments
                .Where(p => p.Status == "Completed" && p.PaymentDate.Date == yesterday)
                .SumAsync(p => p.Amount);

            // 5. Recent bookings
            var recentBookings = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .OrderByDescending(b => b.BookingDate)
                .Take(3)
                .ToListAsync();

            // 6. Recent activity
            // This would be more dynamic in a real implementation with a proper activity log table

            // Pass data to view
            ViewBag.OccupancyRate = occupancyRate;
            ViewBag.TotalRooms = totalRooms;
            ViewBag.OccupiedRooms = occupiedRooms;
            ViewBag.VacantRooms = vacantRooms;
            ViewBag.MaintenanceRooms = maintenanceRooms;
            ViewBag.ActiveStaff = activeStaff;
            ViewBag.TodayRevenue = todayRevenue;
            ViewBag.YesterdayRevenue = yesterdayRevenue;
            ViewBag.RecentBookings = recentBookings;

            return View();
        }

        // GET: HousekeepingDashboard/CleaningSchedule
        public async Task<IActionResult> CleaningSchedule()
        {
            // Check if user is housekeeping or admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrator" && userRole != "Housekeeping")
            {
                return RedirectToAction("Index", "Home");
            }

            // Redirect to the Housekeeping controller's index method
            return RedirectToAction("Index", "Housekeeping");
        }

        // GET: HousekeepingDashboard/Maintenance
        public async Task<IActionResult> Maintenance()
        {
            // Check if user is housekeeping or admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrator" && userRole != "Housekeeping")
            {
                return RedirectToAction("Index", "Home");
            }

            // Redirect to the Maintenance controller's index method
            return RedirectToAction("Index", "Maintenance");
        }
    }
}