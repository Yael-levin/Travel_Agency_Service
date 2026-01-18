using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Travel_Agency_Service.Data;
using Travel_Agency_Service.Filters;
using Travel_Agency_Service.Models;

namespace Travel_Agency_Service.Controllers
{
    public class TripReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TripReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔹 מחזיר HTML למודל – כל הביקורות של טיול
        [HttpGet]
        public IActionResult ByTrip(int tripId)
        {
            var reviews = _context.TripReviews
                .Include(r => r.User)
                .Where(r => r.TripId == tripId)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            return PartialView("~/Views/Trips/_TripReviewsList.cshtml", reviews);
        }

        // 🔹 יצירת ביקורת – רק לטיול עבר + Paid
        [HttpPost]
        public IActionResult Add(int tripId, int rating, string comment)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { success = false, message = "Not authenticated" });

            bool hasPaidPastTrip = _context.Bookings.Any(b =>
                b.UserId == userId.Value &&
                b.TripId == tripId &&
                b.Status == BookingStatus.Paid &&
                b.Trip.EndDate < DateTime.Today);

            if (!hasPaidPastTrip)
            {
                return Json(new
                {
                    success = false,
                    message = "You can review only past paid trips."
                });
            }

            bool alreadyReviewed = _context.TripReviews.Any(r =>
                r.UserId == userId.Value &&
                r.TripId == tripId);

            if (alreadyReviewed)
            {
                return Json(new
                {
                    success = false,
                    message = "You already reviewed this trip."
                });
            }

            var review = new TripReview
            {
                TripId = tripId,
                UserId = userId.Value,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.Now
            };

            _context.TripReviews.Add(review);
            _context.SaveChanges();

            return Json(new { success = true });
        }
        [AdminOnly]
        [HttpPost]
        public IActionResult Delete(int reviewId)
        {

            var review = _context.TripReviews.FirstOrDefault(r => r.ReviewId == reviewId);
            if (review == null)
                return Json(new { success = false, message = "Review not found" });

            _context.TripReviews.Remove(review);
            _context.SaveChanges();

            return Json(new { success = true });
        }

    }
}
