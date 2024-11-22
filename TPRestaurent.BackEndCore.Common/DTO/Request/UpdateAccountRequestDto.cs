namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class UpdateAccountPhoneNumberRequest
    {
        public string? PhoneNumber { get; set; }
        public string? OTPCode { get; set; }
    }
}