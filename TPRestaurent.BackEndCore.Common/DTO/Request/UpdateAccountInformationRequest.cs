namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class UpdateAccountInformationRequest
    {
        public string? PhoneNumber { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool Gender { get; set; }
        public string? Avatar { get; set; }
    }
}