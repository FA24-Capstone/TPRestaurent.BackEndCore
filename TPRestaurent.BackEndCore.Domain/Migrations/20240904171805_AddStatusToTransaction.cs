using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddStatusToTransaction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TransationStatus",
                columns: new[] { "Id", "Name", "VietnameseName" },
                values: new object[] { 3, "APPLIED", null });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TransationStatus",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
