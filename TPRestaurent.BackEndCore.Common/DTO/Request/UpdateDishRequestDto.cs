using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Enums;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class UpdateDishRequestDto
    {
        public Guid DishId { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public IFormFile? MainImageFile { get; set; }
        public List<IFormFile>? ImageFiles { get; set; }
        public double Price { get; set; }
        public double Discount { get; set; }
        public DishItemType DishItemType { get; set; }
        public bool isAvailable { get; set; }
    }
}
