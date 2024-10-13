using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class UpdateStatusName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeviceName",
                table: "Tokens",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeviceToken",
                table: "Tokens",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "OrderDetailStatuses",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "Reserved", "Đặt trước" });

            migrationBuilder.UpdateData(
                table: "OrderDetailStatuses",
                keyColumn: "Id",
                keyValue: 2,
                column: "VietnameseName",
                value: "Chờ xử lý");

            migrationBuilder.UpdateData(
                table: "OrderDetailStatuses",
                keyColumn: "Id",
                keyValue: 4,
                column: "VietnameseName",
                value: "Đã hoàn thành");

            migrationBuilder.UpdateData(
                table: "OrderType",
                keyColumn: "Id",
                keyValue: 3,
                column: "VietnameseName",
                value: "Dùng bữa tại quán");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceName",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "DeviceToken",
                table: "Tokens");

            migrationBuilder.UpdateData(
                table: "OrderDetailStatuses",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "Pending", "Chờ Xử Lý" });

            migrationBuilder.UpdateData(
                table: "OrderDetailStatuses",
                keyColumn: "Id",
                keyValue: 2,
                column: "VietnameseName",
                value: "Chưa Xem");

            migrationBuilder.UpdateData(
                table: "OrderDetailStatuses",
                keyColumn: "Id",
                keyValue: 4,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "OrderType",
                keyColumn: "Id",
                keyValue: 3,
                column: "VietnameseName",
                value: "Dùng bữa tại quán khôn có đặt bàn");
        }
    }
}
