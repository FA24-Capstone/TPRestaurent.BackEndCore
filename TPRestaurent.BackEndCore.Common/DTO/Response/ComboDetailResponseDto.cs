using System.ComponentModel.DataAnnotations.Schema;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class ComboDetailResponseDto
    {
        public ComboResponseDto Combo { get; set; } = null!;
        public List<DishComboDto> DishCombo { get; set; } = new List<DishComboDto>();
        public List<Image> Imgs { get; set; } = new List<Image>();
        public List<RatingResponse> ComboRatings { get; set; } = new List<RatingResponse>();
        public List<DishTag> DishTags { get; set; } = new List<DishTag>();
    }

    public class DishComboDto
    {
        public Guid ComboOptionSetId { get; set; }
        public int OptionSetNumber { get; set; }
        public int NumOfChoice { get; set; }
        public Domain.Enums.DishItemType DishItemTypeId { get; set; }

        [ForeignKey(nameof(DishItemTypeId))]
        public Domain.Models.EnumModels.DishItemType DishItemType { get; set; }

        public List<DishCombo> DishCombo { get; set; } = new List<DishCombo>();
    }
}