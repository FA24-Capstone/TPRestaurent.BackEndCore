using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class ReOrderDishTyeId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 0);

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "APPETIZER", "Khai Vị" });

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "SOUP", "Súp" });

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "HOTPOT", "Lẩu" });

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "BBQ", "Nướng" });

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "HOTPOT_BROTH", "Nước Lẩu" });

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "HOTPOT_MEAT", "Thịt Lẩu" });

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "HOTPOT_SEAFOOD", "Hải Sản Lẩu" });

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "HOTPOT_VEGGIE", "Rau Lẩu" });

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "BBQ_MEAT", "Thịt Nướng" });

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "BBQ_SEAFOOD", "Hải Sản Nướng" });

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "HOTPOT_TOPPING", "Topping Thả Lẩu" });

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "BBQ_TOPPING", "Topping Nướng" });

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "SIDEDISH", "Món Phụ" });

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "DRINK", "Đồ Uống" });

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "DESSERT", "Tráng Miệng" });

            migrationBuilder.InsertData(
                table: "DishItemTypes",
                columns: new[] { "Id", "Name", "VietnameseName" },
                values: new object[] { 16, "SAUCE", "Sốt" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "SOUP", "Súp" });

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "HOTPOT", "Lẩu" });

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "BBQ", "Nướng" });

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "HOTPOT_BROTH", "Nước Lẩu" });

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "HOTPOT_MEAT", "Thịt Lẩu" });

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "HOTPOT_SEAFOOD", "Hải Sản Lẩu" });

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "HOTPOT_VEGGIE", "Rau Lẩu" });

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "BBQ_MEAT", "Thịt Nướng" });

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "BBQ_SEAFOOD", "Hải Sản Nướng" });

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "HOTPOT_TOPPING", "Topping Thả Lẩu" });

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "BBQ_TOPPING", "Topping Nướng" });

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "SIDEDISH", "Món Phụ" });

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "DRINK", "Đồ Uống" });

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "DESSERT", "Tráng Miệng" });

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "Name", "VietnameseName" },
                values: new object[] { "SAUCE", "Sốt" });

            migrationBuilder.InsertData(
                table: "DishItemTypes",
                columns: new[] { "Id", "Name", "VietnameseName" },
                values: new object[] { 0, "APPETIZER", "Khai Vị" });
        }
    }
}
