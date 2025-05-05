// RoomsController.cs
using Microsoft.AspNetCore.Mvc;
using HotelWebsite.Models;
using System.Linq;

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
        public IActionResult Index()
        {
            return View(_context.Rooms.ToList());
        }

        // GET: Rooms/Admin
        public IActionResult Admin()
        {
            return View(_context.Rooms.ToList());
        }
    }
}