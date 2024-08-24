using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class UpdateFieldRalateToAccount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerSavedCoupons_CustomerInfos_CustomerInfoId",
                table: "CustomerSavedCoupons");

            migrationBuilder.DropForeignKey(
                name: "FK_StoreCredits_CustomerInfos_CustomerInfoId",
                table: "StoreCredits");

            migrationBuilder.DropIndex(
                name: "IX_StoreCredits_CustomerInfoId",
                table: "StoreCredits");

            migrationBuilder.DropIndex(
                name: "IX_CustomerSavedCoupons_CustomerInfoId",
                table: "CustomerSavedCoupons");

            migrationBuilder.DropColumn(
                name: "CustomerInfoId",
                table: "StoreCredits");

            migrationBuilder.DropColumn(
                name: "CustomerInfoId",
                table: "CustomerSavedCoupons");

            migrationBuilder.DropColumn(
                name: "LoyaltyPoint",
                table: "CustomerInfos");

            migrationBuilder.AddColumn<string>(
                name: "AccountId",
                table: "StoreCredits",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AccountId",
                table: "CustomerSavedCoupons",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "LoyaltyPoint",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "PreListOrderStatuses",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "READY_TO_SERVE");

            migrationBuilder.CreateIndex(
                name: "IX_StoreCredits_AccountId",
                table: "StoreCredits",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerSavedCoupons_AccountId",
                table: "CustomerSavedCoupons",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerSavedCoupons_AspNetUsers_AccountId",
                table: "CustomerSavedCoupons",
                column: "AccountId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_StoreCredits_AspNetUsers_AccountId",
                table: "StoreCredits",
                column: "AccountId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerSavedCoupons_AspNetUsers_AccountId",
                table: "CustomerSavedCoupons");

            migrationBuilder.DropForeignKey(
                name: "FK_StoreCredits_AspNetUsers_AccountId",
                table: "StoreCredits");

            migrationBuilder.DropIndex(
                name: "IX_StoreCredits_AccountId",
                table: "StoreCredits");

            migrationBuilder.DropIndex(
                name: "IX_CustomerSavedCoupons_AccountId",
                table: "CustomerSavedCoupons");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "StoreCredits");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "CustomerSavedCoupons");

            migrationBuilder.DropColumn(
                name: "LoyaltyPoint",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerInfoId",
                table: "StoreCredits",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerInfoId",
                table: "CustomerSavedCoupons",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "LoyaltyPoint",
                table: "CustomerInfos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "PreListOrderStatuses",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "READY_TOS_ERVE");

            migrationBuilder.CreateIndex(
                name: "IX_StoreCredits_CustomerInfoId",
                table: "StoreCredits",
                column: "CustomerInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerSavedCoupons_CustomerInfoId",
                table: "CustomerSavedCoupons",
                column: "CustomerInfoId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerSavedCoupons_CustomerInfos_CustomerInfoId",
                table: "CustomerSavedCoupons",
                column: "CustomerInfoId",
                principalTable: "CustomerInfos",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_StoreCredits_CustomerInfos_CustomerInfoId",
                table: "StoreCredits",
                column: "CustomerInfoId",
                principalTable: "CustomerInfos",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.NoAction);
        }
    }
}
