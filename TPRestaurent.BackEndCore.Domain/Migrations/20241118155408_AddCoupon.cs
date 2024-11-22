using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddCoupon : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Coupons",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "AssignedCoupons",
                columns: table => new
                {
                    AssignedCouponId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AccountId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CouponProgramId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignedCoupons", x => x.AssignedCouponId);
                    table.ForeignKey(
                        name: "FK_AssignedCoupons_AspNetUsers_AccountId",
                        column: x => x.AccountId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssignedCoupons_Coupons_CouponProgramId",
                        column: x => x.CouponProgramId,
                        principalTable: "Coupons",
                        principalColumn: "CouponProgramId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssignedCoupons_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssignedCoupons_AccountId",
                table: "AssignedCoupons",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignedCoupons_CouponProgramId",
                table: "AssignedCoupons",
                column: "CouponProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignedCoupons_OrderId",
                table: "AssignedCoupons",
                column: "OrderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssignedCoupons");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Coupons");
        }
    }
}