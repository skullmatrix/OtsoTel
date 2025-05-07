using Microsoft.AspNetCore.Mvc;
using HotelWebsite.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;

namespace HotelWebsite.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/
        public async Task<IActionResult> Index()
{
    // Check if user is admin
    var userRole = HttpContext.Session.GetString("UserRole");
    if (userRole != "Administrator")
    {
        return RedirectToAction("Index", "Home");
    }

    try 
    {
        // Get room status counts for dashboard
        int totalRooms = await _context.Rooms.CountAsync();
        
        // Count rooms by status
        int occupiedRooms = await _context.Rooms.CountAsync(r => r.Status == "Occupied");
        int bookedRooms = await _context.Rooms.CountAsync(r => r.Status == "Booked");
        int vacantRooms = await _context.Rooms.CountAsync(r => r.Status == "Vacant");
        int maintenanceRooms = await _context.Rooms.CountAsync(r => r.Status == "Under Maintenance");
        int cleaningRooms = await _context.Rooms.CountAsync(r => r.Status == "Cleaning");
        
        // Calculate occupancy rate - count both occupied and booked rooms as occupied
        double occupancyRate = totalRooms > 0 ? (double)(occupiedRooms + bookedRooms) / totalRooms * 100 : 0;
        
        // Calculate occupancy rate from previous week
        var oneWeekAgo = DateTime.Now.AddDays(-7);
        var previousBookings = await _context.Bookings
            .Where(b => b.BookingDate <= oneWeekAgo && 
                        (b.Status == "Confirmed" || b.Status == "Checked In"))
            .CountAsync();
        
        // Calculate percentage change
        double previousOccupancyRate = totalRooms > 0 ? (double)previousBookings / totalRooms * 100 : 0;
        double occupancyChange = occupancyRate - previousOccupancyRate;

        ViewBag.TotalRooms = totalRooms;
        ViewBag.OccupiedRooms = occupiedRooms;
        ViewBag.BookedRooms = bookedRooms;
        ViewBag.VacantRooms = vacantRooms;
        ViewBag.MaintenanceRooms = maintenanceRooms;
        ViewBag.CleaningRooms = cleaningRooms;
        ViewBag.OccupancyRate = occupancyRate;
        ViewBag.OccupancyChange = occupancyChange;

        // Get staff count
        var activeStaff = await _context.Users
            .Where(u => u.Role != "Guest")
            .CountAsync();
        ViewBag.ActiveStaff = activeStaff;

        // Calculate today's and yesterday's revenue
        var today = DateTime.Today;
        var todayRevenue = await _context.Payments
            .Where(p => p.PaymentDate.Date == today && p.Status == "Completed")
            .SumAsync(p => p.Amount);

        var yesterday = today.AddDays(-1);
        var yesterdayRevenue = await _context.Payments
            .Where(p => p.PaymentDate.Date == yesterday && p.Status == "Completed")
            .SumAsync(p => p.Amount);

        ViewBag.TodayRevenue = todayRevenue;
        ViewBag.YesterdayRevenue = yesterdayRevenue;
        ViewBag.RevenueChange = todayRevenue - yesterdayRevenue;

        // Get recent bookings
        var recentBookings = await _context.Bookings
            .Include(b => b.Room)
            .Include(b => b.User)
            .OrderByDescending(b => b.BookingDate)
            .Take(5)
            .ToListAsync();
        ViewBag.RecentBookings = recentBookings;

        // Get recent activities
        var recentActivities = new List<dynamic>();

        // Recent check-ins
        var recentCheckIns = await _context.Bookings
            .Include(b => b.Room)
            .Include(b => b.CheckedInBy)
            .Where(b => b.Status == "Checked In" && b.ActualCheckInDate.HasValue)
            .OrderByDescending(b => b.ActualCheckInDate)
            .Take(1)
            .Select(b => new {
                Type = "check-in",
                Icon = "fas fa-user-check text-success",
                Title = "New check-in",
                Message = $"Room {b.Room.RoomNumber} checked in by {(b.CheckedInBy != null ? b.CheckedInBy.FirstName : "staff")}",
                Time = b.ActualCheckInDate.Value,
                TimeAgo = GetTimeAgo(b.ActualCheckInDate.Value)
            })
            .ToListAsync();
        recentActivities.AddRange(recentCheckIns);

        // Recent payments
        var recentPayments = await _context.Payments
            .Include(p => p.Booking)
            .Where(p => p.Status == "Completed")
            .OrderByDescending(p => p.PaymentDate)
            .Take(1)
            .Select(p => new {
                Type = "payment",
                Icon = "fas fa-credit-card text-warning",
                Title = "Payment received",
                Message = $"${p.Amount} for booking #{p.BookingId}",
                Time = p.PaymentDate,
                TimeAgo = GetTimeAgo(p.PaymentDate)
            })
            .ToListAsync();
        recentActivities.AddRange(recentPayments);

        // Recent room cleaning
        var recentCleanings = await _context.HousekeepingTasks
            .Include(h => h.Room)
            .Where(h => h.Status == "Completed" && h.CompletedDate.HasValue)
            .OrderByDescending(h => h.CompletedDate)
            .Take(1)
            .Select(h => new {
                Type = "cleaning",
                Icon = "fas fa-broom text-primary",
                Title = "Room cleaning",
                Message = $"Room {h.Room.RoomNumber} marked as cleaned",
                Time = h.CompletedDate.Value,
                TimeAgo = GetTimeAgo(h.CompletedDate.Value)
            })
            .ToListAsync();
        recentActivities.AddRange(recentCleanings);

        // Recent maintenance requests
        var recentMaintenance = await _context.MaintenanceRequests
            .Include(m => m.Room)
            .OrderByDescending(m => m.ReportedDate)  // Changed from CreatedAt to ReportedDate
            .Take(1)
            .Select(m => new {
                Type = "maintenance",
                Icon = "fas fa-tools text-danger",
                Title = "Maintenance request",
                Message = $"Room {m.Room.RoomNumber} - {m.Description}",
                Time = m.ReportedDate,  // Changed from CreatedAt to ReportedDate
                TimeAgo = GetTimeAgo(m.ReportedDate)  // Changed from CreatedAt to ReportedDate
            })
            .ToListAsync();
        recentActivities.AddRange(recentMaintenance);

        // Sort by most recent first and take top 4
        var sortedActivities = recentActivities
            .OrderByDescending(a => ((dynamic)a).Time)
            .Take(4)
            .ToList();

        ViewBag.RecentActivities = sortedActivities;

        return View();
    }
    catch (Exception ex)
    {
        // Provide fallback data if any queries fail
        ViewBag.ErrorMessage = "Error loading dashboard data: " + ex.Message;
        return View();
    }
}

