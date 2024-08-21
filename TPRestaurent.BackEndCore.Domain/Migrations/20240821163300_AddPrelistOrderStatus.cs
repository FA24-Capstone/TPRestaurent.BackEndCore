using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddPrelistOrderStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "EndTime",
                table: "TableSessions",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "PrelistOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PreListOrderStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VietnameseName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreListOrderStatuses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrelistOrders_StatusId",
                table: "PrelistOrders",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_PrelistOrders_PreListOrderStatuses_StatusId",
                table: "PrelistOrders",
                column: "StatusId",
                principalTable: "PreListOrderStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PrelistOrders_PreListOrderStatuses_StatusId",
                table: "PrelistOrders");

            migrationBuilder.DropTable(
                name: "PreListOrderStatuses");

            migrationBuilder.DropIndex(
                name: "IX_PrelistOrders_StatusId",
                table: "PrelistOrders");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "PrelistOrders");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndTime",
                table: "TableSessions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}
