using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class DishCombo
    {
        [Key]
        public Guid DishComboId { get; set; }

        public int Quantity { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsAvailable { get; set; }
        public int? QuantityLeft { get; set; }
        public int DailyCountdown { get; set; }
        public Guid? DishSizeDetailId { get; set; }

        [ForeignKey(nameof(DishSizeDetailId))]
        public DishSizeDetail? DishSizeDetail { get; set; }

        public Guid? ComboOptionSetId { get; set; }

        [ForeignKey(nameof(ComboOptionSetId))]
        public ComboOptionSet? ComboOptionSet { get; set; }
    }
}