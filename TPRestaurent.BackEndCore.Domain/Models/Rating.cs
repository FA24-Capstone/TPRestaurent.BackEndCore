using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TPRestaurent.BackEndCore.Domain.Models.BaseModel;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class Rating : BaseEntity
    {
        [Key]
        public Guid RatingId { get; set; }

        public string Title { get; set; } = null!;
        public Enums.RatingPoint PointId { get; set; }

        [ForeignKey(nameof(PointId))]
        public EnumModels.RatingPoint? Point { get; set; }

        public string Content { get; set; } = null!;
        public Guid? OrderDetailId { get; set; } = null!;

        [ForeignKey(nameof(OrderDetailId))]
        public OrderDetail? OrderDetail { get; set; }
    }
}