using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddLateWarning : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "PreparationTime",
                table: "OrderSession",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.InsertData(
                table: "OrderDetailStatuses",
                columns: new[] { "Id", "Name", "VietnameseName" },
                values: new object[] { 6, "LateWarning", "Đang trễ" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "OrderDetailStatuses",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DropColumn(
                name: "PreparationTime",
                table: "OrderSession");
        }
    }
}