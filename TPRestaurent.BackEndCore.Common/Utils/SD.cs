using System.Globalization;
using System.Text;
using TPRestaurent.BackEndCore.Domain.Enums;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.Utils;

public class SD
{
    public static int MAX_RECORD_PER_PAGE = short.MaxValue;
    public static string DEFAULT_PASSWORD = "TourGuide@123";
    public static string DEFAULT_EMAIL_DOMAIN = "@gmail.com";
    public static DateTime MINIMUM_DATE = DateTime.Parse("0001-01-01 00:00:00.0000000");

    public class AccountDefaultInfomation
    {
        public static string DEFAULT_EMAIL = "TPCustomer";
        public static string DEFAULT_FIRSTNAME = "Firstname";
        public static string DEFAULT_LASTNAME = "Lasttname";
    }

    public static string FormatDateTime(DateTime dateTime)
    {
        return dateTime.ToString("dd/MM/yyyy");
    }

    public static string GenerateOTP(int length = 6)
    {
        // Sử dụng thư viện Random để tạo mã ngẫu nhiên
        Random random = new Random();

        // Tạo chuỗi ngẫu nhiên với độ dài mong muốn
        string otp = "";
        for (int i = 0; i < length; i++)
        {
            otp += random.Next(0, 10); // Sinh số ngẫu nhiên từ 0 đến 9 và thêm vào chuỗi OTP
        }

        return otp;
    }

    public static IEnumerable<WeekForYear> PrintWeeksForYear(int year)
    {
        var weekForYears = new List<WeekForYear>();
        var startDate = new DateTime(year, 1, 1);
        var endDate = startDate.AddDays(6);

        var cultureInfo = CultureInfo.CurrentCulture;

        Console.WriteLine($"Week 1: {startDate.ToString("d", cultureInfo)} - {endDate.ToString("d", cultureInfo)}");
        weekForYears.Add(new WeekForYear
        {
            WeekIndex = 1,
            Timeline = new WeekForYear.TimelineDto
            { StartDate = startDate.ToString("d", cultureInfo), EndDate = endDate.ToString("d", cultureInfo) }
        });

        for (var week = 2; week < 53; week++)
        {
            startDate = endDate.AddDays(1);
            endDate = startDate.AddDays(6);

            Console.WriteLine(
                $"Week {week}: {startDate.ToString("d", cultureInfo)} - {endDate.ToString("d", cultureInfo)}");
            weekForYears.Add(new WeekForYear
            {
                WeekIndex = week,
                Timeline = new WeekForYear.TimelineDto
                { StartDate = startDate.ToString("d", cultureInfo), EndDate = endDate.ToString("d", cultureInfo) }
            });
        }

        return weekForYears;
    }

    public class ResponseMessage
    {
        public static string CREATE_SUCCESSFULLY = "Tạo mới thành công";
        public static string UPDATE_SUCCESSFULLY = "Cập nhật thành công";
        public static string DELETE_SUCCESSFULY = "Xóa thành công";
        public static string CREATE_FAILED = "Có lỗi xảy ra trong quá trình tạo";
        public static string UPDATE_FAILED = "Có lỗi xảy ra trong quá trình cập nhật";
        public static string DELETE_FAILED = "Có lỗi xảy ra trong quá trình xóa";
        public static string LOGIN_FAILED = "Đăng nhập thất bại";
    }

    public class DefaultValue
    {
        public static string AVERAGE_MEAL_DURATION = "AVERAGE_MEAL_DURATION";
        public static string DEPOSIT_PERCENT = "DEPOSIT_PERCENT";
        public static string DEPOSIT_FOR_NORMAL_TABLE = "DEPOSIT_FOR_NORMAL_TABLE";
        public static string DEPOSIT_FOR_PRIVATE_TABLE = "DEPOSIT_FOR_PRIVATE_TABLE";
        public static string TIME_TO_LOOK_UP_FOR_RESERVATION = "TIME_TO_LOOK_UP_FOR_RESERVATION";
        public static string EXPIRE_TIME_FOR_STORE_CREDIT = "EXPIRE_TIME_FOR_STORE_CREDIT";
        public static string REFUND_APPLYING_TIME_FOR_RESEVATION_CANCELATION = "REFUND_APPLYING_TIME_FOR_RESEVATION_CANCELATION";
        public static string TIME_TO_KEEP_RESERVATION = "TIME_TO_KEEP_RESERVATION";
        public static string TIME_TO_KEEP_UNPAID_DELIVERY_ORDER = "TIME_TO_KEEP_UNPAID_DELIVERY_ORDER";
        public static string OPEN_TIME = "OPEN_TIME";
        public static string CLOSED_TIME = "CLOSED_TIME";
        public static string RESTAURANT_LATITUDE = "RESTAURANT_LATITUDE";
        public static string RESTAURANT_LNG = "RESTAURANT_LNG";
        public static string DISTANCE_STEP = "DISTANCE_STEP";
        public static string DISTANCE_ORDER = "DISTANCE_ORDER";
        public static string FLAT_COST_DISTANCE = "FLAT_COST_DISTANCE";
        public static string DISTANCE_STEP_FEE = "DISTANCE_STEP_FEE";
        public static string TIME_TO_RESERVATION_WITH_DISHES_LAST = "TIME_TO_RESERVATION_WITH_DISHES_LAST";
        public static string TIME_TO_RESERVATION_LAST = "TIME_TO_RESERVATION_LAST";
        public static string TIME_FOR_GROUPED_DISH = "TIME_FOR_GROUPED_DISH";
        public static string TIME_FOR_REFUND = "TIME_FOR_REFUND";
        public static string REFUND_PERCENTAGE_AS_CUSTOMER = "REFUND_PERCENTAGE_AS_CUSTOMER";
        public static string REFUND_PERCENTAGE_AS_ADMIN = "REFUND_PERCENTAGE_AS_ADMIN";
        public static string TABLE_IS_SET_UP = "TABLE_IS_SET_UP";
        public static string NEW = "NEW";
        public static string IS_SET_UP = "IS_SET_UP";
        public static string INITIAL = "INITIAL";
        public static string TIME_TO_NOTIFY_DISH_TO_KITCHEN_BEFORE_HAND = "TIME_TO_NOTIFY_DISH_TO_KITCHEN_BEFORE_HAND";
    }

