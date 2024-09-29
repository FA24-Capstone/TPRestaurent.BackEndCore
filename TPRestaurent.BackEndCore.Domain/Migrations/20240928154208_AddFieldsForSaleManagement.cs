using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddFieldsForSaleManagement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DailyCountdown",
                table: "DishSizeDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuantityLeft",
                table: "DishSizeDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Lat",
                table: "CustomerInfoAddress",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Lng",
                table: "CustomerInfoAddress",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "DailyCountdown",
                table: "Combos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuantityLeft",
                table: "Combos",
                type: "int",
                nullable: true);

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
                table: "OrderSessionStatuses",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "LateWarning", "Đang trễ" });

            migrationBuilder.UpdateData(
                table: "OrderSessionStatuses",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "Completed", "Thành Công" });

            migrationBuilder.InsertData(
                table: "OrderSessionStatuses",
                columns: new[] { "Id", "Name", "VietnameseName" },
                values: new object[] { 5, "Cancelled", "Đã Huỷ" });

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "OrderSessionStatuses",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DropColumn(
                name: "DailyCountdown",
                table: "DishSizeDetails");

            migrationBuilder.DropColumn(
                name: "QuantityLeft",
                table: "DishSizeDetails");

            migrationBuilder.DropColumn(
                name: "Lat",
                table: "CustomerInfoAddress");

            migrationBuilder.DropColumn(
                name: "Lng",
                table: "CustomerInfoAddress");

            migrationBuilder.DropColumn(
                name: "DailyCountdown",
                table: "Combos");

            migrationBuilder.DropColumn(
                name: "QuantityLeft",
                table: "Combos");

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
                table: "OrderSessionStatuses",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "Completed", "Thành Công" });

            migrationBuilder.UpdateData(
                table: "OrderSessionStatuses",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "Cancelled", "Đã Huỷ" });

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
    }
}
