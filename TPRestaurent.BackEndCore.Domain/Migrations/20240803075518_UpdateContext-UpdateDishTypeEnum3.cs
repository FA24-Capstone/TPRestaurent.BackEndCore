using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class UpdateContextUpdateDishTypeEnum3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "SOUP");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "HOTPOT_BROTH");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "HOTPOT_MEAT");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "HOTPOT_SEAFOOD");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "HOTPOT_VEGGIE");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 6,
                column: "Name",
                value: "BBQ_MEAT");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 7,
                column: "Name",
                value: "BBQ_SEAFOOD");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 8,
                column: "Name",
                value: "HOTPOT_TOPPING");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 9,
                column: "Name",
                value: "BBQ_TOPPING");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 10,
                column: "Name",
                value: "SIDEDISH");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 11,
                column: "Name",
                value: "DRINK");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 12,
                column: "Name",
                value: "DESSERT");

            migrationBuilder.InsertData(
                table: "DishItemTypes",
                columns: new[] { "Id", "Name" },
                values: new object[] { 13, "SAUCE" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "HOTPOT_BROTH");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "HOTPOT_MEAT");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "HOTPOT_SEAFOOD");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "HOTPOT_VEGGIE");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "BBQ_MEAT");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 6,
                column: "Name",
                value: "BBQ_SEAFOOD");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 7,
                column: "Name",
                value: "HOTPOT_TOPPING");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 8,
                column: "Name",
                value: "BBQ_TOPPING");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 9,
                column: "Name",
                value: "SIDEDISH");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 10,
                column: "Name",
                value: "DRINK");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 11,
                column: "Name",
                value: "DESSERT");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 12,
                column: "Name",
                value: "SAUCE");
        }
    }
}
