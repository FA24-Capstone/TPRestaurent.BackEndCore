using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class FixFieldInCustomerLoveDishClass : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Combo",
                table: "CustomerLovedDishes");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerLovedDishes_ComboId",
                table: "CustomerLovedDishes",
                column: "ComboId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerLovedDishes_Combos_ComboId",
                table: "CustomerLovedDishes",
                column: "ComboId",
                principalTable: "Combos",
                principalColumn: "ComboId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerLovedDishes_Combos_ComboId",
                table: "CustomerLovedDishes");

            migrationBuilder.DropIndex(
                name: "IX_CustomerLovedDishes_ComboId",
                table: "CustomerLovedDishes");

            migrationBuilder.AddColumn<Guid>(
                name: "Combo",
                table: "CustomerLovedDishes",
                type: "uniqueidentifier",
                nullable: true);
        }
    }
}
