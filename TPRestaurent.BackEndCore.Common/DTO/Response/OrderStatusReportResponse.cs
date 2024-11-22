namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class OrderStatusReportResponse
    {
        public int SuccessfullyOrderNumber { get; set; }
        public int CancellingOrderNumber { get; set; }
        public int PendingOrderNumber { get; set; }
    }
}