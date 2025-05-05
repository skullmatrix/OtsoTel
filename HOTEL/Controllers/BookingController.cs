using Microsoft.AspNetCore.Mvc;
using HotelWebsite.Models;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;
using HotelWebsite.Data;

namespace HotelWebsite.Controllers
{
    public class BookingsController : Controller
    {
        private readonly BookingDbContext _context;
        private readonly ApplicationDbContext _appContext;

        public BookingsController(BookingDbContext context, ApplicationDbContext appContext)
        {
            _context = context;
            _appContext = appContext;
        }

        public IActionResult Create(int roomId)
        {
            var room = _appContext.Rooms.FirstOrDefault(r => r.Id == roomId);
            if (room == null) return NotFound();

            ViewBag.Room = room;
            return View(new Booking { RoomId = roomId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Booking booking)
        {
            var room = _appContext.Rooms.FirstOrDefault(r => r.Id == booking.RoomId);
            if (room == null) return NotFound();

            if (booking.CheckOutDate <= booking.CheckInDate)
                ModelState.AddModelError("CheckOutDate", "Check-out must be after check-in");

            if (ModelState.IsValid)
            {
                var isAvailable = !_context.Bookings.Any(b =>
                    b.RoomId == booking.RoomId &&
                    b.Status == "Confirmed" &&
                    ((booking.CheckInDate >= b.CheckInDate && booking.CheckInDate < b.CheckOutDate) ||
                    (booking.CheckOutDate > b.CheckInDate && booking.CheckOutDate <= b.CheckOutDate) ||
                    (booking.CheckInDate <= b.CheckInDate && booking.CheckOutDate >= b.CheckOutDate)));

                if (!isAvailable)
                {
                    ModelState.AddModelError("", "Room not available for selected dates");
                    ViewBag.Room = room;
                    return View(booking);
                }

                _context.Add(booking);
                _context.SaveChanges();
                return RedirectToAction("Confirmation", new { id = booking.Id });
            }

            ViewBag.Room = room;
            return View(booking);
        }

        public IActionResult Confirmation(int id)
        {
            var booking = _context.Bookings
                .Include(b => b.Room)
                .FirstOrDefault(b => b.Id == id);

            return booking == null ? NotFound() : View(booking);
        }
    }
}