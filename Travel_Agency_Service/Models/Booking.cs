namespace Travel_Agency_Service.Models
{
    public enum BookingStatus
    {
        Booked,   // הזמנה בוצעה (חדרים תפוסים)
        Paid,     // שולם
        Cancelled
    }

    public class Booking
    {
        public int BookingId { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int TripId { get; set; }
        public Trip Trip { get; set; }

        public int Rooms { get; set; }

        public DateTime BookingDate { get; set; }

        public BookingStatus Status { get; set; }

        public bool IsFromWaitingList { get; set; }
        
        public DateTime? PromotedAt { get; set; }

        public bool ReminderSent { get; set; }


    }

}
