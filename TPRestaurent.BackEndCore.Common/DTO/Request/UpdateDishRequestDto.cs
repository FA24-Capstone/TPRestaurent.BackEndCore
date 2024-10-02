﻿using Microsoft.AspNetCore.Http;
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
        public bool IsAvailable { get; set; }
        public string Description { get; set; } = null!;
        public List<UpdateDishSizeDetailDto> UpdateDishSizeDetailDtos { get; set; } = new List<UpdateDishSizeDetailDto> { };
        public DishItemType DishItemType { get; set; }
    }

    public class UpdateDishSizeDetailDto
    {
        public Guid? DishSizeDetailId { get; set; }
        public DishSize DishSize { get; set; }
        public bool IsAvailable { get; set; }
        public double Price { get; set; }
        public double Discount { get; set; }
    }
}
