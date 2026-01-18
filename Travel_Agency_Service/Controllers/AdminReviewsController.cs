using Microsoft.AspNetCore.Mvc;
using Travel_Agency_Service.Data;
using Travel_Agency_Service.Filters;

namespace Travel_Agency_Service.Controllers
{
    [AdminOnly]
    public class AdminReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult DeleteTripReview(int reviewId)
        {
            var review = _context.TripReviews.FirstOrDefault(r => r.ReviewId == reviewId);
            if (review == null)
                return Json(new { success = false, message = "Review not found" });

            _context.TripReviews.Remove(review);
            _context.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult DeleteServiceReview(int reviewId)
        {
            var review = _context.ServiceReviews.FirstOrDefault(r => r.ReviewId == reviewId);
            if (review == null)
                return Json(new { success = false, message = "Review not found" });

            _context.ServiceReviews.Remove(review);
            _context.SaveChanges();

            return Json(new { success = true });
        }
    }
}
