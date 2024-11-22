using System.ComponentModel.DataAnnotations;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class Room
    {
        [Key]
        public Guid TableRatingId { get; set; }

        public string Name { get; set; } = null!;
        public bool IsPrivate { get; set; }
    }
}