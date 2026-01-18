namespace Travel_Agency_Service.Models
{
    public class WaitingList
    {
        public int WaitingListId { get; set; }   // PK

        public int UserId { get; set; }           // FK -> Users
        public int TripId { get; set; }           // FK -> Trips

        public int RoomsRequested { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public User User { get; set; }
        public Trip Trip { get; set; }
    }

}
