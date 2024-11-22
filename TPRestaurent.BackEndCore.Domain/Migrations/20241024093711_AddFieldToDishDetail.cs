using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddFieldToDishDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "DishComboComboDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DishComboDetailStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VietnameseName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DishComboDetailStatuses", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "DishComboDetailStatuses",
                columns: new[] { "Id", "Name", "VietnameseName" },
                values: new object[,]
                {
                    { 0, "Reserved", "Đặt trước" },
                    { 1, "Unchecked", "Chờ xử lý" },
                    { 2, "Processing", "Đang Xử Lý" },
                    { 3, "ReadyToServe", "Đã hoàn thành" }
                });

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 3,
                column: "VietnameseName",
                value: "Đã lên hết món");

            migrationBuilder.CreateIndex(
                name: "IX_DishComboComboDetails_StatusId",
                table: "DishComboComboDetails",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_DishComboComboDetails_DishComboDetailStatuses_StatusId",
                table: "DishComboComboDetails",
                column: "StatusId",
                principalTable: "DishComboDetailStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DishComboComboDetails_DishComboDetailStatuses_StatusId",
                table: "DishComboComboDetails");

            migrationBuilder.DropTable(
                name: "DishComboDetailStatuses");

            migrationBuilder.DropIndex(
                name: "IX_DishComboComboDetails_StatusId",
                table: "DishComboComboDetails");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "DishComboComboDetails");

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 3,
                column: "VietnameseName",
                value: null);
        }
    }
}