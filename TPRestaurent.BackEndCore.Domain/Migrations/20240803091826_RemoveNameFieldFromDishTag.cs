using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class RemoveNameFieldFromDishTag : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Combos_ComboCategory_CategoryId",
                table: "Combos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ComboCategory",
                table: "ComboCategory");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "DishTags");

            migrationBuilder.RenameTable(
                name: "ComboCategory",
                newName: "ComboCategories");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ComboCategories",
                table: "ComboCategories",
                column: "Id");

            //migrationBuilder.InsertData(
            //    table: "ComboCategories",
            //    columns: new[] { "Id", "Name" },
            //    values: new object[] { 0, "HOTPOT" });

            //migrationBuilder.InsertData(
            //    table: "ComboCategories",
            //    columns: new[] { "Id", "Name" },
            //    values: new object[] { 1, "BBQ" });

            //migrationBuilder.InsertData(
            //    table: "ComboCategories",
            //    columns: new[] { "Id", "Name" },
            //    values: new object[] { 2, "BOTH" });

            migrationBuilder.AddForeignKey(
                name: "FK_Combos_ComboCategories_CategoryId",
                table: "Combos",
                column: "CategoryId",
                principalTable: "ComboCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Combos_ComboCategories_CategoryId",
                table: "Combos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ComboCategories",
                table: "ComboCategories");

            migrationBuilder.DeleteData(
                table: "ComboCategories",
                keyColumn: "Id",
                keyValue: 0);

            migrationBuilder.DeleteData(
                table: "ComboCategories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ComboCategories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.RenameTable(
                name: "ComboCategories",
                newName: "ComboCategory");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "DishTags",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ComboCategory",
                table: "ComboCategory",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Combos_ComboCategory_CategoryId",
                table: "Combos",
                column: "CategoryId",
                principalTable: "ComboCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }
    }
}
