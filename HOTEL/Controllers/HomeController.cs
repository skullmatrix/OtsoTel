using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using HotelWebsite.Models; // Add this namespace

namespace HotelWebsite.Controllers
{
    public class HomeController : Controller
    {
        // Static list to store user data (replace with a database in production)
        private static List<User> _users = new List<User>();

        public IActionResult Index()
        {
            return View();
        }

        // Action to save user data from Google Sign-In
        [HttpPost]
        public IActionResult SaveUser([FromBody] User user)
        {
            _users.Add(user); // Add the user to the list
            return Json(new { success = true }); // Return success response
        }

        // Action to display the User Management page
        public IActionResult User()
        {
            return View(_users); // Pass the list of users to the view
        }
    }
}