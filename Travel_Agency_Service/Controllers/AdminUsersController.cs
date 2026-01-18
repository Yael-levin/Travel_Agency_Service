using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Travel_Agency_Service.Data;
using Travel_Agency_Service.Filters;
using Travel_Agency_Service.Helpers;
using Travel_Agency_Service.Models;

namespace Travel_Agency_Service.Controllers
{
    [AdminOnly]
    public class AdminUsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminUsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 📋 רשימת משתמשים
        public IActionResult Index()
        {
            var users = _context.Users.ToList();
            return View(users);
        }

        // 🚫 חסימה / פתיחה
        public IActionResult ToggleActive(int id)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");

            if (currentUserId == id)
                return BadRequest("Admin cannot block himself");

            var user = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
                return NotFound();

            user.IsActive = !user.IsActive;
            _context.SaveChanges();

            return RedirectToAction("Index");
        }


        public IActionResult BookingHistory(int userId)
        {
            var bookings = _context.Bookings
                .Include(b => b.Trip)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BookingDate)
                .ToList();

            return PartialView("_BookingHistory", bookings);
        }

        [HttpPost]
        public IActionResult AddUser(string fullName, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                return Json(new { success = false, message = "All fields are required" });
            }

            var emailValidator = new EmailAddressAttribute();
            if (!emailValidator.IsValid(email))
            {
                return Json(new { success = false, message = "Invalid email format" });
            }

            // ⬅️ קודם מייל קיים
            if (_context.Users.Any(u => u.Email == email))
            {
                return Json(new { success = false, message = "Email already exists" });
            }

            // ⬅️ ואז סיסמה (בלי special char)
            if (password.Length < 6 ||
                !password.Any(char.IsUpper) ||
                !password.Any(char.IsDigit))
            {
                return Json(new
                {
                    success = false,
                    message = "Password must be at least 6 characters and include an uppercase letter and a number"
                });
            }

            var user = new User
            {
                FullName = fullName,
                Email = email,
                PasswordHash = PasswordHelper.HashPassword(password),
                Role = "User",
                IsActive = true
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUser(int userId)
        {
            var currentAdminId = HttpContext.Session.GetInt32("UserId");

            if (currentAdminId == userId)
            {
                return Json(new { success = false, message = "Admin cannot delete himself" });
            }

            var user = _context.Users
                .Include(u => u.Bookings)
                .ThenInclude(b => b.Trip)
                .FirstOrDefault(u => u.UserId == userId);

            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            // ❌ Waiting List חוסם מחיקה
            if (_context.WaitingList.Any(w => w.UserId == userId))
            {
                return Json(new
                {
                    success = false,
                    message = "Cannot delete user: user is in waiting list"
                });
            }

            // ❌ Paid עתידי חוסם מחיקה
            bool hasFuturePaid = user.Bookings.Any(b =>
                b.Status == BookingStatus.Paid &&
                b.Trip.StartDate > DateTime.Now);

            if (hasFuturePaid)
            {
                return Json(new
                {
                    success = false,
                    message = "Cannot delete user: user has future paid bookings"
                });
            }

            // 🟢 מותר למחוק:
            // Booked / Cancelled / Paid עבר
            _context.Bookings.RemoveRange(user.Bookings);
            _context.Users.Remove(user);
            _context.SaveChanges();

            return Json(new { success = true });
        }


    }
}
