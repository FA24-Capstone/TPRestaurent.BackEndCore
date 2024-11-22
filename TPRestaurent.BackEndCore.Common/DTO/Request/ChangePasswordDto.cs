namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class ChangePasswordDto
    {
        public string PhoneNumber { get; set; } = null!;
        public string OldPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
        public string OTPCode { get; set; } = null!;
    }
}