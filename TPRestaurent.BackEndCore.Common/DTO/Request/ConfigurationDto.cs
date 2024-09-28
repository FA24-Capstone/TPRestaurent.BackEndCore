using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class ConfigurationDto
    {
        public string Name { get; set; } = null!;
        public string VietnameseName = null!;   
        public string CurrentValue { get; set; } = null!;
        public string Unit { get; set; } = null!;
    }
}
