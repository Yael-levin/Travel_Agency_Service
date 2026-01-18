using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Travel_Agency_Service.Data;
using Travel_Agency_Service.Filters;
using Travel_Agency_Service.Models;

namespace Travel_Agency_Service.Controllers
{
    [AdminOnly]
    public class AdminTripsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminTripsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var trips = _context.Trips.ToList();
            return View(trips);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Trip trip)
        {
            // 🗓 תאריכי טיול
            if (trip.StartDate < DateTime.Today)
            {
                ModelState.AddModelError("", "Trip start date cannot be in the past.");
            }

            if (trip.StartDate >= trip.EndDate)
            {
                ModelState.AddModelError("", "Start date must be before end date.");
            }

            // 💰 מחיר
            if (trip.Price <= 0)
            {
                ModelState.AddModelError("", "Price must be greater than zero.");
            }

            // 🏨 חדרים
            if (trip.AvailableRooms <= 0)
            {
                ModelState.AddModelError("", "Available rooms must be greater than zero.");
            }

            // 🎂 גיל
            if (trip.AgeLimit < 0)
            {
                ModelState.AddModelError("", "Age limit cannot be negative.");
            }

            // ❌ תאריך הנחה בלי מחיר
            if (trip.DiscountPrice == null && trip.DiscountEndDate != null)
            {
                ModelState.AddModelError("",
                    "Discount end date cannot be set without a discount price.");
            }

            // 🔖 הנחה
            if (trip.DiscountPrice != null)
            {
                if (trip.DiscountPrice <= 0)
                {
                    ModelState.AddModelError("", "Discount price must be greater than zero.");
                }
                else if (trip.DiscountPrice >= trip.Price)
                {
                    ModelState.AddModelError("", "Discount price must be lower than regular price.");
                }

                if (trip.DiscountEndDate == null)
                {
                    ModelState.AddModelError("", "Discount end date is required.");
                }
                else
                {
                    if (trip.DiscountEndDate > DateTime.Today.AddDays(7))
                    {
                        ModelState.AddModelError("", "Discount can be active for up to 7 days only.");
                    }

                    if (trip.DiscountEndDate > trip.EndDate)
                    {
                        ModelState.AddModelError("",
                            "Discount end date cannot be after the trip end date.");
                    }
                }
            }

            if (!ModelState.IsValid)
                return View(trip);

            _context.Trips.Add(trip);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }



        public IActionResult ToggleVisibility(int id)
        {
            var trip = _context.Trips.FirstOrDefault(t => t.TripId == id);
            if (trip == null)
                return NotFound();

            trip.IsVisible = !trip.IsVisible;
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var trip = _context.Trips.FirstOrDefault(t => t.TripId == id);
            if (trip == null)
                return NotFound();

            return View(trip);
        }

        [HttpPost]
        public IActionResult Edit(Trip trip)
        {
            var existingTrip = _context.Trips
                .Include(t => t.Bookings)
                .FirstOrDefault(t => t.TripId == trip.TripId);

            if (existingTrip == null)
                return NotFound();

            // 🚫 אם יש הזמנות – אסור לשנות תאריכים
            if (existingTrip.Bookings.Any())
            {
                if (trip.StartDate != existingTrip.StartDate ||
                    trip.EndDate != existingTrip.EndDate)
                {
                    ModelState.AddModelError("",
                        "Cannot change trip dates because there are existing bookings.");
                }
            }

            // 🗓 תאריכי טיול
            if (trip.StartDate < DateTime.Today)
            {
                ModelState.AddModelError("", "Trip start date cannot be in the past.");
            }

            if (trip.StartDate >= trip.EndDate)
            {
                ModelState.AddModelError("", "Start date must be before end date.");
            }

            // 💰 מחיר
            if (trip.Price <= 0)
            {
                ModelState.AddModelError("", "Price must be greater than zero.");
            }

            // 🏨 חדרים
            if (trip.AvailableRooms <= 0)
            {
                ModelState.AddModelError("", "Available rooms must be greater than zero.");
            }

            // 🎂 גיל
            if (trip.AgeLimit < 0)
            {
                ModelState.AddModelError("", "Age limit cannot be negative.");
            }

            // ❌ תאריך הנחה בלי מחיר
            if (trip.DiscountPrice == null && trip.DiscountEndDate != null)
            {
                ModelState.AddModelError("",
                    "Discount end date cannot be set without a discount price.");
            }

            // 🔖 הנחה
            if (trip.DiscountPrice != null)
            {
                if (trip.DiscountPrice <= 0)
                {
                    ModelState.AddModelError("", "Discount price must be greater than zero.");
                }
                else if (trip.DiscountPrice >= trip.Price)
                {
                    ModelState.AddModelError("", "Discount price must be lower than regular price.");
                }

                if (trip.DiscountEndDate == null)
                {
                    ModelState.AddModelError("", "Discount end date is required.");
                }
                else
                {
                    if (trip.DiscountEndDate > DateTime.Today.AddDays(7))
                    {
                        ModelState.AddModelError("", "Discount can be active for up to 7 days only.");
                    }

                    if (trip.DiscountEndDate > trip.EndDate)
                    {
                        ModelState.AddModelError("",
                            "Discount end date cannot be after the trip end date.");
                    }
                }
            }

            if (!ModelState.IsValid)
                return View(trip);

            // ✅ עדכון בטוח
            existingTrip.Destination = trip.Destination;
            existingTrip.Country = trip.Country;
            existingTrip.Description = trip.Description;
            existingTrip.PackageType = trip.PackageType;

            existingTrip.Price = trip.Price;
            existingTrip.AvailableRooms = trip.AvailableRooms;
            existingTrip.AgeLimit = trip.AgeLimit;
            existingTrip.DiscountPrice = trip.DiscountPrice;
            existingTrip.DiscountEndDate = trip.DiscountEndDate;
            existingTrip.IsVisible = trip.IsVisible;
            existingTrip.ImageUrl = trip.ImageUrl;

            // תאריכים – רק אם אין הזמנות
            if (!existingTrip.Bookings.Any())
            {
                existingTrip.StartDate = trip.StartDate;
                existingTrip.EndDate = trip.EndDate;
            }

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var trip = _context.Trips
                .Include(t => t.Bookings)
                .FirstOrDefault(t => t.TripId == id);

            if (trip == null)
                return NotFound();

            if (trip.Bookings.Any())
            {
                return RedirectToAction("Index", new { error = "CannotDelete" });
            }

            _context.Trips.Remove(trip);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }


    }
}
