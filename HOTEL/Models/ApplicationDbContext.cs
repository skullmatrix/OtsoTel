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
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Room>()
                .Property(r => r.Price)
                .HasColumnType("decimal(10,0)");

            modelBuilder.Entity<Booking>()
                .Property(b => b.TotalPrice)
                .HasColumnType("decimal(10,0)");

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(10,0)");

            modelBuilder.Entity<BillItem>()
                .Property(b => b.Amount)
                .HasColumnType("decimal(10,0)");

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
                    Price = 2500,
                    Status = "Vacant",
                    Description = "Comfortable standard room with queen bed",
                    Capacity = 2,
                    ImageUrl = "https://www.citiparkhotel.com.ph/images/uploads/143/6221b2c5dfe34_Standard-Queen.jpg?0.6300843762493157"
                },
                new Room
                {
                    Id = 2,
                    RoomNumber = "201",
                    Type = "Deluxe",
                    Price = 4000,
                    Status = "Vacant",
                    Description = "Spacious deluxe room with king bed",
                    Capacity = 2,
                    ImageUrl = "https://www.theexcelsiorhotel.com.ph/wp-content/uploads/elementor/thumbs/Room-906-Deluxe-Room-King-10-scaled-qsvjndyna7l6ugj7qq7kmpcfulmijl5i1hhxnpodio.jpg"
                },
                new Room
                {
                    Id = 3,
                    RoomNumber = "301",
                    Type = "Suite",
                    Price = 5000,
                    Status = "Vacant",
                    Description = "Luxurious suite with separate living area",
                    Capacity = 4,
                    ImageUrl = "https://www.manila-hotel.com.ph/wp-content/uploads/2020/06/Veranda-Suite-LR-0505-scaled.jpg"
                },
                new Room
                {
                    Id = 4,
                    RoomNumber = "102",
                    Type = "Standard",
                    Price = 2500,
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