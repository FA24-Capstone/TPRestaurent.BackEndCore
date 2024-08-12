using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class RemoveReservationAccount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_AspNetUsers_CustomerAccountId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_CustomerAccountId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "CustomerAccountId",
                table: "Reservations");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerAccountId",
                table: "Reservations",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_CustomerAccountId",
                table: "Reservations",
                column: "CustomerAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_AspNetUsers_CustomerAccountId",
                table: "Reservations",
                column: "CustomerAccountId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
