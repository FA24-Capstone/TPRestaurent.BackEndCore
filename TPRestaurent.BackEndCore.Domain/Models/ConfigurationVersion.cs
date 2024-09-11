using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class ConfigurationVersion
    {
        [Key]
        public Guid ConfigurationVersionId { get; set; }
        public string ActiveValue { get; set; }
        public DateTime ActiveDate { get; set; }
        public Guid ConfigurationId { get; set; }
        [ForeignKey(nameof(ConfigurationId))]
        public Configuration? Configuration { get; set; }
    }
}
