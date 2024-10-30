using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddAssignOrderRequest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderAssignedStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VietnameseName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderAssignedStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderAssignedRequest",
                columns: table => new
                {
                    OrderAssignedRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssignedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ShipperRequestId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ShipperAssignedId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    Reasons = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderAssignedRequest", x => x.OrderAssignedRequestId);
                    table.ForeignKey(
                        name: "FK_OrderAssignedRequest_AspNetUsers_ShipperAssignedId",
                        column: x => x.ShipperAssignedId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderAssignedRequest_AspNetUsers_ShipperRequestId",
                        column: x => x.ShipperRequestId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderAssignedRequest_OrderAssignedStatus_StatusId",
                        column: x => x.StatusId,
                        principalTable: "OrderAssignedStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_OrderAssignedRequest_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.InsertData(
                table: "OrderAssignedStatus",
                columns: new[] { "Id", "Name", "VietnameseName" },
                values: new object[] { 1, "Pending", "Chờ Xử Lý" });

            migrationBuilder.InsertData(
                table: "OrderAssignedStatus",
                columns: new[] { "Id", "Name", "VietnameseName" },
                values: new object[] { 2, "Completed", "Thành Công" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderAssignedRequest_OrderId",
                table: "OrderAssignedRequest",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderAssignedRequest_ShipperAssignedId",
                table: "OrderAssignedRequest",
                column: "ShipperAssignedId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderAssignedRequest_ShipperRequestId",
                table: "OrderAssignedRequest",
                column: "ShipperRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderAssignedRequest_StatusId",
                table: "OrderAssignedRequest",
                column: "StatusId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderAssignedRequest");

            migrationBuilder.DropTable(
                name: "OrderAssignedStatus");
        }
    }
}
