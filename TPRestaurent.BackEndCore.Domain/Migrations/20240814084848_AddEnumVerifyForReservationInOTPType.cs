using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddEnumVerifyForReservationInOTPType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "OTPTypes",
                columns: new[] { "Id", "Name" },
                values: new object[] { 9, "VerifyForReservation" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "OTPTypes",
                keyColumn: "Id",
                keyValue: 9);
        }
    }
}
