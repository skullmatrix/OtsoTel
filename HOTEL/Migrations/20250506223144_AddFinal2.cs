using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HOTEL.Migrations
{
    /// <inheritdoc />
    public partial class AddFinal2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$HOMIaEHsU7xFsm9po1ruv.a1DtkjjF1LinKep3WVZbB4eByF5ujZK");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$JBAnwINLSYEBrChdjnyZZeEm5xYUpvHKGD6pjQ95top2bYunpym6O");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "Password",
                value: "$2a$11$y2odM6ADZUNZq6DhisNFd.jFOemCAQ6l1KCt/SP4QhSJvM/Oh5MMW");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
    }
}
