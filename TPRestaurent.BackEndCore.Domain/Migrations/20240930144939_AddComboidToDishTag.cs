using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddComboidToDishTag : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ComboId",
                table: "DishTags",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "ReadyForDelivery", "Sẳn sàng để giao" });

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "Delivering", "Đang Giao" });

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "Completed", "Thành Công" });

            migrationBuilder.InsertData(
                table: "OrderStatuses",
                columns: new[] { "Id", "Name", "VietnameseName" },
                values: new object[] { 9, "Cancelled", "Đã Huỷ" });

            migrationBuilder.CreateIndex(
                name: "IX_DishTags_ComboId",
                table: "DishTags",
                column: "ComboId");

            migrationBuilder.AddForeignKey(
                name: "FK_DishTags_Combos_ComboId",
                table: "DishTags",
                column: "ComboId",
                principalTable: "Combos",
                principalColumn: "ComboId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DishTags_Combos_ComboId",
                table: "DishTags");

            migrationBuilder.DropIndex(
                name: "IX_DishTags_ComboId",
                table: "DishTags");

            migrationBuilder.DeleteData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DropColumn(
                name: "ComboId",
                table: "DishTags");

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "Delivering", "Đang Giao" });

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "Completed", "Thành Công" });

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "Cancelled", "Đã Huỷ" });
        }
    }
}