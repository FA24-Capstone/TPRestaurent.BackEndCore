using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddTableSessionAndPrelistOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PrelistOrderId",
                table: "DishComboComboDetails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TableSessions",
                columns: table => new
                {
                    TableSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReservationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableSessions", x => x.TableSessionId);
                    table.ForeignKey(
                        name: "FK_TableSessions_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "ReservationId");
                    table.ForeignKey(
                        name: "FK_TableSessions_Tables_TableId",
                        column: x => x.TableId,
                        principalTable: "Tables",
                        principalColumn: "TableId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "PrelistOrders",
                columns: table => new
                {
                    PrelistOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    OrderTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReadyToServeTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReservationDishId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DishSizeDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ComboId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TableSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrelistOrders", x => x.PrelistOrderId);
                    table.ForeignKey(
                        name: "FK_PrelistOrders_Combos_ComboId",
                        column: x => x.ComboId,
                        principalTable: "Combos",
                        principalColumn: "ComboId");
                    table.ForeignKey(
                        name: "FK_PrelistOrders_DishSizeDetails_DishSizeDetailId",
                        column: x => x.DishSizeDetailId,
                        principalTable: "DishSizeDetails",
                        principalColumn: "DishSizeDetailId");
                    table.ForeignKey(
                        name: "FK_PrelistOrders_ReservationDishes_ReservationDishId",
                        column: x => x.ReservationDishId,
                        principalTable: "ReservationDishes",
                        principalColumn: "ReservationDishId");
                    table.ForeignKey(
                        name: "FK_PrelistOrders_TableSessions_TableSessionId",
                        column: x => x.TableSessionId,
                        principalTable: "TableSessions",
                        principalColumn: "TableSessionId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DishComboComboDetails_PrelistOrderId",
                table: "DishComboComboDetails",
                column: "PrelistOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PrelistOrders_ComboId",
                table: "PrelistOrders",
                column: "ComboId");

            migrationBuilder.CreateIndex(
                name: "IX_PrelistOrders_DishSizeDetailId",
                table: "PrelistOrders",
                column: "DishSizeDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_PrelistOrders_ReservationDishId",
                table: "PrelistOrders",
                column: "ReservationDishId");

            migrationBuilder.CreateIndex(
                name: "IX_PrelistOrders_TableSessionId",
                table: "PrelistOrders",
                column: "TableSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_TableSessions_ReservationId",
                table: "TableSessions",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_TableSessions_TableId",
                table: "TableSessions",
                column: "TableId");

            migrationBuilder.AddForeignKey(
                name: "FK_DishComboComboDetails_PrelistOrders_PrelistOrderId",
                table: "DishComboComboDetails",
                column: "PrelistOrderId",
                principalTable: "PrelistOrders",
                principalColumn: "PrelistOrderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DishComboComboDetails_PrelistOrders_PrelistOrderId",
                table: "DishComboComboDetails");

            migrationBuilder.DropTable(
                name: "PrelistOrders");

            migrationBuilder.DropTable(
                name: "TableSessions");

            migrationBuilder.DropIndex(
                name: "IX_DishComboComboDetails_PrelistOrderId",
                table: "DishComboComboDetails");

            migrationBuilder.DropColumn(
                name: "PrelistOrderId",
                table: "DishComboComboDetails");
        }
    }
}
