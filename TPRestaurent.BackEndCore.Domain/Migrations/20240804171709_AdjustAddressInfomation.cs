using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AdjustAddressInfomation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ratings_AspNetUsers_AccountId",
                table: "Ratings");

            migrationBuilder.DropIndex(
                name: "IX_Ratings_AccountId",
                table: "Ratings");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Ratings");

            migrationBuilder.CreateTable(
                name: "CustomerInfoAddress",
                columns: table => new
                {
                    CustomerInfoAddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerInfoAddressName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsCurrentUsed = table.Column<bool>(type: "bit", nullable: false),
                    CustomerInfoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerInfoAddress", x => x.CustomerInfoAddressId);
                    table.ForeignKey(
                        name: "FK_CustomerInfoAddress_CustomerInfos_CustomerInfoId",
                        column: x => x.CustomerInfoId,
                        principalTable: "CustomerInfos",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerInfoAddress_CustomerInfoId",
                table: "CustomerInfoAddress",
                column: "CustomerInfoId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerInfoAddress");

            migrationBuilder.AddColumn<string>(
                name: "AccountId",
                table: "Ratings",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_AccountId",
                table: "Ratings",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ratings_AspNetUsers_AccountId",
                table: "Ratings",
                column: "AccountId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }
    }
}
