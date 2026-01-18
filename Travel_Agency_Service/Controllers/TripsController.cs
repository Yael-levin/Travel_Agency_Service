using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Travel_Agency_Service.Data;
using Travel_Agency_Service.Models;
using Travel_Agency_Service.Services;

namespace Travel_Agency_Service.Controllers
{
    public class TripsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        public TripsController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        

        public IActionResult Trips(string search, string country, string packageType, string sort, bool? onSale, bool? includePast, decimal? minPrice, decimal? maxPrice, DateTime? fromDate, DateTime? toDate)
        {


            if (HttpContext.Session.GetString("UserName") == null)
            {
                return RedirectToAction("Login", "Users");
            }


            var trips = _context.Trips.Where(t => t.IsVisible).AsQueryable();

            if (includePast != true)
            {
                trips = trips.Where(t => t.EndDate >= DateTime.Today);
            }


            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                trips = trips.Where(t =>
                    t.Destination.Contains(search) ||
                    t.Country.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(country))
            {
                trips = trips.Where(t => t.Country == country);
            }

            if (!string.IsNullOrWhiteSpace(packageType))
            {
                trips = trips.Where(t => t.PackageType == packageType);
            }

            if (!string.IsNullOrWhiteSpace(sort))
            {
                switch (sort)
                {
                    case "price_asc":
                        trips = trips.OrderBy(t => t.DiscountPrice ?? t.Price);
                        break;

                    case "price_desc":
                        trips = trips.OrderByDescending(t => t.DiscountPrice ?? t.Price);
                        break;

                    case "date_asc":
                        trips = trips.OrderBy(t => t.StartDate);
                        break;

                    case "date_desc":
                        trips = trips.OrderByDescending(t => t.StartDate);
                        break;
                    case "popular":
                        trips = trips.OrderByDescending(t => t.Popularity);
                        break;

                }

            }

            if (onSale == true)
            {
                trips = trips.Where(t =>
                    t.DiscountPrice != null &&
                    t.DiscountEndDate != null &&
                    t.DiscountEndDate >= DateTime.Today);
            }


            if (minPrice != null)
            {
                trips = trips.Where(t => (t.DiscountPrice ?? t.Price) >= minPrice);
            }

            if (maxPrice != null)
            {
                trips = trips.Where(t => (t.DiscountPrice ?? t.Price) <= maxPrice);
            }

            if (fromDate != null)
            {
                trips = trips.Where(t => t.StartDate >= fromDate);
            }

            if (toDate != null)
            {
                trips = trips.Where(t => t.EndDate <= toDate);
            }

            var waitingCounts = _context.WaitingList
            .GroupBy(w => w.TripId)
            .Select(g => new
            {
                TripId = g.Key,
                Count = g.Count()
            })
            .ToDictionary(x => x.TripId, x => x.Count);

            ViewBag.WaitingCounts = waitingCounts;

            var paidRooms = _context.Bookings
            .Where(b =>
            b.Status == BookingStatus.Paid ||
            (b.Status == BookingStatus.Booked && b.IsFromWaitingList)
            )
            .GroupBy(b => b.TripId)
            .Select(g => new
            {
             TripId = g.Key,
            Rooms = g.Sum(b => b.Rooms)
            })
            .ToDictionary(x => x.TripId, x => x.Rooms);

            ViewBag.PaidRooms = paidRooms;

            ViewBag.ServiceReviews = _context.ServiceReviews
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .ToList();


            return View(trips.ToList());
        }

        public IActionResult Book(int id, bool buyNow = false, bool fromPayment = false)
        {
            var trip = _context.Trips.FirstOrDefault(t => t.TripId == id);
            if (trip == null) return NotFound();

            ViewBag.TripId = trip.TripId;
            ViewBag.Destination = trip.Destination;
            ViewBag.BuyNow = buyNow;

            return View();
        }


        [HttpPost]
        public IActionResult Book(int TripId, int Rooms, bool buyNow = false, bool? joinWaitingList = null)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Users");

            var trip = _context.Trips.FirstOrDefault(t => t.TripId == TripId);
            if (trip == null)
                return NotFound();

            // 🔒 SYSTEM RULE – booking deadline
            var settings = _context.SystemSettings.FirstOrDefault();
            int bookingDeadlineDays = settings?.BookingDeadlineDays ?? 0;

            var daysUntilTrip =
                (trip.StartDate.Date - DateTime.Today).TotalDays;

            if (daysUntilTrip < bookingDeadlineDays)
            {
                TempData["Error"] =
                    $"This trip can be booked only up to {bookingDeadlineDays} days before departure.";
                return RedirectToAction("Trips", "Trips");
            }



