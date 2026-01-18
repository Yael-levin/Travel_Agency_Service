using System.ComponentModel.DataAnnotations;

namespace Travel_Agency_Service.Models
{
    public enum PaymentStatus
    {
        Success,
        Failed
    }

    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int BookingId { get; set; }
        public Booking Booking { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; }

        public PaymentStatus Status { get; set; }
    }
}
