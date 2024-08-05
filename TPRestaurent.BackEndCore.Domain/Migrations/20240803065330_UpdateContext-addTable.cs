using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class UpdateContextaddTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tables_TableSize_TableSizeId",
                table: "Tables");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TableSize",
                table: "TableSize");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReservationRequestStatus",
                table: "ReservationRequestStatus");

            migrationBuilder.RenameTable(
                name: "TableSize",
                newName: "TableSizes");

            migrationBuilder.RenameTable(
                name: "ReservationRequestStatus",
                newName: "ReservationRequestStatuses");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TableSizes",
                table: "TableSizes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReservationRequestStatuses",
                table: "ReservationRequestStatuses",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tables_TableSizes_TableSizeId",
                table: "Tables",
                column: "TableSizeId",
                principalTable: "TableSizes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tables_TableSizes_TableSizeId",
                table: "Tables");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TableSizes",
                table: "TableSizes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReservationRequestStatuses",
                table: "ReservationRequestStatuses");

            migrationBuilder.RenameTable(
                name: "TableSizes",
                newName: "TableSize");

            migrationBuilder.RenameTable(
                name: "ReservationRequestStatuses",
                newName: "ReservationRequestStatus");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TableSize",
                table: "TableSize",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReservationRequestStatus",
                table: "ReservationRequestStatus",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tables_TableSize_TableSizeId",
                table: "Tables",
                column: "TableSizeId",
                principalTable: "TableSize",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }
    }
}
