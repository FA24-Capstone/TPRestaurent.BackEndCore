using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PrelistOrders_TableSessions_TableSessionId",
                table: "PrelistOrders");

            migrationBuilder.AlterColumn<Guid>(
                name: "TableSessionId",
                table: "PrelistOrders",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_PrelistOrders_TableSessions_TableSessionId",
                table: "PrelistOrders",
                column: "TableSessionId",
                principalTable: "TableSessions",
                principalColumn: "TableSessionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PrelistOrders_TableSessions_TableSessionId",
                table: "PrelistOrders");

            migrationBuilder.AlterColumn<Guid>(
                name: "TableSessionId",
                table: "PrelistOrders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PrelistOrders_TableSessions_TableSessionId",
                table: "PrelistOrders",
                column: "TableSessionId",
                principalTable: "TableSessions",
                principalColumn: "TableSessionId",
                onDelete: ReferentialAction.NoAction);
        }
    }
}
