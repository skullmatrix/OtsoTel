using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HOTEL.Migrations
{
    /// <inheritdoc />
    public partial class AddFinal4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$EUazjewgyqRvuEPiBaEJrOQcSTnEIlt4uzSo2FdaQLy9r9RwmzwCm");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$VQmRKs.gnKmCjpUvfJDsxuxVbuXIPwCfwdRXFJHcYpnR0KK0WZBfm");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "Password",
                value: "$2a$11$jJ5AHmqHuRzwo2rOs2x/xOXTxk.6T.MdCyH2XszoTT3g.Een8Ne4W");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$H./aU.I/G5vOqVtS4eZiruwLScSRrZWzSCtGKYmPgYCwfmeVGSYhe");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$jI.REK3jnSp9QRd8C80b1uNkd16/pPuioQfb4EDe2c2e1aYj0Va7a");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "Password",
                value: "$2a$11$pFSi8PgVamjw8/3aL9A4EO5LCZNLmvaRbFN75JiCqlZGAw/PEa1HG");
        }
    }
}
