using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddOrderAssignRequestTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderAssignedStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VietnameseName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderAssignedStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderAssignedRequests",
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
                    table.PrimaryKey("PK_OrderAssignedRequests", x => x.OrderAssignedRequestId);
                    table.ForeignKey(
                        name: "FK_OrderAssignedRequests_AspNetUsers_ShipperAssignedId",
                        column: x => x.ShipperAssignedId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderAssignedRequests_AspNetUsers_ShipperRequestId",
                        column: x => x.ShipperRequestId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderAssignedRequests_OrderAssignedStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "OrderAssignedStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_OrderAssignedRequests_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.InsertData(
                table: "OrderAssignedStatuses",
                columns: new[] { "Id", "Name", "VietnameseName" },
                values: new object[] { 1, "Pending", "Chờ Xử Lý" });

            migrationBuilder.InsertData(
                table: "OrderAssignedStatuses",
                columns: new[] { "Id", "Name", "VietnameseName" },
                values: new object[] { 2, "Completed", "Thành Công" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderAssignedRequests_OrderId",
                table: "OrderAssignedRequests",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderAssignedRequests_ShipperAssignedId",
                table: "OrderAssignedRequests",
                column: "ShipperAssignedId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderAssignedRequests_ShipperRequestId",
                table: "OrderAssignedRequests",
                column: "ShipperRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderAssignedRequests_StatusId",
                table: "OrderAssignedRequests",
                column: "StatusId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderAssignedRequests");

            migrationBuilder.DropTable(
                name: "OrderAssignedStatuses");
        }
    }
}
