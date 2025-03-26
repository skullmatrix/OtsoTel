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
        public DbSet<Room> Rooms { get; set; }

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
                    Password = HashPassword("admin123"),
                    Address = "Admin Address",
                    Photo = "https://cdn-icons-png.flaticon.com/256/2165/2165674.png",
                    IsAdmin = true
                }
            );

            // Seed room data
            modelBuilder.Entity<Room>().HasData(
                new Room
                {
                    Id = 1,
                    RoomNumber = "101",
                    Type = "Standard",
                    Price = 150.00m,
                    Status = "Vacant",
                    Description = "Comfortable standard room with queen bed",
                    Capacity = 2,
                    ImageUrl = "https://example.com/standard-room.jpg"
                },
                new Room
                {
                    Id = 2,
                    RoomNumber = "201",
                    Type = "Deluxe",
                    Price = 250.00m,
                    Status = "Vacant",
                    Description = "Spacious deluxe room with king bed",
                    Capacity = 2,
                    ImageUrl = "https://example.com/deluxe-room.jpg"
                },
                new Room
                {
                    Id = 3,
                    RoomNumber = "301",
                    Type = "Suite",
                    Price = 400.00m,
                    Status = "Vacant",
                    Description = "Luxurious suite with separate living area",
                    Capacity = 4,
                    ImageUrl = "https://example.com/suite-room.jpg"
                },
                new Room
                {
                    Id = 4,
                    RoomNumber = "102",
                    Type = "Standard",
                    Price = 150.00m,
                    Status = "Under Maintenance",
                    Description = "Comfortable standard room with queen bed",
                    Capacity = 2,
                    ImageUrl = "https://example.com/standard-room.jpg"
                }
            );
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}