using TPRestaurent.BackEndCore.Domain.Enums;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class InvoiceFilterRequest
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public OrderType? OrderType { get; set; }
        public int pageNumber { get; set; } = 0;
        public int pageSize { get; set; } = 0;
    }
}