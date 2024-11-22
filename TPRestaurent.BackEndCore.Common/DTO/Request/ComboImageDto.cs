using Microsoft.AspNetCore.Http;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class ComboImageDto
    {
        public Guid ComboId { get; set; }
        public IFormFile Img { get; set; } = null!;
        public string OldImageLink { get; set; } = null!;
    }
}