namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class UpdateCustomerInforAddressRequest
    {
        public Guid CustomerInfoAddressId { get; set; }
        public string CustomerInfoAddressName { get; set; } = null!;
        public bool IsCurrentUsed { get; set; }
        public string? AccountId { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
    }
}