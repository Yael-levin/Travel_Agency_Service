using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Travel_Agency_Service.Data;
using Travel_Agency_Service.Filters;
using Travel_Agency_Service.Models;

namespace Travel_Agency_Service.Controllers
{
    public class ServiceReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServiceReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔹 הצגת ביקורות שירות (Trips page)
        [HttpGet]
        public IActionResult Latest()
        {
            var reviews = _context.ServiceReviews
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToList();

            return PartialView("_ServiceReviewsList", reviews);
        }

        // 🔹 הוספת ביקורת שירות
        [HttpPost]
        public IActionResult Add(int rating, string comment)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { success = false, message = "Not authenticated" });

            var review = new ServiceReview
            {
                UserId = userId.Value,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.Now
            };

            _context.ServiceReviews.Add(review);
            _context.SaveChanges();

            return Json(new { success = true });
        }

        [AdminOnly]
        [HttpPost]
        public IActionResult Delete(int reviewId)
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
