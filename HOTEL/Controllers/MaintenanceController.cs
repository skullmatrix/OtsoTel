using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HotelWebsite.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HotelWebsite.Controllers
{
    public class MaintenanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MaintenanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Maintenance
        public async Task<IActionResult> Index(string status = null)
        {
            // Check if user is maintenance staff or admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrator" && userRole != "Maintenance")
            {
                return RedirectToAction("Index", "Home");
            }

            IQueryable<MaintenanceRequest> requestsQuery = _context.MaintenanceRequests
                .Include(m => m.Room)
                .Include(m => m.ReportedBy)
                .Include(m => m.AssignedTo);

            // Filter by status if provided
            if (!string.IsNullOrEmpty(status))
            {
                requestsQuery = requestsQuery.Where(m => m.Status == status);
            }

            // Get staff for dropdown list
            ViewBag.Staffs = await _context.Users
                .Where(u => u.Role == "Maintenance")
                .Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.FullName })
                .ToListAsync();

            // Get statuses for filter
            ViewBag.Statuses = await _context.MaintenanceRequests
                .Select(m => m.Status)
                .Distinct()
                .ToListAsync();

            var requests = await requestsQuery.OrderBy(m => m.Status == "Pending" ? 0 : 1)
                .ThenBy(m => m.Priority == "Urgent" ? 0 :
                             m.Priority == "High" ? 1 :
                             m.Priority == "Normal" ? 2 : 3)
                .ThenBy(m => m.ReportedDate)
                .ToListAsync();

            // Get dashboard statistics
            ViewBag.PendingRequests = await _context.MaintenanceRequests.CountAsync(m => m.Status == "Pending");
            ViewBag.InProgressRequests = await _context.MaintenanceRequests.CountAsync(m => m.Status == "In Progress");
            ViewBag.CompletedRequests = await _context.MaintenanceRequests.CountAsync(m => m.Status == "Completed");
            ViewBag.UrgentRequests = await _context.MaintenanceRequests.CountAsync(m => m.Priority == "Urgent" && m.Status != "Completed" && m.Status != "Verified");

            return View(requests);
        }

        // GET: Maintenance/Create
        public async Task<IActionResult> Create()
        {
            // Check if user is staff or admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "Guest")
            {
                return RedirectToAction("Index", "Home");
            }

            // Get rooms for dropdown
            ViewBag.Rooms = await _context.Rooms
                .Select(r => new SelectListItem { Value = r.Id.ToString(), Text = $"{r.Type} Room - {r.RoomNumber}" })
                .ToListAsync();

            // Get staff for dropdown
            ViewBag.Staffs = await _context.Users
                .Where(u => u.Role == "Maintenance")
                .Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.FullName })
                .ToListAsync();

            return View();
        }

        // POST: Maintenance/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MaintenanceRequest maintenanceRequest)
        {
            // Check if user is staff or admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "Guest")
            {
                return RedirectToAction("Index", "Home");
            }

            var userId = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userId))
            {
                maintenanceRequest.ReportedByUserId = int.Parse(userId);
            }

            if (ModelState.IsValid)
            {
                _context.Add(maintenanceRequest);
                await _context.SaveChangesAsync();

                // If the room is marked for maintenance, update the room status
                if (maintenanceRequest.Priority == "Urgent" || maintenanceRequest.Priority == "High")
                {
                    var room = await _context.Rooms.FindAsync(maintenanceRequest.RoomId);
                    if (room != null && room.Status != "Occupied")
                    {
                        room.Status = "Under Maintenance";
                        await _context.SaveChangesAsync();
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            // Get rooms for dropdown
            ViewBag.Rooms = await _context.Rooms
                .Select(r => new SelectListItem { Value = r.Id.ToString(), Text = $"{r.Type} Room - {r.RoomNumber}" })
                .ToListAsync();

            // Get staff for dropdown
            ViewBag.Staffs = await _context.Users
                .Where(u => u.Role == "Maintenance")
                .Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.FullName })
                .ToListAsync();

            return View(maintenanceRequest);
        }

        // POST: Maintenance/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            // Check if user is maintenance staff or admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrator" && userRole != "Maintenance")
            {
                return RedirectToAction("Index", "Home");
            }

            var request = await _context.MaintenanceRequests
                .Include(m => m.Room)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (request == null)
            {
                return NotFound();
            }

            request.Status = status;

            // If status is completed, set the completion date
            if (status == "Completed" || status == "Verified")
            {
                request.CompletedDate = DateTime.Now;

                // If the room was under maintenance, update it back to vacant
                if (request.Room.Status == "Under Maintenance")
                {
                    request.Room.Status = "Vacant";
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: Maintenance/Assign
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(int id, int assignedToUserId)
        {
            // Check if user is maintenance staff or admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrator" && userRole != "Maintenance")
            {
                return RedirectToAction("Index", "Home");
            }

            var request = await _context.MaintenanceRequests.FindAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            request.AssignedToUserId = assignedToUserId;
            request.Status = "In Progress";

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}