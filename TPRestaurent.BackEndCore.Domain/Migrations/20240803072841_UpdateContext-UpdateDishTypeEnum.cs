using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class UpdateContextUpdateDishTypeEnum : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Combos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ComboCategory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComboCategory", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 0,
                column: "Name",
                value: "APPETIZER");

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

            migrationBuilder.InsertData(
                table: "DishItemTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 6, "BBQ_SEAFOOD" },
                    { 7, "HOTPOT_TOPPING" },
                    { 8, "BBQ_TOPPING" },
                    { 9, "SIDEDISH" },
                    { 10, "DRINK" },
                    { 11, "DESSERT" },
                    { 12, "SAUCE" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Combos_CategoryId",
                table: "Combos",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Combos_ComboCategory_CategoryId",
                table: "Combos",
                column: "CategoryId",
                principalTable: "ComboCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Combos_ComboCategory_CategoryId",
                table: "Combos");

            migrationBuilder.DropTable(
                name: "ComboCategory");

            migrationBuilder.DropIndex(
                name: "IX_Combos_CategoryId",
                table: "Combos");

            migrationBuilder.DeleteData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Combos");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 0,
                column: "Name",
                value: "BROTH");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "TOPPINGS");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "SIDEDISH");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "DRINK");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "DESSERT");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "SAUCE");
        }
    }
}
