namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class LoginDeviceRequestDto
    {
        public string DeviceCode { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}