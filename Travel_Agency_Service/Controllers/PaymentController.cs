using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Travel_Agency_Service.Data;
using Travel_Agency_Service.Models;
using Travel_Agency_Service.Services;
using Travel_Agency_Service.ViewModels;


namespace Travel_Agency_Service.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Payments/Pay/5
        [HttpGet]
        public IActionResult Pay(int bookingId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Users");

            var booking = _context.Bookings
            .Include(b => b.Trip)
            .FirstOrDefault(b => b.BookingId == bookingId && b.UserId == userId.Value);


            if (booking == null)
            {
                TempData["Error"] = "This payment is no longer available.";
                return RedirectToAction("Trips", "Trips");
            }


            ViewBag.BookingId = booking.BookingId;
            ViewBag.Amount = booking.Rooms * booking.Trip.Price;

            return View();
        }


        // POST: Payments/Pay
        [HttpPost]
        public IActionResult PayConfirm(PaymentViewModel model)
        {
            // 🔹 1. Validation של הטופס
            if (!ModelState.IsValid)
            {
                var booking1 = _context.Bookings
                    .Include(b => b.Trip)
                    .FirstOrDefault(b => b.BookingId == model.BookingId);

                ViewBag.BookingId = model.BookingId;
                ViewBag.Amount = booking1.Rooms * booking1.Trip.Price;

                return View("Pay", model);
            }

            var parts = model.ExpirationDate.Split('/');
            int month = int.Parse(parts[0]);
            int year = int.Parse("20" + parts[1]);

            var lastDayOfMonth = new DateTime(year, month,
                DateTime.DaysInMonth(year, month));

            if (lastDayOfMonth < DateTime.Today)
            {
                ModelState.AddModelError("ExpirationDate", "Card has expired.");

                var booking1 = _context.Bookings
                    .Include(b => b.Trip)
                    .FirstOrDefault(b => b.BookingId == model.BookingId);

                ViewBag.BookingId = model.BookingId;
                ViewBag.Amount = booking1.Rooms * booking1.Trip.Price;

                return View("Pay", model);
            }

            // 🔹 2. בדיקת משתמש
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Users");

            // 🔹 3. שליפת ההזמנה
            var booking = _context.Bookings
                .Include(b => b.Trip)
                .Include(b => b.User)
                .FirstOrDefault(b =>
                    b.BookingId == model.BookingId &&
                    b.UserId == userId.Value &&
                    b.Status == BookingStatus.Booked);

            if (booking == null)
                return NotFound();

            // 🔹 3.5 בדיקת תוקף 24 שעות להזמנה מקודמת
            if (booking.IsFromWaitingList)
            {
                if (!booking.PromotedAt.HasValue ||
                    booking.PromotedAt.Value.AddHours(24) < DateTime.Now)
                {
                    booking.Status = BookingStatus.Cancelled;
                    _context.SaveChanges();

                    TempData["Error"] = "Payment time has expired. The booking was cancelled.";
                    return RedirectToAction("MyTrips", "Dashboard");
                }
            }


            // 🔹 4. בדיקה חוזרת של זמינות
            // 🏨 בדיקת זמינות חדרים לפני מעבר לתשלום
            int bookedRooms = _context.Bookings
            .Where(b =>
                b.TripId == booking.TripId &&
                b.BookingId != booking.BookingId &&   // שלא יספור את עצמו: חשוב!!!
                (
                    b.Status == BookingStatus.Paid ||
                    (b.Status == BookingStatus.Booked && b.IsFromWaitingList)
                )
            )
            .Sum(b => b.Rooms);

            bool hasWaitingList = _context.WaitingList.Any(w => w.TripId == booking.TripId);

            if (!booking.IsFromWaitingList)
            {
                if (bookedRooms + booking.Rooms > booking.Trip.AvailableRooms || hasWaitingList)
                {
                    TempData["Error"] =
                        "This trip is currently full or has a waiting list. You may join it.";

                    return RedirectToAction("Book", "Trips", new
                    {
                        id = booking.Trip.TripId,
                        fromPayment = true
                    });

                }
            }

            // 🔹 5. סימון הזמנה כמשולמת
            booking.Status = BookingStatus.Paid;

            // for popularity sort
            booking.Trip.Popularity += booking.Rooms;


            // 🔹 6. יצירת קבלה (Payment)
            var payment = new Payment
            {
                UserId = userId.Value,
                BookingId = booking.BookingId,
                Amount = booking.Rooms * booking.Trip.Price,
                PaymentDate = DateTime.Now,
                Status = PaymentStatus.Success
            };

            _context.Payments.Add(payment);
            _context.SaveChanges();


            var email = HttpContext.Session.GetString("UserEmail");

            if (!string.IsNullOrEmpty(email))
            {
                var pdfService = new PdfService();
                var emailService = new EmailService();


                byte[] paymentPdf = pdfService.GeneratePaymentPdf(payment);
                emailService.SendPaymentEmail(
                    booking.User.Email,
                    booking,
                    paymentPdf
                );
            }


            // 🔹 7. הצלחה
            TempData["Success"] = "Payment completed successfully!";
            return RedirectToAction("Trips", "Trips");
        }

        // GET: Payments/Confirm/5
        public IActionResult Confirm(int bookingId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Users");

            var booking = _context.Bookings
                .Include(b => b.Trip)
                .FirstOrDefault(b => b.BookingId == bookingId && b.UserId == userId);

            if (booking == null || booking.Status != BookingStatus.Booked)
                return NotFound();

            // ⏰ בדיקת תוקף 24 שעות להזמנה מקודמת מ-Waiting List
            if (booking.IsFromWaitingList)
            {
                if (!booking.PromotedAt.HasValue ||
                    booking.PromotedAt.Value.AddHours(24) < DateTime.Now)
                {
                    booking.Status = BookingStatus.Cancelled;
                    _context.SaveChanges();

                    TempData["Error"] =
                        "Payment time has expired. The booking was cancelled.";

                    return RedirectToAction("MyTrips", "Dashboard");
                }
            }

            // 🏨 בדיקת זמינות חדרים לפני מעבר לתשלום
            int bookedRooms = _context.Bookings
            .Where(b =>
                b.TripId == booking.TripId &&
                b.BookingId != booking.BookingId &&   // שלא יספור את עצמו: חשוב!!!
                (
                    b.Status == BookingStatus.Paid ||
                    (b.Status == BookingStatus.Booked && b.IsFromWaitingList)
                )
            )
            .Sum(b => b.Rooms);


            int realAvailableRooms = booking.Trip.AvailableRooms - bookedRooms;


            if (booking.Rooms > realAvailableRooms && !booking.IsFromWaitingList)
            {
             
                TempData["Error"] =
                    "The trip is currently full. You may the waiting list.";

                return RedirectToAction("Book", "Trips", new
                {
                    id = booking.Trip.TripId,
                    fromPayment = true
                });



            }

            return View(booking);
        }
        
        /*
        [HttpGet]
        [ActionName("PayConfirm")]
        public IActionResult PayConfirmFallback()
        {
            TempData["Error"] = "Something went wrong during payment. Please try again.";
            return RedirectToAction("Trips", "Trips");
        }
        */

    }
}
