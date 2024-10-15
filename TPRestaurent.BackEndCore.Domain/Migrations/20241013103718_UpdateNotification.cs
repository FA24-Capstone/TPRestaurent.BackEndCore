using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class UpdateNotification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountId",
                table: "NotificationMessages",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationMessages_AccountId",
                table: "NotificationMessages",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationMessages_AspNetUsers_AccountId",
                table: "NotificationMessages",
                column: "AccountId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationMessages_AspNetUsers_AccountId",
                table: "NotificationMessages");

            migrationBuilder.DropIndex(
                name: "IX_NotificationMessages_AccountId",
                table: "NotificationMessages");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "NotificationMessages");
        }
    }
}
