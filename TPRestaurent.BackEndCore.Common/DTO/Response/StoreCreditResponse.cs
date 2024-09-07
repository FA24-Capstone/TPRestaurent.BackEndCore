using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class StoreCreditResponse
    {
        public CustomerInfo CustomerInfo { get; set; }
        public StoreCredit StoreCredit { get; set; }
        //public List<StoreCreditHistory> StoreCreditHistories { get; set; } = new List<StoreCreditHistory>();
    }
}
