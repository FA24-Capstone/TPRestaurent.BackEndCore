using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class ComboDetailResponseDto
    {
        public Combo Combo { get; set; } = null!;
        public double AverageRating { get; set; } = 0;
        public int NumberOfRating { get; set; } = 0;
        public List<DishComboDto> DishCombo { get; set; } = new List<DishComboDto>();
        public List<Image> Imgs { get; set; } = new List<Image>();
        public List<RatingResponse> ComboRatings { get; set; } = new List<RatingResponse>();
        public List<DishTag> DishTags = new List<DishTag>();
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
