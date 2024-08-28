using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddCustomerInfoidToTableSession : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "PrelistOrders",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Note",
                table: "PrelistOrders");
        }
    }
}
