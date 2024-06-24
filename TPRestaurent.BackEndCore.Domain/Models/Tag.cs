using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class Tag
    {
        [Key]
        public Guid TagId { get; set; }
        public string Name { get; set; } = null!;
    }
}
