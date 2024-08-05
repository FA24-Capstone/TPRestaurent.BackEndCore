using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class RemovePriceFromDish : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DishCombos_Dishes_DishId",
                table: "DishCombos");

            migrationBuilder.DropColumn(
                name: "Discount",
                table: "Dishes");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Dishes");

            migrationBuilder.RenameColumn(
                name: "Point",
                table: "Ratings",
                newName: "PointId");

            migrationBuilder.RenameColumn(
                name: "PaymentMethod",
                table: "Orders",
                newName: "PaymentMethodId");

            migrationBuilder.RenameColumn(
                name: "DishItemType",
                table: "Dishes",
                newName: "DishItemTypeId");

            migrationBuilder.RenameColumn(
                name: "DishId",
                table: "DishCombos",
                newName: "DishSizeDetailId");

            migrationBuilder.RenameIndex(
                name: "IX_DishCombos_DishId",
                table: "DishCombos",
                newName: "IX_DishCombos_DishSizeDetailId");

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "DishCombos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DishSizes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DishSizes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DishSizeDetails",
                columns: table => new
                {
                    DishSizeDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    Discount = table.Column<double>(type: "float", nullable: false),
                    DishId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DishSizeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DishSizeDetails", x => x.DishSizeDetailId);
                    table.ForeignKey(
                        name: "FK_DishSizeDetails_Dishes_DishId",
                        column: x => x.DishId,
                        principalTable: "Dishes",
                        principalColumn: "DishId");
                    table.ForeignKey(
                        name: "FK_DishSizeDetails_DishSizes_DishSizeId",
                        column: x => x.DishSizeId,
                        principalTable: "DishSizes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.InsertData(
                table: "DishSizes",
                columns: new[] { "Id", "Name" },
                values: new object[] { 0, "SMALL" });

            migrationBuilder.InsertData(
                table: "DishSizes",
                columns: new[] { "Id", "Name" },
                values: new object[] { 1, "MEDIUM" });

            migrationBuilder.InsertData(
                table: "DishSizes",
                columns: new[] { "Id", "Name" },
                values: new object[] { 2, "LARGE" });

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_PointId",
                table: "Ratings",
                column: "PointId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PaymentMethodId",
                table: "Orders",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_Dishes_DishItemTypeId",
                table: "Dishes",
                column: "DishItemTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DishSizeDetails_DishId",
                table: "DishSizeDetails",
                column: "DishId");

            migrationBuilder.CreateIndex(
                name: "IX_DishSizeDetails_DishSizeId",
                table: "DishSizeDetails",
                column: "DishSizeId");

            migrationBuilder.AddForeignKey(
                name: "FK_DishCombos_DishSizeDetails_DishSizeDetailId",
                table: "DishCombos",
                column: "DishSizeDetailId",
                principalTable: "DishSizeDetails",
                principalColumn: "DishSizeDetailId",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Dishes_DishItemTypes_DishItemTypeId",
                table: "Dishes",
                column: "DishItemTypeId",
                principalTable: "DishItemTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_PaymentMethods_PaymentMethodId",
                table: "Orders",
                column: "PaymentMethodId",
                principalTable: "PaymentMethods",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Ratings_RatingPoints_PointId",
                table: "Ratings",
                column: "PointId",
                principalTable: "RatingPoints",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DishCombos_DishSizeDetails_DishSizeDetailId",
                table: "DishCombos");

            migrationBuilder.DropForeignKey(
                name: "FK_Dishes_DishItemTypes_DishItemTypeId",
                table: "Dishes");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_PaymentMethods_PaymentMethodId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Ratings_RatingPoints_PointId",
                table: "Ratings");

            migrationBuilder.DropTable(
                name: "DishSizeDetails");

            migrationBuilder.DropTable(
                name: "DishSizes");

            migrationBuilder.DropIndex(
                name: "IX_Ratings_PointId",
                table: "Ratings");

            migrationBuilder.DropIndex(
                name: "IX_Orders_PaymentMethodId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Dishes_DishItemTypeId",
                table: "Dishes");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "DishCombos");

            migrationBuilder.RenameColumn(
                name: "PointId",
                table: "Ratings",
                newName: "Point");

            migrationBuilder.RenameColumn(
                name: "PaymentMethodId",
                table: "Orders",
                newName: "PaymentMethod");

            migrationBuilder.RenameColumn(
                name: "DishItemTypeId",
                table: "Dishes",
                newName: "DishItemType");

            migrationBuilder.RenameColumn(
                name: "DishSizeDetailId",
                table: "DishCombos",
                newName: "DishId");

            migrationBuilder.RenameIndex(
                name: "IX_DishCombos_DishSizeDetailId",
                table: "DishCombos",
                newName: "IX_DishCombos_DishId");

            migrationBuilder.AddColumn<double>(
                name: "Discount",
                table: "Dishes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Price",
                table: "Dishes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddForeignKey(
                name: "FK_DishCombos_Dishes_DishId",
                table: "DishCombos",
                column: "DishId",
                principalTable: "Dishes",
                principalColumn: "DishId",
                onDelete: ReferentialAction.NoAction);
        }
    }
}
