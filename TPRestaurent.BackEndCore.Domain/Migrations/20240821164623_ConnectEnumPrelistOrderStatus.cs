using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class ConnectEnumPrelistOrderStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "PreListOrderStatuses",
                columns: new[] { "Id", "Name", "VietnameseName" },
                values: new object[] { 0, "UNCHECKED", null });

            migrationBuilder.InsertData(
                table: "PreListOrderStatuses",
                columns: new[] { "Id", "Name", "VietnameseName" },
                values: new object[] { 1, "READ", null });

            migrationBuilder.InsertData(
                table: "PreListOrderStatuses",
                columns: new[] { "Id", "Name", "VietnameseName" },
                values: new object[] { 2, "READY_TOS_ERVE", null });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PreListOrderStatuses",
                keyColumn: "Id",
                keyValue: 0);

            migrationBuilder.DeleteData(
                table: "PreListOrderStatuses",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "PreListOrderStatuses",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
