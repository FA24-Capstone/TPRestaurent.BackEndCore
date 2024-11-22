using Microsoft.AspNetCore.Http;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class UpdateDishImageRequest
    {
        public Guid DishId { get; set; }
        public IFormFile Image { get; set; } = null!;
        public string OldImageLink { get; set; } = null!;
    }
}