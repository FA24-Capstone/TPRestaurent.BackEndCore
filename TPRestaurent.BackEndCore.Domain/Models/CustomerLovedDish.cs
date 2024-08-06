using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class CustomerLovedDish
    {
        [Key]
        public Guid CustomerLovedDishId { get; set; }
        public Guid? DishId { get; set; }
        [ForeignKey(nameof(DishId))]
        public Dish? Dish { get; set; } 
        public Guid? ComboId { get; set; }
        [ForeignKey(nameof(ComboId))]
        public Combo? Combo { get; set; }
        public Guid CustomerInfoId { get; set; }
        [ForeignKey(nameof(CustomerInfoId))]
        public CustomerInfo? CustomerInfo { get; set; }
    }
}
