using Microsoft.AspNetCore.Http;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class ConfirmedOrderRequest
    {
        public Guid OrderId { get; set; }
        public bool? IsSuccessful { get; set; } = true;
        public bool RefundRequired { get; set; }
        public double? Lat { get; set; }
        public double? Lng { get; set; }
        public string? CancelReason { get; set; } = string.Empty;
        public IFormFile? Image { get; set; } = null!;
    }
}