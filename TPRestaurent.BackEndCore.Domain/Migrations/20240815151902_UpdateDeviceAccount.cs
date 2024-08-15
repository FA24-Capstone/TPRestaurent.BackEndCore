using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class UpdateDeviceAccount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Tables_TableId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Tables_AspNetUsers_AccountId",
                table: "Tables");

            migrationBuilder.DropIndex(
                name: "IX_Tables_AccountId",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Tables");

            migrationBuilder.RenameColumn(
                name: "TableId",
                table: "AspNetUsers",
                newName: "DeviceId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_TableId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_DeviceId");

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
                name: "FK_AspNetUsers_Devices_DeviceId",
                table: "AspNetUsers",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "DeviceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_AspNetUsers_AccountId",
                table: "Devices",
                column: "AccountId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Devices_DeviceId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Devices_AspNetUsers_AccountId",
                table: "Devices");

            migrationBuilder.DropIndex(
                name: "IX_Devices_AccountId",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Devices");

            migrationBuilder.RenameColumn(
                name: "DeviceId",
                table: "AspNetUsers",
                newName: "TableId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_DeviceId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_TableId");

            migrationBuilder.AddColumn<string>(
                name: "AccountId",
                table: "Tables",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tables_AccountId",
                table: "Tables",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Tables_TableId",
                table: "AspNetUsers",
                column: "TableId",
                principalTable: "Tables",
                principalColumn: "TableId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tables_AspNetUsers_AccountId",
                table: "Tables",
                column: "AccountId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
