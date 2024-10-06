using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddOrderStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "AssignedToShipper", "Đã tiếp nhận đơn" });

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "Delivering", "Đang Giao" });

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "Completed", "Thành Công" });

            migrationBuilder.InsertData(
                table: "OrderStatuses",
                columns: new[] { "Id", "Name", "VietnameseName" },
                values: new object[] { 10, "Cancelled", "Đã Huỷ" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "Delivering", "Đang Giao" });

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "Completed", "Thành Công" });

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "Cancelled", "Đã Huỷ" });
        }
    }
}
