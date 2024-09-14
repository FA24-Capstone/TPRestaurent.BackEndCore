using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddInterpolationEnumName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ComboCategories",
                keyColumn: "Id",
                keyValue: 0,
                column: "VietnameseName",
                value: "Lẩu");

            migrationBuilder.UpdateData(
                table: "ComboCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "VietnameseName",
                value: "Nướng");

            migrationBuilder.UpdateData(
                table: "ComboCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "VietnameseName",
                value: "Lẩu-Nướng");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 0,
                column: "VietnameseName",
                value: "Khai Vị");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "VietnameseName",
                value: "Súp");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "VietnameseName",
                value: "Lẩu");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "VietnameseName",
                value: "Nướng");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "VietnameseName",
                value: "Nước Lẩu");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "VietnameseName",
                value: "Thịt Lẩu");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 6,
                column: "VietnameseName",
                value: "Hải Sản Lẩu");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 7,
                column: "VietnameseName",
                value: "Rau Lẩu");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 8,
                column: "VietnameseName",
                value: "Thịt Nướng");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 9,
                column: "VietnameseName",
                value: "Hải Sản Nướng");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 10,
                column: "VietnameseName",
                value: "Topping Thả Lẩu");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 11,
                column: "VietnameseName",
                value: "Topping Nướng");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 12,
                column: "VietnameseName",
                value: "Món Phụ");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 13,
                column: "VietnameseName",
                value: "Đồ Uống");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 14,
                column: "VietnameseName",
                value: "Tráng Miệng");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 15,
                column: "VietnameseName",
                value: "Sốt");

            migrationBuilder.UpdateData(
                table: "DishSizes",
                keyColumn: "Id",
                keyValue: 0,
                column: "VietnameseName",
                value: "Nhỏ");

            migrationBuilder.UpdateData(
                table: "DishSizes",
                keyColumn: "Id",
                keyValue: 1,
                column: "VietnameseName",
                value: "Vừa");

            migrationBuilder.UpdateData(
                table: "DishSizes",
                keyColumn: "Id",
                keyValue: 2,
                column: "VietnameseName",
                value: "Lớn");

            migrationBuilder.UpdateData(
                table: "OTPTypes",
                keyColumn: "Id",
                keyValue: 0,
                column: "VietnameseName",
                value: "Đăng Nhập");

            migrationBuilder.UpdateData(
                table: "OTPTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "VietnameseName",
                value: "Đăng Ký");

            migrationBuilder.UpdateData(
                table: "OTPTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "VietnameseName",
                value: "Quên Mật Khẩu");

            migrationBuilder.UpdateData(
                table: "OTPTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "VietnameseName",
                value: "Đổi Mật Khẩu");

            migrationBuilder.UpdateData(
                table: "OTPTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "VietnameseName",
                value: "Đổi Email");

            migrationBuilder.UpdateData(
                table: "OTPTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "VietnameseName",
                value: "Đổi SĐT");

            migrationBuilder.UpdateData(
                table: "OTPTypes",
                keyColumn: "Id",
                keyValue: 6,
                column: "VietnameseName",
                value: "Xác Nhận Email");

            migrationBuilder.UpdateData(
                table: "OTPTypes",
                keyColumn: "Id",
                keyValue: 7,
                column: "VietnameseName",
                value: "Xác Nhận SĐT");

            migrationBuilder.UpdateData(
                table: "OTPTypes",
                keyColumn: "Id",
                keyValue: 8,
                column: "VietnameseName",
                value: "Xác Nhận Thanh Toán");

            migrationBuilder.UpdateData(
                table: "OTPTypes",
                keyColumn: "Id",
                keyValue: 9,
                column: "VietnameseName",
                value: "Xác Nhận Đặt Bàn");

            migrationBuilder.UpdateData(
                table: "OrderDetailStatuses",
                keyColumn: "Id",
                keyValue: 1,
                column: "VietnameseName",
                value: "Chờ Xử Lý");

            migrationBuilder.UpdateData(
                table: "OrderDetailStatuses",
                keyColumn: "Id",
                keyValue: 2,
                column: "VietnameseName",
                value: "Chưa Xem");

            migrationBuilder.UpdateData(
                table: "OrderDetailStatuses",
                keyColumn: "Id",
                keyValue: 3,
                column: "VietnameseName",
                value: "Đã Xem");

            migrationBuilder.UpdateData(
                table: "OrderDetailStatuses",
                keyColumn: "Id",
                keyValue: 4,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "OrderDetailStatuses",
                keyColumn: "Id",
                keyValue: 5,
                column: "VietnameseName",
                value: "Đã Huỷ");

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 1,
                column: "VietnameseName",
                value: "Đã Xếp Bàn");

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 2,
                column: "VietnameseName",
                value: "Đã Đặt Cọc");

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 3,
                column: "VietnameseName",
                value: "Đang Dùng Bữa");

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 4,
                column: "VietnameseName",
                value: "Chờ Xử Lý");

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 5,
                column: "VietnameseName",
                value: "Đang Xử Lý");

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 6,
                column: "VietnameseName",
                value: "Đang Giao");

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 7,
                column: "VietnameseName",
                value: "Thành Công");

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 8,
                column: "VietnameseName",
                value: "Đã Huỷ");

            migrationBuilder.UpdateData(
                table: "OrderType",
                keyColumn: "Id",
                keyValue: 1,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "OrderType",
                keyColumn: "Id",
                keyValue: 2,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "OrderType",
                keyColumn: "Id",
                keyValue: 3,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "PaymentMethods",
                keyColumn: "Id",
                keyValue: 1,
                column: "VietnameseName",
                value: "Tiền Mặt");

            migrationBuilder.UpdateData(
                table: "RatingPoints",
                keyColumn: "Id",
                keyValue: 1,
                column: "VietnameseName",
                value: "1");

            migrationBuilder.UpdateData(
                table: "RatingPoints",
                keyColumn: "Id",
                keyValue: 2,
                column: "VietnameseName",
                value: "2");

            migrationBuilder.UpdateData(
                table: "RatingPoints",
                keyColumn: "Id",
                keyValue: 3,
                column: "VietnameseName",
                value: "3");

            migrationBuilder.UpdateData(
                table: "RatingPoints",
                keyColumn: "Id",
                keyValue: 4,
                column: "VietnameseName",
                value: "4");

            migrationBuilder.UpdateData(
                table: "RatingPoints",
                keyColumn: "Id",
                keyValue: 5,
                column: "VietnameseName",
                value: "5");

            migrationBuilder.UpdateData(
                table: "TableSizes",
                keyColumn: "Id",
                keyValue: 2,
                column: "VietnameseName",
                value: "2");

            migrationBuilder.UpdateData(
                table: "TableSizes",
                keyColumn: "Id",
                keyValue: 4,
                column: "VietnameseName",
                value: "4");

            migrationBuilder.UpdateData(
                table: "TableSizes",
                keyColumn: "Id",
                keyValue: 6,
                column: "VietnameseName",
                value: "6");

            migrationBuilder.UpdateData(
                table: "TableSizes",
                keyColumn: "Id",
                keyValue: 8,
                column: "VietnameseName",
                value: "8");

            migrationBuilder.UpdateData(
                table: "TableSizes",
                keyColumn: "Id",
                keyValue: 10,
                column: "VietnameseName",
                value: "10");

            migrationBuilder.UpdateData(
                table: "TransationStatus",
                keyColumn: "Id",
                keyValue: 0,
                column: "VietnameseName",
                value: "Chờ Xử Lý");

            migrationBuilder.UpdateData(
                table: "TransationStatus",
                keyColumn: "Id",
                keyValue: 1,
                column: "VietnameseName",
                value: "Thất Bại");

            migrationBuilder.UpdateData(
                table: "TransationStatus",
                keyColumn: "Id",
                keyValue: 2,
                column: "VietnameseName",
                value: "Thành Công");

            migrationBuilder.UpdateData(
                table: "TransationStatus",
                keyColumn: "Id",
                keyValue: 3,
                column: "VietnameseName",
                value: null);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ComboCategories",
                keyColumn: "Id",
                keyValue: 0,
                column: "VietnameseName",
                value: "HOTPOT");

            migrationBuilder.UpdateData(
                table: "ComboCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "VietnameseName",
                value: "BBQ");

            migrationBuilder.UpdateData(
                table: "ComboCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "VietnameseName",
                value: "BOTH");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 0,
                column: "VietnameseName",
                value: "APPETIZER");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "VietnameseName",
                value: "SOUP");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "VietnameseName",
                value: "HOTPOT");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "VietnameseName",
                value: "BBQ");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "VietnameseName",
                value: "HOTPOT_BROTH");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "VietnameseName",
                value: "HOTPOT_MEAT");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 6,
                column: "VietnameseName",
                value: "HOTPOT_SEAFOOD");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 7,
                column: "VietnameseName",
                value: "HOTPOT_VEGGIE");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 8,
                column: "VietnameseName",
                value: "BBQ_MEAT");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 9,
                column: "VietnameseName",
                value: "BBQ_SEAFOOD");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 10,
                column: "VietnameseName",
                value: "HOTPOT_TOPPING");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 11,
                column: "VietnameseName",
                value: "BBQ_TOPPING");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 12,
                column: "VietnameseName",
                value: "SIDEDISH");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 13,
                column: "VietnameseName",
                value: "DRINK");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 14,
                column: "VietnameseName",
                value: "DESSERT");

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 15,
                column: "VietnameseName",
                value: "SAUCE");

            migrationBuilder.UpdateData(
                table: "DishSizes",
                keyColumn: "Id",
                keyValue: 0,
                column: "VietnameseName",
                value: "SMALL");

            migrationBuilder.UpdateData(
                table: "DishSizes",
                keyColumn: "Id",
                keyValue: 1,
                column: "VietnameseName",
                value: "MEDIUM");

            migrationBuilder.UpdateData(
                table: "DishSizes",
                keyColumn: "Id",
                keyValue: 2,
                column: "VietnameseName",
                value: "LARGE");

            migrationBuilder.UpdateData(
                table: "OTPTypes",
                keyColumn: "Id",
                keyValue: 0,
                column: "VietnameseName",
                value: "Login");

            migrationBuilder.UpdateData(
                table: "OTPTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "VietnameseName",
                value: "Register");

            migrationBuilder.UpdateData(
                table: "OTPTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "VietnameseName",
                value: "ForgotPassword");

            migrationBuilder.UpdateData(
                table: "OTPTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "VietnameseName",
                value: "ChangePassword");

            migrationBuilder.UpdateData(
                table: "OTPTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "VietnameseName",
                value: "ChangeEmail");

            migrationBuilder.UpdateData(
                table: "OTPTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "VietnameseName",
                value: "ChangePhone");

            migrationBuilder.UpdateData(
                table: "OTPTypes",
                keyColumn: "Id",
                keyValue: 6,
                column: "VietnameseName",
                value: "ConfirmEmail");

            migrationBuilder.UpdateData(
                table: "OTPTypes",
                keyColumn: "Id",
                keyValue: 7,
                column: "VietnameseName",
                value: "ConfirmPhone");

            migrationBuilder.UpdateData(
                table: "OTPTypes",
                keyColumn: "Id",
                keyValue: 8,
                column: "VietnameseName",
                value: "ConfirmPayment");

            migrationBuilder.UpdateData(
                table: "OTPTypes",
                keyColumn: "Id",
                keyValue: 9,
                column: "VietnameseName",
                value: "VerifyForReservation");

            migrationBuilder.UpdateData(
                table: "OrderDetailStatuses",
                keyColumn: "Id",
                keyValue: 1,
                column: "VietnameseName",
                value: "Pending");

            migrationBuilder.UpdateData(
                table: "OrderDetailStatuses",
                keyColumn: "Id",
                keyValue: 2,
                column: "VietnameseName",
                value: "Unchecked");

            migrationBuilder.UpdateData(
                table: "OrderDetailStatuses",
                keyColumn: "Id",
                keyValue: 3,
                column: "VietnameseName",
                value: "Read");

            migrationBuilder.UpdateData(
                table: "OrderDetailStatuses",
                keyColumn: "Id",
                keyValue: 4,
                column: "VietnameseName",
                value: "ReadyToServe");

            migrationBuilder.UpdateData(
                table: "OrderDetailStatuses",
                keyColumn: "Id",
                keyValue: 5,
                column: "VietnameseName",
                value: "Cancelled");

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 1,
                column: "VietnameseName",
                value: "TableAssigned");

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 2,
                column: "VietnameseName",
                value: "DepositPaid");

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 3,
                column: "VietnameseName",
                value: "Dining");

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 4,
                column: "VietnameseName",
                value: "Pending");

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 5,
                column: "VietnameseName",
                value: "Processing");

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 6,
                column: "VietnameseName",
                value: "Delivering");

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 7,
                column: "VietnameseName",
                value: "Completed");

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 8,
                column: "VietnameseName",
                value: "Cancelled");

            migrationBuilder.UpdateData(
                table: "OrderType",
                keyColumn: "Id",
                keyValue: 1,
                column: "VietnameseName",
                value: "Reservation");

            migrationBuilder.UpdateData(
                table: "OrderType",
                keyColumn: "Id",
                keyValue: 2,
                column: "VietnameseName",
                value: "Delivery");

            migrationBuilder.UpdateData(
                table: "OrderType",
                keyColumn: "Id",
                keyValue: 3,
                column: "VietnameseName",
                value: "MealWithoutReservation");

            migrationBuilder.UpdateData(
                table: "PaymentMethods",
                keyColumn: "Id",
                keyValue: 1,
                column: "VietnameseName",
                value: "Cash");

            migrationBuilder.UpdateData(
                table: "RatingPoints",
                keyColumn: "Id",
                keyValue: 1,
                column: "VietnameseName",
                value: "One");

            migrationBuilder.UpdateData(
                table: "RatingPoints",
                keyColumn: "Id",
                keyValue: 2,
                column: "VietnameseName",
                value: "Two");

            migrationBuilder.UpdateData(
                table: "RatingPoints",
                keyColumn: "Id",
                keyValue: 3,
                column: "VietnameseName",
                value: "Three");

            migrationBuilder.UpdateData(
                table: "RatingPoints",
                keyColumn: "Id",
                keyValue: 4,
                column: "VietnameseName",
                value: "Four");

            migrationBuilder.UpdateData(
                table: "RatingPoints",
                keyColumn: "Id",
                keyValue: 5,
                column: "VietnameseName",
                value: "Five");

            migrationBuilder.UpdateData(
                table: "TableSizes",
                keyColumn: "Id",
                keyValue: 2,
                column: "VietnameseName",
                value: "TWO");

            migrationBuilder.UpdateData(
                table: "TableSizes",
                keyColumn: "Id",
                keyValue: 4,
                column: "VietnameseName",
                value: "FOUR");

            migrationBuilder.UpdateData(
                table: "TableSizes",
                keyColumn: "Id",
                keyValue: 6,
                column: "VietnameseName",
                value: "SIX");

            migrationBuilder.UpdateData(
                table: "TableSizes",
                keyColumn: "Id",
                keyValue: 8,
                column: "VietnameseName",
                value: "EIGHT");

            migrationBuilder.UpdateData(
                table: "TableSizes",
                keyColumn: "Id",
                keyValue: 10,
                column: "VietnameseName",
                value: "TEN");

            migrationBuilder.UpdateData(
                table: "TransationStatus",
                keyColumn: "Id",
                keyValue: 0,
                column: "VietnameseName",
                value: "PENDING");

            migrationBuilder.UpdateData(
                table: "TransationStatus",
                keyColumn: "Id",
                keyValue: 1,
                column: "VietnameseName",
                value: "FAILED");

            migrationBuilder.UpdateData(
                table: "TransationStatus",
                keyColumn: "Id",
                keyValue: 2,
                column: "VietnameseName",
                value: "SUCCESSFUL");

            migrationBuilder.UpdateData(
                table: "TransationStatus",
                keyColumn: "Id",
                keyValue: 3,
                column: "VietnameseName",
                value: "APPLIED");
        }
    }
}
