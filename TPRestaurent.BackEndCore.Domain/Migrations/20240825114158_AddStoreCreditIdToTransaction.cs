using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddStoreCreditIdToTransaction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StoreCreditId",
                table: "Transactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_StoreCreditId",
                table: "Transactions",
                column: "StoreCreditId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_StoreCredits_StoreCreditId",
                table: "Transactions",
                column: "StoreCreditId",
                principalTable: "StoreCredits",
                principalColumn: "StoreCreditId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_StoreCredits_StoreCreditId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_StoreCreditId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "StoreCreditId",
                table: "Transactions");
        }
    }
}