    public class SubjectMail
    {
        public static string VERIFY_ACCOUNT = "CHÀO MỪNG BẠN ĐẾN VỚI NHÀ HÀNG THIÊN PHÚ. VUI LÒNG XÁC MINH TÀI KHOẢN";
        public static string WELCOME = "CHÀO MỪNG BẠN ĐẾN VỚI NHÀ HÀNG THIÊN PHÚ";
        public static string REMIND_PAYMENT = "NHẮC NHỞ THANH TOÁN";
        public static string PASSCODE_FORGOT_PASSWORD = "MÃ XÁC THỰC QUÊN MẬT KHẨU";
        public static string NOTIFY_RESERVATION = "NHÀ HÀNG THIÊN PHÚ XIN THÔNG BÁO";
    }

    public class WeekForYear
    {
        public int WeekIndex { get; set; }
        public TimelineDto? Timeline { get; set; }

        public class TimelineDto
        {
            public string? StartDate { get; set; }
            public string? EndDate { get; set; }
        }
    }

    public class Regex
    {
        public static string PHONENUMBER = "^0\\d{9,10}$";
        public static string EMAIL = "^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$";
        public static string NAME = "^[\\p{L}\\s,'.-]+$";
        public static string CHASSISNUMBER = "^[A-HJ-NPR-Z0-9]{12,17}$";
        public static string ENGINENUMBER = "^[A-Za-z0-9]{6,17}$";
        public static string PLATE = "^(?!13)[0-9]{2}[A-Z]{1}[A-Z0-9]{0,1}-[0-9]{4,5}$";
    }

    public class CommonInformation
    {
        public static List<string> COMMON_BUS_BRAND_NAME_LIST = new List<string>
        {
            "Hyundai",
            "Toyota",
            "Kia",
            "Ford",
            "Mercedes-Benz",
            "Mitsubishi",
            "Hino",
            "Isuzu",
            "Thaco",
            "Daewoo",
            "VinFast",
            "Fuso",
            "Yutong",
            "Suzuki",
            "Volkswagen",
            "Honda",
            "Volvo",
            "Nissan",
            "Scania",
            "Ford Transit"
        };
    }

    public class EnumType
    {
        public static Dictionary<string, int> MaterialUnit = new Dictionary<string, int> { { "KG", 0 }, { "M3", 1 }, { "BAR", 2 }, { "ITEM", 3 }, { "Kg", 0 }, { "m3", 1 }, { "Bar", 2 }, { "Item", 3 }, { "kg", 0 }, { "bar", 2 }, { "item", 3 } };
        public static Dictionary<string, int> ServiceCostUnit = new Dictionary<string, int> { { "DAY", 0 }, { "Day", 0 }, { "day", 0 }, { "ROOM", 1 }, { "Room", 1 }, { "room", 1 }, { "PERSON", 2 }, { "Person", 2 }, { "person", 2 } };
        public static Dictionary<string, int> VehicleType = new Dictionary<string, int> { { "DAY", 0 }, { "Day", 0 }, { "day", 0 }, { "ROOM", 1 }, { "Room", 1 }, { "room", 1 }, { "PERSON", 2 }, { "Person", 2 }, { "person", 2 } };
        public static HashSet<DishItemType> MainItemType = new HashSet<DishItemType> { DishItemType.APPETIZER, DishItemType.SOUP, DishItemType.HOTPOT, DishItemType.BBQ, DishItemType.SIDEDISH, DishItemType.DRINK, DishItemType.DESSERT };
    }

    public class ConfigName
    {
        public static string CONFIG_NAME = "Chênh lệch Tour guide";
    }

