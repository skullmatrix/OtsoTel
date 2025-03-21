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
                // Check if the email already exists
                var existingUser = _context.Users.FirstOrDefault(u => u.Email == user.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "An account with this email already exists.");
                    return View(user);
                }

                // Validate email format
                if (!IsValidEmail(user.Email))
                {
                    ModelState.AddModelError("Email", "Invalid email format.");
                    return View(user);
                }

                // Set the default profile image for regular users
                user.Photo = "https://cdn-icons-png.flaticon.com/256/727/727410.png";

                // Hash the password before saving
                user.Password = HashPassword(user.Password);

                // Save the new user
                _context.Users.Add(user);
                _context.SaveChanges();

                // Redirect to the home page and open the login modal
                TempData["ShowLoginModal"] = true;
                return RedirectToAction("Index", "Home");
            }

            // If the model is invalid, return to the sign-up page with validation errors
            return View(user);
        }

        // Helper method to validate email format
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // Helper method to hash passwords
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        [HttpPost]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
                if (user != null && BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                {
                    // Store user data in session
                    this.HttpContext.Session.SetString("UserId", user.Id.ToString());
                    this.HttpContext.Session.SetString("UserName", $"{user.FirstName} {user.LastName}");
                    this.HttpContext.Session.SetString("UserEmail", user.Email);
                    this.HttpContext.Session.SetString("UserPhoto", user.Photo);
                    this.HttpContext.Session.SetString("IsAdmin", user.IsAdmin.ToString());

                    return Json(new { success = true, isAdmin = user.IsAdmin });
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
                    // Set default values for required fields
                    user.Password = "google-auth"; // Placeholder for Google-authenticated users
                    user.IsAdmin = false; // Default to non-admin
                    user.Photo = user.Photo ?? "https://cdn-icons-png.flaticon.com/256/727/727410.png"; // Default profile image

                    // Save the new user
                    _context.Users.Add(user);
                    _context.SaveChanges();
                }
                else
                {
                    // Use the existing user
                    user = existingUser;
                }

                // Store user data in session
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("UserName", $"{user.FirstName} {user.LastName}");
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("UserPhoto", user.Photo);
                HttpContext.Session.SetString("IsAdmin", user.IsAdmin.ToString());

                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Invalid user data." });
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