using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using HotelWebsite.Models; // Add this namespace

namespace HotelWebsite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // Rename this method to avoid conflict
        public IActionResult UserManagement()
        {
            var users = _context.Users.ToList();
            return View(users);
        }
    }
}