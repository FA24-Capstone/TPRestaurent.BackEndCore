using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class RemoveDeviceAndStoreCredit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_StoreCredits_StoreCreditId",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "StoreCredits");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_StoreCreditId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "StoreCreditId",
                table: "Transactions");

            migrationBuilder.AddColumn<string>(
                name: "AccountId",
                table: "Transactions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeviceCode",
                table: "Tables",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DevicePassword",
                table: "Tables",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiredDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<double>(
                name: "StoreCreditAmount",
                table: "AspNetUsers",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "GroupedDishCrafts",
                columns: table => new
                {
                    GroupedDishCraftId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrderDetailidList = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GroupedDishJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupedDishCrafts", x => x.GroupedDishCraftId);
                });

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "TemporarilyCompleted", null });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_AccountId",
                table: "Transactions",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_AspNetUsers_AccountId",
                table: "Transactions",
                column: "AccountId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_AspNetUsers_AccountId",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "GroupedDishCrafts");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_AccountId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "DeviceCode",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "DevicePassword",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "ExpiredDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "StoreCreditAmount",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<Guid>(
                name: "StoreCreditId",
                table: "Transactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    DeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DevicePassword = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.DeviceId);
                    table.ForeignKey(
                        name: "FK_Devices_Tables_TableId",
                        column: x => x.TableId,
                        principalTable: "Tables",
                        principalColumn: "TableId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "StoreCredits",
                columns: table => new
                {
                    StoreCreditId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Amount = table.Column<double>(type: "float", nullable: false),
                    ExpiredDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreCredits", x => x.StoreCreditId);
                    table.ForeignKey(
                        name: "FK_StoreCredits_AspNetUsers_AccountId",
                        column: x => x.AccountId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "Dining", "Đang Dùng Bữa" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_StoreCreditId",
                table: "Transactions",
                column: "StoreCreditId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_TableId",
                table: "Devices",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreCredits_AccountId",
                table: "StoreCredits",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_StoreCredits_StoreCreditId",
                table: "Transactions",
                column: "StoreCreditId",
                principalTable: "StoreCredits",
                principalColumn: "StoreCreditId");
        }
    }
}
