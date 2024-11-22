namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class DeviceResponse
    {
        public string DeviceCode { get; set; } = null!;
        public string DevicePassword { get; set; } = null!;
        public Guid TableId { get; set; }
        public string TableName { get; set; }
        public string MainRole { get; set; } = "DEVICE";
    }
}