using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddTransaction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReservationDishes_Dishes_DishId",
                table: "ReservationDishes");

            migrationBuilder.RenameColumn(
                name: "DishId",
                table: "ReservationDishes",
                newName: "DishSizeDetailId");

            migrationBuilder.RenameIndex(
                name: "IX_ReservationDishes_DishId",
                table: "ReservationDishes",
                newName: "IX_ReservationDishes_DishSizeDetailId");

            migrationBuilder.CreateTable(
                name: "ReservationTableDetail",
                columns: table => new
                {
                    ReservationTableDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReservationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationTableDetail", x => x.ReservationTableDetailId);
                    table.ForeignKey(
                        name: "FK_ReservationTableDetail_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "ReservationId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_ReservationTableDetail_Tables_TableId",
                        column: x => x.TableId,
                        principalTable: "Tables",
                        principalColumn: "TableId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<double>(type: "float", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentMethodId = table.Column<int>(type: "int", nullable: false),
                    ReservationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId");
                    table.ForeignKey(
                        name: "FK_Transactions_PaymentMethods_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalTable: "PaymentMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Transactions_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "ReservationId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReservationTableDetail_ReservationId",
                table: "ReservationTableDetail",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationTableDetail_TableId",
                table: "ReservationTableDetail",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_OrderId",
                table: "Transactions",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_PaymentMethodId",
                table: "Transactions",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ReservationId",
                table: "Transactions",
                column: "ReservationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReservationDishes_DishSizeDetails_DishSizeDetailId",
                table: "ReservationDishes",
                column: "DishSizeDetailId",
                principalTable: "DishSizeDetails",
                principalColumn: "DishSizeDetailId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReservationDishes_DishSizeDetails_DishSizeDetailId",
                table: "ReservationDishes");

            migrationBuilder.DropTable(
                name: "ReservationTableDetail");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.RenameColumn(
                name: "DishSizeDetailId",
                table: "ReservationDishes",
                newName: "DishId");

            migrationBuilder.RenameIndex(
                name: "IX_ReservationDishes_DishSizeDetailId",
                table: "ReservationDishes",
                newName: "IX_ReservationDishes_DishId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReservationDishes_Dishes_DishId",
                table: "ReservationDishes",
                column: "DishId",
                principalTable: "Dishes",
                principalColumn: "DishId");
        }
    }
}
