using Microsoft.AspNetCore.Http;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class UpdateAccountImageRequest
    {
        public string? AccountId { get; set; }
        public string? Avatar { get; set; }
        public IFormFile Image { get; set; } = null!;
    }
}