using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddNewPaymentMethod : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "814f9270-78f5-4503-b7d3-0c567e5812ba",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "ADMIN", "admin" });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "85b6791c-49d8-4a61-ad0b-8274ec27e764",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "SHIPPER", "shipper" });

            migrationBuilder.UpdateData(
                table: "OrderType",
                keyColumn: "Id",
                keyValue: 1,
                column: "VietnameseName",
                value: "Đặt bàn");

            migrationBuilder.UpdateData(
                table: "OrderType",
                keyColumn: "Id",
                keyValue: 2,
                column: "VietnameseName",
                value: "Giao hàng tận nơi");

            migrationBuilder.UpdateData(
                table: "OrderType",
                keyColumn: "Id",
                keyValue: 3,
                column: "VietnameseName",
                value: "Dùng bữa tại quán khôn có đặt bàn");

            migrationBuilder.InsertData(
                table: "PaymentMethods",
                columns: new[] { "Id", "Name", "VietnameseName" },
                values: new object[] { 5, "STORE_CREDIT", "Số dư tài khoản" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PaymentMethods",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "814f9270-78f5-4503-b7d3-0c567e5812ba",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "MANAGER", "manager" });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "85b6791c-49d8-4a61-ad0b-8274ec27e764",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "STAFF", "staff" });

            migrationBuilder.UpdateData(
                table: "OrderType",
                keyColumn: "Id",
                keyValue: 1,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "OrderType",
                keyColumn: "Id",
                keyValue: 2,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "OrderType",
                keyColumn: "Id",
                keyValue: 3,
                column: "VietnameseName",
                value: null);
        }
    }
}
