using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddTiming : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CancelTime",
                table: "OrderSession",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadyToServeTime",
                table: "OrderSession",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartProcessingTime",
                table: "OrderSession",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelDeliveryReason",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelTime",
                table: "OrderDetails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HandOverTime",
                table: "OrderDetails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartProcessingTime",
                table: "OrderDetails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreparationTime",
                table: "Combos",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelTime",
                table: "OrderSession");

            migrationBuilder.DropColumn(
                name: "ReadyToServeTime",
                table: "OrderSession");

            migrationBuilder.DropColumn(
                name: "StartProcessingTime",
                table: "OrderSession");

            migrationBuilder.DropColumn(
                name: "CancelDeliveryReason",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CancelTime",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "HandOverTime",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "StartProcessingTime",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "PreparationTime",
                table: "Combos");
        }
    }
}