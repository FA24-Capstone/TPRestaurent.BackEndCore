using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class DishTag
    {
        [Key]
        public Guid DishTagId { get; set; }

        public Guid? DishId { get; set; }

        [ForeignKey(nameof(DishId))]
        public Dish? Dish { get; set; }

        public Guid? ComboId { get; set; }

        [ForeignKey(nameof(ComboId))]
        public Combo? Combo { get; set; }

        public Guid? TagId { get; set; }

        [ForeignKey(nameof(TagId))]
        public Tag? Tag { get; set; }
    }
}