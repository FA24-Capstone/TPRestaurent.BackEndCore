using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class UpdateConfigurationDto
    {
        public Guid ConfigurationId { get; set; }
        public string? Name { get; set; } 
        public string? VietnameseName { get; set; }
        public string? CurrentValue { get; set; } 
        public string? Unit { get; set; }
    }
}
