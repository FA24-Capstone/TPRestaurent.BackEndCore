using System.ComponentModel.DataAnnotations;

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