using System.ComponentModel.DataAnnotations;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class Tag
    {
        [Key]
        public Guid TagId { get; set; }

        public string Name { get; set; } = null!;
    }
}