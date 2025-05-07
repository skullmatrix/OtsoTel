using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HOTEL.Migrations
{
    /// <inheritdoc />
    public partial class AddFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$qdJwKUP9UYuiIUKkWr3m1eXQ77kSB9DJT.2Ba7dshRIKajtcuDd3.");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$74ODUMaFUd9xS.ET11LGQ.YNHb5w5tTSSWztCp86NMC2Yoxz8.NQu");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "Password",
                value: "$2a$11$/VMKHXlu9vvV3734IzSAwuOEcC2D42eLNPABDGgKxVNOagE1Pz24a");
        }
    }
}
