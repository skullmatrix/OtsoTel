using Microsoft.AspNetCore.Mvc;
using HotelWebsite.Models;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace HotelWebsite.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Admin Dashboard
        public IActionResult Index()
        {
            // Check if user is admin
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }
            
            return View();
        }

        // GET: /Admin/UserManagement
        public IActionResult UserManagement()
        {
            // Check if user is admin
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }
            
            // Get all users except the current admin
            var currentUserId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
            var users = _context.Users.Where(u => u.Id != currentUserId).ToList();
            
            return View(users);
        }

        // GET: /Admin/EditUser/5
        public IActionResult EditUser(int id)
        {
            // Check if user is admin
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }
            
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }
            
            return View(user);
        }

        // POST: /Admin/EditUser
        [HttpPost]
        public IActionResult EditUser(User user, string NewPassword)
        {
            // Check if user is admin
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }
            
            if (ModelState.IsValid)
            {
                // Get the existing user to keep the password if not changed
                var existingUser = _context.Users.Find(user.Id);
                if (existingUser == null)
                {
                    return NotFound();
                }
                
                // Update user properties
                existingUser.FirstName = user.FirstName;
                existingUser.LastName = user.LastName;
                existingUser.MiddleName = user.MiddleName;
                existingUser.Email = user.Email;
                existingUser.Address = user.Address;
                existingUser.IsAdmin = user.IsAdmin;
                
                // Update photo if provided
                if (!string.IsNullOrEmpty(user.Photo))
                {
                    existingUser.Photo = user.Photo;
                }
                
                // Update password if provided
                if (!string.IsNullOrEmpty(NewPassword))
                {
                    existingUser.Password = BCrypt.Net.BCrypt.HashPassword(NewPassword);
                }
                
                _context.SaveChanges();
                
                // Add success message to TempData
                TempData["SuccessMessage"] = "User updated successfully.";
                
                return RedirectToAction("UserManagement");
            }
            
            return View(user);
        }

        // POST: /Admin/DeleteUser
        [HttpPost]
        public IActionResult DeleteUser(int id)
        {
            // Check if user is admin
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }
            
            var user = _context.Users.Find(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
                
                // Add success message to TempData
                TempData["SuccessMessage"] = "User deleted successfully.";
            }
            
            return RedirectToAction("UserManagement");
        }
        
        // POST: /Admin/AddUser
        [HttpPost]
        public IActionResult AddUser([FromBody] User user)
        {
            // Check if user is admin
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Unauthorized" });
            }
            
            if (user == null)
            {
                return BadRequest(new { success = false, message = "Invalid user data" });
            }
            
            // Check if email already exists
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                return BadRequest(new { success = false, message = "Email already exists" });
            }
            
            // Set default photo if not provided
            if (string.IsNullOrEmpty(user.Photo))
            {
                user.Photo = "https://cdn-icons-png.flaticon.com/256/727/727410.png";
            }
            
            // Hash the password
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            
            // Add the user
            _context.Users.Add(user);
            _context.SaveChanges();
            
            return Json(new { success = true, userId = user.Id });
        }

        // Helper method to check if current user is admin
        private bool IsAdmin()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            return isAdmin == "True";
        }
    }
}