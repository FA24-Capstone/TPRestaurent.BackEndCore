using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class OrderSessionResponse
    {
        public OrderSession OrderSession { get; set; } = null!;
        public Table? Table { get; set; } = null!;
        public Order Order { get; set; } = null!;
        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}