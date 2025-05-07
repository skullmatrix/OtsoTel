using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HOTEL.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Users_CheckedInById",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Users_CheckedOutById",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CheckedInByUserId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CheckedOutByUserId",
                table: "Bookings");

            migrationBuilder.AlterColumn<int>(
                name: "CheckedOutById",
                table: "Bookings",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "CheckedInById",
                table: "Bookings",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Users_CheckedInById",
                table: "Bookings",
                column: "CheckedInById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Users_CheckedOutById",
                table: "Bookings",
                column: "CheckedOutById",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Users_CheckedInById",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Users_CheckedOutById",
                table: "Bookings");

            migrationBuilder.AlterColumn<int>(
                name: "CheckedOutById",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CheckedInById",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CheckedInByUserId",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CheckedOutByUserId",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$wiOvkzNz92Minh6qAfcp1edvtc/UgDxjajF8qwwhorBGK6aJ0mJGi");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$CPVylWNze3Om/sQQeLc1B.rEHYcCrkRH0T5C3Kjh3v3vp6EthcwHW");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "Password",
                value: "$2a$11$ohssdchh7fyb0RfJQMKinO3hqps28dKO42VkVkeH.e7DO8Z7XVLQ2");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Users_CheckedInById",
                table: "Bookings",
                column: "CheckedInById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Users_CheckedOutById",
                table: "Bookings",
                column: "CheckedOutById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
