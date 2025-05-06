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
    public class HousekeepingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HousekeepingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Housekeeping
        public async Task<IActionResult> Index(string status = null)
        {
            // Check if user is housekeeping or admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrator" && userRole != "Housekeeping")
            {
                return RedirectToAction("Index", "Home");
            }

            IQueryable<HousekeepingTask> tasksQuery = _context.HousekeepingTasks
                .Include(h => h.Room)
                .Include(h => h.AssignedTo);

            // Filter by status if provided
            if (!string.IsNullOrEmpty(status))
            {
                tasksQuery = tasksQuery.Where(h => h.Status == status);
            }

            // Get staff for dropdown list
            ViewBag.Staffs = await _context.Users
                .Where(u => u.Role == "Housekeeping")
                .Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.FullName })
                .ToListAsync();

            // Get statuses for filter
            ViewBag.Statuses = await _context.HousekeepingTasks
                .Select(h => h.Status)
                .Distinct()
                .ToListAsync();

            var tasks = await tasksQuery.OrderBy(h => h.Status == "Pending" ? 0 : 1)
                .ThenBy(h => h.Priority == "Urgent" ? 0 :
                             h.Priority == "High" ? 1 :
                             h.Priority == "Normal" ? 2 : 3)
                .ThenBy(h => h.ScheduledDate)
                .ToListAsync();

            // Get dashboard statistics
            ViewBag.PendingTasks = await _context.HousekeepingTasks.CountAsync(h => h.Status == "Pending");
            ViewBag.InProgressTasks = await _context.HousekeepingTasks.CountAsync(h => h.Status == "In Progress");
            ViewBag.CompletedTasks = await _context.HousekeepingTasks.CountAsync(h => h.Status == "Completed");
            ViewBag.TodayTasks = await _context.HousekeepingTasks.CountAsync(h => h.ScheduledDate.Date == DateTime.Now.Date);

            return View(tasks);
        }

        // GET: Housekeeping/Create
        public async Task<IActionResult> Create()
        {
            // Check if user is housekeeping or admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrator" && userRole != "Housekeeping")
            {
                return RedirectToAction("Index", "Home");
            }

            // Get rooms for dropdown
            ViewBag.Rooms = await _context.Rooms
                .Select(r => new SelectListItem { Value = r.Id.ToString(), Text = $"{r.Type} Room - {r.RoomNumber}" })
                .ToListAsync();

            // Get staff for dropdown
            ViewBag.Staffs = await _context.Users
                .Where(u => u.Role == "Housekeeping")
                .Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.FullName })
                .ToListAsync();

            return View();
        }

        // POST: Housekeeping/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HousekeepingTask housekeepingTask)
        {
            // Check if user is housekeeping or admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrator" && userRole != "Housekeeping")
            {
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                _context.Add(housekeepingTask);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            // Get rooms for dropdown
            ViewBag.Rooms = await _context.Rooms
                .Select(r => new SelectListItem { Value = r.Id.ToString(), Text = $"{r.Type} Room - {r.RoomNumber}" })
                .ToListAsync();

            // Get staff for dropdown
            ViewBag.Staffs = await _context.Users
                .Where(u => u.Role == "Housekeeping")
                .Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.FullName })
                .ToListAsync();

            return View(housekeepingTask);
        }

        // POST: Housekeeping/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            // Check if user is housekeeping or admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrator" && userRole != "Housekeeping")
            {
                return RedirectToAction("Index", "Home");
            }

            var task = await _context.HousekeepingTasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            task.Status = status;

            // If status is completed, set the completion date
            if (status == "Completed" || status == "Verified")
            {
                task.CompletedDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: Housekeeping/Assign
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(int id, int assignedToUserId)
        {
            // Check if user is housekeeping or admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrator" && userRole != "Housekeeping")
            {
                return RedirectToAction("Index", "Home");
            }

            var task = await _context.HousekeepingTasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            task.AssignedToUserId = assignedToUserId;
            task.Status = "In Progress";

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}