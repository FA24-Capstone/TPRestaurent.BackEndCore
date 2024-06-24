using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class DishTag
    {
        [Key]
        public Guid DishTagId { get; set; }
        public string Name { get; set; } = null!;
        public Guid? DishId { get; set; }
        [ForeignKey(nameof(DishId))]
        public Dish? Dish { get; set; }
        public Guid? TagId { get; set; }
        [ForeignKey(nameof(TagId))]
        public Tag? Tag { get; set; }

    }
}
