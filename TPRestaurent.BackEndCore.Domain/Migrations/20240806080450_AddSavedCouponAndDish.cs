using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddSavedCouponAndDish : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiredDate",
                table: "StoreCredits",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerSavedCouponId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CustomerLovedDishes",
                columns: table => new
                {
                    CustomerLovedDishId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DishId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ComboId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Combo = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomerInfoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerLovedDishes", x => x.CustomerLovedDishId);
                    table.ForeignKey(
                        name: "FK_CustomerLovedDishes_CustomerInfos_CustomerInfoId",
                        column: x => x.CustomerInfoId,
                        principalTable: "CustomerInfos",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_CustomerLovedDishes_Dishes_DishId",
                        column: x => x.DishId,
                        principalTable: "Dishes",
                        principalColumn: "DishId");
                });

            migrationBuilder.CreateTable(
                name: "CustomerSavedCoupons",
                columns: table => new
                {
                    CustomerSavedCouponId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsUsedOrExpired = table.Column<bool>(type: "bit", nullable: false),
                    CustomerInfoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CouponId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerSavedCoupons", x => x.CustomerSavedCouponId);
                    table.ForeignKey(
                        name: "FK_CustomerSavedCoupons_Coupons_CouponId",
                        column: x => x.CouponId,
                        principalTable: "Coupons",
                        principalColumn: "CouponId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_CustomerSavedCoupons_CustomerInfos_CustomerInfoId",
                        column: x => x.CustomerInfoId,
                        principalTable: "CustomerInfos",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.UpdateData(
                table: "ReservationStatuses",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "DINING");

            migrationBuilder.InsertData(
                table: "ReservationStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[] { 3, "CANCELLED" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerSavedCouponId",
                table: "Orders",
                column: "CustomerSavedCouponId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerLovedDishes_CustomerInfoId",
                table: "CustomerLovedDishes",
                column: "CustomerInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerLovedDishes_DishId",
                table: "CustomerLovedDishes",
                column: "DishId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerSavedCoupons_CouponId",
                table: "CustomerSavedCoupons",
                column: "CouponId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerSavedCoupons_CustomerInfoId",
                table: "CustomerSavedCoupons",
                column: "CustomerInfoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_CustomerSavedCoupons_CustomerSavedCouponId",
                table: "Orders",
                column: "CustomerSavedCouponId",
                principalTable: "CustomerSavedCoupons",
                principalColumn: "CustomerSavedCouponId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_CustomerSavedCoupons_CustomerSavedCouponId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "CustomerLovedDishes");

            migrationBuilder.DropTable(
                name: "CustomerSavedCoupons");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CustomerSavedCouponId",
                table: "Orders");

            migrationBuilder.DeleteData(
                table: "ReservationStatuses",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DropColumn(
                name: "ExpiredDate",
                table: "StoreCredits");

            migrationBuilder.DropColumn(
                name: "CustomerSavedCouponId",
                table: "Orders");

            migrationBuilder.UpdateData(
                table: "ReservationStatuses",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "CANCELLED");
        }
    }
}
