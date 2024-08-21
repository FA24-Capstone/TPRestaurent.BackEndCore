using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class Configuration
    {
        [Key] 
        public Guid ConfigurationId { get; set; }
        public string Name { get; set; } = null!;
        public string? VietnameseName { get; set; } = null!;
        public string PreValue { get; set; } = null!;
        public string? ActiveValue { get; set; } = null!;
        public DateTime? ActiveDate { get; set; }
    }
}
