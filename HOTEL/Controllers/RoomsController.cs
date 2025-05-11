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
        public IActionResult Details(decimal id)
        {
            var room = _context.Rooms.Find(id);
            if (room == null)
            {
                return NotFound();
            }

            ViewBag.IsLoggedIn = User.Identity.IsAuthenticated;
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
            ViewBag.MadecimalenanceCount = await _context.Rooms.CountAsync(r => r.Status == "Under Madecimalenance");

            // Get distinct statuses for filter
            ViewBag.Statuses = await _context.Rooms
                .Select(r => r.Status)
                .Distinct()
                .ToListAsync();

            return View(rooms);
        }

        // POST: Rooms/UpdateStatus
        [HttpPost]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateRoomStatusModel model)
        {
            try
            {
                var room = await _context.Rooms.FindAsync(model.Id);
                if (room == null)
                {
                    return NotFound(new { success = false, message = "Room not found" });
                }

                room.Status = model.Status;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Room {room.RoomNumber} status updated to {model.Status}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating room status: " + ex.Message });
            }
        }

        public class UpdateRoomStatusModel
        {
            public int Id { get; set; }
            public string Status { get; set; }
        }

        // GET: Rooms/GetRoomDetails/5
        [HttpGet]
        public async Task<IActionResult> GetRoomDetails(decimal id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }
            return Json(room);
        }

        // POST: Rooms/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoomNumber,Type,Price,Capacity,Description,ImageUrl,Status")] Room room)
        {
            if (ModelState.IsValid)
            {
                try 
                {
                    _context.Add(room);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Room added successfully.";
                    return RedirectToAction(nameof(Admin));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Error creating room: " + ex.Message;
                }
            }
            return RedirectToAction(nameof(Admin));
        }

        // POST: Rooms/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Room room)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(room);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Room updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomExists(room.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Admin));
            }
            return RedirectToAction(nameof(Admin));
        }

        // POST: Rooms/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Room deleted successfully.";
            }
            return RedirectToAction(nameof(Admin));
        }

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.Id == id);
        }
    }
}