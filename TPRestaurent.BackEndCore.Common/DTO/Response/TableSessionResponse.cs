using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class TableSessionResponse
    {
        public TableSession TableSession { get; set; } = null!;
        public List<PrelistOrder> PrelistOrders { get; set; } = new List<PrelistOrder>();       
    }
}
