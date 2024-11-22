using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class RemoveCouponForRename : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssignedCoupons");

            migrationBuilder.DropTable(
                name: "Coupons");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Coupons",
                columns: table => new
                {
                    CouponProgramId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiscountPercent = table.Column<int>(type: "int", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Img = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    MinimumAmount = table.Column<double>(type: "float", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coupons", x => x.CouponProgramId);
                    table.ForeignKey(
                        name: "FK_Coupons_AspNetUsers_AccountId",
                        column: x => x.AccountId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AssignedCoupons",
                columns: table => new
                {
                    AssignedCouponId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CouponProgramId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignedCoupons", x => x.AssignedCouponId);
                    table.ForeignKey(
                        name: "FK_AssignedCoupons_AspNetUsers_AccountId",
                        column: x => x.AccountId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_AssignedCoupons_Coupons_CouponProgramId",
                        column: x => x.CouponProgramId,
                        principalTable: "Coupons",
                        principalColumn: "CouponProgramId",
                        onDelete: ReferentialAction.NoAction);
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

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_AccountId",
                table: "Coupons",
                column: "AccountId");
        }
    }
}