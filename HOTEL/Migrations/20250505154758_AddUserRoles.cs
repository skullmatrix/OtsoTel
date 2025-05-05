using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HOTEL.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Users",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Password", "Role" },
                values: new object[] { "$2a$11$hzNimG1ocG4nQ33Fn4dn3eRbFDJuJ9BbnF2lM0k9.fCF8f7r0x2XO", "Administrator" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Address", "Email", "FirstName", "IsAdmin", "LastName", "MiddleName", "Password", "Photo", "Role" },
                values: new object[,]
                {
                    { 2, "Hotel Front Desk", "frontdesk@matrix.com", "John", false, "Doe", null, "$2a$11$P16IFGO/5EXYpI2RWksZmOtVfkS.WFiqJtVG8iJ71hpPkNjabPfpi", "https://cdn-icons-png.flaticon.com/512/3135/3135715.png", "FrontDesk" },
                    { 3, "Hotel Housekeeping", "housekeeping@matrix.com", "Jane", false, "Smith", null, "$2a$11$YKqyIJqvtXTHAKVzZbVWfuk56qmyMQdyzdvHvi8nWA5VDRteUM8FS", "https://cdn-icons-png.flaticon.com/512/4128/4128176.png", "Housekeeping" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$uJPpsiQWEhChhgmIJCNvDOFM9Kyuz3Xte8CpsLfgPRldTWQiRK6Xa");
        }
    }
}
