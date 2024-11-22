namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class DeviceReponse
    {
        public Guid TableId { get; set; }
        public string DeviceCode { get; set; } = null!;
        public string DevicePassword { get; set; } = null!;
    }
}