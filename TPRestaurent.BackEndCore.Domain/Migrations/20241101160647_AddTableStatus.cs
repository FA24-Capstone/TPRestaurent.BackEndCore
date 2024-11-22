using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddTableStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TableStatusId",
                table: "Tables",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TableStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VietnameseName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableStatuses", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "TableStatuses",
                columns: new[] { "Id", "Name", "VietnameseName" },
                values: new object[] { 0, "NEW", "Vừa tạo" });

            migrationBuilder.InsertData(
                table: "TableStatuses",
                columns: new[] { "Id", "Name", "VietnameseName" },
                values: new object[] { 1, "AVAILABLE", "Trống" });

            migrationBuilder.InsertData(
                table: "TableStatuses",
                columns: new[] { "Id", "Name", "VietnameseName" },
                values: new object[] { 2, "CURRENTLYUSED", "Đang dùng" });

            migrationBuilder.CreateIndex(
                name: "IX_Tables_TableStatusId",
                table: "Tables",
                column: "TableStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tables_TableStatuses_TableStatusId",
                table: "Tables",
                column: "TableStatusId",
                principalTable: "TableStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tables_TableStatuses_TableStatusId",
                table: "Tables");

            migrationBuilder.DropTable(
                name: "TableStatuses");

            migrationBuilder.DropIndex(
                name: "IX_Tables_TableStatusId",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "TableStatusId",
                table: "Tables");
        }
    }
}