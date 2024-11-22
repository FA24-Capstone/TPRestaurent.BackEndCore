namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class LoginRequestDto
    {
        public string PhoneNumber { get; set; } = null!;
        public string? OTPCode { get; set; }
    }
}