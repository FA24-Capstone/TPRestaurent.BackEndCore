using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddVietnameseFieldName2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VietnameseName",
                table: "TableSizes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VietnameseName",
                table: "ReservationStatuses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VietnameseName",
                table: "ReservationRequestStatuses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VietnameseName",
                table: "RatingPoints",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VietnameseName",
                table: "PaymentMethods",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VietnameseName",
                table: "OTPTypes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VietnameseName",
                table: "OrderStatuses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VietnameseName",
                table: "DishSizes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VietnameseName",
                table: "DishItemTypes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VietnameseName",
                table: "ComboCategories",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VietnameseName",
                table: "TableSizes");

            migrationBuilder.DropColumn(
                name: "VietnameseName",
                table: "ReservationStatuses");

            migrationBuilder.DropColumn(
                name: "VietnameseName",
                table: "ReservationRequestStatuses");

            migrationBuilder.DropColumn(
                name: "VietnameseName",
                table: "RatingPoints");

            migrationBuilder.DropColumn(
                name: "VietnameseName",
                table: "PaymentMethods");

            migrationBuilder.DropColumn(
                name: "VietnameseName",
                table: "OTPTypes");

            migrationBuilder.DropColumn(
                name: "VietnameseName",
                table: "OrderStatuses");

            migrationBuilder.DropColumn(
                name: "VietnameseName",
                table: "DishSizes");

            migrationBuilder.DropColumn(
                name: "VietnameseName",
                table: "DishItemTypes");

            migrationBuilder.DropColumn(
                name: "VietnameseName",
                table: "ComboCategories");
        }
    }
}
