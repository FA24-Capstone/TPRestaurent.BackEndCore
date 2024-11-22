namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class StatisticReportDashboardResponse
    {
        public OrderStatusReportResponse OrderStatusReportResponse { get; set; } = null!;
        public Dictionary<string, decimal> MonthlyRevenue { get; set; } = null!;
    }
}