using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class OrderReponse
    {
        public Order Order { get; set; }
        public List<OrderDetailResponse> OrderDetails { get; set; } = new List<OrderDetailResponse>();
    }

    public class OrderDetailResponse
    {
        public OrderDetail OrderDetail { get; set; }
        public List<ComboOrderDetail> ComboOrderDetails { get; set; } = new List<ComboOrderDetail> { };
    }
}