using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Travel_Agency_Service.Data;
using Travel_Agency_Service.Models;

namespace Travel_Agency_Service.Services
{
    public class AutoCancelService
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public AutoCancelService(ApplicationDbContext context)
        {
            _context = context;
            _emailService = new EmailService();
        }

        public void Run()
        {
            // =========================
            // 🔔 REMINDER BEFORE TRIP
            // =========================

            var settings = _context.SystemSettings.First();

            var reminderBookings = _context.Bookings
             .Include(b => b.Trip)
             .Include(b => b.User)
             .Where(b =>
                 b.Status == BookingStatus.Paid &&
                 !b.ReminderSent &&
                 b.Trip.StartDate.Date > DateTime.Today
             )
             .AsEnumerable() 
             .Where(b =>
             {
                 int daysLeft = (b.Trip.StartDate.Date - DateTime.Today).Days;
                 return daysLeft <= settings.ReminderDaysBeforeTrip;
             })
             .ToList();


            foreach (var booking in reminderBookings)
            {
                int daysLeft = (booking.Trip.StartDate.Date - DateTime.Today).Days;

                _emailService.SendTripReminderEmail(
                    booking.User.Email,
                    booking.Trip,
                    daysLeft
                );

                booking.ReminderSent = true;
            }


            if (reminderBookings.Any())
            {
                _context.SaveChanges();
            }

            // =========================
            // ⛔ AUTO CANCEL (Waiting List)
            // =========================
            var expiredBookings = _context.Bookings
                .Include(b => b.Trip)
                .Include(b => b.User)
                .Where(b =>
                    b.Status == BookingStatus.Booked &&
                    b.IsFromWaitingList &&
                    b.PromotedAt.HasValue &&
                    b.PromotedAt.Value.AddHours(24) < DateTime.Now
                )
                .ToList();

            foreach (var booking in expiredBookings)
            {
                // 1 ביטול
                booking.Status = BookingStatus.Cancelled;

                // 2 שמירה כדי לשחרר חדרים
                _context.SaveChanges();

                // 3 מייל
                _emailService.SendAutoCancellationEmail(
                    booking.User.Email,
                    booking.Trip
                );

                // 4 קידום Waiting List
                PromoteWaitingList(booking.Trip.TripId);
            }

        }

        private void PromoteWaitingList(int tripId)
        {
            var trip = _context.Trips.First(t => t.TripId == tripId);

            int bookedRooms = _context.Bookings
            .Where(b =>
            b.TripId == tripId &&
            (
            b.Status == BookingStatus.Paid ||
            (b.Status == BookingStatus.Booked && b.IsFromWaitingList)
            )
            )
            .Sum(b => b.Rooms);


            int availableRooms = trip.AvailableRooms - bookedRooms;

            var waitingQueue = _context.WaitingList
                .Where(w => w.TripId == tripId)
                .OrderBy(w => w.CreatedAt)
                .ToList();

            foreach (var w in waitingQueue)
            {
                if (availableRooms <= 0)
                    break;

                if (w.RoomsRequested > availableRooms)
                    break;

                var booking = new Booking
                {
                    TripId = tripId,
                    UserId = w.UserId,
                    Rooms = w.RoomsRequested,
                    BookingDate = DateTime.Now,
                    Status = BookingStatus.Booked,
                    IsFromWaitingList = true,
                    PromotedAt = DateTime.Now
                };

                _context.Bookings.Add(booking);
                _context.WaitingList.Remove(w);

                availableRooms -= w.RoomsRequested;

                _context.SaveChanges();

                var user = _context.Users.FirstOrDefault(u => u.UserId == w.UserId);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    _emailService.SendWaitingListPromotionEmail(user.Email, trip);
                }
            }
        }



    }
}