            // ✅ ולידציה בסיסית – חשוב, לא לגעת
            if (Rooms <= 0)
            {
                TempData["Error"] = "Number of rooms must be greater than zero.";
                return RedirectToAction("Trips", "Trips");
            }

            // ❌ בקשה מעל המקסימום של הטיול – חסימה מוחלטת
            if (Rooms > trip.AvailableRooms)
            {
                TempData["Error"] =
                    $"You can request up to {trip.AvailableRooms} rooms for this trip.";
                return RedirectToAction("Trips", "Trips");
            }

            // =====================================================
            // ✅ חישוב חדרים תפוסים בפועל
            // ✔️ Paid
            // ✔️ Booked שקודם מרשימת המתנה
            // ❌ WaitingList רגיל – לא נספר
            // =====================================================
            int bookedRooms = _context.Bookings
                .Where(b =>
                    b.TripId == TripId &&
                    (
                        b.Status == BookingStatus.Paid ||
                        (b.Status == BookingStatus.Booked && b.IsFromWaitingList)
                    )
                )
                .Sum(b => b.Rooms);

            bool tripIsFull = bookedRooms + Rooms > trip.AvailableRooms;

            // ✔️ בדיקה לוגית: אם קיימת Waiting List – חוסמים הזמנה רגילה
            bool hasWaitingList = _context.WaitingList.Any(w => w.TripId == TripId);

            // =====================================================
            // 🔴 אם הטיול מלא או שיש Waiting List → מציעים YES / NO
            // =====================================================
            if (tripIsFull || hasWaitingList)
            {
                if (joinWaitingList == null)
                {
                    ViewBag.TripIsFull = true;
                    ViewBag.TripId = TripId;
                    ViewBag.Rooms = Rooms;
                    ViewBag.BuyNow = buyNow;
                    return View();   // ← כאן מופיע YES / NO
                }

                if (joinWaitingList == true)
                {
                    bool alreadyWaiting = _context.WaitingList.Any(w =>
                        w.UserId == userId.Value &&
                        w.TripId == TripId);
                    
                    bool hasCartBooking = _context.Bookings.Any(b =>
                        b.UserId == userId.Value &&
                        b.TripId == TripId &&
                        b.Status == BookingStatus.Booked
                    );

                    // ✔️ אם הוא כבר ברשימת המתנה
                    if (alreadyWaiting)
                    {
                        // ✔️ ואין לו עגלה – נוסיף עגלה
                        if (!hasCartBooking)
                        {
                            var cartBooking = new Booking
                            {
                                TripId = TripId,
                                UserId = userId.Value,
                                Rooms = Rooms,
                                BookingDate = DateTime.Now,
                                Status = BookingStatus.Booked
                            };

                            _context.Bookings.Add(cartBooking);
                            _context.SaveChanges();

                            TempData["Success"] =
                                "Booking added to cart. You are already on the waiting list.";
                        }
                        else
                        {
                            TempData["Error"] =
                                "You already have this trip in your cart and are on the waiting list.";
                        }
                    }
                    else
                    {
                        // לא ברשימת המתנה – מוסיפים לרשימה
                        _context.WaitingList.Add(new WaitingList
                        {
                            UserId = userId.Value,
                            TripId = TripId,
                            RoomsRequested = Rooms,
                            CreatedAt = DateTime.Now
                        });

                        _context.SaveChanges();

                        TempData["Success"] = "You have been added to the waiting list.";
                    }
                }

                else
                {
                    TempData["Error"] = "You chose not to join the waiting list.";
                }

                return RedirectToAction("Trips", "Trips");
            }

            // =====================================================
            // 🟢 מכאן והלאה – יש מקום ואין Waiting List
            // =====================================================

            // ❗ בדיקה אם כבר יש הזמנה בעגלה
            bool alreadyInCart = _context.Bookings.Any(b =>
                b.UserId == userId.Value &&
                b.TripId == TripId &&
                b.Status == BookingStatus.Booked
            );

            if (alreadyInCart)
            {
                TempData["Error"] = "This trip is already in your cart.";
                return RedirectToAction("MyTrips", "Dashboard");
            }

            // =====================================================
            // ✅ מגבלת 3 הזמנות פעילות – נשאר כמו שהיה
            // =====================================================
            int activeBookings = _context.Bookings
                .Include(b => b.Trip)
                .Where(b =>
                    b.UserId == userId &&
                    b.Trip.StartDate.Date >= DateTime.Today &&
                    b.Status == BookingStatus.Paid
                )
                .Count();

            if (activeBookings >= 3)
            {
                TempData["Error"] = "You cannot have more than 3 active trips.";
                return RedirectToAction("Trips", "Trips");
            }

