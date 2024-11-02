using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Enums;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class ComboDto
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; }
        public double Price { get; set; }
        public IFormFile MainImg { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<DishComboDto> DishComboDtos { get; set; } = new List<DishComboDto>();
        public List<Guid> TagIds { get; set; } = new List<Guid>();
        public List<IFormFile>? Imgs { get; set; } = new List<IFormFile>();
        public int? PreparationTime { get; set; }
    }

    public class DishComboDto
    {
        public Guid? OptionSetId { get; set; }
        public int OptionSetNumber { get; set; }
        public Domain.Enums.DishItemType DishItemType { get; set; }
        public int NumOfChoice { get; set; }
        public List<ComboDishSizeDetailDto> ListDishId { get; set; } = new List<ComboDishSizeDetailDto>();  
    }

    public class ComboDishSizeDetailDto
    {
        public int Quantity { get; set; }
        public Guid DishSizeDetailId { get; set; }

    }
}