// Helper method to get time ago string
private string GetTimeAgo(DateTime dateTime)
{
    var timeSpan = DateTime.Now - dateTime;
    
    if (timeSpan.TotalMinutes < 1)
        return "just now";
    if (timeSpan.TotalMinutes < 60)
        return $"{(int)timeSpan.TotalMinutes} minutes ago";
    if (timeSpan.TotalHours < 24)
        return $"{(int)timeSpan.TotalHours} hours ago";
    if (timeSpan.TotalDays < 7)
        return $"{(int)timeSpan.TotalDays} days ago";
    
    return dateTime.ToString("MMM dd, yyyy");
}

        // GET: /Admin/UserManagement
        public IActionResult UserManagement()
        {
            // Exclude the admin user from the list
            var users = _context.Users.Where(u => !u.IsAdmin).ToList();
            return View(users);
        }

        // GET: /Admin/EditUser/5
        public IActionResult EditUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: /Admin/EditUser/5
        [HttpPost]
        public IActionResult EditUser(User user)
        {
            if (ModelState.IsValid)
            {
                _context.Users.Update(user);
                _context.SaveChanges();
                return RedirectToAction("UserManagement");
            }
            return View(user);
        }

        // GET: /Admin/DeleteUser/5
        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
            return RedirectToAction("UserManagement");
        }
    }
}