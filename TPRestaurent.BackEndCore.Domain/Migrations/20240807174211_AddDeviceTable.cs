using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddDeviceTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ComboId",
                table: "Ratings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrderId",
                table: "Ratings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDelivering",
                table: "Orders",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TableId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "OrderDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_ComboId",
                table: "Ratings",
                column: "ComboId");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_OrderId",
                table: "Ratings",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TableId",
                table: "Orders",
                column: "TableId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Tables_TableId",
                table: "Orders",
                column: "TableId",
                principalTable: "Tables",
                principalColumn: "TableId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ratings_Combos_ComboId",
                table: "Ratings",
                column: "ComboId",
                principalTable: "Combos",
                principalColumn: "ComboId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ratings_Orders_OrderId",
                table: "Ratings",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Tables_TableId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Ratings_Combos_ComboId",
                table: "Ratings");

            migrationBuilder.DropForeignKey(
                name: "FK_Ratings_Orders_OrderId",
                table: "Ratings");

            migrationBuilder.DropIndex(
                name: "IX_Ratings_ComboId",
                table: "Ratings");

            migrationBuilder.DropIndex(
                name: "IX_Ratings_OrderId",
                table: "Ratings");

            migrationBuilder.DropIndex(
                name: "IX_Orders_TableId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ComboId",
                table: "Ratings");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "Ratings");

            migrationBuilder.DropColumn(
                name: "IsDelivering",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TableId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "OrderDetails");
        }
    }
}
