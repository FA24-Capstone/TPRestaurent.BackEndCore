using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class FixTypos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_UserRanges_UserRangeId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_CouponPrograms_UserRanges_UserRangeId",
                table: "CouponPrograms");

            migrationBuilder.DropTable(
                name: "UserRanges");

            migrationBuilder.RenameColumn(
                name: "UserRangeId",
                table: "CouponPrograms",
                newName: "UserRankId");

            migrationBuilder.RenameIndex(
                name: "IX_CouponPrograms_UserRangeId",
                table: "CouponPrograms",
                newName: "IX_CouponPrograms_UserRankId");

            migrationBuilder.RenameColumn(
                name: "UserRangeId",
                table: "AspNetUsers",
                newName: "UserRankId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_UserRangeId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_UserRankId");

            migrationBuilder.CreateTable(
                name: "UserRanks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VietnameseName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRanks", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "UserRanks",
                columns: new[] { "Id", "Name", "VietnameseName" },
                values: new object[,]
                {
                    { 1, "BRONZE", "Đồng" },
                    { 2, "SILVER", "Bạc" },
                    { 3, "GOLD", "Vàng" },
                    { 4, "DIAMOND", "Kim cương" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_UserRanks_UserRankId",
                table: "AspNetUsers",
                column: "UserRankId",
                principalTable: "UserRanks",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CouponPrograms_UserRanks_UserRankId",
                table: "CouponPrograms",
                column: "UserRankId",
                principalTable: "UserRanks",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_UserRanks_UserRankId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_CouponPrograms_UserRanks_UserRankId",
                table: "CouponPrograms");

            migrationBuilder.DropTable(
                name: "UserRanks");

            migrationBuilder.RenameColumn(
                name: "UserRankId",
                table: "CouponPrograms",
                newName: "UserRangeId");

            migrationBuilder.RenameIndex(
                name: "IX_CouponPrograms_UserRankId",
                table: "CouponPrograms",
                newName: "IX_CouponPrograms_UserRangeId");

            migrationBuilder.RenameColumn(
                name: "UserRankId",
                table: "AspNetUsers",
                newName: "UserRangeId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_UserRankId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_UserRangeId");

            migrationBuilder.CreateTable(
                name: "UserRanges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VietnameseName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRanges", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "UserRanges",
                columns: new[] { "Id", "Name", "VietnameseName" },
                values: new object[,]
                {
                    { 1, "BRONZE", "Đồng" },
                    { 2, "SILVER", "Bạc" },
                    { 3, "GOLD", "Vàng" },
                    { 4, "DIAMOND", "Kim cương" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_UserRanges_UserRangeId",
                table: "AspNetUsers",
                column: "UserRangeId",
                principalTable: "UserRanges",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CouponPrograms_UserRanges_UserRangeId",
                table: "CouponPrograms",
                column: "UserRangeId",
                principalTable: "UserRanges",
                principalColumn: "Id");
        }
    }
}
