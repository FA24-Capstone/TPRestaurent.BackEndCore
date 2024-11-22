using Microsoft.AspNetCore.Http;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class CreateRatingRequestDto
    {
        public string? Title { get; set; } = null!;
        public Domain.Enums.RatingPoint PointId { get; set; }
        public string Content { get; set; } = null!;
        public Guid OrderDetailId { get; set; }
        public List<IFormFile>? ImageFiles { get; set; } = new List<IFormFile>();
        public string? AccountId { get; set; }
    }
}