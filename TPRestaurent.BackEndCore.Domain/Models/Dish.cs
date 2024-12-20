﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class Dish
    {
        [Key]
        public Guid DishId { get; set; }

        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Image { get; set; } = null!;
        public Enums.DishItemType DishItemTypeId { get; set; }

        [ForeignKey(nameof(DishItemTypeId))]
        public EnumModels.DishItemType? DishItemType { get; set; }

        public bool isAvailable { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsMainItem { get; set; }
        public int? PreparationTime { get; set; }
    }
}