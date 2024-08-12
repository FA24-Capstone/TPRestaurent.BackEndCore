using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AdjustReservationAccountToCustomerInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_AspNetUsers_CustomerAccountId",
                table: "Reservations");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerAccountId",
                table: "Reservations",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerInfoId",
                table: "Reservations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_CustomerInfoId",
                table: "Reservations",
                column: "CustomerInfoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_AspNetUsers_CustomerAccountId",
                table: "Reservations",
                column: "CustomerAccountId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_CustomerInfos_CustomerInfoId",
                table: "Reservations",
                column: "CustomerInfoId",
                principalTable: "CustomerInfos",
                principalColumn: "CustomerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_AspNetUsers_CustomerAccountId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_CustomerInfos_CustomerInfoId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_CustomerInfoId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "CustomerInfoId",
                table: "Reservations");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerAccountId",
                table: "Reservations",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_AspNetUsers_CustomerAccountId",
                table: "Reservations",
                column: "CustomerAccountId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }
    }
}
