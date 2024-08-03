using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class UpdateDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_CustomerInfos_CustomerId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Tables_TableId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_CustomerId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_TableId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "TableId",
                table: "Reservations");

            migrationBuilder.AddColumn<string>(
                name: "CustomerAccountId",
                table: "Reservations",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "Reservations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ComboId",
                table: "ReservationDishes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Img",
                table: "Coupons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_CustomerAccountId",
                table: "Reservations",
                column: "CustomerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationDishes_ComboId",
                table: "ReservationDishes",
                column: "ComboId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReservationDishes_Combos_ComboId",
                table: "ReservationDishes",
                column: "ComboId",
                principalTable: "Combos",
                principalColumn: "ComboId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Accounts_CustomerAccountId",
                table: "Reservations",
                column: "CustomerAccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReservationDishes_Combos_ComboId",
                table: "ReservationDishes");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Accounts_CustomerAccountId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_CustomerAccountId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_ReservationDishes_ComboId",
                table: "ReservationDishes");

            migrationBuilder.DropColumn(
                name: "CustomerAccountId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "ComboId",
                table: "ReservationDishes");

            migrationBuilder.DropColumn(
                name: "Img",
                table: "Coupons");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "Reservations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TableId",
                table: "Reservations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_CustomerId",
                table: "Reservations",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_TableId",
                table: "Reservations",
                column: "TableId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_CustomerInfos_CustomerId",
                table: "Reservations",
                column: "CustomerId",
                principalTable: "CustomerInfos",
                principalColumn: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Tables_TableId",
                table: "Reservations",
                column: "TableId",
                principalTable: "Tables",
                principalColumn: "TableId");
        }
    }
}
