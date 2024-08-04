using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddDiscountToCombo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Discount",
                table: "Combos",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discount",
                table: "Combos");
        }
    }
}
