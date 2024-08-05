using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class UpdateTableTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Capacity",
                table: "Tables",
                newName: "TableSizeId");

            migrationBuilder.CreateTable(
                name: "ReservationRequestStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationRequestStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TableSize",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableSize", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "DishItemTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 0, "BROTH" },
                    { 1, "TOPPINGS" },
                    { 2, "SIDEDISH" },
                    { 3, "DRINK" },
                    { 4, "DESSERT" },
                    { 5, "SAUCE" }
                });

            migrationBuilder.InsertData(
                table: "ReservationRequestStatus",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 0, "PENDING" },
                    { 1, "SUCCESSFUL" },
                    { 2, "FAILED" }
                });

            migrationBuilder.InsertData(
                table: "TableSize",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 2, "TWO" },
                    { 4, "FOUR" },
                    { 6, "SIX" },
                    { 8, "EIGHT" },
                    { 10, "TEN" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tables_TableSizeId",
                table: "Tables",
                column: "TableSizeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tables_TableSize_TableSizeId",
                table: "Tables",
                column: "TableSizeId",
                principalTable: "TableSize",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tables_TableSize_TableSizeId",
                table: "Tables");

            migrationBuilder.DropTable(
                name: "ReservationRequestStatus");

            migrationBuilder.DropTable(
                name: "TableSize");

            migrationBuilder.DropIndex(
                name: "IX_Tables_TableSizeId",
                table: "Tables");

            migrationBuilder.DeleteData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 0);

            migrationBuilder.DeleteData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.RenameColumn(
                name: "TableSizeId",
                table: "Tables",
                newName: "Capacity");
        }
    }
}