    public class ExcelHeaders
    {
        public static List<string> SERVICE_QUOTATION = new List<string> { "No", "Tên dịch vụ", "Đơn vị", "MOQ", "Giá người lớn", "Giá trẻ em" };
        public static List<string> MENU_SERVICE_QUOTATION = new List<string> { "No", "Tên dịch vụ", "Tên menu", "Đơn vị", "MOQ", "Giá người lớn" };
        public static List<string> TOURGUIDE_REGISTRATION = new List<string> { "No", "Tên", "Họ", "Email", "Số điện thoại", "Giới tính" };
        public static List<string> VEHICLE_REGISTRATION = new List<string> { "No", "Biển số", "Dung tích", "Số động cơ", "Số khung", "Thương hiệu", "Tên chủ sở hữu", "Màu" };
        public static List<string> MENU_DISH = new List<string> { "No", "Tên dịch vụ", "Tên menu", "Tên món", "Mô tả", "Loại món" };
        //public int No { get; set; }
        //public string? FacilityServiceName { get; set; }
        //public string? MenuName { get; set; }
        //public string? DishName { get; set; }
        //public string? Description { get; set; }
        //public string? MenuType { get; set; }
    }
    public class FirebasePathName
    {
        public static string BASE_URL = "https://firebasestorage.googleapis.com/v0/b/hcqs-project.appspot.com/o/";
        public static string DISH_PREFIX = "dish/";
        public static string COMBO_PREFIX = "combo/";
        public static string COUPON_PREFIX = "coupon/";
        public static string ORDER_PREFIX = "order/";
        public static string INVOICE_PREFIX = "invoice/";
        public static string RATING_PREFIX = "rating/";

    }

    public class SignalMessages
    {
        public static string LOAD_ORDER_SESIONS = "LOAD_ORDER_SESIONS";
        public static string LOAD_GROUPED_DISHES = "LOAD_GROUPED_DISHES";
        public static string LOAD_ORDER_DETAIL_STATUS = "LOAD_ORDER_DETAIL_STATUS";
        public static string LOAD_ORDER = "LOAD_ORDER";
        public static string LOAD_ORDER_DELIVERY = "LOAD_ORDER_DELIVERY";
        public static string LOAD_RE_DELIVERING_REQUEST = "LOAD_RE_DELIVERING_REQUEST";
        public static string LOAD_ASSIGNED_ORDER = "LOAD_ASSIGNED_ORDER";
        public static string LOAD_NOTIFICATION = "LOAD_NOTIFICATION";
        public static string LOAD_USER_ORDER = "LOAD_USER_ORDER";
        public static string LOAD_FINISHED_DISHES = "LOAD_FINISHED_DISHES ";
    }

    public class NotificationHeader
    {
        public static string CREATE_ORDER_SESSION = "Thông báo cho nhà bếp";
    }

    public class RoleName
    {
        public static string ROLE_CHEF = "CHEF";
        public static string ROLE_ADMIN = "ADMIN";
        public static string ROLE_SHIPPER = "SHIPPER";
        public static string ROLE_CUSTOMER = "CUSTOMER";

    }

    public class CancelledReason
    {
        public static string CANCELLED_BY_SYSTEM = "Đơn hàng được hủy bởi hệ thống";
    }

    public class OpenAIPrompt
    {
        public static string HOT_FIX_PROMPT = "Nếu câu hỏi thuộc các dạng câu: Món nào có vị thanh mát? ->trigger_tags: thanh mát|, " +
            "Có khai vị món nào giá từ 10000 đến 100000 không?->trigger_price=khai vị,10000-100000|, " +
            "Có bò lúc lắc không? -> trigger_find_dish_name: bò lúc lắc, " +
            "Tôi muốn đặt bàn cho 7 người vào 7h tối ngày mai.->trigger_find_table: startTime={{TODAY+1}}T19:00, endTime={{TODAY+1}}T20:00, numOfPeople=7|, " +
            "Giao hàng tới Chợ Bến Thành ko?->trigger_distance: address=Chợ Bến Thành, Quận 1, TP. HCM|." +
            "Nếu không thuộc các trigger trên, trả lời bình thường. Câu hỏi:";
        public static string GetCustomerGreeting(Account customer, string dishName, bool isFirstCall)
        {
            StringBuilder response = new StringBuilder();
            Random random = new Random();
            if (isFirstCall)
            {
                response.Append("Nhà hàng Thiên Phú xin chào ");
            }
            if (customer.Gender.HasValue)
            {
                if (customer.Gender.Value)
                {
                    response.Append("Anh ");
                }
                else
                {
                    response.Append("Chị ");
                }
            }
            else
            {
                response.Append("anh/chị ");
            }

            response.Append($"{customer.FirstName}");
            if (!isFirstCall)
            {
                return response.ToString();
            }
            int greetingRandom = random.Next(0, 7);

            if (greetingRandom == 1)
            {
                response.Append(", em có thể giúp gì cho mình ạ?");
            }
            else if (greetingRandom == 2)
            {
                response.Append(", hôm nay quý khách muốn dùng món nào ạ?");
            }
            else
            {
                if (!string.IsNullOrEmpty(dishName))
                {
                    response.Append($", hôm nay em gợi ý cho mình dùng món {dishName} hôm trước quý khách rất thích ạ");
                }
                else
                {
                    response.Append(", hôm nay quý khách muốn dùng món nào ạ?");
                }
            }
            return response.ToString();
        }
    }

}