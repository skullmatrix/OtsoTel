using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HOTEL.Migrations
{
    /// <inheritdoc />
    public partial class AddPreferredPaymentMethodToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreferredPaymentMethod",
                table: "Users",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Bookings",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Password", "PreferredPaymentMethod" },
                values: new object[] { "$2a$11$wiOvkzNz92Minh6qAfcp1edvtc/UgDxjajF8qwwhorBGK6aJ0mJGi", null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Password", "PreferredPaymentMethod" },
                values: new object[] { "$2a$11$CPVylWNze3Om/sQQeLc1B.rEHYcCrkRH0T5C3Kjh3v3vp6EthcwHW", null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Password", "PreferredPaymentMethod" },
                values: new object[] { "$2a$11$ohssdchh7fyb0RfJQMKinO3hqps28dKO42VkVkeH.e7DO8Z7XVLQ2", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreferredPaymentMethod",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Bookings");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$XsCDWTTriIx8osC7tUIUn.HoVaKVtWkq2aynBH9yo8LM1eW48JpBy");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$iMbTR0kC0ZanzN0jV3RDi.1gjp2lWmZSvrOWgH6tiI.2pKAwN/wKq");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "Password",
                value: "$2a$11$Ifk7mZCe5NuNDRZoB/NX/OtwRtvyQguMsAFzz.kEMDewJ4Idg/9K2");
        }
    }
}
