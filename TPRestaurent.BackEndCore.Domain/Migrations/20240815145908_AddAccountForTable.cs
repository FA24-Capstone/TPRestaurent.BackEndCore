using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddAccountForTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountId",
                table: "Tables",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TableId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tables_AccountId",
                table: "Tables",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_TableId",
                table: "AspNetUsers",
                column: "TableId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Tables_TableId",
                table: "AspNetUsers",
                column: "TableId",
                principalTable: "Tables",
                principalColumn: "TableId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tables_AspNetUsers_AccountId",
                table: "Tables",
                column: "AccountId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Tables_TableId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Tables_AspNetUsers_AccountId",
                table: "Tables");

            migrationBuilder.DropIndex(
                name: "IX_Tables_AccountId",
                table: "Tables");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_TableId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "TableId",
                table: "AspNetUsers");
        }
    }
}
