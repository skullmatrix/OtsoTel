using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HOTEL.Migrations
{
    /// <inheritdoc />
    public partial class AddHotelManagementFeatures3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AlterColumn<string>(
                name: "IdVerification",
                table: "Bookings",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$1qbIEpZDGxEC855kzhlXdeEB/HAz8dpQZ.8ZNzvXzCUJlghfEQfX.");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$qCPZDj3w2P8/jakbdU4McebnkcrmTIVjs7wL850oe.BQKR0BBhSnC");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "Password",
                value: "$2a$11$3IrG0O8/QCgFzP2BuMPH0OC7e4S8mnWoMWCsMhkPS2kGJmYx7JuPW");

            migrationBuilder.AlterColumn<string>(
                name: "IdVerification",
                table: "Bookings",
                nullable: false);
        }
    }
}
