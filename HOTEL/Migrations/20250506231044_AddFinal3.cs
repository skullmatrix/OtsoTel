using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HOTEL.Migrations
{
    /// <inheritdoc />
    public partial class AddFinal3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$9cR.Lsh.HTzcGi4U4czQzOLZz2/hKpb0F8bdz7EVXisWfG7iA2lmS");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$aAU2HpEcHd5WL6uTj3xWV.RxNZ4zk0m7HkfPxDr8COHjmhdBq8gji");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "Password",
                value: "$2a$11$Q2yl6elVSvjC/jYaROCbxOTjskky9iSaAs8/b2rqm5L/fMvi/JEci");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
    }
}
