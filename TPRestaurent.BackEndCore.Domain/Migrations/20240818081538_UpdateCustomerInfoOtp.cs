using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class UpdateCustomerInfoOtp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OTPs_AspNetUsers_AccountId",
                table: "OTPs");

            migrationBuilder.AlterColumn<string>(
                name: "AccountId",
                table: "OTPs",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerInfoId",
                table: "OTPs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "CustomerInfos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "DOB",
                table: "CustomerInfos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Gender",
                table: "CustomerInfos",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "CustomerInfos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "VerifyCode",
                table: "CustomerInfos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OTPs_CustomerInfoId",
                table: "OTPs",
                column: "CustomerInfoId");

            migrationBuilder.AddForeignKey(
                name: "FK_OTPs_AspNetUsers_AccountId",
                table: "OTPs",
                column: "AccountId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OTPs_CustomerInfos_CustomerInfoId",
                table: "OTPs",
                column: "CustomerInfoId",
                principalTable: "CustomerInfos",
                principalColumn: "CustomerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OTPs_AspNetUsers_AccountId",
                table: "OTPs");

            migrationBuilder.DropForeignKey(
                name: "FK_OTPs_CustomerInfos_CustomerInfoId",
                table: "OTPs");

            migrationBuilder.DropIndex(
                name: "IX_OTPs_CustomerInfoId",
                table: "OTPs");

            migrationBuilder.DropColumn(
                name: "CustomerInfoId",
                table: "OTPs");

            migrationBuilder.DropColumn(
                name: "DOB",
                table: "CustomerInfos");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "CustomerInfos");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "CustomerInfos");

            migrationBuilder.DropColumn(
                name: "VerifyCode",
                table: "CustomerInfos");

            migrationBuilder.AlterColumn<string>(
                name: "AccountId",
                table: "OTPs",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "CustomerInfos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OTPs_AspNetUsers_AccountId",
                table: "OTPs",
                column: "AccountId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }
    }
}
