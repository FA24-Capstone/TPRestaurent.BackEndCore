using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddVietnameseNameForTransactionStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "TransationStatus",
                keyColumn: "Id",
                keyValue: 3,
                column: "VietnameseName",
                value: "Đã áp dụng");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "TransationStatus",
                keyColumn: "Id",
                keyValue: 3,
                column: "VietnameseName",
                value: null);
        }
    }
}