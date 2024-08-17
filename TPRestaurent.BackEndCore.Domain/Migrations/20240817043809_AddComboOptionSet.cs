using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddComboOptionSet : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_AspNetUsers_AccountId",
                table: "Devices");

            migrationBuilder.DropForeignKey(
                name: "FK_DishCombos_Combos_ComboId",
                table: "DishCombos");

            migrationBuilder.DropIndex(
                name: "IX_Devices_AccountId",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "HasOptions",
                table: "DishCombos");

            migrationBuilder.DropColumn(
                name: "OptionSetNumber",
                table: "DishCombos");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Devices");

            migrationBuilder.RenameColumn(
                name: "ComboId",
                table: "DishCombos",
                newName: "ComboOptionSetId");

            migrationBuilder.RenameIndex(
                name: "IX_DishCombos_ComboId",
                table: "DishCombos",
                newName: "IX_DishCombos_ComboOptionSetId");

            migrationBuilder.CreateTable(
                name: "ComboOptionSets",
                columns: table => new
                {
                    ComboOptionSetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OptionSetNumber = table.Column<int>(type: "int", nullable: false),
                    NumOfChoice = table.Column<int>(type: "int", nullable: false),
                    DishItemTypeId = table.Column<int>(type: "int", nullable: false),
                    ComboId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComboOptionSets", x => x.ComboOptionSetId);
                    table.ForeignKey(
                        name: "FK_ComboOptionSets_Combos_ComboId",
                        column: x => x.ComboId,
                        principalTable: "Combos",
                        principalColumn: "ComboId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_ComboOptionSets_DishItemTypes_DishItemTypeId",
                        column: x => x.DishItemTypeId,
                        principalTable: "DishItemTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComboOptionSets_ComboId",
                table: "ComboOptionSets",
                column: "ComboId");

            migrationBuilder.CreateIndex(
                name: "IX_ComboOptionSets_DishItemTypeId",
                table: "ComboOptionSets",
                column: "DishItemTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_DishCombos_ComboOptionSets_ComboOptionSetId",
                table: "DishCombos",
                column: "ComboOptionSetId",
                principalTable: "ComboOptionSets",
                principalColumn: "ComboOptionSetId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DishCombos_ComboOptionSets_ComboOptionSetId",
                table: "DishCombos");

            migrationBuilder.DropTable(
                name: "ComboOptionSets");

            migrationBuilder.RenameColumn(
                name: "ComboOptionSetId",
                table: "DishCombos",
                newName: "ComboId");

            migrationBuilder.RenameIndex(
                name: "IX_DishCombos_ComboOptionSetId",
                table: "DishCombos",
                newName: "IX_DishCombos_ComboId");

            migrationBuilder.AddColumn<bool>(
                name: "HasOptions",
                table: "DishCombos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OptionSetNumber",
                table: "DishCombos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountId",
                table: "Devices",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Devices_AccountId",
                table: "Devices",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_AspNetUsers_AccountId",
                table: "Devices",
                column: "AccountId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DishCombos_Combos_ComboId",
                table: "DishCombos",
                column: "ComboId",
                principalTable: "Combos",
                principalColumn: "ComboId");
        }
    }
}
