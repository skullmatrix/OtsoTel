using Microsoft.AspNetCore.Mvc;
using HOTEL.Models;

namespace HOTEL.Controllers
{
    public class RoomsController : Controller
    {
        public IActionResult Index()
        {
            var rooms = new List<Room>
            {
                new Room { Id = 1, Name = "Deluxe Room", ImageUrl = "/images/deluxe.jpg", Price = 25000, IsAvailable = true },
                new Room { Id = 2, Name = "Luxury Suite", ImageUrl = "/images/luxury.jpg", Price = 50000, IsAvailable = false },
                new Room { Id = 3, Name = "Family Suite", ImageUrl = "/images/family.jpg", Price = 35000, IsAvailable = true }
            };

            return View(rooms);
        }
    }
}
