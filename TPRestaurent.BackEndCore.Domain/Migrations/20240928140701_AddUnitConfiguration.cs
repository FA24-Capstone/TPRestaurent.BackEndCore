using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddUnitConfiguration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "Configurations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "OrderSessionStatuses",
                keyColumn: "Id",
                keyValue: 0,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "OrderSessionStatuses",
                keyColumn: "Id",
                keyValue: 1,
                column: "VietnameseName",
                value: null);

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

            migrationBuilder.UpdateData(
                table: "PaymentMethods",
                keyColumn: "Id",
                keyValue: 5,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "TransactionTypes",
                keyColumn: "Id",
                keyValue: 0,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "TransactionTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "TransactionTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "TransationStatus",
                keyColumn: "Id",
                keyValue: 3,
                column: "VietnameseName",
                value: null);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Unit",
                table: "Configurations");

            migrationBuilder.UpdateData(
                table: "OrderSessionStatuses",
                keyColumn: "Id",
                keyValue: 0,
                column: "VietnameseName",
                value: "Đã đặt trước");

            migrationBuilder.UpdateData(
                table: "OrderSessionStatuses",
                keyColumn: "Id",
                keyValue: 1,
                column: "VietnameseName",
                value: "Xác nhận");

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

            migrationBuilder.UpdateData(
                table: "PaymentMethods",
                keyColumn: "Id",
                keyValue: 5,
                column: "VietnameseName",
                value: "Số dư tài khoản");

            migrationBuilder.UpdateData(
                table: "TransactionTypes",
                keyColumn: "Id",
                keyValue: 0,
                column: "VietnameseName",
                value: "Đặt cọc");

            migrationBuilder.UpdateData(
                table: "TransactionTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "VietnameseName",
                value: "Đơn");

            migrationBuilder.UpdateData(
                table: "TransactionTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "VietnameseName",
                value: "Nạp tài khoản");

            migrationBuilder.UpdateData(
                table: "TransationStatus",
                keyColumn: "Id",
                keyValue: 3,
                column: "VietnameseName",
                value: "Đã áp dụng");
        }
    }
}
