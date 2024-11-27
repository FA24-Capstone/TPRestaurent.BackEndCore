using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.Utils
{
    public class Permission
    {
        public const string ADMIN = "ADMIN";
        public const string CHEF = "CHEF";
        public const string SHIPPER = "SHIPPER";
        public const string CUSTOMER = "CUSTOMER";
        public const string DEVICE = "DEVICE";

        public const string ALL = $"{ADMIN},{CHEF},{SHIPPER},{CUSTOMER},{DEVICE}";
        public const string ALL_ACTORS = $"{ADMIN},{CHEF},{SHIPPER},{CUSTOMER}";
        public const string PAYMENT = $"{ADMIN},{CUSTOMER},{DEVICE}";
        public const string KITCHEN = $"{ADMIN},{CHEF}";
        public const string DELIVERY = $"{ADMIN},{SHIPPER}";
        public const string DELIVERY_WITH_CUSTOMER = $"{ADMIN},{SHIPPER},{CUSTOMER}";
        public const string CREATE_ORDER = $"{CUSTOMER},{DEVICE}";
        public const string ORDER_STATUS_MANAGEMENT = $"{ADMIN},{SHIPPER},{DEVICE}";
        public const string RESERVATION_TIME_VIEW = $"{ADMIN},{DEVICE}";
        public const string CANCEL_DISH = $"{CHEF},{CUSTOMER}";
    }
}
