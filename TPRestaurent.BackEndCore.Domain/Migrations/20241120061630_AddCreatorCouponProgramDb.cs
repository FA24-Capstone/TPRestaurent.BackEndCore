using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddCreatorCouponProgramDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreateBy",
                table: "CouponPrograms",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "CouponPrograms",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdateBy",
                table: "CouponPrograms",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateDate",
                table: "CouponPrograms",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_CouponPrograms_CreateBy",
                table: "CouponPrograms",
                column: "CreateBy");

            migrationBuilder.CreateIndex(
                name: "IX_CouponPrograms_UpdateBy",
                table: "CouponPrograms",
                column: "UpdateBy");

            migrationBuilder.AddForeignKey(
                name: "FK_CouponPrograms_AspNetUsers_CreateBy",
                table: "CouponPrograms",
                column: "CreateBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_CouponPrograms_AspNetUsers_UpdateBy",
                table: "CouponPrograms",
                column: "UpdateBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CouponPrograms_AspNetUsers_CreateBy",
                table: "CouponPrograms");

            migrationBuilder.DropForeignKey(
                name: "FK_CouponPrograms_AspNetUsers_UpdateBy",
                table: "CouponPrograms");

            migrationBuilder.DropIndex(
                name: "IX_CouponPrograms_CreateBy",
                table: "CouponPrograms");

            migrationBuilder.DropIndex(
                name: "IX_CouponPrograms_UpdateBy",
                table: "CouponPrograms");

            migrationBuilder.DropColumn(
                name: "CreateBy",
                table: "CouponPrograms");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "CouponPrograms");

            migrationBuilder.DropColumn(
                name: "UpdateBy",
                table: "CouponPrograms");

            migrationBuilder.DropColumn(
                name: "UpdateDate",
                table: "CouponPrograms");
        }
    }
}
