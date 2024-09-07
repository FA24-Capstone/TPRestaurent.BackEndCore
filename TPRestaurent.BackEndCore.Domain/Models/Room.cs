using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class Room
    {
        [Key]
        public Guid TableRatingId { get; set; }
        public string Name { get; set; } = null!;
    }
}
