using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelWebsite.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HotelWebsite.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Booking/Create/5 (roomId)
        public async Task<IActionResult> Create(int roomId)
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ShowLoginModal"] = true;
                return RedirectToAction("Index", "Rooms");
            }

            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null)
            {
                return NotFound();
            }

            // If room is not available, redirect
            if (room.Status != "Vacant")
            {
                TempData["ErrorMessage"] = "This room is not available for booking.";
                return RedirectToAction("Index", "Rooms");
            }

            // Get user's saved payment methods
            var user = await _context.Users.FindAsync(int.Parse(userId));

            // Get existing payments made by this user to use as payment methods
            var userPayments = await _context.Payments
                .Where(p => p.Booking.UserId == int.Parse(userId) && !string.IsNullOrEmpty(p.PaymentMethod))
                .Select(p => p.PaymentMethod)
                .Distinct()
                .ToListAsync();

            ViewBag.Room = room;
            ViewBag.SavedPaymentMethods = userPayments;

            return View(new Booking
            {
                RoomId = roomId,
                CheckInDate = DateTime.Now.Date.AddDays(1),
                CheckOutDate = DateTime.Now.Date.AddDays(2),
                NumberOfGuests = 1
            });
        }

        // POST: /Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking, string paymentMethod, string newPaymentMethod)
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ShowLoginModal"] = true;
                return RedirectToAction("Index", "Rooms");
            }

            // Validate the booking
            if (booking.CheckInDate < DateTime.Now.Date)
            {
                ModelState.AddModelError("CheckInDate", "Check-in date cannot be in the past.");
            }

            if (booking.CheckOutDate <= booking.CheckInDate)
            {
                ModelState.AddModelError("CheckOutDate", "Check-out date must be after check-in date.");
            }

            var room = await _context.Rooms.FindAsync(booking.RoomId);
            if (room == null)
            {
                return NotFound();
            }

            ViewBag.Room = room;

            // Get user's saved payment methods
            var userPayments = await _context.Payments
                .Where(p => p.Booking.UserId == int.Parse(userId) && !string.IsNullOrEmpty(p.PaymentMethod))
                .Select(p => p.PaymentMethod)
                .Distinct()
                .ToListAsync();

            ViewBag.SavedPaymentMethods = userPayments;

            if (ModelState.IsValid)
            {
                try
                {
                    // Determine which payment method to use
                    string actualPaymentMethod = !string.IsNullOrEmpty(newPaymentMethod) ? newPaymentMethod : paymentMethod;

                    if (string.IsNullOrEmpty(actualPaymentMethod))
                    {
                        ModelState.AddModelError("", "Please select a payment method.");
                        return View(booking);
                    }

                    booking.UserId = int.Parse(userId);
                    booking.Status = "Confirmed"; // Auto-confirm for simplicity
                    booking.BookingDate = DateTime.Now;

                    // Calculate total price based on number of days
                    var days = (booking.CheckOutDate - booking.CheckInDate).Days;
                    booking.TotalPrice = room.Price * days;

                    // Set payment status
                    booking.PaymentStatus = "Pending";

                    // Set the IdVerification field to avoid null error
                    booking.IdVerification = "Online Booking"; // This is a placeholder

                    // Update room status to indicate it's booked
                    room.Status = "Booked";
                    _context.Update(room);

                    // Save the booking
                    _context.Bookings.Add(booking);
                    await _context.SaveChangesAsync();

                    // Create a room charge bill item
                    var roomCharge = new BillItem
                    {
                        BookingId = booking.Id,
                        Description = $"{room.Type} Room - {room.RoomNumber} ({days} night{(days > 1 ? "s" : "")})",
                        Amount = booking.TotalPrice,
                        Category = "Room Charge",
                        DateAdded = DateTime.Now
                    };

                    _context.BillItems.Add(roomCharge);

                    // Process payment
                    var payment = new Payment
                    {
                        BookingId = booking.Id,
                        Amount = booking.TotalPrice,
                        PaymentDate = DateTime.Now,
                        PaymentMethod = actualPaymentMethod,
                        TransactionReference = $"TXN-{DateTime.Now.ToString("yyyyMMddHHmmss")}",
                        Status = "Completed",
                        Notes = "Payment made during booking"
                    };

                    _context.Payments.Add(payment);

                    // Update booking payment status
                    booking.PaymentStatus = "Paid";
                    _context.Update(booking);

                    await _context.SaveChangesAsync();

                    // Set success message for confirmation modal
                    TempData["BookingSuccess"] = true;
                    TempData["BookingId"] = booking.Id;

                    return RedirectToAction("MyBookings");
                }
                catch (Exception ex)
                {
                    // Log the error
                    Console.Error.WriteLine($"Error saving booking: {ex}");
                    ModelState.AddModelError("", $"An error occurred while saving the booking: {ex.InnerException?.Message ?? ex.Message}");
                    return View(booking);
                }
            }

            return View(booking);
        }

        // GET: /Booking/MyBookings
        public async Task<IActionResult> MyBookings()
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ShowLoginModal"] = true;
                return RedirectToAction("Index", "Home");
            }

            var bookings = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.BillItems)
                .Include(b => b.Payments)
                .Where(b => b.UserId == int.Parse(userId))
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            // For booking success modal
            ViewBag.BookingSuccess = TempData["BookingSuccess"];
            ViewBag.BookingId = TempData["BookingId"];

            return View(bookings);
        }

        // GET: /Booking/Cancel/5
        public async Task<IActionResult> Cancel(int id)
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ShowLoginModal"] = true;
                return RedirectToAction("Index", "Home");
            }

            var booking = await _context.Bookings
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == int.Parse(userId));

            if (booking == null)
            {
                return NotFound();
            }

            // Only allow cancellation if check-in hasn't happened yet
            if (booking.CheckInDate <= DateTime.Now.Date)
            {
                TempData["ErrorMessage"] = "You cannot cancel a booking on or after the check-in date.";
                return RedirectToAction("MyBookings");
            }

            return View(booking);
        }

        // POST: /Booking/Cancel/5
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ShowLoginModal"] = true;
                return RedirectToAction("Index", "Home");
            }

            var booking = await _context.Bookings
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == int.Parse(userId));

            if (booking == null)
            {
                return NotFound();
            }

            // Update booking status
            booking.Status = "Cancelled";

            // Set room status back to vacant if it was reserved for this booking
            booking.Room.Status = "Vacant";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Booking cancelled successfully.";
            return RedirectToAction("MyBookings");
        }

        // GET: /Booking/Details/5
        public async Task<IActionResult> Details(int id)
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ShowLoginModal"] = true;
                return RedirectToAction("Index", "Home");
            }

            var booking = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.BillItems)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == int.Parse(userId));

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }
    }
}