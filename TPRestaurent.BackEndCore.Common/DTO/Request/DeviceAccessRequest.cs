namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class DeviceAccessRequest
    {
        public string DeviceCode { get; set; } = null!;
        public string DevicePassword { get; set; } = null!;
        public Guid TableId { get; set; }
    }
}