namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class ForgotPasswordDto
    {
        public string PhoneNumber { get; set; } = null!;
        public string? RecoveryCode { get; set; }
        public string? NewPassword { get; set; }
    }
}