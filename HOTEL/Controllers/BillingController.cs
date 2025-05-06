using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelWebsite.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HotelWebsite.Controllers
{
    public class BillingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BillingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Billing
        public async Task<IActionResult> Index(string status = null)
        {
            // Check if user is front desk or admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrator" && userRole != "FrontDesk")
            {
                return RedirectToAction("Index", "Home");
            }

            IQueryable<Booking> bookingsQuery = _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .Include(b => b.BillItems)
                .Include(b => b.Payments);

            // Filter by payment status if provided
            if (!string.IsNullOrEmpty(status))
            {
                bookingsQuery = bookingsQuery.Where(b => b.PaymentStatus == status);
            }

            var bookings = await bookingsQuery
                .Where(b => b.Status == "Checked In" || b.Status == "Checked Out")
                .OrderByDescending(b => b.Status == "Checked In")
                .ThenByDescending(b => b.CheckOutDate)
                .ToListAsync();

            // Get dashboard statistics
            ViewBag.TotalRevenue = await _context.Payments
                .Where(p => p.Status == "Completed")
                .SumAsync(p => p.Amount);

            ViewBag.TodayRevenue = await _context.Payments
                .Where(p => p.Status == "Completed" && p.PaymentDate.Date == DateTime.Now.Date)
                .SumAsync(p => p.Amount);

            ViewBag.PendingPayments = await _context.Bookings
                .Where(b => b.PaymentStatus == "Pending" || b.PaymentStatus == "Partial")
                .CountAsync();

            ViewBag.CompletedPayments = await _context.Bookings
                .Where(b => b.PaymentStatus == "Paid")
                .CountAsync();

            return View(bookings);
        }

        // GET: Billing/GuestBill/5
        public async Task<IActionResult> GuestBill(int id)
        {
            // Check if user is front desk or admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrator" && userRole != "FrontDesk")
            {
                return RedirectToAction("Index", "Home");
            }

            var booking = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .Include(b => b.BillItems)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: Billing/AddBillItem/5
        public async Task<IActionResult> AddBillItem(int id)
        {
            // Check if user is front desk or admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrator" && userRole != "FrontDesk")
            {
                return RedirectToAction("Index", "Home");
            }

            var booking = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            ViewBag.Booking = booking;
            return View(new BillItem { BookingId = id });
        }

        // POST: Billing/AddBillItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBillItem(BillItem billItem)
        {
            // Check if user is front desk or admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrator" && userRole != "FrontDesk")
            {
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                _context.Add(billItem);
                await _context.SaveChangesAsync();

                // Update booking payment status
                var booking = await _context.Bookings
                    .Include(b => b.BillItems)
                    .Include(b => b.Payments)
                    .FirstOrDefaultAsync(b => b.Id == billItem.BookingId);

                if (booking != null)
                {
                    var totalBill = booking.BillItems.Sum(b => b.Amount);
                    var totalPaid = booking.Payments.Where(p => p.Status == "Completed").Sum(p => p.Amount);

                    if (totalPaid >= totalBill)
                    {
                        booking.PaymentStatus = "Paid";
                    }
                    else if (totalPaid > 0)
                    {
                        booking.PaymentStatus = "Partial";
                    }
                    else
                    {
                        booking.PaymentStatus = "Pending";
                    }

                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("GuestBill", new { id = billItem.BookingId });
            }

            // If we got this far, something failed, redisplay form
            var bookingInfo = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == billItem.BookingId);

            ViewBag.Booking = bookingInfo;
            return View(billItem);
        }

        // GET: Billing/AddPayment/5
        public async Task<IActionResult> AddPayment(int id)
        {
            // Check if user is front desk or admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrator" && userRole != "FrontDesk")
            {
                return RedirectToAction("Index", "Home");
            }

            var booking = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .Include(b => b.BillItems)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            ViewBag.Booking = booking;
            ViewBag.Balance = booking.Balance;

            return View(new Payment { BookingId = id, Amount = booking.Balance });
        }

        // POST: Billing/AddPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPayment(Payment payment)
        {
            // Check if user is front desk or admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Administrator" && userRole != "FrontDesk")
            {
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                payment.Status = "Completed";
                _context.Add(payment);
                await _context.SaveChangesAsync();

                // Update booking payment status
                var booking = await _context.Bookings
                    .Include(b => b.BillItems)
                    .Include(b => b.Payments)
                    .FirstOrDefaultAsync(b => b.Id == payment.BookingId);

                if (booking != null)
                {
                    var totalBill = booking.BillItems.Sum(b => b.Amount);
                    var totalPaid = booking.Payments.Where(p => p.Status == "Completed").Sum(p => p.Amount);

                    if (totalPaid >= totalBill)
                    {
                        booking.PaymentStatus = "Paid";
                    }
                    else if (totalPaid > 0)
                    {
                        booking.PaymentStatus = "Partial";
                    }
                    else
                    {
                        booking.PaymentStatus = "Pending";
                    }

                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("GuestBill", new { id = payment.BookingId });
            }

            // If we got this far, something failed, redisplay form
            var bookingInfo = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .Include(b => b.BillItems)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.Id == payment.BookingId);

            ViewBag.Booking = bookingInfo;
            ViewBag.Balance = bookingInfo.Balance;
            return View(payment);
        }

        // GET: Billing/Invoice/5
        public async Task<IActionResult> Invoice(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .Include(b => b.BillItems)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            // Check permissions
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");

            // Only allow access if user is admin/staff or the booking belongs to this user
            if (userRole != "Administrator" && userRole != "FrontDesk" &&
                (string.IsNullOrEmpty(userId) || booking.UserId != int.Parse(userId)))
            {
                return RedirectToAction("Index", "Home");
            }

            return View(booking);
        }

        // GET: Billing/Receipt/5 (Payment receipt)
        public async Task<IActionResult> Receipt(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Booking)
                .ThenInclude(b => b.User)
                .Include(p => p.Booking)
                .ThenInclude(b => b.Room)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
            {
                return NotFound();
            }

            // Check permissions
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");

            // Only allow access if user is admin/staff or the payment belongs to this user
            if (userRole != "Administrator" && userRole != "FrontDesk" &&
                (string.IsNullOrEmpty(userId) || payment.Booking.UserId != int.Parse(userId)))
            {
                return RedirectToAction("Index", "Home");
            }

            return View(payment);
        }
    }
}