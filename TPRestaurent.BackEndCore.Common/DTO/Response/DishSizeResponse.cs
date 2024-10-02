using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class DishSizeResponse
    {
        public DishReponse Dish { get; set; } = null!;
        public List<DishSizeDetail> DishSizeDetails { get; set; }  = new List<DishSizeDetail>();   
        
    }

    public class DishReponse {
        public Guid DishId { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Image { get; set; } = null!;
        public Domain.Enums.DishItemType DishItemTypeId { get; set; }
        [ForeignKey(nameof(DishItemTypeId))]
        public Domain.Models.EnumModels.DishItemType? DishItemType { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsDeleted { get; set; }
        public double AverageRating { get; set; } = 0;
        public int NumberOfRating { get; set; } = 0;

    }
}
