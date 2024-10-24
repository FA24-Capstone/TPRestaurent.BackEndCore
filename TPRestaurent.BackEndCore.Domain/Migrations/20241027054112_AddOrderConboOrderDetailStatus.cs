using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddOrderConboOrderDetailStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "DishComboDetailStatuses",
                columns: new[] { "Id", "Name", "VietnameseName" },
                values: new object[] { 4, "Cancelled", "Đã Huỷ" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DishComboDetailStatuses",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
