using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class StatisticReportDashboardResponse
    {
        public OrderStatusReportResponse OrderStatusReportResponse { get; set; } = null!;
        public Dictionary<int, decimal> MonthlyRevenue { get; set; } = null!;
    }
}
