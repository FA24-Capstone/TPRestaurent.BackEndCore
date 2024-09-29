using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        public string CurrentValue { get; set; } = null!;
        public string Unit { get; set; } = null!;
    }
}
