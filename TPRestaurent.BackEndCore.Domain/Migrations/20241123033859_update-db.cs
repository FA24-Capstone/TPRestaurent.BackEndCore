using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class updatedb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUserTokens_AccountId",
                table: "AspNetUserTokens");

            migrationBuilder.DropColumn(
                name: "AccessTokenValue",
                table: "AspNetUserTokens");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "AspNetUserTokens");

            migrationBuilder.DropColumn(
                name: "CreateDateAccessToken",
                table: "AspNetUserTokens");

            migrationBuilder.DropColumn(
                name: "CreateRefreshToken",
                table: "AspNetUserTokens");

            migrationBuilder.DropColumn(
                name: "DeviceIP",
                table: "AspNetUserTokens");

            migrationBuilder.DropColumn(
                name: "DeviceName",
                table: "AspNetUserTokens");

            migrationBuilder.DropColumn(
                name: "DeviceToken",
                table: "AspNetUserTokens");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AspNetUserTokens");

            migrationBuilder.DropColumn(
                name: "ExpiryTimeAccessToken",
                table: "AspNetUserTokens");

            migrationBuilder.DropColumn(
                name: "ExpiryTimeRefreshToken",
                table: "AspNetUserTokens");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AspNetUserTokens");

            migrationBuilder.DropColumn(
                name: "LastLogin",
                table: "AspNetUserTokens");

            migrationBuilder.DropColumn(
                name: "RefreshTokenValue",
                table: "AspNetUserTokens");

            migrationBuilder.CreateTable(
                name: "Tokens",
                columns: table => new
                {
                    TokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccessTokenValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeviceIP = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDateAccessToken = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryTimeAccessToken = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RefreshTokenValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeviceToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeviceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateRefreshToken = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryTimeRefreshToken = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AccountId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tokens", x => x.TokenId);
                    table.ForeignKey(
                        name: "FK_Tokens_AspNetUsers_AccountId",
                        column: x => x.AccountId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_AccountId",
                table: "Tokens",
                column: "AccountId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tokens");

            migrationBuilder.AddColumn<string>(
                name: "AccessTokenValue",
                table: "AspNetUserTokens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountId",
                table: "AspNetUserTokens",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDateAccessToken",
                table: "AspNetUserTokens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateRefreshToken",
                table: "AspNetUserTokens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeviceIP",
                table: "AspNetUserTokens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeviceName",
                table: "AspNetUserTokens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeviceToken",
                table: "AspNetUserTokens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AspNetUserTokens",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryTimeAccessToken",
                table: "AspNetUserTokens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryTimeRefreshToken",
                table: "AspNetUserTokens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AspNetUserTokens",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLogin",
                table: "AspNetUserTokens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefreshTokenValue",
                table: "AspNetUserTokens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserTokens_AccountId",
                table: "AspNetUserTokens",
                column: "AccountId");
        }
    }
}
