namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class StatisticReportNumberResponse
    {
        public ProfitReportResponse ProfitReportResponse { get; set; } = null!;
        public int TotalChefNumber { get; set; }
        public CustomerStasticResponse CustomerStasticResponse { get; set; } = null!;
        public int TotalDeliveringOrderNumber { get; set; }
        public int TotalReservationNumber { get; set; }
        public ShipperStatisticResponse ShipperStatisticResponse { get; set; } = null!;
    }
}