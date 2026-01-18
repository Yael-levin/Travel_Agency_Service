using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Travel_Agency_Service.Data;
using Travel_Agency_Service.Helpers;
using Travel_Agency_Service.Models;
using Travel_Agency_Service.Services;
using Travel_Agency_Service.ViewModels;


namespace Travel_Agency_Service.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult MyTrips()
        {

            // 🔐 בדיקת Session

            var userId = HttpContext.Session.GetInt32("UserId");
            
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            // 🔔 הודעה למשתמש שקודם מרשימת המתנה
            var promotedUserId = HttpContext.Session.GetInt32("PromotedUserId");

            if (promotedUserId != null && promotedUserId == userId)
            {
                TempData["Success"] = "Good news! You have been promoted from the waiting list. A booking has been created for you.";
                HttpContext.Session.Remove("PromotedUserId");
            }

           

            int uid = userId.Value;


            // ⏰ ניקוי הזמנות Waiting List שפג תוקפן (24h)
            var expiredBookings = _context.Bookings
                .Where(b =>
                    b.UserId == userId.Value &&
                    b.Status == BookingStatus.Booked &&
                    b.IsFromWaitingList &&
                    b.PromotedAt.HasValue &&
                    b.PromotedAt.Value.AddHours(24) < DateTime.Now
                )
                .ToList();

            foreach (var b in expiredBookings)
            {
                b.Status = BookingStatus.Cancelled;
            }

            if (expiredBookings.Any())
            {
                _context.SaveChanges();
            }

            var bookings = _context.Bookings
                .Include(b => b.Trip)
                .Where(b => b.UserId == uid)
                .ToList();
           
            var payments = _context.Payments
                .Where(p => p.UserId == uid && p.Status == PaymentStatus.Success)
                .ToList();

            var waitingTrips = _context.WaitingList
            .Include(w => w.Trip)
            .Where(w => w.UserId == uid)
            .OrderBy(w => w.CreatedAt)
            .ToList();

            ViewBag.Payments = payments;

            var model = new DashboardViewModel
            {
                // 🛒 Pending Payment / Cart
                PendingBookings = bookings.Where(b => b.Status == BookingStatus.Booked && b.Trip.StartDate.Date >= DateTime.Today).ToList(),

                // ✈ Upcoming Trips
                UpcomingTrips = bookings.Where(b => b.Trip.StartDate.Date >= DateTime.Today && 
                (b.Status == BookingStatus.Paid)).ToList(),


                PastTrips = bookings.Where(b => b.Status == BookingStatus.Paid &&  b.Trip.StartDate.Date < DateTime.Today).ToList(),

                WaitingTrips = waitingTrips

            };

            return View(model);
        }

        public IActionResult DownloadBookingPdf(int bookingId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Users");

            var booking = _context.Bookings
                .Include(b => b.Trip)
                .Include(b => b.User)
                .FirstOrDefault(b =>
                    b.BookingId == bookingId &&
                    b.UserId == userId.Value &&
                    b.Status == BookingStatus.Paid);

            if (booking == null)
                return NotFound();

            var pdfService = new PdfService();
            var pdfBytes = pdfService.GenerateBookingPdf(booking);

            return File(pdfBytes, "application/pdf", "Itinerary.pdf");
        }

        public IActionResult DownloadPaymentPdf(int paymentId)
        {
            var payment = _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Trip)
                .Include(p => p.User)
                .FirstOrDefault(p => p.PaymentId == paymentId);

            if (payment == null)
                return NotFound();

            var pdfService = new PdfService(); 

            var pdfBytes = pdfService.GeneratePaymentPdf(payment);

            return File(pdfBytes, "application/pdf", $"Payment_{payment.PaymentId}.pdf");
        }
        
        [HttpPost]
        public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Not authenticated" });
            }

            if (string.IsNullOrWhiteSpace(currentPassword) ||
                string.IsNullOrWhiteSpace(newPassword) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                return Json(new { success = false, message = "All fields are required" });
            }

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId.Value);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            // 1 בדיקת סיסמה נוכחית – לפני הכל
            if (!PasswordHelper.VerifyPassword(user.PasswordHash, currentPassword))
            {
                return Json(new { success = false, message = "Current password is incorrect" });
            }

            // 2 התאמה
            if (newPassword != confirmPassword)
            {
                return Json(new { success = false, message = "Passwords do not match" });
            }

            // 3 חוזק סיסמה חדשה
            if (newPassword.Length < 6 ||
                !newPassword.Any(char.IsUpper) ||
                !newPassword.Any(char.IsDigit)
                )
            {
                return Json(new
                {
                    success = false,
                    message = "Password must be at least 6 characters and include an uppercase letter and a number"
                });
            }

            // 4 עדכון
            user.PasswordHash = PasswordHelper.HashPassword(newPassword);
            _context.SaveChanges();

            return Json(new { success = true });
        }


        [HttpPost]
        public IActionResult LeaveWaitingList(int waitingListId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Users");

            var entry = _context.WaitingList
                .FirstOrDefault(w => w.WaitingListId == waitingListId && w.UserId == userId.Value);

            if (entry == null)
            {
                TempData["Error"] = "Waiting list entry not found.";
                return RedirectToAction("MyTrips");
            }

            _context.WaitingList.Remove(entry);
            _context.SaveChanges();

            TempData["Success"] = "You have been removed from the waiting list.";
            return RedirectToAction("MyTrips");
        }

    }
}
