using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddTransactionToStoreCreditHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TransactionId",
                table: "StoreCreditHistories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoreCreditHistories_TransactionId",
                table: "StoreCreditHistories",
                column: "TransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_StoreCreditHistories_Transactions_TransactionId",
                table: "StoreCreditHistories",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoreCreditHistories_Transactions_TransactionId",
                table: "StoreCreditHistories");

            migrationBuilder.DropIndex(
                name: "IX_StoreCreditHistories_TransactionId",
                table: "StoreCreditHistories");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "StoreCreditHistories");
        }
    }
}
