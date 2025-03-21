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
                    Id = 1, // Ensure this ID matches the existing admin user's ID
                    FirstName = "Admin",
                    LastName = "Matrix",
                    Email = "admin@matrix.com",
                    Password = HashPassword("admin123"), // Hash the admin password
                    Address = "Admin Address",
                    Photo = "https://cdn-icons-png.flaticon.com/256/2165/2165674.png", // Admin profile image
                    IsAdmin = true
                }
            );
        }

        // Helper method to hash passwords
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}