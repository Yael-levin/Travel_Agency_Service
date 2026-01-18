using System.ComponentModel.DataAnnotations;

namespace Travel_Agency_Service.Models
{
    public class SystemSettings
    {
        public int Id { get; set; }

        public int BookingDeadlineDays { get; set; }

        public int CancellationDeadlineDays { get; set; }

        public int ReminderDaysBeforeTrip { get; set; }
    }
}
