using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class OrderStatusReportResponse
    {
        public int SuccessfullyOrderNumber { get; set; }
        public int CancellingOrderNumber { get; set; }
        public int PendingOrderNumber { get; set; }      
    }
}
