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

        public const string ALL = $"{ADMIN},{CHEF},{SHIPPER},{CUSTOMER}, {DEVICE}";
        public const string ALL_ACTORS = $"{ADMIN},{CHEF},{SHIPPER},{CUSTOMER}";
        public const string OFFLINE = $"{ADMIN},{CUSTOMER}, {DEVICE}";
        public const string PAYMENT = $"{ADMIN},{CUSTOMER}";
        public const string KITCHEN = $"{ADMIN},{CHEF}";
        public const string DELIVERY = $"{ADMIN},{SHIPPER}";
        public const string DELIVERY_WITH_CUSTOMER = $"{ADMIN},{SHIPPER}";
    }
}
