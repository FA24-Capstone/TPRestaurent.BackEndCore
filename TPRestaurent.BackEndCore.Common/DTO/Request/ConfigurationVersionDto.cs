using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class ConfigurationVersionDto
    {
        public string ActiveValue { get; set; } = null!;
        public DateTime ActiveDate { get; set; }
        public Guid ConfigurationId { get; set; }
    }
}