            // =====================================================
            // ✅ יצירת Booking רגיל
            // =====================================================
            var booking = new Booking
            {
                TripId = TripId,
                UserId = userId.Value,
                Rooms = Rooms,
                BookingDate = DateTime.Now,
                Status = BookingStatus.Booked
            };

            _context.Bookings.Add(booking);
            _context.SaveChanges();

            // Buy Now → תשלום
            if (buyNow)
                return RedirectToAction("Confirm", "Payments", new { bookingId = booking.BookingId });

            TempData["Success"] = "Booking created successfully!";
            return RedirectToAction("Trips", "Trips");
        }


        public IActionResult Buy(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Users");

            // שולח למסך Book עם buyNow=true
            // המשתמש יבחר Rooms ואז ה-POST ימשיך ישר לתשלום
            return RedirectToAction("Book", new { id = id, buyNow = true });
        }


        public IActionResult EditBooking(int bookingId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Users");

            var booking = _context.Bookings
                .Include(b => b.Trip)
                .FirstOrDefault(b =>
                    b.BookingId == bookingId &&
                    b.UserId == userId &&
                    b.Status == BookingStatus.Booked);

            if (booking == null)
                return NotFound();

            return View(booking);
        }

        [HttpPost]
        public IActionResult EditBooking(int bookingId, int newRooms)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Users");

            if (newRooms <= 0)
            {
                TempData["Error"] =
                    "Number of rooms must be greater than zero.";
                return RedirectToAction("EditBooking", new { bookingId });
            }

            var booking = _context.Bookings
                .Include(b => b.Trip)
                .FirstOrDefault(b =>
                    b.BookingId == bookingId &&
                    b.UserId == userId &&
                    b.Status == BookingStatus.Booked);

            if (booking == null)
                return NotFound();

            // ❗ בדיקה יחידה: לא לעבור את מקסימום החדרים של הטיול
            if (newRooms > booking.Trip.AvailableRooms)
            {
                TempData["Error"] =
                    $"You can book up to {booking.Trip.AvailableRooms} rooms for this trip.";
                return RedirectToAction("EditBooking", new { bookingId });
            }

            booking.Rooms = newRooms;
            _context.SaveChanges();

            TempData["Success"] =
                "Booking updated successfully.";

            return RedirectToAction("MyTrips", "Dashboard");
        }



        public IActionResult CancelBooking(int bookingId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Users");

            var booking = _context.Bookings
                .Include(b => b.Trip)
                .Include(b => b.User)
                .FirstOrDefault(b =>
                    b.BookingId == bookingId &&
                    b.UserId == userId &&
                    (b.Status == BookingStatus.Booked || b.Status == BookingStatus.Paid));

            if (booking == null)
                return NotFound();

            // 🔒 SYSTEM RULE – cancellation deadline
            var settings = _context.SystemSettings.FirstOrDefault();
            int cancellationDeadlineDays = settings?.CancellationDeadlineDays ?? 0;

            var daysUntilTrip =
                (booking.Trip.StartDate.Date - DateTime.Today).TotalDays;

            if (daysUntilTrip < cancellationDeadlineDays)
            {
                TempData["Error"] =
                    $"Cancellation is not allowed less than {cancellationDeadlineDays} days before departure.";
                return RedirectToAction("MyTrips", "Dashboard");
            }


            bool shouldPromote =
                booking.Status == BookingStatus.Paid ||
                booking.IsFromWaitingList;

            booking.Status = BookingStatus.Cancelled;
            _context.SaveChanges();

            // 📧 מייל ביטול ידני
            _emailService.SendUserCancellationEmail(
                booking.User.Email,
                booking.Trip
            );


            // ✅ מקדמים Waiting List רק אם שולם
            if (shouldPromote)
            {
                PromoteWaitingList(booking.Trip.TripId);
            }


            TempData["Success"] = "Booking cancelled successfully.";
            return RedirectToAction("MyTrips", "Dashboard");
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

            // ⬅️ טוענים פעם אחת FIFO
            var waitingQueue = _context.WaitingList
                .Where(w => w.TripId == tripId)
                .OrderBy(w => w.CreatedAt)
                .ToList();

            foreach (var w in waitingQueue)
            {
                if (availableRooms <= 0)
                    break;

                // FIFO אמיתי – לא מדלגים
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

                // 📧 מייל
                var user = _context.Users.FirstOrDefault(u => u.UserId == w.UserId);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    _emailService.SendWaitingListPromotionEmail(user.Email, trip);
                }
            }

            _context.SaveChanges();
        }


    }
}
