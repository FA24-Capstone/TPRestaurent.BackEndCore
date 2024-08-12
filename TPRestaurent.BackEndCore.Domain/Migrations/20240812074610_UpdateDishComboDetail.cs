using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class UpdateDishComboDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DishComboComboDetails_OrderDetails_OrderDetailId",
                table: "DishComboComboDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_DishComboComboDetails_ReservationDishes_ReservationDishId",
                table: "DishComboComboDetails");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReservationDishId",
                table: "DishComboComboDetails",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "OrderDetailId",
                table: "DishComboComboDetails",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_DishComboComboDetails_OrderDetails_OrderDetailId",
                table: "DishComboComboDetails",
                column: "OrderDetailId",
                principalTable: "OrderDetails",
                principalColumn: "OrderDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_DishComboComboDetails_ReservationDishes_ReservationDishId",
                table: "DishComboComboDetails",
                column: "ReservationDishId",
                principalTable: "ReservationDishes",
                principalColumn: "ReservationDishId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DishComboComboDetails_OrderDetails_OrderDetailId",
                table: "DishComboComboDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_DishComboComboDetails_ReservationDishes_ReservationDishId",
                table: "DishComboComboDetails");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReservationDishId",
                table: "DishComboComboDetails",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "OrderDetailId",
                table: "DishComboComboDetails",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DishComboComboDetails_OrderDetails_OrderDetailId",
                table: "DishComboComboDetails",
                column: "OrderDetailId",
                principalTable: "OrderDetails",
                principalColumn: "OrderDetailId",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_DishComboComboDetails_ReservationDishes_ReservationDishId",
                table: "DishComboComboDetails",
                column: "ReservationDishId",
                principalTable: "ReservationDishes",
                principalColumn: "ReservationDishId",
                onDelete: ReferentialAction.NoAction);
        }
    }
}
