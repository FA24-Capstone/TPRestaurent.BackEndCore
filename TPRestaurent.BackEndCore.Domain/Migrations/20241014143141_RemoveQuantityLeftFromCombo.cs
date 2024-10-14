using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class RemoveQuantityLeftFromCombo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyCountdown",
                table: "Combos");

            migrationBuilder.DropColumn(
                name: "QuantityLeft",
                table: "Combos");

            migrationBuilder.AddColumn<int>(
                name: "DailyCountdown",
                table: "DishCombos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "DishCombos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "QuantityLeft",
                table: "DishCombos",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyCountdown",
                table: "DishCombos");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "DishCombos");

            migrationBuilder.DropColumn(
                name: "QuantityLeft",
                table: "DishCombos");

            migrationBuilder.AddColumn<int>(
                name: "DailyCountdown",
                table: "Combos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuantityLeft",
                table: "Combos",
                type: "int",
                nullable: true);
        }
    }
}
