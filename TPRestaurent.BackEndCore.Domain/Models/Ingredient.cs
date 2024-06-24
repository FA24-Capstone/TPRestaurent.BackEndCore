using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class Ingredient
    {
        [Key]   
        public Guid IngredientId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; } 
        public string Image { get; set; } = null!;
    }
}
