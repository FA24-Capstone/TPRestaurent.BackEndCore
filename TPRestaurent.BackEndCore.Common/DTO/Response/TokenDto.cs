namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class TokenDto
    {
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public string? MainRole { get; set; }
        public AccountResponse Account { get; set; }
        public DeviceResponse? DeviceResponse { get; set; }
    }
}