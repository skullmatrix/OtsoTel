using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using HotelWebsite.Models;

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

            // Get available rooms for the hotel carousel
            var rooms = _context.Rooms
                .Where(r => r.Status == "Vacant")  // Only show available rooms
                .OrderBy(r => r.Price)  // Order by price (cheapest first)
                .Take(3)  // Limit to 3 rooms initially
                .ToList();

            // Create a view model to pass to the view
            var viewModel = new HomeViewModel
            {
                Rooms = rooms
            };

            return View(viewModel);
        }   

        // Rename this method to avoid conflict
        public IActionResult UserManagement()
        {
            var users = _context.Users.ToList();
            return View(users);
        }
    }
}