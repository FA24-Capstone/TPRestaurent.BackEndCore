using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class StaticFile
    {
        [Key]
        public Guid StaticFileId { get; set; }
        public string Path { get; set; } = null!;
        public Guid? DishId { get; set; }
        [ForeignKey(nameof(DishId))]
        public Dish? Dish { get; set; }
        public Guid? BlogId { get; set; }
        [ForeignKey(nameof(BlogId))]
        public Blog? Blog { get; set; }
    }
}
