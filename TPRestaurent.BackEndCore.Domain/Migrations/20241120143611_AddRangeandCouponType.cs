using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddRangeandCouponType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CouponPrograms_AspNetUsers_CreateBy",
                table: "CouponPrograms");

            migrationBuilder.DropForeignKey(
                name: "FK_Coupons_AspNetUsers_AccountId",
                table: "Coupons");

            migrationBuilder.DropForeignKey(
                name: "FK_Coupons_CouponPrograms_CouponProgramId",
                table: "Coupons");

            migrationBuilder.AddColumn<int>(
                name: "CouponProgramTypeId",
                table: "CouponPrograms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UserRangeId",
                table: "CouponPrograms",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserRangeId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CouponProgramTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VietnameseName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponProgramTypes", x => x.Id);
                });

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
                table: "CouponProgramTypes",
                columns: new[] { "Id", "Name", "VietnameseName" },
                values: new object[,]
                {
                    { 1, "BIRTHDAY", "Sinh nhật" },
                    { 2, "NEWBIE", "Khách hàng mới" },
                    { 3, "TO_RANGE", "Theo phân hạng" }
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

            migrationBuilder.CreateIndex(
                name: "IX_CouponPrograms_CouponProgramTypeId",
                table: "CouponPrograms",
                column: "CouponProgramTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CouponPrograms_UserRangeId",
                table: "CouponPrograms",
                column: "UserRangeId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_UserRangeId",
                table: "AspNetUsers",
                column: "UserRangeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_UserRanges_UserRangeId",
                table: "AspNetUsers",
                column: "UserRangeId",
                principalTable: "UserRanges",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CouponPrograms_AspNetUsers_CreateBy",
                table: "CouponPrograms",
                column: "CreateBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_CouponPrograms_CouponProgramTypes_CouponProgramTypeId",
                table: "CouponPrograms",
                column: "CouponProgramTypeId",
                principalTable: "CouponProgramTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_CouponPrograms_UserRanges_UserRangeId",
                table: "CouponPrograms",
                column: "UserRangeId",
                principalTable: "UserRanges",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Coupons_AspNetUsers_AccountId",
                table: "Coupons",
                column: "AccountId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Coupons_CouponPrograms_CouponProgramId",
                table: "Coupons",
                column: "CouponProgramId",
                principalTable: "CouponPrograms",
                principalColumn: "CouponProgramId",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_UserRanges_UserRangeId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_CouponPrograms_AspNetUsers_CreateBy",
                table: "CouponPrograms");

            migrationBuilder.DropForeignKey(
                name: "FK_CouponPrograms_CouponProgramTypes_CouponProgramTypeId",
                table: "CouponPrograms");

            migrationBuilder.DropForeignKey(
                name: "FK_CouponPrograms_UserRanges_UserRangeId",
                table: "CouponPrograms");

            migrationBuilder.DropForeignKey(
                name: "FK_Coupons_AspNetUsers_AccountId",
                table: "Coupons");

            migrationBuilder.DropForeignKey(
                name: "FK_Coupons_CouponPrograms_CouponProgramId",
                table: "Coupons");

            migrationBuilder.DropTable(
                name: "CouponProgramTypes");

            migrationBuilder.DropTable(
                name: "UserRanges");

            migrationBuilder.DropIndex(
                name: "IX_CouponPrograms_CouponProgramTypeId",
                table: "CouponPrograms");

            migrationBuilder.DropIndex(
                name: "IX_CouponPrograms_UserRangeId",
                table: "CouponPrograms");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_UserRangeId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CouponProgramTypeId",
                table: "CouponPrograms");

            migrationBuilder.DropColumn(
                name: "UserRangeId",
                table: "CouponPrograms");

            migrationBuilder.DropColumn(
                name: "UserRangeId",
                table: "AspNetUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_CouponPrograms_AspNetUsers_CreateBy",
                table: "CouponPrograms",
                column: "CreateBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Coupons_AspNetUsers_AccountId",
                table: "Coupons",
                column: "AccountId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Coupons_CouponPrograms_CouponProgramId",
                table: "Coupons",
                column: "CouponProgramId",
                principalTable: "CouponPrograms",
                principalColumn: "CouponProgramId");
        }
    }
}