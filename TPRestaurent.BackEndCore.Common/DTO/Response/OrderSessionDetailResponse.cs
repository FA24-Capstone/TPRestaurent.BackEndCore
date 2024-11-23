using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class OrderSessionDetailResponse
    {
        public OrderSession OrderSession { get; set; } = null!;
        public Table? Table { get; set; } = null!;
        public Order Order { get; set; } = null!;
        public List<OrderDetailResponse> OrderDetails { get; set; } = new List<OrderDetailResponse>();
    }
}