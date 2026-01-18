using Microsoft.AspNetCore.Mvc;
using Travel_Agency_Service.Data;
using Travel_Agency_Service.Filters;

namespace Travel_Agency_Service.Controllers
{
    [AdminOnly]
    public class AdminSettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminSettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var settings = _context.SystemSettings.First();
            return View(settings);
        }

        [HttpPost]
        public IActionResult Save(int bookingDeadlineDays,  int cancellationDeadlineDays, int reminderDaysBeforeTrip)
        {
            var settings = _context.SystemSettings.First();

            settings.BookingDeadlineDays = bookingDeadlineDays;
            settings.CancellationDeadlineDays = cancellationDeadlineDays;
            settings.ReminderDaysBeforeTrip = reminderDaysBeforeTrip;

            _context.SaveChanges();

            TempData["Success"] = "System rules updated successfully.";
            return RedirectToAction("Index");
        }

    }
}
