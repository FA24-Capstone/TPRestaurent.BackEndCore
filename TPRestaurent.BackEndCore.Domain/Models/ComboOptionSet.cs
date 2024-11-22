using System.ComponentModel.DataAnnotations.Schema;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class ComboOptionSet
    {
        public Guid ComboOptionSetId { get; set; }
        public int OptionSetNumber { get; set; }
        public int NumOfChoice { get; set; }
        public bool IsDeleted { get; set; }
        public Enums.DishItemType DishItemTypeId { get; set; }

        [ForeignKey(nameof(DishItemTypeId))]
        public EnumModels.DishItemType DishItemType { get; set; }

        public Guid ComboId { get; set; }

        [ForeignKey(nameof(ComboId))]
        public Combo? Combo { get; set; }
    }
}