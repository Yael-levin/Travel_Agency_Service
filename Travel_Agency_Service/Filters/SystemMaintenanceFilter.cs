using Microsoft.AspNetCore.Mvc.Filters;
using Travel_Agency_Service.Data;
using Travel_Agency_Service.Services;
using System;
using System.Linq;

public class SystemMaintenanceFilter : IActionFilter
{
    private readonly ApplicationDbContext _context;

    public SystemMaintenanceFilter(ApplicationDbContext context)
    {
        _context = context;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        // 1️⃣ Auto cancel להזמנות Waiting List
        var autoCancelService = new AutoCancelService(_context);
        autoCancelService.Run();

        // 2️⃣ פקיעת הנחות
        ExpireDiscounts();
    }

    private void ExpireDiscounts()
    {
        var expiredDiscounts = _context.Trips
            .Where(t =>
                t.DiscountPrice != null &&
                t.DiscountEndDate != null &&
                t.DiscountEndDate < DateTime.Today
            )
            .ToList();

        foreach (var trip in expiredDiscounts)
        {
            trip.DiscountPrice = null;
            trip.DiscountEndDate = null;
        }

        if (expiredDiscounts.Any())
        {
            _context.SaveChanges();
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
