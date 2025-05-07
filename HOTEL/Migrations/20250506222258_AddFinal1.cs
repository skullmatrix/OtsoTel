using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HOTEL.Migrations
{
    /// <inheritdoc />
    public partial class AddFinal1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$gvaL4VWtV4THUIuq264kZu2S6J8Jk576B.yXMJ0nDHybi385Zitzu");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$7sT.VqKJXoXhWnfTwrmW/evaR8RC/eWEdhUrvuHXwbSH2kBbxPnwS");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "Password",
                value: "$2a$11$tsMXJLYQNHv/uy6s10LqYuntTpcKR/8CI3JlK1PTJcqaOfEg.eP/G");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$i8xU75fH3TPwYr82CnGEAuJAacVM2ECwImCleWL8J7/RkAZlDia0y");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$Km8Rl9xgPtb9TOefEgxlIO00HXb49h5QHUcm0.h.S4XVqh2zaQ55a");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "Password",
                value: "$2a$11$rM3nBsczpUufrQqiGdKw3.H97Z1TI9F/ujuVoVsejBIWBerhR/ANe");
        }
    }
}
