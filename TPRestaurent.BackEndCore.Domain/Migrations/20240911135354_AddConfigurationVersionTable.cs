using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddConfigurationVersionTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiveDate",
                table: "Configurations");

            migrationBuilder.DropColumn(
                name: "ActiveValue",
                table: "Configurations");

            migrationBuilder.RenameColumn(
                name: "PreValue",
                table: "Configurations",
                newName: "CurrentValue");

            migrationBuilder.CreateTable(
                name: "ConfigurationVersions",
                columns: table => new
                {
                    ConfigurationVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActiveValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationVersions", x => x.ConfigurationVersionId);
                    table.ForeignKey(
                        name: "FK_ConfigurationVersions_Configurations_ConfigurationId",
                        column: x => x.ConfigurationId,
                        principalTable: "Configurations",
                        principalColumn: "ConfigurationId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationVersions_ConfigurationId",
                table: "ConfigurationVersions",
                column: "ConfigurationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfigurationVersions");

            migrationBuilder.RenameColumn(
                name: "CurrentValue",
                table: "Configurations",
                newName: "PreValue");

            migrationBuilder.AddColumn<DateTime>(
                name: "ActiveDate",
                table: "Configurations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActiveValue",
                table: "Configurations",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
