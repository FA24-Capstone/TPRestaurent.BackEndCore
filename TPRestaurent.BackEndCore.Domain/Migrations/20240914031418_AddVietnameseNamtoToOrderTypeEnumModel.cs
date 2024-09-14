using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class AddVietnameseNamtoToOrderTypeEnumModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VietnameseName",
                table: "OrderType",
                type: "nvarchar(max)",
                nullable: true);

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
                table: "PaymentMethods",
                keyColumn: "Id",
                keyValue: 2,
                column: "VietnameseName",
                value: "VNPAY");

            migrationBuilder.UpdateData(
                table: "PaymentMethods",
                keyColumn: "Id",
                keyValue: 3,
                column: "VietnameseName",
                value: "MOMO");

            migrationBuilder.UpdateData(
                table: "PaymentMethods",
                keyColumn: "Id",
                keyValue: 4,
                column: "VietnameseName",
                value: "ZALOPAY");

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VietnameseName",
                table: "OrderType");

            migrationBuilder.UpdateData(
                table: "ComboCategories",
                keyColumn: "Id",
                keyValue: 0,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "ComboCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "ComboCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 0,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 6,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 7,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 8,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 9,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 10,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 11,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 12,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 13,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 14,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "DishItemTypes",
                keyColumn: "Id",
                keyValue: 15,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "DishSizes",
                keyColumn: "Id",
                keyValue: 0,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "DishSizes",
                keyColumn: "Id",
                keyValue: 1,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "DishSizes",
                keyColumn: "Id",
                keyValue: 2,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "OTPTypes",
                keyColumn: "Id",
                keyValue: 0,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "OTPTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "OTPTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "OTPTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "OTPTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "OTPTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "OTPTypes",
                keyColumn: "Id",
                keyValue: 6,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "OTPTypes",
                keyColumn: "Id",
                keyValue: 7,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "OTPTypes",
                keyColumn: "Id",
                keyValue: 8,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "OTPTypes",
                keyColumn: "Id",
                keyValue: 9,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "OrderDetailStatuses",
                keyColumn: "Id",
                keyValue: 1,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "OrderDetailStatuses",
                keyColumn: "Id",
                keyValue: 2,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "OrderDetailStatuses",
                keyColumn: "Id",
                keyValue: 3,
                column: "VietnameseName",
                value: null);

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
                value: null);

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 1,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 2,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 3,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 4,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 5,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 6,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 7,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 8,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "PaymentMethods",
                keyColumn: "Id",
                keyValue: 1,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "PaymentMethods",
                keyColumn: "Id",
                keyValue: 2,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "PaymentMethods",
                keyColumn: "Id",
                keyValue: 3,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "PaymentMethods",
                keyColumn: "Id",
                keyValue: 4,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "RatingPoints",
                keyColumn: "Id",
                keyValue: 1,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "RatingPoints",
                keyColumn: "Id",
                keyValue: 2,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "RatingPoints",
                keyColumn: "Id",
                keyValue: 3,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "RatingPoints",
                keyColumn: "Id",
                keyValue: 4,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "RatingPoints",
                keyColumn: "Id",
                keyValue: 5,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "TableSizes",
                keyColumn: "Id",
                keyValue: 2,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "TableSizes",
                keyColumn: "Id",
                keyValue: 4,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "TableSizes",
                keyColumn: "Id",
                keyValue: 6,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "TableSizes",
                keyColumn: "Id",
                keyValue: 8,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "TableSizes",
                keyColumn: "Id",
                keyValue: 10,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "TransationStatus",
                keyColumn: "Id",
                keyValue: 0,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "TransationStatus",
                keyColumn: "Id",
                keyValue: 1,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "TransationStatus",
                keyColumn: "Id",
                keyValue: 2,
                column: "VietnameseName",
                value: null);

            migrationBuilder.UpdateData(
                table: "TransationStatus",
                keyColumn: "Id",
                keyValue: 3,
                column: "VietnameseName",
                value: null);
        }
    }
}
