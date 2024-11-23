using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddCreatorCouponProgram : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CouponPrograms_AspNetUsers_AccountId",
                table: "CouponPrograms");

            migrationBuilder.DropIndex(
                name: "IX_CouponPrograms_AccountId",
                table: "CouponPrograms");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "CouponPrograms");

            migrationBuilder.AddColumn<bool>(
                name: "IsUsedOrExpired",
                table: "Coupons",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUsedOrExpired",
                table: "Coupons");

            migrationBuilder.AddColumn<string>(
                name: "AccountId",
                table: "CouponPrograms",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CouponPrograms_AccountId",
                table: "CouponPrograms",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_CouponPrograms_AspNetUsers_AccountId",
                table: "CouponPrograms",
                column: "AccountId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}