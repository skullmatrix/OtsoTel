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
            // Check user role
            var userRole = HttpContext.Session.GetString("UserRole");

            // Redirect based on role
            if (userRole == "FrontDesk")
            {
                return RedirectToAction("Index", "FrontDesk");
            }
            else if (userRole == "Administrator")
            {
                return RedirectToAction("Index", "Admin");
            }
            else if (userRole == "Housekeeping")
            {
                return RedirectToAction("Index", "Housekeeping");
            }

            // Default view for guests and non-logged in users
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