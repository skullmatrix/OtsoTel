using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using HotelWebsite.Models;
using System;
using System.Threading.Tasks;

namespace HotelWebsite.Controllers
{
    public class GuestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GuestController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Guest/Services
        public IActionResult Services()
        {
            return View();
        }

        // GET: Guest/Feedback
        public IActionResult Feedback()
        {
            return View();
        }

        // POST: Guest/Feedback
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Feedback(string name, string email, string subject, string message, int rating)
        {
            // In a real application, save the feedback to a database
            // For now, just show a success message
            TempData["SuccessMessage"] = "Thank you for your feedback! We appreciate your input.";
            return RedirectToAction(nameof(Feedback));
        }
    }
}