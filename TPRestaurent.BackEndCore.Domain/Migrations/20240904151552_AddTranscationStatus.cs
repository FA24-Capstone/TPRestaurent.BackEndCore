using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddTranscationStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_StoreCredits_StoreCreditId",
                table: "Transactions");

            migrationBuilder.RenameColumn(
                name: "StoreCreditId",
                table: "Transactions",
                newName: "StoreCreditHistoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_StoreCreditId",
                table: "Transactions",
                newName: "IX_Transactions_StoreCreditHistoryId");

            migrationBuilder.CreateTable(
                name: "TranscationStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VietnameseName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TranscationStatus", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "TranscationStatus",
                columns: new[] { "Id", "Name", "VietnameseName" },
                values: new object[] { 0, "PENDING", null });

            migrationBuilder.InsertData(
                table: "TranscationStatus",
                columns: new[] { "Id", "Name", "VietnameseName" },
                values: new object[] { 1, "FAILED", null });

            migrationBuilder.InsertData(
                table: "TranscationStatus",
                columns: new[] { "Id", "Name", "VietnameseName" },
                values: new object[] { 2, "SUCCESSFUL", null });

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_StoreCreditHistories_StoreCreditHistoryId",
                table: "Transactions",
                column: "StoreCreditHistoryId",
                principalTable: "StoreCreditHistories",
                principalColumn: "StoreCreditHistoryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_StoreCreditHistories_StoreCreditHistoryId",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "TranscationStatus");

            migrationBuilder.RenameColumn(
                name: "StoreCreditHistoryId",
                table: "Transactions",
                newName: "StoreCreditId");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_StoreCreditHistoryId",
                table: "Transactions",
                newName: "IX_Transactions_StoreCreditId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_StoreCredits_StoreCreditId",
                table: "Transactions",
                column: "StoreCreditId",
                principalTable: "StoreCredits",
                principalColumn: "StoreCreditId");
        }
    }
}
