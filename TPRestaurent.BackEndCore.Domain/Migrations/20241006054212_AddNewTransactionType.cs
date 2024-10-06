using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddNewTransactionType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TransactionTypes",
                keyColumn: "Id",
                keyValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Combos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "TransactionTypes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "Deposit", "Đặt cọc" });

            migrationBuilder.UpdateData(
                table: "TransactionTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "Order", "Đơn" });

            migrationBuilder.InsertData(
                table: "TransactionTypes",
                columns: new[] { "Id", "Name", "VietnameseName" },
                values: new object[,]
                {
                    { 3, "CreditStore", "Nạp tài khoản" },
                    { 4, "Refund", "Hoàn tiền" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TransactionTypes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "TransactionTypes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Combos");

            migrationBuilder.UpdateData(
                table: "TransactionTypes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "Order", "Đơn" });

            migrationBuilder.UpdateData(
                table: "TransactionTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "CreditStore", "Nạp tài khoản" });

            migrationBuilder.InsertData(
                table: "TransactionTypes",
                columns: new[] { "Id", "Name", "VietnameseName" },
                values: new object[] { 0, "Deposit", "Đặt cọc" });
        }
    }
}
