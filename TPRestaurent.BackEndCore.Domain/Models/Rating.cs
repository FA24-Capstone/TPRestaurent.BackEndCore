using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Enums;
using TPRestaurent.BackEndCore.Domain.Models.BaseModel;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class Rating: BaseEntity
    {
        [Key]
        public Guid RatingId { get; set; }
        public string Title { get; set; } = null!;
        public RatingPoint Point { get; set; }
        public string Content { get; set; } = null!;
        public Guid? DishId { get; set; } = null!;
        [ForeignKey(nameof(DishId))]
        public Dish? Dish { get; set; }
        public string AccountId { get; set; } = null!;
        [ForeignKey(nameof(AccountId))]
        public Account? Account { get; set; }
    }
}
