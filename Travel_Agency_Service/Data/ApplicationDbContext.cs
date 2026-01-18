using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Travel_Agency_Service.Models;

namespace Travel_Agency_Service.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }


        public DbSet<Trip> Trips { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<WaitingList> WaitingList { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<SystemSettings> SystemSettings { get; set; }
        public DbSet<TripReview> TripReviews { get; set; }
        public DbSet<ServiceReview> ServiceReviews { get; set; }


    }



}
