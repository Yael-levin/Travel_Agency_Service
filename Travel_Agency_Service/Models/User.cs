using System.ComponentModel.DataAnnotations;

namespace Travel_Agency_Service.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; }

        [Required]
        public string Role { get; set; }   // "Admin" / "User"

        public bool IsActive { get; set; }

        public List<Booking> Bookings { get; set; } = new List<Booking>();

    }
}
