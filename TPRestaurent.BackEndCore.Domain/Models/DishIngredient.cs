using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class DishIngredient
    {
        [Key]
        public Guid DishIngredientId { get; set; }
        public Guid DishId { get; set; }
        [ForeignKey(nameof(DishId))]
        public Dish? Dish { get; set; }
        public Guid? IngredientId { get; set; }
        [ForeignKey(nameof(IngredientId))]
        public Ingredient? Ingredient { get; set; }
    }
}
