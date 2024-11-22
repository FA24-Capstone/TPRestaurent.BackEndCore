using System.ComponentModel.DataAnnotations.Schema;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class ComboResponseDto
    {
        public Guid ComboId { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; }
        public string? Image { get; set; }
        public double Price { get; set; }
        public double Discount { get; set; }
        public Domain.Enums.ComboCategory CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Domain.Models.EnumModels.ComboCategory? Category { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsDeleted { get; set; }
        public double AverageRating { get; set; } = 0;
        public int NumberOfRating { get; set; } = 0;
        public int? QuantityLeft { get; set; }
        public int DailyCountdown { get; set; }
        public int? PreparationTime { get; set; }
    }
}