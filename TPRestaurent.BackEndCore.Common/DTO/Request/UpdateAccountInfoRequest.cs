using Microsoft.AspNetCore.Http;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class UpdateAccountInfoRequest
    {
        public string? AccountId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateTime? DOB { get; set; }
        public bool Gender { get; set; }
        public IFormFile? Image { get; set; } = null!;
    }
}