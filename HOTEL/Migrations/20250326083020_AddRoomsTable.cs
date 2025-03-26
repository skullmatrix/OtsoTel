using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HOTEL.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoomNumber = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Price = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "Id", "Capacity", "Description", "ImageUrl", "Price", "RoomNumber", "Status", "Type" },
                values: new object[,]
                {
                    { 1, 2, "Comfortable standard room with queen bed", "https://example.com/standard-room.jpg", 150.00m, "101", "Vacant", "Standard" },
                    { 2, 2, "Spacious deluxe room with king bed", "https://example.com/deluxe-room.jpg", 250.00m, "201", "Vacant", "Deluxe" },
                    { 3, 4, "Luxurious suite with separate living area", "https://example.com/suite-room.jpg", 400.00m, "301", "Vacant", "Suite" },
                    { 4, 2, "Comfortable standard room with queen bed", "https://example.com/standard-room.jpg", 150.00m, "102", "Under Maintenance", "Standard" }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$uJPpsiQWEhChhgmIJCNvDOFM9Kyuz3Xte8CpsLfgPRldTWQiRK6Xa");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$UPgDv1BDIT.Dk/xscmYhQ.VIqRP.pHhxvICTO9/Rmi2XfpIvM5RhW");
        }
    }
}
