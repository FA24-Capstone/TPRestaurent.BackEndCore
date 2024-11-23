using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddIsAvailableToCombo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Combos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "OrderDetailStatuses",
                keyColumn: "Id",
                keyValue: 3,
                column: "VietnameseName",
                value: "Đang Xử Lý");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Combos");

            migrationBuilder.UpdateData(
                table: "OrderDetailStatuses",
                keyColumn: "Id",
                keyValue: 3,
                column: "VietnameseName",
                value: "Đã Xem");
        }
    }
}