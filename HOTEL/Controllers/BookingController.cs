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
            var savedPaymentMethods = new List<string>();
            
            // Add user's preferred payment method if it exists
            if (user != null && !string.IsNullOrEmpty(user.PreferredPaymentMethod))
            {
                savedPaymentMethods.Add(user.PreferredPaymentMethod);
            }

            // Get any other payment methods the user has used
            var userPayments = await _context.Payments
                .Where(p => p.Booking.UserId == int.Parse(userId) && !string.IsNullOrEmpty(p.PaymentMethod))
                .Select(p => p.PaymentMethod)
                .Distinct()
                .ToListAsync();
            
            // Add payment methods that aren't already in the list
            foreach (var method in userPayments)
            {
                if (!savedPaymentMethods.Contains(method))
                {
                    savedPaymentMethods.Add(method);
                }
            }

            ViewBag.Room = room;
            ViewBag.SavedPaymentMethods = savedPaymentMethods;

            return View(new Booking
            {
                RoomId = roomId,
                CheckInDate = DateTime.Now.Date.AddDays(1),
                CheckOutDate = DateTime.Now.Date.AddDays(2),
                NumberOfGuests = 1,
                Status = "Pending",
                PaymentStatus = "Pending",
                IdVerification = "Pending Verification",
                SpecialRequests = "",
                CreatedAt = DateTime.Now
            });
        }

        // POST: /Booking/Create
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(Booking booking, string paymentMethod, bool savePaymentMethod = false)
{
    if (!ModelState.IsValid)
    {
        var room = await _context.Rooms.FindAsync(booking.RoomId);
        ViewBag.Room = room;

        // Get user's saved payment methods again
        var userId = HttpContext.Session.GetString("UserId");
        if (!string.IsNullOrEmpty(userId))
        {
            var userPayments = await _context.Payments
                .Where(p => p.Booking.UserId == int.Parse(userId) && !string.IsNullOrEmpty(p.PaymentMethod))
                .Select(p => p.PaymentMethod)
                .Distinct()
                .ToListAsync();
            ViewBag.SavedPaymentMethods = userPayments;
        }
        else
        {
            ViewBag.SavedPaymentMethods = new List<string>();
        }

        return View(booking);
    }

    try
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }

        // Start a transaction to ensure all operations complete together
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // Set booking properties
                booking.UserId = int.Parse(userId);
                booking.BookingDate = DateTime.Now;
                booking.CreatedAt = DateTime.Now;
                
                // Set booking status based on payment method
                if (paymentMethod.Contains("Credit Card") || 
                    paymentMethod.Contains("Debit Card") || 
                    paymentMethod.Contains("PayPal"))
                {
                    booking.Status = "Confirmed";
                    booking.PaymentStatus = "Completed"; // Auto-confirm for digital payments
                }
                else
                {
                    booking.Status = "Pending"; // Cash payments need front desk confirmation
                    booking.PaymentStatus = "Pending";
                }
                
                // Set default values for non-nullable fields
                booking.IdVerification = "Pending Verification";
                booking.SpecialRequests = booking.SpecialRequests ?? "";
                
                // Explicitly set foreign key fields to null for a new booking
                booking.CheckedInById = null;
                booking.CheckedOutById = null;
                booking.ActualCheckInDate = null;
                booking.ActualCheckOutDate = null;
                
                // Make sure any other required fields are not null
                if (booking.NumberOfGuests <= 0)
                {
                    booking.NumberOfGuests = 1;
                }

                // Calculate total price
                var days = (booking.CheckOutDate - booking.CheckInDate).Days;
                if (days <= 0)
                {
                    ModelState.AddModelError("CheckOutDate", "Check-out date must be after check-in date");
                    
                    // Retrieve room details for the view
                    var room = await _context.Rooms.FindAsync(booking.RoomId);
                    ViewBag.Room = room;
                    
                    // Get saved payment methods for the view
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var userPayments = await _context.Payments
                            .Where(p => p.Booking.UserId == int.Parse(userId) && !string.IsNullOrEmpty(p.PaymentMethod))
                            .Select(p => p.PaymentMethod)
                            .Distinct()
                            .ToListAsync();
                        ViewBag.SavedPaymentMethods = userPayments;
                    }
                    else
                    {
                        ViewBag.SavedPaymentMethods = new List<string>();
                    }
                    
                    return View(booking);
                }

                // Get room details
                var roomInfo = await _context.Rooms.FindAsync(booking.RoomId);
                if (roomInfo != null)
                {
                    booking.TotalPrice = roomInfo.Price * days;
                    
                    // Update room status to "Booked"
                    roomInfo.Status = "Booked";
                    _context.Rooms.Update(roomInfo);
                }

                // Save booking first to get the ID
                await _context.Bookings.AddAsync(booking);
                await _context.SaveChangesAsync();

                // Handle payment method
                string finalPaymentMethod = paymentMethod;
                bool isExistingCard = paymentMethod.Contains("ending in");

                if (!isExistingCard && (paymentMethod.Contains("Credit Card") || paymentMethod.Contains("Debit Card")))
                {
                    // Only process new card details if it's not a saved card
                    string cardNumber = Request.Form["cardNumber"].ToString();
                    if (!string.IsNullOrEmpty(cardNumber) && cardNumber.Length >= 4)
                    {
                        string last4 = cardNumber.Substring(cardNumber.Length - 4);
                        finalPaymentMethod = $"{paymentMethod} ending in {last4}";
                    }
                }

                // Save payment method if requested
                if (savePaymentMethod && !isExistingCard)
                {
                    var user = await _context.Users.FindAsync(int.Parse(userId));
                    if (user != null)
                    {
                        user.PreferredPaymentMethod = finalPaymentMethod;
                        _context.Users.Update(user);
                        await _context.SaveChangesAsync();
                    }
                }

                // Create a payment record
                var payment = new Payment
                {
                    BookingId = booking.Id,
                    Amount = booking.TotalPrice,
                    PaymentMethod = finalPaymentMethod,
                    PaymentDate = DateTime.Now,
                    Status = finalPaymentMethod.Contains("Card") || finalPaymentMethod.Contains("PayPal") 
                            ? "Completed" : "Pending",
                    TransactionReference = $"REF-{DateTime.Now:yyyyMMddHHmmss}",
                    Notes = $"Payment via {finalPaymentMethod}"
                };
                
                await _context.Payments.AddAsync(payment);
                await _context.SaveChangesAsync();

                // Create bill item for room charge
                var billItem = new BillItem
                {
                    BookingId = booking.Id,
                    Description = $"Room Charge - {roomInfo?.RoomNumber} ({roomInfo?.Type})",
                    Amount = booking.TotalPrice,
                    Category = "Room Charge",
                    DateAdded = DateTime.Now,
                    Notes = "Initial room booking charge" // Add this line to provide a default value
                };

                await _context.BillItems.AddAsync(billItem);
                await _context.SaveChangesAsync();

                // Commit the transaction
                await transaction.CommitAsync();

                // Set success message for modal
                TempData["BookingSuccess"] = true;
                TempData["BookingId"] = booking.Id;

                return RedirectToAction(nameof(MyBookings));
            }
            catch (Exception ex)
            {
                // If anything goes wrong, roll back all changes
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
    catch (Exception ex)
    {
        ModelState.AddModelError("", "Error creating booking: " + ex.Message);
        
        if (ex.InnerException != null)
        {
            ModelState.AddModelError("", "Inner error: " + ex.InnerException.Message);
        }
        
        var room = await _context.Rooms.FindAsync(booking.RoomId);
        ViewBag.Room = room;
        
        var userId = HttpContext.Session.GetString("UserId");
        if (!string.IsNullOrEmpty(userId))
        {
            var userPayments = await _context.Payments
                .Where(p => p.Booking.UserId == int.Parse(userId) && !string.IsNullOrEmpty(p.PaymentMethod))
                .Select(p => p.PaymentMethod)
                .Distinct()
                .ToListAsync();
            ViewBag.SavedPaymentMethods = userPayments;
        }
        else
        {
            ViewBag.SavedPaymentMethods = new List<string>();
        }
        
        return View(booking);
    }
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

        [HttpPost]
        public async Task<IActionResult> ConfirmBooking([FromBody] Booking model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid booking data" });
            }

            try
            {
                var userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not logged in" });
                }

                // Create new booking
                var booking = new Booking
                {
                    UserId = int.Parse(userId),
                    RoomId = model.RoomId,
                    CheckInDate = model.CheckInDate,
                    CheckOutDate = model.CheckOutDate,
                    BookingDate = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    Status = "Confirmed",
                    PaymentStatus = "Pending",
                    NumberOfGuests = model.NumberOfGuests,
                    SpecialRequests = model.SpecialRequests
                };

                // Calculate total price
                var days = (booking.CheckOutDate - booking.CheckInDate).Days;
                var room = await _context.Rooms.FindAsync(model.RoomId);
                if (room != null)
                {
                    booking.TotalPrice = room.Price * days;
                    
                    // Update room status
                    room.Status = "Booked";
                    _context.Rooms.Update(room);
                }

                // Save booking
                await _context.Bookings.AddAsync(booking);
                await _context.SaveChangesAsync();

                // Create bill item
                var billItem = new BillItem
                {
                    BookingId = booking.Id,
                    Description = $"Room Charge - {room?.RoomNumber} ({room?.Type})",
                    Amount = booking.TotalPrice,
                    Category = "Room Charge",
                    DateAdded = DateTime.Now,
                    Notes = "Initial room booking charge" // Add this line to provide a default value
                };

                await _context.BillItems.AddAsync(billItem);
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = "Booking confirmed successfully!",
                    bookingId = booking.Id,
                    redirectUrl = Url.Action("MyBookings", "Booking")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to process booking: " + ex.Message });
            }
        }

        // Add this action to your BookingController
