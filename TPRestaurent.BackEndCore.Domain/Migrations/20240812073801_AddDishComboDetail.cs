using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddDishComboDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DishCombos_Combos_ComboId",
                table: "DishCombos");

            migrationBuilder.DropForeignKey(
                name: "FK_DishCombos_DishSizeDetails_DishSizeDetailId",
                table: "DishCombos");

            migrationBuilder.AlterColumn<Guid>(
                name: "DishSizeDetailId",
                table: "DishCombos",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "ComboId",
                table: "DishCombos",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateTable(
                name: "DishComboComboDetails",
                columns: table => new
                {
                    ComboOrderDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DishComboId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReservationDishId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DishComboComboDetails", x => x.ComboOrderDetailId);
                    table.ForeignKey(
                        name: "FK_DishComboComboDetails_DishCombos_DishComboId",
                        column: x => x.DishComboId,
                        principalTable: "DishCombos",
                        principalColumn: "DishComboId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_DishComboComboDetails_OrderDetails_OrderDetailId",
                        column: x => x.OrderDetailId,
                        principalTable: "OrderDetails",
                        principalColumn: "OrderDetailId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_DishComboComboDetails_ReservationDishes_ReservationDishId",
                        column: x => x.ReservationDishId,
                        principalTable: "ReservationDishes",
                        principalColumn: "ReservationDishId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DishComboComboDetails_DishComboId",
                table: "DishComboComboDetails",
                column: "DishComboId");

            migrationBuilder.CreateIndex(
                name: "IX_DishComboComboDetails_OrderDetailId",
                table: "DishComboComboDetails",
                column: "OrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_DishComboComboDetails_ReservationDishId",
                table: "DishComboComboDetails",
                column: "ReservationDishId");

            migrationBuilder.AddForeignKey(
                name: "FK_DishCombos_Combos_ComboId",
                table: "DishCombos",
                column: "ComboId",
                principalTable: "Combos",
                principalColumn: "ComboId");

            migrationBuilder.AddForeignKey(
                name: "FK_DishCombos_DishSizeDetails_DishSizeDetailId",
                table: "DishCombos",
                column: "DishSizeDetailId",
                principalTable: "DishSizeDetails",
                principalColumn: "DishSizeDetailId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DishCombos_Combos_ComboId",
                table: "DishCombos");

            migrationBuilder.DropForeignKey(
                name: "FK_DishCombos_DishSizeDetails_DishSizeDetailId",
                table: "DishCombos");

            migrationBuilder.DropTable(
                name: "DishComboComboDetails");

            migrationBuilder.AlterColumn<Guid>(
                name: "DishSizeDetailId",
                table: "DishCombos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ComboId",
                table: "DishCombos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DishCombos_Combos_ComboId",
                table: "DishCombos",
                column: "ComboId",
                principalTable: "Combos",
                principalColumn: "ComboId",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_DishCombos_DishSizeDetails_DishSizeDetailId",
                table: "DishCombos",
                column: "DishSizeDetailId",
                principalTable: "DishSizeDetails",
                principalColumn: "DishSizeDetailId",
                onDelete: ReferentialAction.NoAction);
        }
    }
}
