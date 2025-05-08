using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HOTEL.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$zg6JJ8v9KvEPY0UY27Gq8ecMW55CEGXtMHUfPEL.rmGSA3nJVgxU2");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$mS0kzt5z1ppE1sOKNlYmOOdBTCc7rdgrNrIQxomol4v1yvlhavSce");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "Password",
                value: "$2a$11$O9LAlPai2rENJygqLo0.CeABJLuOuzEwwJlrxfXRT/wlegBxcmNwy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$hzNimG1ocG4nQ33Fn4dn3eRbFDJuJ9BbnF2lM0k9.fCF8f7r0x2XO");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$P16IFGO/5EXYpI2RWksZmOtVfkS.WFiqJtVG8iJ71hpPkNjabPfpi");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "Password",
                value: "$2a$11$YKqyIJqvtXTHAKVzZbVWfuk56qmyMQdyzdvHvi8nWA5VDRteUM8FS");
        }
    }
}
