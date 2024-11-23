namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class EnableNotificationRequest
    {
        public string DeviceName { get; set; } = null!;
        public string Token { get; set; } = null!;
    }
}