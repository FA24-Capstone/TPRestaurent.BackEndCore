using Microsoft.AspNetCore.Http;
using TPRestaurent.BackEndCore.Domain.Enums;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class DishDto
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public IFormFile? MainImageFile { get; set; }
        public List<IFormFile>? ImageFiles { get; set; } = new List<IFormFile>();
        public List<DishSizeDetailDto> DishSizeDetailDtos { get; set; } = new List<DishSizeDetailDto>();
        public List<Guid> TagIds { get; set; } = new List<Guid>();
        public DishItemType DishItemType { get; set; }
        public int? PreparationTime { get; set; }
    }

    public class DishSizeDetailDto
    {
        public double Price { get; set; }
        public double Discount { get; set; }
        public DishSize DishSize { get; set; }
        public int? QuantityLeft { get; set; }
        public int DailyCountdown { get; set; }
    }
}