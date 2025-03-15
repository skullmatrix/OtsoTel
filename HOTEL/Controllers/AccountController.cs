using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http; // Add this for session management
using HotelWebsite.Models;
using System.Linq;

namespace HotelWebsite.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Account/SignUp
        public IActionResult SignUp()
        {
            return View();
        }

        // POST: /Account/SignUp
        [HttpPost]
        public IActionResult SignUp(User user)
        {
            if (ModelState.IsValid)
            {
                // Save the new user to the database
                _context.Users.Add(user);
                _context.SaveChanges();

                // Redirect to the home page and open the login modal
                TempData["ShowLoginModal"] = true; // Use TempData to trigger the modal
                return RedirectToAction("Index", "Home");
            }

            // If the model is invalid, return to the sign-up page with validation errors
            return View(user);
        }

        [HttpPost]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Email == request.Email && u.Password == request.Password);
                if (user != null)
                {
                    // Store user data in session
                    this.HttpContext.Session.SetString("UserId", user.Id.ToString());
                    this.HttpContext.Session.SetString("UserName", $"{user.FirstName} {user.LastName}"); // Combine first and last name
                    this.HttpContext.Session.SetString("UserEmail", user.Email);
                    this.HttpContext.Session.SetString("UserPhoto", user.Photo);

                    // Check if the user is an admin
                    bool isAdmin = user.Email == "admin@hotel.com" && user.Password == "admin123";

                    return Json(new { success = true, isAdmin });
                }
                return Json(new { success = false, message = "Invalid email or password." });
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                Console.Error.WriteLine($"Error during login: {ex}");

                // Return a JSON response with the error message
                return StatusCode(500, new { success = false, message = "An error occurred during login. Please try again." });
            }
        }

        [HttpPost]
        public IActionResult UpdateProfile(User updatedUser)
        {
            var userId = this.HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index", "Home"); // Redirect if not logged in
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == int.Parse(userId));
            if (user == null)
            {
                return RedirectToAction("Index", "Home"); // Redirect if user not found
            }

            // Update user details
            user.FirstName = updatedUser.FirstName;
            user.LastName = updatedUser.LastName;
            user.MiddleName = updatedUser.MiddleName;
            user.Email = updatedUser.Email;
            user.Address = updatedUser.Address;
            user.Photo = updatedUser.Photo;
            _context.SaveChanges();

            // Update session data
            this.HttpContext.Session.SetString("UserName", $"{user.FirstName} {user.LastName}"); // Combine first and last name
            this.HttpContext.Session.SetString("UserPhoto", user.Photo);

            return RedirectToAction("UserProfile");
        }

        public IActionResult UserProfile()
        {
            var userId = this.HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index", "Home"); // Redirect if not logged in
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == int.Parse(userId));
            if (user == null)
            {
                return RedirectToAction("Index", "Home"); // Redirect if user not found
            }

            return View(user);
        }

        [HttpPost]
        public IActionResult SaveGoogleUser([FromBody] User user)
        {
            if (ModelState.IsValid)
            {
                // Check if the user already exists
                var existingUser = _context.Users.FirstOrDefault(u => u.Email == user.Email);
                if (existingUser == null)
                {
                    // Save the new user
                    _context.Users.Add(user);
                    _context.SaveChanges();
                }

                // Store user data in session
                this.HttpContext.Session.SetString("UserId", user.Id.ToString());
                this.HttpContext.Session.SetString("UserName", $"{user.FirstName} {user.LastName}");
                this.HttpContext.Session.SetString("UserEmail", user.Email);
                this.HttpContext.Session.SetString("UserPhoto", user.Photo);

                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

       

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            this.HttpContext.Session.Clear(); // Clear all session data
            return RedirectToAction("Index", "Home");
        }

        public class LoginRequest
        {
            public string Email { get; set; } = string.Empty; // Initialize with default value
            public string Password { get; set; } = string.Empty; // Initialize with default value
        }

    }

    


}