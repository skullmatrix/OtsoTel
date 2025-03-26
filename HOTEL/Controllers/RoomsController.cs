using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelWebsite.Models;
using Microsoft.AspNetCore.Authorization;

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
        public async Task<IActionResult> Index()
        {
            var rooms = await _context.Rooms.Where(r => r.Status == "Vacant").ToListAsync();
            return View(rooms);
        }

        // GET: Rooms/Admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Admin()
        {
            var rooms = await _context.Rooms.ToListAsync();
            return View(rooms);
        }

        // POST: Rooms/UpdateStatus
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
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