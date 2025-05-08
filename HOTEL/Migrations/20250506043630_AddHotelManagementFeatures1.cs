using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HOTEL.Migrations
{
    /// <inheritdoc />
    public partial class AddHotelManagementFeatures1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$PfZGD.nAv4VIkwa30DOyT.vBvlC.UTeiucjLQ4KQGmKpmmBrqWs1u");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$9tZ/lI1Cm/vqCkSgu1ZdYOu2jCic9WDuLiDeSHZ8cSTPyWv9cWmlm");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "Password",
                value: "$2a$11$fPJJU.T/2DB.0W4qbW5QEubNcvdCjhHQissz6RwR9oaSJwKcbzf.q");
        }
    }
}