public async Task<IActionResult> TestDatabaseConnection()
{
    try
    {
        // Test if we can read from the database
        var rooms = await _context.Rooms.Take(5).ToListAsync();
        var result = new
        {
            Success = true,
            RoomCount = rooms.Count,
            FirstRoomId = rooms.Any() ? rooms.First().Id : 0,
            Message = "Database connection successful"
        };
        
        // Test if we can write to the database
        var testBooking = new Booking
        {
            RoomId = rooms.Any() ? rooms.First().Id : 1,
            UserId = 1, // Use a valid user ID from your database
            CheckInDate = DateTime.Now.AddDays(1),
            CheckOutDate = DateTime.Now.AddDays(2),
            Status = "Test",
            BookingDate = DateTime.Now,
            CreatedAt = DateTime.Now,
            NumberOfGuests = 1,
            TotalPrice = 100,
            PaymentStatus = "Test"
        };
        
        _context.Bookings.Add(testBooking);
        var saveResult = await _context.SaveChangesAsync();
        
        // Now try to retrieve it
        var savedBooking = await _context.Bookings.FindAsync(testBooking.Id);
        
        // Delete the test booking
        if (savedBooking != null)
        {
            _context.Bookings.Remove(savedBooking);
            await _context.SaveChangesAsync();
        }
        
        return Json(new { 
            Success = true, 
            SaveResult = saveResult,
            BookingId = testBooking.Id,
            BookingRetrieved = savedBooking != null,
            Message = "Database write test successful"
        });
    }
    catch (Exception ex)
    {
        return Json(new { 
            Success = false, 
            Error = ex.Message,
            InnerError = ex.InnerException?.Message,
            StackTrace = ex.StackTrace
        });
    }
}

    }
}