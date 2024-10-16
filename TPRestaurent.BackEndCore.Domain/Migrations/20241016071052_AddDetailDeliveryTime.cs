using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddDetailDeliveryTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeliveryTime",
                table: "Orders",
                newName: "StartDeliveringTime");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveredTime",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NotifyTime",
                table: "NotificationMessages",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveredTime",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "NotifyTime",
                table: "NotificationMessages");

            migrationBuilder.RenameColumn(
                name: "StartDeliveringTime",
                table: "Orders",
                newName: "DeliveryTime");
        }
    }
}
