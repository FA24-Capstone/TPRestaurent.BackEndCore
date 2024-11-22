namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class EmployeeSignUpRequest
    {
        public string? Email { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public bool Gender { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public string RoleName { get; set; } = null!;
    }
}