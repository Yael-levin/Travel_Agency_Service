using System.ComponentModel.DataAnnotations;

namespace Travel_Agency_Service.Models
{
    public class ServiceReview
    {
        [Key]
        public int ReviewId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
