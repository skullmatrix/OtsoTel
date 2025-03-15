using Microsoft.AspNetCore.Mvc;
using HotelWebsite.Models;
using System.Linq;
using HOTEL.Models;

namespace HotelWebsite.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult UserManagement()
        {
            var users = _context.Users.ToList();
            return View(users);
        }

        [HttpPost]
        public IActionResult EditUser(User user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();
            return RedirectToAction("UserManagement");
        }

        [HttpPost]
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