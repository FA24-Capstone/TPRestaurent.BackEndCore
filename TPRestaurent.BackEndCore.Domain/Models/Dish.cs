using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Enums;

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
    }
}
