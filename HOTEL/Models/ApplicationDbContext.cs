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
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<BillItem> BillItems { get; set; }
        public DbSet<HousekeepingTask> HousekeepingTasks { get; set; }
        public DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }

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
                    IsAdmin = true,
                    Role = "Administrator"
                },
                // Front desk staff
                new User
                {
                    Id = 2,
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "frontdesk@matrix.com",
                    Password = HashPassword("frontdesk123"),
                    Address = "Hotel Front Desk",
                    Photo = "https://cdn-icons-png.flaticon.com/512/3135/3135715.png",
                    IsAdmin = false,
                    Role = "FrontDesk"
                },
                // Housekeeping staff
                new User
                {
                    Id = 3,
                    FirstName = "Jane",
                    LastName = "Smith",
                    Email = "housekeeping@matrix.com",
                    Password = HashPassword("housekeeping123"),
                    Address = "Hotel Housekeeping",
                    Photo = "https://cdn-icons-png.flaticon.com/512/4128/4128176.png",
                    IsAdmin = false,
                    Role = "Housekeeping"
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
            // Configure relationships
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Room)
                .WithMany()
                .HasForeignKey(b => b.RoomId);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Booking)
                .WithMany(b => b.Payments)
                .HasForeignKey(p => p.BookingId);

            modelBuilder.Entity<BillItem>()
                .HasOne(b => b.Booking)
                .WithMany(b => b.BillItems)
                .HasForeignKey(b => b.BookingId);

            modelBuilder.Entity<HousekeepingTask>()
                .HasOne(h => h.Room)
                .WithMany()
                .HasForeignKey(h => h.RoomId);

            modelBuilder.Entity<HousekeepingTask>()
                .HasOne(h => h.AssignedTo)
                .WithMany()
                .HasForeignKey(h => h.AssignedToUserId)
                .IsRequired(false);

            modelBuilder.Entity<MaintenanceRequest>()
                .HasOne(m => m.Room)
                .WithMany()
                .HasForeignKey(m => m.RoomId);

            modelBuilder.Entity<MaintenanceRequest>()
                .HasOne(m => m.ReportedBy)
                .WithMany()
                .HasForeignKey(m => m.ReportedByUserId)
                .IsRequired(false);

            modelBuilder.Entity<MaintenanceRequest>()
                .HasOne(m => m.AssignedTo)
                .WithMany()
                .HasForeignKey(m => m.AssignedToUserId)
                .IsRequired(false);
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}