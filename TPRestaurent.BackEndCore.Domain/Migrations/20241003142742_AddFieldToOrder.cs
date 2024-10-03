using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddFieldToOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShipperId",
                table: "Orders",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValidatingImg",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ShipperId",
                table: "Orders",
                column: "ShipperId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AspNetUsers_ShipperId",
                table: "Orders",
                column: "ShipperId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AspNetUsers_ShipperId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ShipperId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShipperId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ValidatingImg",
                table: "Orders");
        }
    }
}
