using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Models.BaseModel;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class Blog : BaseEntity
    {
        [Key]
        public Guid BlogId { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string Image { get; set; } =null!;

    }
}
