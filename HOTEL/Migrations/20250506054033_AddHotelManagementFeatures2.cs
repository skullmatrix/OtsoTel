using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HOTEL.Migrations
{
    /// <inheritdoc />
    public partial class AddHotelManagementFeatures2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$NDTwoQOItegQWxKDnjj7x.eQAW74aqZCTp160jm7N4pW4SGSQ3HFe");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$BV8vvE7/a9x4zYk3Qv0vV.J4NUlDOXCH7.ooWU.aehhF51ULHKbW.");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "Password",
                value: "$2a$11$j9mtC4Ouc2r8MXYl6tT9UO2FJzMvzBgD3aUjj2EF7XYiSbHMOmUxm");
        }
    }
}
