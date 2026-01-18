using System.ComponentModel.DataAnnotations;

namespace Travel_Agency_Service.Models
{
    public class Trip
    {
        [Key]
        public int TripId { get; set; }

        [Required]
        [StringLength(100)]
        public string Destination { get; set; }

        [Required]
        [StringLength(100)]
        public string Country { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public decimal Price { get; set; }

        public decimal? DiscountPrice { get; set; }

        [Required]
        public int AvailableRooms { get; set; }

        [Required]
        [StringLength(50)]
        public string PackageType { get; set; }
        // Family / Honeymoon / Adventure / Cruise / Luxury

        public int? AgeLimit { get; set; }

        [Required]
        public string Description { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsVisible { get; set; }

        public DateTime? DiscountEndDate { get; set; }


        public int Popularity { get; set; }

        public List<Booking> Bookings { get; set; } = new List<Booking>();


    }
}
