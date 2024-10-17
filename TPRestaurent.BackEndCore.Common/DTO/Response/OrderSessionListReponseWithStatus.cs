using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class OrderSessionListReponseWithStatus
    {
        public List<OrderSessionResponse>? Items { get; set; }
        public int TotalPages { get; set; }
        public int PreOrderQuantity { get; set; }
        public int ConfirmedQuantity { get; set; }
        public int ProcessingQuantity { get; set; }
        public int LateWarningQuantity { get; set; }
        public int CompletedQuantity { get; set; }
        public int CancelledQuantity { get; set; }
    }
}
