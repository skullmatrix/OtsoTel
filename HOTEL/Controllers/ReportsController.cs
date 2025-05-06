using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelWebsite.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HotelWebsite.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reports
        public async Task<IActionResult> Index()
        {
            // Check if user is admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrator")
            {
                return RedirectToAction("Index", "Home");
            }

            // Get dashboard statistics
            // 1. Room occupancy rate
            int totalRooms = await _context.Rooms.CountAsync();
            int occupiedRooms = await _context.Rooms.CountAsync(r => r.Status == "Occupied");
            double occupancyRate = totalRooms > 0 ? (double)occupiedRooms / totalRooms * 100 : 0;

            // 2. Total revenue
            decimal totalRevenue = await _context.Payments
                .Where(p => p.Status == "Completed")
                .SumAsync(p => p.Amount);

            // 3. Revenue this month
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            decimal monthlyRevenue = await _context.Payments
                .Where(p => p.Status == "Completed" &&
                       p.PaymentDate.Month == currentMonth &&
                       p.PaymentDate.Year == currentYear)
                .SumAsync(p => p.Amount);

            // 4. Total bookings
            int totalBookings = await _context.Bookings.CountAsync();

            // 5. Bookings this month
            int monthlyBookings = await _context.Bookings
                .Where(b => b.BookingDate.Month == currentMonth &&
                       b.BookingDate.Year == currentYear)
                .CountAsync();

            // Pass data to view
            ViewBag.OccupancyRate = occupancyRate;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.MonthlyRevenue = monthlyRevenue;
            ViewBag.TotalBookings = totalBookings;
            ViewBag.MonthlyBookings = monthlyBookings;
            ViewBag.TotalRooms = totalRooms;
            ViewBag.OccupiedRooms = occupiedRooms;

            return View();
        }

        // GET: Reports/OccupancyReport
        public async Task<IActionResult> OccupancyReport(DateTime? startDate = null, DateTime? endDate = null)
        {
            // Check if user is admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrator")
            {
                return RedirectToAction("Index", "Home");
            }

            // Set default date range if not provided
            if (!startDate.HasValue)
                startDate = DateTime.Now.AddDays(-30);

            if (!endDate.HasValue)
                endDate = DateTime.Now;

            // Get daily occupancy data for the period
            var dailyOccupancy = await _context.Bookings
                .Where(b => b.Status != "Cancelled" &&
                       ((b.CheckInDate <= endDate && b.CheckOutDate >= startDate)))
                .GroupBy(b => new { Date = b.CheckInDate.Date })
                .Select(g => new {
                    Date = g.Key.Date,
                    BookingCount = g.Count()
                })
                .OrderBy(r => r.Date)
                .ToListAsync();

            // Prepare complete date range with data
            List<object> occupancyData = new List<object>();

            for (DateTime date = startDate.Value.Date; date <= endDate.Value.Date; date = date.AddDays(1))
            {
                var dayData = dailyOccupancy.FirstOrDefault(d => d.Date == date);
                occupancyData.Add(new
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    BookingCount = dayData?.BookingCount ?? 0
                });
            }

            // Calculate average occupancy
            int totalRooms = await _context.Rooms.CountAsync();
            double averageOccupancy = 0;

            if (totalRooms > 0 && occupancyData.Count > 0)
            {
                averageOccupancy = occupancyData.Average(d => (int)((dynamic)d).BookingCount) / (double)totalRooms * 100;
            }

            // Room type breakdown
            var roomTypeOccupancy = await _context.Bookings
                .Where(b => b.Status != "Cancelled" &&
                       ((b.CheckInDate <= endDate && b.CheckOutDate >= startDate)))
                .Include(b => b.Room)
                .GroupBy(b => b.Room.Type)
                .Select(g => new {
                    RoomType = g.Key,
                    BookingCount = g.Count()
                })
                .OrderByDescending(r => r.BookingCount)
                .ToListAsync();

            // Pass data to view
            ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");
            ViewBag.OccupancyData = occupancyData;
            ViewBag.AverageOccupancy = averageOccupancy;
            ViewBag.RoomTypeOccupancy = roomTypeOccupancy;
            ViewBag.TotalRooms = totalRooms;

            return View();
        }

        // GET: Reports/RevenueReport
        public async Task<IActionResult> RevenueReport(DateTime? startDate = null, DateTime? endDate = null)
        {
            // Check if user is admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrator")
            {
                return RedirectToAction("Index", "Home");
            }

            // Set default date range if not provided
            if (!startDate.HasValue)
                startDate = DateTime.Now.AddDays(-30);

            if (!endDate.HasValue)
                endDate = DateTime.Now;

            // Get daily revenue data for the period
            var dailyRevenue = await _context.Payments
                .Where(p => p.Status == "Completed" &&
                       p.PaymentDate.Date >= startDate.Value.Date &&
                       p.PaymentDate.Date <= endDate.Value.Date)
                .GroupBy(p => new { Date = p.PaymentDate.Date })
                .Select(g => new {
                    Date = g.Key.Date,
                    Amount = g.Sum(p => p.Amount)
                })
                .OrderBy(r => r.Date)
                .ToListAsync();

            // Prepare complete date range with data
            List<object> revenueData = new List<object>();

            for (DateTime date = startDate.Value.Date; date <= endDate.Value.Date; date = date.AddDays(1))
            {
                var dayData = dailyRevenue.FirstOrDefault(d => d.Date == date);
                revenueData.Add(new
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    Amount = dayData?.Amount ?? 0
                });
            }

            // Calculate total and average daily revenue
            decimal totalRevenue = dailyRevenue.Sum(d => d.Amount);
            decimal averageDailyRevenue = dailyRevenue.Count > 0 ? totalRevenue / dailyRevenue.Count : 0;

            // Revenue by payment method
            var paymentMethodRevenue = await _context.Payments
                .Where(p => p.Status == "Completed" &&
                       p.PaymentDate.Date >= startDate.Value.Date &&
                       p.PaymentDate.Date <= endDate.Value.Date)
                .GroupBy(p => p.PaymentMethod)
                .Select(g => new {
                    PaymentMethod = g.Key,
                    Amount = g.Sum(p => p.Amount)
                })
                .OrderByDescending(r => r.Amount)
                .ToListAsync();

            // Revenue by room type
            var roomTypeRevenue = await _context.BillItems
                .Where(b => b.Category == "Room Charge" &&
                       b.DateAdded.Date >= startDate.Value.Date &&
                       b.DateAdded.Date <= endDate.Value.Date)
                .Join(_context.Bookings,
                      bi => bi.BookingId,
                      b => b.Id,
                      (bi, b) => new { BillItem = bi, Booking = b })
                .Join(_context.Rooms,
                      j => j.Booking.RoomId,
                      r => r.Id,
                      (j, r) => new { BillItem = j.BillItem, RoomType = r.Type })
                .GroupBy(j => j.RoomType)
                .Select(g => new {
                    RoomType = g.Key,
                    Amount = g.Sum(j => j.BillItem.Amount)
                })
                .OrderByDescending(r => r.Amount)
                .ToListAsync();

            // Pass data to view
            ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");
            ViewBag.RevenueData = revenueData;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.AverageDailyRevenue = averageDailyRevenue;
            ViewBag.PaymentMethodRevenue = paymentMethodRevenue;
            ViewBag.RoomTypeRevenue = roomTypeRevenue;

            return View();
        }

        // GET: Reports/GuestDemographics
        public async Task<IActionResult> GuestDemographics()
        {
            // Check if user is admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrator")
            {
                return RedirectToAction("Index", "Home");
            }

            // Get total number of guests
            int totalGuests = await _context.Users.CountAsync(u => u.Role == "Guest");

            // Get guest nationality breakdown
            var nationalityBreakdown = await _context.Users
                .Where(u => u.Role == "Guest" && !string.IsNullOrEmpty(u.Nationality))
                .GroupBy(u => u.Nationality)
                .Select(g => new {
                    Nationality = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(n => n.Count)
                .Take(10)
                .ToListAsync();

            // Get repeat guest count
            var repeatGuestCount = await _context.Bookings
                .GroupBy(b => b.UserId)
                .Where(g => g.Count() > 1)
                .CountAsync();

            // Get loyalty program members
            int loyaltyMembers = await _context.Users
                .CountAsync(u => u.Role == "Guest" && !string.IsNullOrEmpty(u.LoyaltyMembershipId));

            // Get average guest age
            var usersWithAge = await _context.Users
                .Where(u => u.Role == "Guest" && u.DateOfBirth.HasValue)
                .ToListAsync();

            double averageAge = 0;
            if (usersWithAge.Count > 0)
            {
                averageAge = usersWithAge.Average(u => (DateTime.Now - u.DateOfBirth.Value).TotalDays / 365.25);
            }

            // Pass data to view
            ViewBag.TotalGuests = totalGuests;
            ViewBag.NationalityBreakdown = nationalityBreakdown;
            ViewBag.RepeatGuestCount = repeatGuestCount;
            ViewBag.RepeatGuestPercentage = totalGuests > 0 ? (double)repeatGuestCount / totalGuests * 100 : 0;
            ViewBag.LoyaltyMembers = loyaltyMembers;
            ViewBag.LoyaltyPercentage = totalGuests > 0 ? (double)loyaltyMembers / totalGuests * 100 : 0;
            ViewBag.AverageAge = Math.Round(averageAge, 1);

            return View();
        }
    }
}