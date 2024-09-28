using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class DishSizeDetail
    {
        [Key]
        public Guid DishSizeDetailId { get; set; }
        public bool IsAvailable { get; set; }
        public double Price { get; set; }
        public double Discount { get; set; }
        public Guid? DishId { get; set; }
        [ForeignKey(nameof(DishId))]
        public Dish? Dish { get; set; }
        public Enums.DishSize DishSizeId { get; set; }
        [ForeignKey(nameof(DishSizeId))]
        public EnumModels.DishSize? DishSize { get; set; }
        public int? QuantityLeft { get; set; }
        public int DailyCountdown { get; set; }
    }
}
