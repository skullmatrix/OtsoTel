using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelWebsite.Models;
using System.Linq;
using System.Threading.Tasks;

namespace HotelWebsite.Controllers
{
    public class RoomsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RoomsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Rooms
        public async Task<IActionResult> Index(string type = null)
        {
            // Show error or success messages if any
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }

            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }

            IQueryable<Room> rooms = _context.Rooms.Where(r => r.Status == "Vacant");

            // Filter by room type if specified
            if (!string.IsNullOrEmpty(type))
            {
                rooms = rooms.Where(r => r.Type == type);
            }

            // Get distinct room types for the filter
            ViewBag.RoomTypes = await _context.Rooms
                .Select(r => r.Type)
                .Distinct()
                .ToListAsync();

            return View(await rooms.ToListAsync());
        }

        // GET: Rooms/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        // GET: Rooms/Admin
        public async Task<IActionResult> Admin(string status = null)
        {
            // Check if user is admin
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "True";
            if (!isAdmin)
            {
                return RedirectToAction("Index", "Home");
            }

            IQueryable<Room> roomsQuery = _context.Rooms;

            // Filter by status if provided
            if (!string.IsNullOrEmpty(status))
            {
                roomsQuery = roomsQuery.Where(r => r.Status == status);
            }

            var rooms = await roomsQuery.ToListAsync();

            // Get room status counts for dashboard
            ViewBag.VacantCount = await _context.Rooms.CountAsync(r => r.Status == "Vacant");
            ViewBag.OccupiedCount = await _context.Rooms.CountAsync(r => r.Status == "Occupied");
            ViewBag.MaintenanceCount = await _context.Rooms.CountAsync(r => r.Status == "Under Maintenance");

            // Get distinct statuses for filter
            ViewBag.Statuses = await _context.Rooms
                .Select(r => r.Status)
                .Distinct()
                .ToListAsync();

            return View(rooms);
        }

        // POST: Rooms/UpdateStatus
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            // Check if user is admin
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "True";
            if (!isAdmin)
            {
                return RedirectToAction("Index", "Home");
            }

            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            room.Status = status;
            _context.Update(room);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Admin));
        }
    }
}