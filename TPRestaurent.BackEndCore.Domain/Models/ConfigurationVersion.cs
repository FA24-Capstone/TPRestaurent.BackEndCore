using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class ConfigurationVersion
    {
        [Key]
        public Guid ConfigurationVersionId { get; set; }

        public string ActiveValue { get; set; }
        public DateTime ActiveDate { get; set; }
        public bool IsApplied { get; set; }
        public Guid ConfigurationId { get; set; }

        [ForeignKey(nameof(ConfigurationId))]
        public Configuration? Configuration { get; set; }
    }
}