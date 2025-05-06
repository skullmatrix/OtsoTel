using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using HotelWebsite.Models;
using System.Linq;
using System;

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

                // Set the default profile image and role for regular users
                user.Photo = "https://cdn-icons-png.flaticon.com/256/727/727410.png";
                user.Role = "Guest";

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
                    this.HttpContext.Session.SetString("UserRole", user.Role);

                    // Determine redirect URL based on user role
                    string redirectUrl = "/";

                    if (user.Role == "Administrator")
                    {
                        redirectUrl = "/Admin";
                    }
                    else if (user.Role == "FrontDesk")
                    {
                        redirectUrl = "/FrontDesk";
                    }
                    else if (user.Role == "Housekeeping")
                    {
                        redirectUrl = "/Housekeeping"; // Housekeeping dashboard
                    }

                    return Json(new
                    {
                        success = true,
                        isAdmin = user.IsAdmin,
                        role = user.Role,
                        redirectUrl = redirectUrl
                    });
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
        public IActionResult UpdateProfile([FromBody] User updatedUser)
        {
            try
            {
                var userId = this.HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not logged in" });
                }

                var user = _context.Users.FirstOrDefault(u => u.Id == int.Parse(userId));
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Check if email is being changed to one that already exists
                if (user.Email != updatedUser.Email && _context.Users.Any(u => u.Email == updatedUser.Email))
                {
                    return Json(new { success = false, message = "Email already in use by another account" });
                }

                // Update user details
                user.FirstName = updatedUser.FirstName;
                user.LastName = updatedUser.LastName;
                user.MiddleName = updatedUser.MiddleName;
                user.Email = updatedUser.Email;
                user.Address = updatedUser.Address;

                // Only update photo if a new one was provided
                if (!string.IsNullOrEmpty(updatedUser.Photo) && updatedUser.Photo != user.Photo)
                {
                    user.Photo = updatedUser.Photo;
                }

                _context.SaveChanges();

                // Update session data
                this.HttpContext.Session.SetString("UserName", $"{user.FirstName} {user.LastName}");
                this.HttpContext.Session.SetString("UserEmail", user.Email);
                this.HttpContext.Session.SetString("UserPhoto", user.Photo);

                return Json(new { success = true, message = "Profile updated successfully", photo = user.Photo });
            }
            catch (Exception ex)
            {
                // Log the error
                Console.Error.WriteLine($"Error updating profile: {ex}");
                return Json(new { success = false, message = "An error occurred while updating the profile" });
            }
        }

        [HttpPost]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var userId = this.HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not logged in" });
                }

                var user = _context.Users.FirstOrDefault(u => u.Id == int.Parse(userId));
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Verify current password
                if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.Password))
                {
                    return Json(new { success = false, message = "Current password is incorrect" });
                }

                // Validate new password
                if (string.IsNullOrEmpty(request.NewPassword) || request.NewPassword.Length < 8)
                {
                    return Json(new { success = false, message = "New password must be at least 8 characters long" });
                }

                // Check if new password is same as current
                if (BCrypt.Net.BCrypt.Verify(request.NewPassword, user.Password))
                {
                    return Json(new { success = false, message = "New password cannot be the same as current password" });
                }

                // Check if passwords match
                if (request.NewPassword != request.ConfirmNewPassword)
                {
                    return Json(new { success = false, message = "New passwords do not match" });
                }

                // Hash and update the new password
                user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                _context.SaveChanges();

                return Json(new { success = true, message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                // Log the error
                Console.Error.WriteLine($"Error changing password: {ex}");
                return Json(new { success = false, message = "An error occurred while changing the password" });
            }
        }

        public class ChangePasswordRequest
        {
            public string CurrentPassword { get; set; } = string.Empty;
            public string NewPassword { get; set; } = string.Empty;
            public string ConfirmNewPassword { get; set; } = string.Empty;
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
            try
            {
                if (user == null || string.IsNullOrEmpty(user.Email))
                {
                    return Json(new { success = false, message = "Invalid user data." });
                }

                // Check if the user already exists
                var existingUser = _context.Users.FirstOrDefault(u => u.Email == user.Email);
                if (existingUser == null)
                {
                    // Set default values for new users
                    user.Password = "google-auth"; // Placeholder password for Google users
                    user.IsAdmin = false; // Default to non-admin
                    user.Role = "Guest"; // Default role for Google users
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
                this.HttpContext.Session.SetString("UserId", user.Id.ToString());
                this.HttpContext.Session.SetString("UserName", $"{user.FirstName} {user.LastName}");
                this.HttpContext.Session.SetString("UserEmail", user.Email);
                this.HttpContext.Session.SetString("UserPhoto", user.Photo);
                this.HttpContext.Session.SetString("IsAdmin", user.IsAdmin.ToString());
                this.HttpContext.Session.SetString("UserRole", user.Role);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                Console.Error.WriteLine($"Error saving Google user: {ex}");

                // Return a JSON response with the error message
                return Json(new { success = false, message = "An error occurred while saving user data. Please try again." });
            }
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            this.HttpContext.Session.Clear(); // Clear all session data
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult CheckSession()
        {
            var userId = HttpContext.Session.GetString("UserId");
            return Json(new { isAuthenticated = !string.IsNullOrEmpty(userId) });
        }

        public class LoginRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }
    }
}