﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Enums;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class DishDto
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public IFormFile? MainImageFile { get; set; }
        public List<IFormFile>? ImageFiles { get; set; } = new List<IFormFile>();
        public List<DishSizeDetailDto> DishSizeDetailDtos { get; set; } = new List<DishSizeDetailDto> { };
        public DishItemType DishItemType { get; set; }
    }

    public class DishSizeDetailDto
    {
        public double Price { get; set; }
        public double Discount { get; set; }
        public DishSize DishSize { get; set; }
    }
}
