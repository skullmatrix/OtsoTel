using Microsoft.EntityFrameworkCore;

namespace HotelWebsite.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed admin user
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    FirstName = "Admin",
                    LastName = "Matrix",
                    Email = "admin@matrix.com",
                    Password = "admin123",
                    Address = "Admin Address",
                    Photo = "https://via.placeholder.com/150",
                    IsAdmin = true
                }
            );
        }
    }
}