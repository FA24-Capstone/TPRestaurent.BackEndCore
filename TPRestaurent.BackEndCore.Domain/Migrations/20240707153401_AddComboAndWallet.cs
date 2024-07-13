using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddComboAndWallet : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_Orders_OrderId",
                table: "OrderDetails");

            migrationBuilder.DropTable(
                name: "DishIngredients");

            migrationBuilder.DropTable(
                name: "Ingredients");

            migrationBuilder.AddColumn<Guid>(
                name: "ComboId",
                table: "StaticFiles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "OrderId",
                table: "OrderDetails",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ComboId",
                table: "OrderDetails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DishItemType",
                table: "Dishes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "isAvailable",
                table: "Dishes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Combos",
                columns: table => new
                {
                    ComboId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Combos", x => x.ComboId);
                });

            migrationBuilder.CreateTable(
                name: "DishItemTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DishItemTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StoreCredits",
                columns: table => new
                {
                    StoreCreditId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<double>(type: "float", nullable: false),
                    CustomerInfoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreCredits", x => x.StoreCreditId);
                    table.ForeignKey(
                        name: "FK_StoreCredits_CustomerInfos_CustomerInfoId",
                        column: x => x.CustomerInfoId,
                        principalTable: "CustomerInfos",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "DishCombos",
                columns: table => new
                {
                    DishComboId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HasOptions = table.Column<bool>(type: "bit", nullable: false),
                    OptionSetNumber = table.Column<int>(type: "int", nullable: true),
                    DishId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComboId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DishCombos", x => x.DishComboId);
                    table.ForeignKey(
                        name: "FK_DishCombos_Combos_ComboId",
                        column: x => x.ComboId,
                        principalTable: "Combos",
                        principalColumn: "ComboId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_DishCombos_Dishes_DishId",
                        column: x => x.DishId,
                        principalTable: "Dishes",
                        principalColumn: "DishId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "StoreCreditHistories",
                columns: table => new
                {
                    StoreCreditHistoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsInput = table.Column<bool>(type: "bit", nullable: false),
                    Amount = table.Column<double>(type: "float", nullable: false),
                    StoreCreditId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreCreditHistories", x => x.StoreCreditHistoryId);
                    table.ForeignKey(
                        name: "FK_StoreCreditHistories_StoreCredits_StoreCreditId",
                        column: x => x.StoreCreditId,
                        principalTable: "StoreCredits",
                        principalColumn: "StoreCreditId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StaticFiles_ComboId",
                table: "StaticFiles",
                column: "ComboId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_ComboId",
                table: "OrderDetails",
                column: "ComboId");

            migrationBuilder.CreateIndex(
                name: "IX_DishCombos_ComboId",
                table: "DishCombos",
                column: "ComboId");

            migrationBuilder.CreateIndex(
                name: "IX_DishCombos_DishId",
                table: "DishCombos",
                column: "DishId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreCreditHistories_StoreCreditId",
                table: "StoreCreditHistories",
                column: "StoreCreditId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreCredits_CustomerInfoId",
                table: "StoreCredits",
                column: "CustomerInfoId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_Combos_ComboId",
                table: "OrderDetails",
                column: "ComboId",
                principalTable: "Combos",
                principalColumn: "ComboId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_Orders_OrderId",
                table: "OrderDetails",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_StaticFiles_Combos_ComboId",
                table: "StaticFiles",
                column: "ComboId",
                principalTable: "Combos",
                principalColumn: "ComboId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_Combos_ComboId",
                table: "OrderDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_Orders_OrderId",
                table: "OrderDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_StaticFiles_Combos_ComboId",
                table: "StaticFiles");

            migrationBuilder.DropTable(
                name: "DishCombos");

            migrationBuilder.DropTable(
                name: "DishItemTypes");

            migrationBuilder.DropTable(
                name: "StoreCreditHistories");

            migrationBuilder.DropTable(
                name: "Combos");

            migrationBuilder.DropTable(
                name: "StoreCredits");

            migrationBuilder.DropIndex(
                name: "IX_StaticFiles_ComboId",
                table: "StaticFiles");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetails_ComboId",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "ComboId",
                table: "StaticFiles");

            migrationBuilder.DropColumn(
                name: "ComboId",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "DishItemType",
                table: "Dishes");

            migrationBuilder.DropColumn(
                name: "isAvailable",
                table: "Dishes");

            migrationBuilder.AlterColumn<Guid>(
                name: "OrderId",
                table: "OrderDetails",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateTable(
                name: "Ingredients",
                columns: table => new
                {
                    IngredientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingredients", x => x.IngredientId);
                });

            migrationBuilder.CreateTable(
                name: "DishIngredients",
                columns: table => new
                {
                    DishIngredientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DishId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IngredientId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DishIngredients", x => x.DishIngredientId);
                    table.ForeignKey(
                        name: "FK_DishIngredients_Dishes_DishId",
                        column: x => x.DishId,
                        principalTable: "Dishes",
                        principalColumn: "DishId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_DishIngredients_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "IngredientId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DishIngredients_DishId",
                table: "DishIngredients",
                column: "DishId");

            migrationBuilder.CreateIndex(
                name: "IX_DishIngredients_IngredientId",
                table: "DishIngredients",
                column: "IngredientId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_Orders_OrderId",
                table: "OrderDetails",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderId");
        }
    }
}
