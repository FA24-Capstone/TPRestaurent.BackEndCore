using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddOrderSessionStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderSession_OrderSessionStatus_OrderSessionStatusId",
                table: "OrderSession");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderSessionStatus",
                table: "OrderSessionStatus");

            migrationBuilder.RenameTable(
                name: "OrderSessionStatus",
                newName: "OrderSessionStatuses");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderSessionStatuses",
                table: "OrderSessionStatuses",
                column: "Id");

            migrationBuilder.InsertData(
                table: "OrderSessionStatuses",
                columns: new[] { "Id", "Name", "VietnameseName" },
                values: new object[,]
                {
                    { 0, "PreOrder", "Đã đặt trước" },
                    { 1, "Confirmed", "Xác nhận" },
                    { 2, "Processing", "Đang Xử Lý" },
                    { 3, "Completed", "Thành Công" },
                    { 4, "Cancelled", "Đã Huỷ" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_OrderSession_OrderSessionStatuses_OrderSessionStatusId",
                table: "OrderSession",
                column: "OrderSessionStatusId",
                principalTable: "OrderSessionStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderSession_OrderSessionStatuses_OrderSessionStatusId",
                table: "OrderSession");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderSessionStatuses",
                table: "OrderSessionStatuses");

            migrationBuilder.DeleteData(
                table: "OrderSessionStatuses",
                keyColumn: "Id",
                keyValue: 0);

            migrationBuilder.DeleteData(
                table: "OrderSessionStatuses",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "OrderSessionStatuses",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "OrderSessionStatuses",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "OrderSessionStatuses",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.RenameTable(
                name: "OrderSessionStatuses",
                newName: "OrderSessionStatus");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderSessionStatus",
                table: "OrderSessionStatus",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderSession_OrderSessionStatus_OrderSessionStatusId",
                table: "OrderSession",
                column: "OrderSessionStatusId",
                principalTable: "OrderSessionStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }
    }
}
